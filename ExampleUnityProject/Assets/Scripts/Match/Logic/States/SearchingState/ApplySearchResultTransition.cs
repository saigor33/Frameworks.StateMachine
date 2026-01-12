using System;

namespace Match.Logic
{
    partial class SearchingState
    {
        class ApplySearchResultTransition : BaseTransition<IMatchMaker.ISearchHandler.BaseResult>
        {
            readonly StatesContext _statesContext;

            public ApplySearchResultTransition(StatesContext statesContext)
            {
                _statesContext = statesContext;
            }

            protected override ExecuteResult Execute(IMatchMaker.ISearchHandler.BaseResult searchResult)
            {
                // Save data and other logic

                return new ExecuteResult
                {
                    nextState = searchResult switch
                    {
                        IMatchMaker.ISearchHandler.CancelResult => new WaitStartSearchState(_statesContext),
                        IMatchMaker.ISearchHandler.SuccessResult successSearchResult =>
                            new InMatchState(_statesContext, successSearchResult.matchId),
                        _ => throw new ArgumentOutOfRangeException(nameof(searchResult))
                    }
                };
            }
        }
    }
}