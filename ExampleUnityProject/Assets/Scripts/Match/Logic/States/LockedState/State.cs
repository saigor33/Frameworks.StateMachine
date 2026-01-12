namespace Match.Logic
{
    partial class LockedState : BaseState
    {
        readonly StatesContext _statesContext;
        IGameplayProgression _gameplayProgression => _statesContext.gameplayProgression;
        Data.Config _config => _statesContext.config;

        public LockedState(StatesContext statesContext)
        {
            _statesContext = statesContext;
        }

        protected override void OnEnter()
        {
            _gameplayProgression.onPointsAdded += OnGameplayPointsAdded;
        }

        protected override void OnExit()
        {
            _gameplayProgression.onPointsAdded -= OnGameplayPointsAdded;
        }

        void OnGameplayPointsAdded()
        {
            if (_gameplayProgression.points >= _config.minGameplayPoints)
            {
                Leave(new MinGameplayPointsReachedTransition(_statesContext));
            }
        }
    }
}