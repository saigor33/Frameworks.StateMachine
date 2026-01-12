namespace Match.Logic
{
    abstract class BaseTransition<TContext> : Frameworks.StateMachine.BaseTransition<BaseState, TContext> { }
    abstract class BaseTransition : Frameworks.StateMachine.BaseTransition<BaseState> { }
}