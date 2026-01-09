namespace Frameworks.StateMachine
{
    class TransitionWithContextWrapper<TState, TContext> : BaseTransition<TState> where TState : BaseState<TState>
    {
        readonly BaseTransition<TState, TContext> _transitionWithContext;
        readonly TContext _context;

        public TransitionWithContextWrapper(BaseTransition<TState, TContext> transitionWithContext, TContext context)
        {
            _transitionWithContext = transitionWithContext;
            _context = context;
        }

        protected override ExecuteResult Execute()
        {
            BaseTransition<TState, TContext>.ExecuteResult executeResult =
                _transitionWithContext.ExecuteInternal(_context);

            return new ExecuteResult
            {
                nextState = executeResult.nextState,
                onTransitionFinished = () => executeResult.onTransitionFinished?.Invoke(_context)
            };
        }
    }
}