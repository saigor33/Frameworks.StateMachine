namespace Match
{
    class Component
    {
        public Component(IGameplayProgression gameplayProgression, IMatchMaker matchMaker, Data.Config config)
        {
            var statesContext = new Logic.StatesContext(gameplayProgression, matchMaker, config);
            var stateMachine = new Logic.StateMachine(new Logic.LockedState(statesContext));
        }
    }
}