using System;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpening = false;
    public Vector3 newPosition;
    

    public float openSpeed = 0.5f;
    
    public void Interacted(GameObject interactor)
    {
        Debug.Log(interactor.name + " interacted with" + this.name);
        newPosition = new Vector3(transform.position.x, transform.position.y - 1.1f, transform.position.z);
        isOpening = true;
    }

    private void Update()
    {
        if (isOpening)
        {
            //transform.position = Vector3.Lerp(transform.position, newPosition,openSpeed);
            transform.position = Vector3.MoveTowards(transform.position, newPosition, Time.deltaTime * openSpeed);

            if (Vector3.Distance(transform.position, newPosition) <= 0.1f)
            {
                isOpening = false;
                transform.position = newPosition;
            }
            
        }
    }

    public Transform GetTransform()
    {
        return this.transform;
    }
}
