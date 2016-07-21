using ConsoleApplication1.GoogleAPI.Entities.Timeline;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using System.Collections.Generic;

namespace ConsoleApplication1.GoogleAPI.Entities
{
    public class ChampionStats
    {
        public string championName { get; set; }

        public int numGames { get; set; }
        public double win { get; set; }
        public double kills { get; set; }
        public double deaths { get; set; }
        public double assists { get; set; }

        public double goldPerMin { get; set; }
        public double creepsPerMin { get; set; }
        public double killParticipation { get; set; }
        public double pctGold { get; set; }
        public double pctCreepDamage { get; set; }
        public double pctPlayerDamage { get; set; }
        public double pctDamageTaken { get; set; }
        public double pctKills { get; set; }
        public double pctDeaths { get; set; }
        public double pctAssists { get; set; }

        public double healing { get; set; }
        public double physPlayerDamage { get; set; }
        public double physCreepDamage { get; set; }
        public double crowdControl { get; set; }

        public double dmgPerDeath { get; set; }

        public double playerDmgPerMin { get; set; }
        public double creepDmgPerMin { get; set; }

        public TimelineStats goldStats { get; set; }
        public TimelineStats creepStats { get; set; }

        public ChampionStats(string championName)
        {
            this.championName = championName;
        }   
    }
}