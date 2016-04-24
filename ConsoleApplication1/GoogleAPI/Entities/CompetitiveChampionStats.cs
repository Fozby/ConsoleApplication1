using System.Collections.Generic;

namespace ConsoleApplication1.GoogleAPI.Entities
{
    class CompetitiveChampionStats
    {
        public ChampionStats championStats { get; set; }
        public Dictionary<string, PlayerStats> playerStats { get; set; } = new Dictionary<string, PlayerStats>();
    }
}
