namespace Frameworks.StateMachine
{
    public abstract class BaseState<TState> where TState : BaseState<TState>
    {
        enum InternalState
        {
            WaitEnter,
            Entering,
            WaitLeave,
            Leaving,
            Leaved
        }

        internal event Action<BaseTransition<TState>> onLeaveRequested;

        InternalState _currentInternalState;

        protected BaseState()
        {
            _currentInternalState = InternalState.WaitEnter;
        }

        internal void OnEnterInternal()
        {
            ChangeInternalState(targetState: InternalState.WaitEnter, newState: InternalState.Entering);

            OnEnter();

            ChangeInternalState(targetState: InternalState.Entering, newState: InternalState.WaitLeave);
        }

        internal void OnExitInternal()
        {
            ChangeInternalState(targetState: InternalState.WaitLeave, newState: InternalState.Leaving);

            OnExit();

            ChangeInternalState(targetState: InternalState.Leaving, newState: InternalState.Leaved);
        }

        protected abstract void OnEnter();
        protected abstract void OnExit();

        internal protected void OnAfterTransitionFinished() { }

        protected void Leave(BaseTransition<TState> transition)
        {
            onLeaveRequested.Invoke(transition);
        }

        protected void Leave<TContext>(BaseTransition<TState, TContext> transition, TContext context)
        {
            Leave(new TransitionWithContextWrapper<TState, TContext>(transition, context));
        }

        void ChangeInternalState(InternalState targetState, InternalState newState)
        {
            if (_currentInternalState != targetState)
            {
                throw new Exception($"StateMachine.State \"{GetType().FullName}\": Invalid internal state")
                {
                    Data =
                    {
                        { "CurrentState", _currentInternalState },
                        { "TargetState", targetState },
                        { "NewState", newState }
                    }
                };
            }

            _currentInternalState = newState;
        }
    }
}