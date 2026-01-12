namespace Match.Logic
{
    partial class InMatchState
    {
        class FinishMatchTransition : BaseTransition<FinishMatchTransition.Context>
        {
            public class Context
            {
                public string matchId;
                public bool isPlayerWon;
            }

            readonly StatesContext _statesContext;

            public FinishMatchTransition(StatesContext statesContext)
            {
                _statesContext = statesContext;
            }

            protected override ExecuteResult Execute(Context context)
            {
                if (context.isPlayerWon)
                {
                    // Apply win result logic
                }
                else
                {
                    // Apply lose result logic
                }

                return new ExecuteResult
                {
                    nextState = new WaitStartSearchState(_statesContext)
                };
            }
        }
    }
}