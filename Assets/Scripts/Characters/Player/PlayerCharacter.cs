using UnityEngine;

/// <summary>
/// 플레이어 캐릭터의 상위 오케스트레이션을 담당합니다.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteractionScanner))]
[RequireComponent(typeof(PlayerSound))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(CharacterRunAnimation))]
public class PlayerCharacter : CharacterBase
{
    public static PlayerCharacter Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerInteractionScanner interactionScanner;
    [SerializeField] private PlayerSound playerSound;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private CharacterRunAnimation runAnimation;

    private void Awake()
    {
        Instance = this;
        ResolveDependencies();
    }

    private void Update()
    {
        bool movementLocked = IsInputLocked;

        movement.Tick(movementLocked);
        playerJump.Tick(IsInputLocked);

        CollisionFlags collisionFlags = movement.ApplyMotion(playerJump.VerticalVelocity, Time.deltaTime);
        bool groundedAfterMove = (collisionFlags & CollisionFlags.Below) != 0;
        playerJump.SetGrounded(groundedAfterMove);

        moveDirection = movement.MoveDirection;
        moveSpeed = movement.MoveSpeed;

        if (movementLocked)
        {
            interactionScanner.ClearCurrentTarget();
        }
        else
        {
            interactionScanner.Tick();
        }

        bool footstepLocked = IsInputLocked || playerJump.IsAirborne;
        playerSound.Tick(moveDirection, footstepLocked);
        runAnimation.Tick(movement.IsMoving && !movementLocked && !playerJump.IsAirborne);
    }

    public override void OnCharacterTouched()
    {
        base.OnCharacterTouched();
        playerSound.PlayTouchedSound();
    }

    public void RequestJump()
    {
        bool isChatOpen = ChatUI.Instance != null && ChatUI.Instance.IsOpen;
        bool canJump = !IsInputLocked && !isChatOpen;
        playerJump.TryJump(canJump);
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionScanner == null)
        {
            return;
        }

        interactionScanner.DrawGizmosSelected();
    }

    private void ResolveDependencies()
    {
        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }

        if (interactionScanner == null)
        {
            interactionScanner = GetComponent<PlayerInteractionScanner>();
        }

        if (playerSound == null)
        {
            playerSound = GetComponent<PlayerSound>();
        }

        if (playerJump == null)
        {
            playerJump = GetComponent<PlayerJump>();
        }

        if (runAnimation == null)
        {
            runAnimation = GetComponent<CharacterRunAnimation>();
        }
    }
}
