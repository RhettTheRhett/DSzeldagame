using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public InputAction interactAction;
    public float interactDistance;
    public Vector3 interactCenter;

    private Animator animator;

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        interactCenter = transform.position;
        if (WasInteractPressed())
        {
            animator.SetTrigger("Interact");
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

            // Use closest point on the collider instead of pivot position.
            // On long objects this means the edge nearest the player is used
            // for both distance and alignment checks, not the far-away center.
            Vector3 closestPoint = hitCollider.ClosestPoint(interactCenter);
            Vector3 toClosest = closestPoint - interactCenter;
            float distance = toClosest.magnitude;

            if (distance > interactDistance) continue;

            // Avoid divide-by-zero if player is inside the collider
            Vector3 direction = distance > 0.001f ? toClosest.normalized : transform.forward;

            float alignment = Vector3.Dot(direction, transform.forward);
            if (alignment < 0.25f) continue;

            float score = alignment * 2f - (distance / interactDistance);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = interactable;
            }

            Debug.DrawLine(interactCenter, closestPoint, Color.yellow, 1f);
        }

        if (bestTarget != null)
        {
            Debug.DrawLine(interactCenter, bestTarget.GetTransform().position, Color.green, 1f);
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