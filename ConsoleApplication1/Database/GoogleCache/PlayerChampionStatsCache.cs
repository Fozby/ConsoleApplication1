using ConsoleApplication1.GoogleAPI.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Database.GoogleCache
{
    public class PlayerChampionStatsCache
    { 
        public ObjectId Id { get; set; }

        public PlayerChampionStats playerStats { get; set; }
        public long edition { get; set; } //Edition is #games stored, we assume cache is out of date if main DB has more games than cache

        public PlayerChampionStatsCache(PlayerChampionStats stats)
        {
            this.playerStats = stats;
            this.edition = stats.championStats.numGames;
        }
    }
}
