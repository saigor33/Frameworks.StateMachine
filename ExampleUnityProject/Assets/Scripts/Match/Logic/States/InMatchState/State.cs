namespace Match.Logic
{
    partial class InMatchState : BaseState
    {
        readonly StatesContext _statesContext;
        readonly string _matchId;

        public InMatchState(StatesContext statesContext, string matchId)
        {
            _matchId = matchId;
            _statesContext = statesContext;
        }

        protected override void OnEnter() { }
        protected override void OnExit() { }

        public void FinishMatch(bool isPlayerWon)
        {
            Leave(new FinishMatchTransition(_statesContext), new FinishMatchTransition.Context
            {
                isPlayerWon = isPlayerWon,
                matchId = _matchId
            });
        }
    }
}