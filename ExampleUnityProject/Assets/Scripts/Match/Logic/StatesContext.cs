namespace Match.Logic
{
    class StatesContext
    {
        public IGameplayProgression gameplayProgression { get; }
        public IMatchMaker matchMaker { get; }
        public Data.Config config { get; }

        public StatesContext(IGameplayProgression gameplayProgression, IMatchMaker matchMaker, Data.Config config)
        {
            this.gameplayProgression = gameplayProgression;
            this.matchMaker = matchMaker;
            this.config = config;
        }
    }
}