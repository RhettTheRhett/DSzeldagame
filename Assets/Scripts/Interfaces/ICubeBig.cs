using UnityEngine;

public class ICubeBig : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interacted(GameObject interactor)
    {
        transform.localScale = new Vector3(transform.localScale.x * 2, transform.localScale.y* 2, transform.localScale.z* 2);
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
