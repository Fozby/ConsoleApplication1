namespace ConsoleApplication1.GoogleAPI.Entities
{
    public class PlayerStats
    {
        public string player { get; set; }
        public int numGames { get; set; }
        public double win { get; set; }
        public double avgKills { get; set; }
        public double avgDeaths { get; set; }
        public double avgAssists { get; set; }
        public double avgKillParticipation { get; set; }
        public double avgKillsPct { get; set; }
        public double avgDeathsPct { get; set; }
        public double avgAssistsPct { get; set; }
        public double avgMinionDmgPct { get; set; }
        public double avgMinionKillsPct { get; set; }
        public double avgPlayerDmgPct { get; set; }
        public double avgDmgTakenPct { get; set; }
        public double avgGoldPct { get; set; }
    }
}