namespace Match.Logic
{
    partial class SearchingState : BaseState
    {
        readonly StatesContext _statesContext;
        readonly IMatchMaker.ISearchHandler _searchHandler;

        public SearchingState(StatesContext statesContext, IMatchMaker.ISearchHandler searchHandler)
        {
            _statesContext = statesContext;
            _searchHandler = searchHandler;
        }

        protected override void OnEnter()
        {
            _searchHandler.onSearchFinished += OnSearchFinished;
        }

        protected override void OnExit()
        {
            _searchHandler.onSearchFinished -= OnSearchFinished;
        }

        protected override void OnAfterTransitionFinished()
        {
            _searchHandler.StartSearch();
        }

        void OnSearchFinished(IMatchMaker.ISearchHandler.BaseResult searchResult)
        {
            Leave(new ApplySearchResultTransition(_statesContext), searchResult);
        }

        public void CancelSearch()
        {
            _searchHandler.CancelSearch();
        }
    }
}