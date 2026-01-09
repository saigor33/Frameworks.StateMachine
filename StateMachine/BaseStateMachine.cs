namespace Frameworks.StateMachine
{
    public abstract class BaseStateMachine<TState> where TState : BaseState<TState>
    {
        enum InternalState
        {
            WaitStartTransition,
            InTransition,
            Disposed
        }

        public TState currentState => _currentState;

        InternalState _currentInternalState;
        TState _currentState;

        protected BaseStateMachine(TState initialState)
        {
            initialState.OnEnterInternal();
            initialState.onLeaveRequested += OnStateLeaveRequested;

            _currentState = initialState;
            _currentInternalState = InternalState.WaitStartTransition;
        }

        void OnStateLeaveRequested(BaseTransition<TState> transition)
        {
            ChangeInternalState(targetState: InternalState.WaitStartTransition, newState: InternalState.InTransition);

            _currentState.onLeaveRequested -= OnStateLeaveRequested;
            _currentState.OnExitInternal();

            BaseTransition<TState>.ExecuteResult transitionExecuteResult = transition.ExecuteInternal();
            _currentState = transitionExecuteResult.nextState;

            _currentState.OnEnterInternal();
            _currentState.onLeaveRequested += OnStateLeaveRequested;

            ChangeInternalState(targetState: InternalState.InTransition, newState: InternalState.WaitStartTransition);

            _currentState.OnAfterTransitionFinished();
            transitionExecuteResult.onTransitionFinished?.Invoke();
        }

        public void Dispose()
        {
            ChangeInternalState(targetState: InternalState.WaitStartTransition, newState: InternalState.Disposed);

            _currentState.OnExitInternal();
            _currentState = null;
        }

        void ChangeInternalState(InternalState targetState, InternalState newState)
        {
            if (_currentInternalState != targetState)
            {
                throw new Exception($"StateMachine \"{GetType().FullName}\": Invalid internal state")
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

        public T GetTypedState<T>() where T : TState
        {
            if (_currentState is T typedState)
            {
                return typedState;
            }

            throw new Exception($"StateMachine \"{GetType().FullName}\": Wrong target state")
            {
                Data =
                {
                    { "CurrentState", _currentState.GetType().FullName },
                    { "TargetState", typeof(T).FullName }
                }
            };
        }
    }
}