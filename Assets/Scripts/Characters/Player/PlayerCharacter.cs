using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : CharacterBase
{
    public static PlayerCharacter Instance { get; private set; }

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;

    private CharacterController controller;
    private IInteractable currentTarget;

    private void Awake()
    {
        Instance = this;
        controller = GetComponent<CharacterController>();
        moveSpeed = 5f;
    }

    private void Update()
    {
        if (IsInputLocked)
        {
            controller.SimpleMove(Vector3.zero);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(h, 0, v);
        moveDirection = dir;
        controller.SimpleMove(moveDirection * moveSpeed);

        DetectInteractable();
    }

    private void DetectInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);

        IInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = interactable;
            }
        }

        if (closest != currentTarget)
        {
            currentTarget?.HideInteractionPrompt();
            currentTarget = closest;
            currentTarget?.ShowInteractionPrompt();
        }
    }

    public override void OnCharacterTouched()
    {
        base.OnCharacterTouched();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
