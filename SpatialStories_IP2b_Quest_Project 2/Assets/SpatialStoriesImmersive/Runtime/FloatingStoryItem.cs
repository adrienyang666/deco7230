using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace SpatialStories.Immersive
{
    /// <summary>
    /// One spatial photo, caption or sticker. The controller decides what a release
    /// means; this component owns XR events and safe, bounded manipulation only.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
    public sealed class FloatingStoryItem : MonoBehaviour
    {
        public ImmersiveStory controller;
        public string itemId;
        public bool isPhoto;
        public bool isText;
        public bool placed;
        public bool deleted;
        public Vector2 size = new Vector2(0.3f, 0.3f);
        public GameObject highlight;
        public bool persistentHighlight;
        public string content;
        public Color textColor = Color.white;
        public Text label;
        public int stickerIndex = -1;
        [HideInInspector] public bool isCutout, isSourceHandle, justLifted;
        [HideInInspector] public bool isDiscoveryHandle;
        [HideInInspector] public string sourcePhotoId;
        [HideInInspector] public FloatingStoryItem ownerPhoto;
        [HideInInspector] public int collageLayer;

        XRGrabInteractable grab;
        Rigidbody body;
        Coroutine releaseRoutine;
        bool listening;
        bool activeGrab;
        bool cancelledDuringGrab;
        bool hasSavedPose;
        Transform savedParent;
        Vector3 savedPosition;
        Quaternion savedRotation;
        Vector3 savedScale;
        Vector3 savedWorldPosition;
        Quaternion savedWorldRotation;
        Vector3 previousPosition;
        Vector3 estimatedVelocity;
        float previousSampleTime;
        bool enlargingWhileHeld;
        Vector3 enlargementStartScale;
        Vector3 enlargementTargetScale;
        float enlargementStartTime;
        float enlargementDuration;

        public XRGrabInteractable Grab
        {
            get
            {
                if (!grab) grab = GetComponent<XRGrabInteractable>();
                return grab;
            }
        }

        public bool Held => Grab && Grab.isSelected;
        public bool Busy => Held || releaseRoutine != null;
        public int SelectingHandCount => Grab ? Grab.interactorsSelecting.Count : 0;
        public bool ResizedWithTwoHandsDuringGrab { get; private set; }

        /// <summary>
        /// Smoothed world velocity at final release, limited to a safe range.
        /// The rigidbody never receives this automatically. The controller may
        /// use it for a deliberate throw toward the spatial bin.
        /// </summary>
        public Vector3 ReleaseVelocity { get; private set; }

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            ConfigureGrab();
            SavePose();
        }

        void OnEnable()
        {
            if (!Grab || listening) return;
            grab.firstHoverEntered.AddListener(OnHoverEntered);
            grab.lastHoverExited.AddListener(OnHoverExited);
            grab.firstSelectEntered.AddListener(OnFirstGrab);
            grab.selectEntered.AddListener(OnAnyHandGrabbed);
            grab.selectExited.AddListener(OnAnyHandReleased);
            grab.lastSelectExited.AddListener(OnFinalRelease);
            grab.activated.AddListener(OnActivated);
            listening = true;
            UpdateHighlight();
        }

        void OnDisable()
        {
            Unsubscribe();
            if (releaseRoutine != null)
            {
                StopCoroutine(releaseRoutine);
                releaseRoutine = null;
            }
            // Deletion/reset is owned by the controller. In particular, do not
            // send a release callback while its scene objects are being disabled.
            activeGrab = false;
            cancelledDuringGrab = false;
            enlargingWhileHeld = false;
            if (highlight) highlight.SetActive(false);
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        void Unsubscribe()
        {
            if (!grab || !listening) return;
            grab.firstHoverEntered.RemoveListener(OnHoverEntered);
            grab.lastHoverExited.RemoveListener(OnHoverExited);
            grab.firstSelectEntered.RemoveListener(OnFirstGrab);
            grab.selectEntered.RemoveListener(OnAnyHandGrabbed);
            grab.selectExited.RemoveListener(OnAnyHandReleased);
            grab.lastSelectExited.RemoveListener(OnFinalRelease);
            grab.activated.RemoveListener(OnActivated);
            listening = false;
        }

        /// <summary>Also available to the editor scene builder before saving.</summary>
        public void ConfigureGrab()
        {
            if (!Grab) return;
            if (!body) body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            grab.addDefaultGrabTransformers = false;
            grab.selectMode = InteractableSelectMode.Multiple;
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.throwOnDetach = false;
            grab.useDynamicAttach = true;
            grab.matchAttachPosition = true;
            grab.matchAttachRotation = true;
            grab.reinitializeDynamicAttachEverySingleGrab = true;
            grab.unparentTransformOnGrab = false;
            grab.retainTransformParent = true;
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.trackScale = true;

            var transformer = GetComponent<XRGeneralGrabTransformer>();
            if (!transformer) transformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            transformer.allowOneHandedScaling = false;
            transformer.allowTwoHandedScaling = true;
            transformer.allowTwoHandedRotation = XRGeneralGrabTransformer.TwoHandedRotationMode.TwoHandedAverage;
            transformer.clampScaling = true;
            transformer.minimumScaleRatio = 0.35f;
            transformer.maximumScaleRatio = 2.5f;
            transformer.thresholdMoveRatioForScale = 0.03f;

            if (Application.isPlaying)
            {
                // XRI registration is idempotent; a saved starting transformer
                // and a runtime ConfigureGrab call therefore cannot duplicate it.
                grab.AddSingleGrabTransformer(transformer);
                grab.AddMultipleGrabTransformer(transformer);
            }
            else
            {
                if (!grab.startingSingleGrabTransformers.Contains(transformer))
                    grab.startingSingleGrabTransformers.Add(transformer);
                if (!grab.startingMultipleGrabTransformers.Contains(transformer))
                    grab.startingMultipleGrabTransformers.Add(transformer);
            }
        }

        public void SavePose()
        {
            savedParent = transform.parent;
            savedPosition = transform.localPosition;
            savedRotation = transform.localRotation;
            savedScale = transform.localScale;
            savedWorldPosition = transform.position;
            savedWorldRotation = transform.rotation;
            hasSavedPose = true;
            if (Application.isPlaying && Grab && !Held)
            {
                // Home/Undo can run after RestorePose has queued an XRI target.
                // Keep that pending target aligned with the newly saved pose so
                // re-enabling the grab component cannot jump to an earlier pose.
                grab.SetTargetPose(new Pose(transform.position, transform.rotation));
                grab.SetTargetLocalScale(transform.localScale);
            }
        }

        public void EnableStableTwoHandScaling()
        {
            var stable = GetComponent<CollageScaleRotationLock>();
            if (!stable) stable = gameObject.AddComponent<CollageScaleRotationLock>();
            Grab.RemoveMultipleGrabTransformer(stable);
            Grab.AddMultipleGrabTransformer(stable);
        }

        /// <summary>
        /// Bring a gallery photo up to inspection size while it remains attached
        /// to the gripping hand. XRI remains the sole writer of the actual scale
        /// and attachment pose. A second grip immediately takes over resizing.
        /// </summary>
        public void BeginHeldEnlargement(Vector3 targetScale, float duration = 0.3f)
        {
            if (!isPhoto || !Held || SelectingHandCount != 1) return;
            enlargementStartScale = Grab.GetTargetLocalScale();
            enlargementTargetScale = targetScale;
            enlargementDuration = Mathf.Max(0.02f, duration);
            enlargementStartTime = Time.unscaledTime;
            enlargingWhileHeld = true;
        }

        public void RestorePose()
        {
            if (!hasSavedPose) return;
            transform.SetParent(savedParent, false);
            if (savedParent)
            {
                transform.localPosition = savedPosition;
                transform.localRotation = savedRotation;
            }
            else
            {
                transform.SetPositionAndRotation(savedWorldPosition, savedWorldRotation);
            }
            transform.localScale = savedScale;
            ReleaseVelocity = Vector3.zero;
            if (Grab)
            {
                // Keep XRI's pending target in agreement with the restored pose.
                grab.SetTargetPose(new Pose(transform.position, transform.rotation));
                grab.SetTargetLocalScale(transform.localScale);
            }
        }

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            UpdateHighlight();
            if (controller && controller.isActiveAndEnabled) controller.ItemHovered(this, true);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            UpdateHighlight();
            if (controller && controller.isActiveAndEnabled) controller.ItemHovered(this, false);
        }

        void OnFirstGrab(SelectEnterEventArgs args)
        {
            if (releaseRoutine != null)
            {
                StopCoroutine(releaseRoutine);
                releaseRoutine = null;
            }
            SavePose();
            activeGrab = true;
            cancelledDuringGrab = false;
            enlargingWhileHeld = false;
            ResizedWithTwoHandsDuringGrab = false;
            estimatedVelocity = Vector3.zero;
            ReleaseVelocity = Vector3.zero;
            previousPosition = transform.position;
            previousSampleTime = Time.unscaledTime;
            UpdateHighlight();
            if (controller && controller.isActiveAndEnabled) controller.ItemGrabbed(this);
        }

        void OnAnyHandReleased(SelectExitEventArgs args)
        {
            // Losing either tracked hand during a two-hand grab must not turn a
            // later release into a destructive action.
            cancelledDuringGrab |= args.isCanceled;
        }

        void OnAnyHandGrabbed(SelectEnterEventArgs args)
        {
            if (SelectingHandCount <= 1) return;
            ResizedWithTwoHandsDuringGrab = true;
            enlargingWhileHeld = false;
        }

        void OnFinalRelease(SelectExitEventArgs args)
        {
            if (!activeGrab) return;
            activeGrab = false;
            enlargingWhileHeld = false;
            var cancelled = args.isCanceled || cancelledDuringGrab;
            ReleaseVelocity = cancelled ? Vector3.zero : Vector3.ClampMagnitude(estimatedVelocity, 3f);
            if (!isActiveAndEnabled) return;
            releaseRoutine = StartCoroutine(FinishRelease(cancelled));
        }

        IEnumerator FinishRelease(bool cancelled)
        {
            // XRGrabInteractable finishes detach in LateUpdate. Changing parents,
            // snapping, or restoring within selectExited can be overwritten there.
            yield return new WaitForEndOfFrame();
            releaseRoutine = null;
            if (!this || Held || !isActiveAndEnabled) yield break;
            if (cancelled) RestorePose();
            UpdateHighlight();
            if (controller && controller.isActiveAndEnabled) controller.ItemReleased(this, cancelled);
        }

        void OnActivated(ActivateEventArgs args)
        {
            if (controller && controller.isActiveAndEnabled) controller.ItemActivated(this);
        }

        void LateUpdate()
        {
            UpdateHighlight();
            if (!Held)
            {
                enlargingWhileHeld = false;
                return;
            }
            if (enlargingWhileHeld)
            {
                if (SelectingHandCount != 1) enlargingWhileHeld = false;
                else
                {
                    var progress = Mathf.Clamp01((Time.unscaledTime - enlargementStartTime) / enlargementDuration);
                    // With one-handed user scaling disabled, the general grab
                    // transformer keeps this target scale and uses it to preserve
                    // the local contact point while the photo grows.
                    grab.SetTargetLocalScale(Vector3.Lerp(enlargementStartScale, enlargementTargetScale,
                        Mathf.SmoothStep(0f, 1f, progress)));
                    if (progress >= 1f) enlargingWhileHeld = false;
                }
            }
            var now = Time.unscaledTime;
            var delta = now - previousSampleTime;
            if (delta > 0.0001f)
            {
                var velocity = (transform.position - previousPosition) / delta;
                estimatedVelocity = Vector3.Lerp(estimatedVelocity, Vector3.ClampMagnitude(velocity, 3f),
                    1f - Mathf.Exp(-18f * delta));
            }
            previousPosition = transform.position;
            previousSampleTime = now;
        }

        void UpdateHighlight()
        {
            if (!highlight) return;
            var lit = !deleted && (persistentHighlight || (Grab && (grab.isHovered || grab.isSelected)));
            if (highlight.activeSelf != lit) highlight.SetActive(lit);
        }
    }
}
