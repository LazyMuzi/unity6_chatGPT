using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 점프 버튼 입력을 플레이어 점프 요청으로 전달합니다.
/// </summary>
public class PlayerJumpButtonUI : MonoBehaviour
{
    [SerializeField] private Button jumpButton;

    private void Awake()
    {
        if (jumpButton == null)
        {
            jumpButton = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpClicked);
        }
    }

    private void OnDisable()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveListener(OnJumpClicked);
        }
    }

    private void OnJumpClicked()
    {
        if (PlayerCharacter.Instance == null)
        {
            return;
        }

        PlayerCharacter.Instance.RequestJump();
    }
}
