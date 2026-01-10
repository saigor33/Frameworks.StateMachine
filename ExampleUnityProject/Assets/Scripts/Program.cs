abstract class BaseState : Frameworks.StateMachine.BaseState<BaseState> { }
abstract class BaseTransition : Frameworks.StateMachine.BaseTransition<BaseState> { }
abstract class BaseTransition<TContext> : Frameworks.StateMachine.BaseTransition<BaseState, TContext> { }

class DummyTransition : BaseTransition
{
    protected override ExecuteResult Execute()
    {
        return new ExecuteResult
        {
            nextState = new DummyState()
        };
    }
}

class WithContextDummyTransition : BaseTransition<WithContextDummyTransition.Context>
{
    internal class Context { }

    protected override ExecuteResult Execute(Context context)
    {
        return new ExecuteResult
        {
            nextState = new DummyState()
        };
    }
}

class DummyState : BaseState
{
    protected override void OnEnter() { }

    protected override void OnExit()
    {
        Leave(new DummyTransition());
        Leave(new WithContextDummyTransition(), new WithContextDummyTransition.Context());
    }
}

class StateMachine : Frameworks.StateMachine.BaseStateMachine<BaseState>
{
    public StateMachine(BaseState initialState) : base(initialState) { }
}

class Program
{
    public static void Main(string[] args)
    {
        var stateMachine = new StateMachine(new DummyState());
        var dummyState = stateMachine.GetTypedState<DummyState>();
    }
}