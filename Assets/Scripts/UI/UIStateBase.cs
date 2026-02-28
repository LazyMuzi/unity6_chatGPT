/// <summary>
/// 상태별 공통 진입 로직을 제공하는 UI 상태 기본 클래스입니다.
/// </summary>
public abstract class UIStateBase : IUIState
{
    protected readonly UIStateContext context;

    protected UIStateBase(UIStateContext context)
    {
        this.context = context;
    }

    public abstract UIStateType StateType { get; }

    public virtual void Enter()
    {
        context.Apply(StateType);
    }

    public virtual void Exit()
    {
    }
}
