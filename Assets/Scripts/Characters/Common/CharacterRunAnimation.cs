using UnityEngine;

/// <summary>
/// 캐릭터 이동 상태를 기반으로 run/idle 트리거를 제어합니다.
/// </summary>
[DisallowMultipleComponent]
public class CharacterRunAnimation : MonoBehaviour
{
    private const string DefaultRunTrigger = "run";
    private const string DefaultIdleTrigger = "idle";

    [SerializeField] private Animator animator;
    [SerializeField] private string runTriggerName = DefaultRunTrigger;
    [SerializeField] private string idleTriggerName = DefaultIdleTrigger;
    [SerializeField] private bool useIdleTriggerWhenStop = true;

    private int runTriggerHash;
    private int idleTriggerHash;
    private bool wasMoving;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        runTriggerHash = Animator.StringToHash(runTriggerName);
        idleTriggerHash = Animator.StringToHash(idleTriggerName);
    }

    public void Tick(bool isMoving)
    {
        if (animator == null)
        {
            return;
        }

        if (isMoving == wasMoving)
        {
            return;
        }

        wasMoving = isMoving;

        if (isMoving)
        {
            animator.SetTrigger(runTriggerHash);
            return;
        }

        if (useIdleTriggerWhenStop)
        {
            animator.SetTrigger(idleTriggerHash);
        }
    }
}
