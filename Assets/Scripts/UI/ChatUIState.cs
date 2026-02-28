/// <summary>
/// 채팅 진행 상태에서의 UI 표시 규칙을 담당합니다.
/// </summary>
public class ChatUIState : UIStateBase
{
    public ChatUIState(UIStateContext context)
        : base(context)
    {
    }

    public override UIStateType StateType => UIStateType.Chat;
}
