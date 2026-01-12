namespace Match.Logic
{
    partial class LockedState
    {
        class MinGameplayPointsReachedTransition : BaseTransition
        {
            readonly StatesContext _statesContext;

            public MinGameplayPointsReachedTransition(StatesContext statesContext)
            {
                _statesContext = statesContext;
            }

            protected override ExecuteResult Execute()
            {
                // Save data and other logic

                return new ExecuteResult
                {
                    nextState = new WaitStartSearchState(_statesContext)
                };
            }
        }
    }
}