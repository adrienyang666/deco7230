using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    public virtual void SetHovered(bool hovered) { }
    public abstract void Interact(SpatialInteractor interactor);
}
