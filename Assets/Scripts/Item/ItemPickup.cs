using UnityEngine;

/// <summary>
/// 월드에 배치되는 아이템 오브젝트. 플레이어가 상호작용하면 인벤토리에 추가됩니다.
/// </summary>
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;
    [SerializeField] private InteractionPromptUI promptUI;

    public void Interact(PlayerCharacter player)
    {
        if (itemData == null) return;

        PlayerInventory.Instance.AddItem(itemData, amount);
        gameObject.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (promptUI != null)
            promptUI.Show();
    }

    public void HideInteractionPrompt()
    {
        if (promptUI != null)
            promptUI.Hide();
    }
}
