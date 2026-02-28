using UnityEngine;

/// <summary>
/// 플레이어 이동 입력을 읽어 CharacterController 이동으로 변환합니다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool rotateToMovement = true;
    [SerializeField] private float rotateSpeed = 10f;

    private CharacterController controller;

    public Vector3 MoveDirection { get; private set; }
    public Vector3 RawInputDirection { get; private set; }
    public float MoveSpeed => moveSpeed;
    public bool IsMoving { get; private set; }
    public bool IsGrounded => controller != null && controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Tick(bool isInputLocked)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        RawInputDirection = Vector3.ClampMagnitude(new Vector3(horizontal, 0f, vertical), 1f);

        if (isInputLocked)
        {
            MoveDirection = Vector3.zero;
            IsMoving = false;
            return;
        }

        MoveDirection = RawInputDirection;
        IsMoving = MoveDirection.sqrMagnitude > 0.01f;
    }

    public CollisionFlags ApplyMotion(float verticalVelocity, float deltaTime)
    {
        if (controller == null)
        {
            return CollisionFlags.None;
        }

        Vector3 velocity = MoveDirection * moveSpeed;
        velocity.y = verticalVelocity;
        CollisionFlags flags = controller.Move(velocity * deltaTime);

        if (rotateToMovement && IsMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(MoveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * deltaTime);
        }

        return flags;
    }
}
