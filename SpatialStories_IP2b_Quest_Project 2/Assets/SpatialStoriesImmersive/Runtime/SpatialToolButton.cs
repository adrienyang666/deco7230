using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using InputXRController = UnityEngine.InputSystem.XR.XRController;

namespace SpatialStories.Immersive
{
    /// <summary>
    /// Selectable spherical spatial tool. Its separate world-space UI face may
    /// use the same controller action for ray-trigger and poke interactions.
    /// Mere hover or contact never invokes a destructive action.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class SpatialToolButton : MonoBehaviour
    {
        public ImmersiveStory controller;
        public int action;
        public GameObject highlight;
        [Min(0f)] public float debounceSeconds = 0.35f;
        [Tooltip("Optional physical tips. Only active, tracked controller interactors can press the face.")]
        public Transform[] pressTips;
        public XRBaseInteractor[] pressInteractors;
        [Tooltip("The press surface. Its local -Z faces the approaching user.")]
        public Transform pressFace;
        [Min(0.01f)] public float pressRadius = 0.10f;
        [Tooltip("Optional child visual which moves briefly inward when pressed.")]
        public Transform cap;

        XRSimpleInteractable interactable;
        bool listening;
        float lastPressed = float.NegativeInfinity;
        Vector3[] previousTipPositions;
        bool[] validTipSamples;
        bool[] frontArmed;
        XRBaseInteractor[] resolvedInteractors;
        Vector3 capRestPosition;
        bool hasCapRestPosition;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            CaptureCap();
        }

        void OnEnable()
        {
            if (!interactable) interactable = GetComponent<XRSimpleInteractable>();
            if (!interactable || listening) return;
            interactable.firstHoverEntered.AddListener(OnHoverEntered);
            interactable.lastHoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelected);
            interactable.activated.AddListener(OnActivated);
            listening = true;
            ResetPressHistory();
            CaptureCap();
            RefreshHighlight();
        }

        void OnDisable()
        {
            Unsubscribe();
            ResetPressHistory();
            if (cap && hasCapRestPosition) cap.localPosition = capRestPosition;
            if (highlight) highlight.SetActive(false);
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        void Unsubscribe()
        {
            if (!interactable || !listening) return;
            interactable.firstHoverEntered.RemoveListener(OnHoverEntered);
            interactable.lastHoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelected);
            interactable.activated.RemoveListener(OnActivated);
            listening = false;
        }

        void OnSelected(SelectEnterEventArgs args) => Press();
        void OnActivated(ActivateEventArgs args) => Press();
        void OnHoverEntered(HoverEnterEventArgs args) => RefreshHighlight();
        void OnHoverExited(HoverExitEventArgs args) => RefreshHighlight();

        /// <summary>UnityEvent-compatible entry point for a world-space UI face.</summary>
        public void Press()
        {
            if (!isActiveAndEnabled || !controller || !controller.isActiveAndEnabled ||
                !controller.IsActionAllowed(action)) return;
            var now = Time.unscaledTime;
            if (now - lastPressed < debounceSeconds) return;
            lastPressed = now;
            controller.Act(action);
        }

        void LateUpdate()
        {
            RefreshHighlight();
            CheckPhysicalPress();
            if (cap && hasCapRestPosition)
            {
                var progress = Mathf.Clamp01((Time.unscaledTime - lastPressed) / 0.22f);
                var displacement = Mathf.Sin(progress * Mathf.PI) * 0.02f;
                cap.localPosition = capRestPosition + Vector3.forward * displacement;
            }
        }

        void CaptureCap()
        {
            if (!cap || hasCapRestPosition) return;
            capRestPosition = cap.localPosition;
            hasCapRestPosition = true;
        }

        void ResetPressHistory()
        {
            var count = pressTips == null ? 0 : pressTips.Length;
            if (validTipSamples == null || validTipSamples.Length != count)
            {
                previousTipPositions = new Vector3[count];
                validTipSamples = new bool[count];
                frontArmed = new bool[count];
                resolvedInteractors = new XRBaseInteractor[count];
            }
            for (var index = 0; index < count; index++)
            {
                validTipSamples[index] = false;
                frontArmed[index] = false;
                resolvedInteractors[index] = pressInteractors != null && index < pressInteractors.Length
                    ? pressInteractors[index] : null;
                if (!resolvedInteractors[index] && pressTips[index])
                    resolvedInteractors[index] = pressTips[index].GetComponentInParent<XRBaseInteractor>();
            }
        }

        bool TipIsTracked(int index)
        {
            var tip = pressTips[index];
            var owner = resolvedInteractors[index];
            if (!tip || !tip.gameObject.activeInHierarchy || !owner || !owner.isActiveAndEnabled) return false;
            InputXRController device;
            switch (owner.handedness)
            {
                case InteractorHandedness.Left: device = InputXRController.leftHand; break;
                case InteractorHandedness.Right: device = InputXRController.rightHand; break;
                default: return false;
            }
            if (device == null) return false;
            // Same position/rotation fallback used by XRI's modality manager.
            return device.isTracked.isPressed || (device.trackingState.ReadValue() & 3) == 3;
        }

        void CheckPhysicalPress()
        {
            if (!pressFace || pressTips == null || pressTips.Length == 0) return;
            if (validTipSamples == null || validTipSamples.Length != pressTips.Length) ResetPressHistory();
            if (!controller || !controller.isActiveAndEnabled || !controller.IsActionAllowed(action))
            {
                ResetPressHistory();
                return;
            }

            var inverseRotation = Quaternion.Inverse(pressFace.rotation);
            var workspaceScale = controller.contentRoot ? Mathf.Abs(controller.contentRoot.lossyScale.x) : 1f;
            var radius = pressRadius * workspaceScale;
            var radiusSquared = radius * radius;
            for (var index = 0; index < pressTips.Length; index++)
            {
                if (!TipIsTracked(index))
                {
                    validTipSamples[index] = false;
                    frontArmed[index] = false;
                    continue;
                }
                // Use world metres, independent of a scaled sphere hierarchy.
                var point = inverseRotation * (pressTips[index].position - pressFace.position);
                if (!validTipSamples[index])
                {
                    previousTipPositions[index] = point;
                    validTipSamples[index] = true;
                    frontArmed[index] = false;
                    continue;
                }

                var previous = previousTipPositions[index];
                previousTipPositions[index] = point;
                var insideRadius = point.x * point.x + point.y * point.y <= radiusSquared;
                // Reject teleportation/tracking jumps and entering from behind.
                if (!insideRadius || point.z < -0.25f || point.z > 0.08f ||
                    (point - previous).sqrMagnitude > 0.04f)
                {
                    frontArmed[index] = false;
                    continue;
                }
                if (point.z < -0.02f) frontArmed[index] = true;
                if (frontArmed[index] && previous.z < -0.015f && point.z >= -0.015f)
                {
                    frontArmed[index] = false;
                    Press();
                    // The controller may hide this button synchronously.
                    if (!isActiveAndEnabled) return;
                }
            }
        }

        void RefreshHighlight()
        {
            if (!highlight) return;
            var allowed = controller && controller.isActiveAndEnabled && controller.IsActionAllowed(action);
            var lit = allowed && interactable && (interactable.isHovered || interactable.isSelected);
            if (highlight.activeSelf != lit) highlight.SetActive(lit);
        }
    }
}
