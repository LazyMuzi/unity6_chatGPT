/// <summary>
/// UI 상태머신의 상태 계약입니다.
/// </summary>
public interface IUIState
{
    UIStateType StateType { get; }
    void Enter();
    void Exit();
}
