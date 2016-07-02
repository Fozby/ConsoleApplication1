﻿using System.Collections.Generic;

namespace ConsoleApplication1.GoogleAPI.Entities
{
    class CompetitiveStats
    {
        public ChampionStats championStats { get; }
        public List<PlayerChampionStats> playerStats { get; }
        public int totalCompetitiveGames { get; }
       
        public CompetitiveStats(ChampionStats cStats, List<PlayerChampionStats> pStats)
        {
            this.championStats = cStats;
            this.playerStats = pStats;

            foreach (PlayerChampionStats stats in playerStats)
            {
                totalCompetitiveGames += stats.championStats.numGames;
            }
        }
    }
}
