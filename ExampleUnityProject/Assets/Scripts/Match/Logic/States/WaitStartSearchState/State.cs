namespace Match.Logic
{
    partial class WaitStartSearchState : BaseState
    {
        readonly StatesContext _statesContext;

        public WaitStartSearchState(StatesContext statesContext)
        {
            _statesContext = statesContext;
        }

        protected override void OnEnter() { }
        protected override void OnExit() { }

        public void Search()
        {
            Leave(new StartSearchTransition(_statesContext));
        }
    }
}