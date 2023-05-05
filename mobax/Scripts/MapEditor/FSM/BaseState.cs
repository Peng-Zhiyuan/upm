/// <summary>
/// Base Scene Class
/// </summary>
public abstract class BaseState
{
    protected StateMachine m_StateMachine = null;

    protected bool _isActive = false;

    public abstract void OnInit(StateMachine param_StateMachine, object owner);

    public abstract void OnDestroy();

    public abstract bool Condition(BaseState curState);

    public abstract void OnStateChangeRequest(int newState, object param_UserData);

    public abstract void OnEnter(object param_UserData);

    public virtual void OnLeave(int newState) { }

    public abstract void OnLeave();

    public abstract void Update(float param_deltaTime);

    public abstract void FixedUpdate(float param_deltaTime);

    public abstract void LateUpdate(float param_deltaTime);

    public abstract int GetStateID();

    protected StateMachine StateMachine
    {
        get { return m_StateMachine; }
    }

    //这个接口只允许在State_XXXX类中使用, 外部不能访问
    //也不可以在State_XXXX类中访问State_YYYY类中的active
    public bool active
    {
        set { _isActive = value; }
        get { return _isActive; }
    }
}