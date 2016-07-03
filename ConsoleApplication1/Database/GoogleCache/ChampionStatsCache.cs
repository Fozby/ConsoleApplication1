using ConsoleApplication1.GoogleAPI.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Database.GoogleCache
{
    class ChampionStatsCache
    {
        public ObjectId Id { get; set; }

        public int x { get; set; }
        public ChampionStats championStats { get; set; }
        public long edition { get; set; } //Edition is #games stored, we assume cache is out of date if main DB has more games than cache

        public ChampionStatsCache()
        {

        }

        public ChampionStatsCache(ChampionStats stats)
        {
            this.championStats = stats;
            this.edition = stats.numGames;
        }
    }
}
