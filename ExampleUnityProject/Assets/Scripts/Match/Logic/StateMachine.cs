namespace Match.Logic
{
    class StateMachine : Frameworks.StateMachine.BaseStateMachine<BaseState>
    {
        public StateMachine(BaseState initialState) : base(initialState) { }
    }
}