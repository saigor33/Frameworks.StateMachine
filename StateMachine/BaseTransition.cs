namespace Frameworks.StateMachine
{
    public abstract class BaseTransition<TState> where TState : BaseState<TState>
    {
        public class ExecuteResult
        {
            public TState nextState;
            public Action onTransitionFinished;
        }

        internal ExecuteResult ExecuteInternal()
        {
            return Execute();
        }

        protected abstract ExecuteResult Execute();
    }

    public abstract class BaseTransition<TState, TContext> where TState : BaseState<TState>
    {
        public class ExecuteResult
        {
            public TState nextState;
            public Action<TContext> onTransitionFinished;
        }

        internal ExecuteResult ExecuteInternal(TContext context)
        {
            return Execute(context);
        }

        protected abstract ExecuteResult Execute(TContext context);
    }
}