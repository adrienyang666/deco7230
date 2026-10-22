using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SpatialStories.Quest
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class VROverlay : MonoBehaviour
    {
        public VRWorkshop workshop;
        public string itemId;
        public Vector2 size;
        public GameObject outline;
        public XRGrabInteractable Grab { get; private set; }
        public bool Held => Grab != null && Grab.isSelected;
        public bool Busy => Held || release != null;
        public bool Snapped { get; private set; }
        public Vector3 SavedPosition { get; private set; }
        public Quaternion SavedRotation { get; private set; }
        public Vector3 SavedScale { get; private set; }
        Coroutine release;
        Vector3 grabbedPosition;
        Quaternion grabbedRotation;
        Vector3 grabbedScale;
        bool loggedTwoHands;

        void Awake()
        {
            Grab = GetComponent<XRGrabInteractable>();
            Grab.selectEntered.AddListener(Grabbed);
            Grab.selectExited.AddListener(Released);
            Grab.firstHoverEntered.AddListener(HoverOn);
            Grab.lastHoverExited.AddListener(HoverOff);
        }
        public void SavePlacement(bool snapped)
        {
            Snapped = snapped;
            SavedPosition = transform.localPosition; SavedRotation = transform.localRotation;
            SavedScale = transform.localScale;
        }
        public void RestorePlacement()
        {
            transform.localPosition = SavedPosition; transform.localRotation = SavedRotation;
            transform.localScale = SavedScale;
        }
        void Grabbed(SelectEnterEventArgs args)
        {
            if (release != null) { StopCoroutine(release); release = null; }
            if (Grab.interactorsSelecting.Count == 1)
            {
                grabbedPosition = transform.position; grabbedRotation = transform.rotation;
                grabbedScale = transform.localScale; loggedTwoHands = false;
                workshop.Log.Event("grab", workshop.StageName, itemId);
                workshop.Hint("Hold grip to move. Turn your wrist to rotate.\nGrab with both hands and spread them to resize.");
            }
            else if (!loggedTwoHands)
            {
                loggedTwoHands = true;
                workshop.Log.Event("two_hand_grab", workshop.StageName, itemId);
            }
            if (outline) outline.SetActive(true);
        }
        void Released(SelectExitEventArgs args)
        {
            if (Grab.isSelected || !gameObject.activeInHierarchy) return;
            if (args.isCanceled)
            {
                RestorePlacement();
                workshop.Log.Event("grab_canceled_return", workshop.StageName, itemId);
                if (outline) outline.SetActive(false);
                return;
            }
            release = StartCoroutine(FinishRelease());
        }
        IEnumerator FinishRelease()
        {
            // XRI applies detach work late in the frame. Snap only after it has finished.
            yield return new WaitForEndOfFrame();
            release = null;
            if (Held || !gameObject.activeInHierarchy) yield break;
            float moved = Vector3.Distance(grabbedPosition, transform.position);
            float rotated = Quaternion.Angle(grabbedRotation, transform.rotation);
            float resized = Mathf.Abs(transform.localScale.x - grabbedScale.x);
            workshop.Log.Event("release", workshop.StageName,
                itemId + " | moved=" + moved.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) +
                " | rotation=" + rotated.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) +
                " | scale_delta=" + resized.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
            workshop.ResolveDrop(this);
            if (outline) outline.SetActive(false);
        }
        void HoverOn(HoverEnterEventArgs args) { if (outline) outline.SetActive(true); }
        void HoverOff(HoverExitEventArgs args) { if (!Held && outline) outline.SetActive(false); }
        void OnDisable()
        {
            if (release != null) { StopCoroutine(release); release = null; }
            if (outline) outline.SetActive(false);
        }
        void OnDestroy()
        {
            if (!Grab) return;
            Grab.selectEntered.RemoveListener(Grabbed); Grab.selectExited.RemoveListener(Released);
            Grab.firstHoverEntered.RemoveListener(HoverOn); Grab.lastHoverExited.RemoveListener(HoverOff);
        }
    }
}
