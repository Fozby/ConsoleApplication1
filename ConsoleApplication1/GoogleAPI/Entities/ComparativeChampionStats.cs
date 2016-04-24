using System.Collections.Generic;

namespace ConsoleApplication1.GoogleAPI.Entities
{
    class ComparativeChampionStats
    {
        public ChampionStats championStats { get; set; }
        public Dictionary<string, PlayerStats> playerStats { get; set; } = new Dictionary<string, PlayerStats>();

        public long getNumFriendlyGames()
        {
            long gameCount = 0;
            foreach (PlayerStats stats in playerStats.Values)
            {
                gameCount += stats.numGames;
            }

            return gameCount;
        }
    }
}
