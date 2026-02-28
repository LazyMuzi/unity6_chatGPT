using UnityEngine;

/// <summary>
/// 플레이어 점프의 수직 속도와 중력을 관리합니다.
/// </summary>
public class PlayerJump : MonoBehaviour
{
    private const string DefaultJumpTrigger = "jump";

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string jumpTriggerName = DefaultJumpTrigger;

    [Header("Timing")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float jumpCooldown = 0.1f;
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float groundedStickVelocity = -2f;

    private int jumpTriggerHash;
    private float cooldownRemainTime;
    private bool jumpRequested;
    private bool isGrounded;

    public bool IsJumping { get; private set; }
    public bool IsAirborne => !isGrounded;
    public float VerticalVelocity { get; private set; }
    public bool IsMovementLocked => false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        jumpTriggerHash = Animator.StringToHash(jumpTriggerName);
        isGrounded = true;
        VerticalVelocity = groundedStickVelocity;
    }

    public void Tick(bool isInputLocked)
    {
        if (cooldownRemainTime > 0f)
        {
            cooldownRemainTime -= Time.deltaTime;
        }

        if (isGrounded)
        {
            if (VerticalVelocity < groundedStickVelocity)
            {
                VerticalVelocity = groundedStickVelocity;
            }

            if (jumpRequested && !isInputLocked && cooldownRemainTime <= 0f)
            {
                StartJump();
            }
            else
            {
                IsJumping = false;
            }

            jumpRequested = false;
            return;
        }

        VerticalVelocity += gravity * Time.deltaTime;
        if (VerticalVelocity < -maxFallSpeed)
        {
            VerticalVelocity = -maxFallSpeed;
        }

        IsJumping = VerticalVelocity > 0f;
    }

    public bool TryJump(bool canJump)
    {
        if (!canJump || !isGrounded || cooldownRemainTime > 0f)
        {
            return false;
        }

        jumpRequested = true;
        return true;
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
        if (isGrounded && VerticalVelocity < groundedStickVelocity)
        {
            VerticalVelocity = groundedStickVelocity;
        }
    }

    private void StartJump()
    {
        float safeGravity = Mathf.Min(gravity, -0.01f);
        VerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * safeGravity);
        IsJumping = true;
        cooldownRemainTime = Mathf.Max(0f, jumpCooldown);
        jumpRequested = false;

        animator?.SetTrigger(jumpTriggerHash);
    }
}
