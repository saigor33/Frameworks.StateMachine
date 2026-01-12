namespace Match.Logic
{
    partial class WaitStartSearchState
    {
        class StartSearchTransition : BaseTransition
        {
            readonly StatesContext _statesContext;
            IMatchMaker _matchMaker => _statesContext.matchMaker;

            public StartSearchTransition(StatesContext statesContext)
            {
                _statesContext = statesContext;
            }

            protected override ExecuteResult Execute()
            {
                // Save data and other logic

                IMatchMaker.ISearchHandler searchHandler = _matchMaker.BuildRequest();

                return new ExecuteResult
                {
                    nextState = new SearchingState(_statesContext, searchHandler)
                };
            }
        }
    }
}