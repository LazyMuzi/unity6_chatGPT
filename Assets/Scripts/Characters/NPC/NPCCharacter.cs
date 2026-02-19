using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// NPC 캐릭터. 상태 머신으로 행동을 관리하며, 친밀도가 높으면 플레이어에게 접근합니다.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCCharacter : CharacterBase, IInteractable
{
    public NPCBrain brain;

    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Interaction Timing")]
    [SerializeField] private float lookDuration = 0.4f;
    [SerializeField] private float dotsDelay = 1f;

    [Header("Approach Behavior")]
    [SerializeField] private float stoppingDistance = 3f;
    [SerializeField] private int approachAffinityThreshold = 50;

    private NavMeshAgent agent;
    private NPCState currentState = NPCState.Idle;
    private Vector3 homePosition;
    private bool isInteracting;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 2f;
        agent.stoppingDistance = stoppingDistance;
        homePosition = transform.position;
    }

    private void Update()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                UpdateIdle();
                break;
            case NPCState.Approach:
                UpdateApproach();
                break;
            case NPCState.WaitForChat:
                UpdateWaitForChat();
                break;
        }
    }

    #region State Machine

    private void UpdateIdle()
    {
        if (isInteracting) return;
        if (brain.relationship.affinity < approachAffinityThreshold) return;

        Transform player = GetPlayerTransform();
        if (player == null) return;

        float detectRange = GetDetectionRange();
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectRange)
            TransitionTo(NPCState.Approach);
    }

    private void UpdateApproach()
    {
        Transform player = GetPlayerTransform();
        if (player == null)
        {
            ReturnHome();
            return;
        }

        float detectRange = GetDetectionRange();
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > detectRange)
        {
            ReturnHome();
            return;
        }

        agent.SetDestination(player.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            TransitionTo(NPCState.WaitForChat);
    }

    private void UpdateWaitForChat()
    {
        Transform player = GetPlayerTransform();
        if (player == null)
        {
            ReturnHome();
            return;
        }

        float detectRange = GetDetectionRange();
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > detectRange)
        {
            ReturnHome();
            return;
        }

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 5f);
    }

    private void TransitionTo(NPCState newState)
    {
        if (currentState != newState)
            Debug.Log($"[State:{brain.profile.npcName}] {currentState} → {newState}");

        currentState = newState;

        switch (newState)
        {
            case NPCState.Idle:
                agent.isStopped = true;
                break;
            case NPCState.Approach:
                agent.isStopped = false;
                break;
            case NPCState.WaitForChat:
                agent.isStopped = true;
                break;
            case NPCState.Interacting:
                agent.isStopped = true;
                break;
        }
    }

    private void ReturnHome()
    {
        agent.isStopped = false;
        agent.SetDestination(homePosition);
        TransitionTo(NPCState.Idle);
    }

    private float GetDetectionRange()
    {
        if (brain.relationship.affinity >= 80) return 15f;
        return 10f;
    }

    private Transform GetPlayerTransform()
    {
        if (PlayerCharacter.Instance == null) return null;
        return PlayerCharacter.Instance.transform;
    }

    #endregion

    #region Interaction

    public void Interact(PlayerCharacter player)
    {
        if (isInteracting) return;
        StartCoroutine(InteractionSequence(player));
    }

    private IEnumerator InteractionSequence(PlayerCharacter player)
    {
        isInteracting = true;
        TransitionTo(NPCState.Interacting);
        player.IsInputLocked = true;

        Coroutine playerLook = player.LookAtSmooth(transform, lookDuration);
        Coroutine npcLook = LookAtSmooth(player.transform, lookDuration);

        CameraManager.Instance.StartInteraction(player.transform, transform);

        yield return playerLook;
        yield return npcLook;

        ChatUI.Instance.Open(this, brain.profile.npcName);

        yield return new WaitForSeconds(dotsDelay);

        string greeting = brain.relationship.GetGreeting();
        ChatUI.Instance.UpdateGreeting(brain.profile.npcName, greeting);
    }

    public void OnInteractionEnd()
    {
        isInteracting = false;
        homePosition = transform.position;
        TransitionTo(NPCState.Idle);
    }

    public void ShowInteractionPrompt()
    {
        if (isInteracting) return;
        promptUI.Show();
    }

    public void HideInteractionPrompt()
    {
        promptUI.Hide();
    }

    public void SendPlayerMessage(string playerInput)
    {
        brain.HandlePlayerInteraction(playerInput);
    }

    public override void OnCharacterTouched()
    {
        base.OnCharacterTouched();
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (brain == null) return;

        float range = brain.relationship.affinity >= 80 ? 15f : 10f;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, range);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
