using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 토글 버튼 입력을 UIManager로 전달합니다.
/// </summary>
public class InventoryToggleButtonUI : MonoBehaviour
{
    [SerializeField] private Button toggleButton;

    private void Awake()
    {
        if (toggleButton == null)
        {
            toggleButton = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(OnClicked);
        }
    }

    private void OnDisable()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        UIManager.Instance?.ToggleInventory();
    }
}
