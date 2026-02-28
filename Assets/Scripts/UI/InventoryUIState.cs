/// <summary>
/// 인벤토리 UI 상태의 표시/입력 규칙을 담당합니다.
/// </summary>
public class InventoryUIState : UIStateBase
{
    public InventoryUIState(UIStateContext context)
        : base(context)
    {
    }

    public override UIStateType StateType => UIStateType.Inventory;
}
