using UnityEngine;

public abstract class InputReciever : MonoBehaviour
{
    public virtual void OnMouseDown() { }
    public virtual void OnMouseUp() { }
    public virtual void OnHorizontalKeyDown(float horizontal) { }
    public virtual void OnVerticalKeyDown(float vertical) { }
    public virtual void OnJumpDown() { }
}