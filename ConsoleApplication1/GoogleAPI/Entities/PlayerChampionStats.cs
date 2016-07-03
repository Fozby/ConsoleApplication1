namespace ConsoleApplication1.GoogleAPI.Entities
{
    public class PlayerChampionStats
    {
        public string summonerName { get; set; }
        public ChampionStats championStats { get; set; }
        
        public PlayerChampionStats(string summonerName, ChampionStats championStats)
        {
            this.summonerName = summonerName;
            this.championStats = championStats;
        }
    }
}