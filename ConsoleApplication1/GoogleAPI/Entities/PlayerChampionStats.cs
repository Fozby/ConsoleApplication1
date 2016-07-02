namespace ConsoleApplication1.GoogleAPI.Entities
{
    class PlayerChampionStats
    {
        public string summonerName { get; }
        public ChampionStats championStats { get; }
        
        public PlayerChampionStats(string summonerName, ChampionStats championStats)
        {
            this.summonerName = summonerName;
            this.championStats = championStats;
        }
    }
}