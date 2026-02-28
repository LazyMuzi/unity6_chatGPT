/// <summary>
/// 일반 플레이 상태에서의 UI 표시 규칙을 담당합니다.
/// </summary>
public class GameplayUIState : UIStateBase
{
    public GameplayUIState(UIStateContext context)
        : base(context)
    {
    }

    public override UIStateType StateType => UIStateType.Gameplay;
}
