using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public InputAction interactAction;

    public float interactDistance;
    public Vector3 interactCenter;
    
    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        interactCenter = transform.position;
        if (WasInteractPressed())
        {
            Interact();
        }
    }

    private bool WasInteractPressed()
    {
        return interactAction.WasPressedThisFrame();
    }

    private void Interact()
    {
        Collider[] hitColliders = Physics.OverlapSphere(interactCenter, interactDistance);

        float bestScore = float.MinValue;
        IInteractable bestTarget = null;

        foreach (var hitCollider in hitColliders)
        {
            IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            Transform targetTransform = interactable.GetTransform();

            Vector3 toTarget = targetTransform.position - interactCenter;
            float distance = toTarget.magnitude;

            
            if (distance > interactDistance) continue;

            Vector3 direction = toTarget.normalized;

            float alignment = Vector3.Dot(direction, transform.forward);

            
            if (alignment < 0.25f) continue;

            
            float score = alignment * 2f - (distance / interactDistance);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = interactable;
            }
            
            Debug.DrawLine(interactCenter, targetTransform.position, Color.yellow,1f);
        }

        if (bestTarget != null)
        {
            Debug.DrawLine(interactCenter, bestTarget.GetTransform().position, Color.green,1f);
        }
        
        if (bestTarget != null)
        {
            bestTarget.Interacted(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(interactCenter, interactDistance);
    }
}
