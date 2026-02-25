using UnityEngine;

/// <summary>
/// UI 상태가 제어해야 하는 공통 참조 모음입니다.
/// </summary>
public class UIStateContext
{
    private readonly GameObject playerControllerUIRoot;
    private readonly InventoryUI inventoryUI;

    public UIStateContext(
        GameObject playerControllerUIRoot,
        InventoryUI inventoryUI)
    {
        this.playerControllerUIRoot = playerControllerUIRoot;
        this.inventoryUI = inventoryUI;
    }

    public void Apply(UIStateType stateType)
    {
        switch (stateType)
        {
            case UIStateType.Gameplay:
                SetPlayerControllerVisible(true);
                SetInventoryVisible(false);
                SetPlayerInputLocked(false);
                break;
            case UIStateType.Chat:
                SetPlayerControllerVisible(false);
                SetInventoryVisible(false);
                SetPlayerInputLocked(true);
                break;
            case UIStateType.Inventory:
                SetPlayerControllerVisible(false);
                SetInventoryVisible(true);
                SetPlayerInputLocked(true);
                break;
        }
    }

    private void SetPlayerControllerVisible(bool visible)
    {
        if (playerControllerUIRoot == null)
        {
            return;
        }

        playerControllerUIRoot.SetActive(visible);
    }

    private void SetInventoryVisible(bool visible)
    {
        if (inventoryUI == null)
        {
            return;
        }

        inventoryUI.SetVisible(visible);
    }

    private void SetPlayerInputLocked(bool locked)
    {
        if (PlayerCharacter.Instance == null)
        {
            return;
        }

        PlayerCharacter.Instance.IsInputLocked = locked;
    }
}
