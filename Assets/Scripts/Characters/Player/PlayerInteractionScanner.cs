using UnityEngine;

/// <summary>
/// 플레이어 주변 상호작용 대상을 탐색하고 프롬프트 상태를 관리합니다.
/// </summary>
public class PlayerInteractionScanner : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private int maxDetectionColliders = 16;

    private IInteractable currentTarget;
    private Collider[] detectionBuffer;

    private void Awake()
    {
        int bufferSize = Mathf.Max(1, maxDetectionColliders);
        detectionBuffer = new Collider[bufferSize];
    }

    public void Tick()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactionRange, detectionBuffer);

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = detectionBuffer[i];
            if (hit == null)
            {
                continue;
            }

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        UpdateTarget(closest);
    }

    public void ClearCurrentTarget()
    {
        UpdateTarget(null);
    }

    public void DrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    private void OnDisable()
    {
        ClearCurrentTarget();
    }

    private void UpdateTarget(IInteractable newTarget)
    {
        if (newTarget == currentTarget)
        {
            return;
        }

        currentTarget?.HideInteractionPrompt();
        currentTarget = newTarget;
        currentTarget?.ShowInteractionPrompt();
    }
}
