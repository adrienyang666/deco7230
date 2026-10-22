using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace SpatialStories.Immersive
{
    /// <summary>Runs after the general transformer: two hands change size, one hand rotates.</summary>
    public sealed class CollageScaleRotationLock : XRBaseGrabTransformer
    {
        protected override RegistrationMode registrationMode => RegistrationMode.Multiple;
        Quaternion rotation;

        public override void OnGrabCountChanged(XRGrabInteractable grab, Pose targetPose, Vector3 localScale)
        {
            base.OnGrabCountChanged(grab, targetPose, localScale);
            if (grab.interactorsSelecting.Count > 1) rotation = targetPose.rotation;
        }

        public override void Process(XRGrabInteractable grab, XRInteractionUpdateOrder.UpdatePhase phase,
            ref Pose targetPose, ref Vector3 localScale)
        {
            if (grab.interactorsSelecting.Count > 1) targetPose.rotation = rotation;
        }
    }
}
