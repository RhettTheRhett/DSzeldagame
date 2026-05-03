using UnityEngine;

public interface IInteractable 
{
    public void Interacted(GameObject interactor);
    public Transform GetTransform();
}
