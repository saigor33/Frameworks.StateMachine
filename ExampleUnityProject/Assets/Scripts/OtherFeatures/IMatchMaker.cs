using System;

interface IMatchMaker
{
    interface ISearchHandler
    {
        abstract class BaseResult { }

        class SuccessResult : BaseResult
        {
            public string matchId;
        }

        class CancelResult : BaseResult { }

        event Action<BaseResult> onSearchFinished;
        void StartSearch();
        void CancelSearch();
    }

    ISearchHandler BuildRequest();
}