using ConsoleApplication1.GoogleAPI.Entities.Timeline;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using System.Collections.Generic;

namespace ConsoleApplication1.GoogleAPI.Entities
{
    public class ChampionStats : Stats
    {
        public string championName { get; set; }

        public ChampionStats(string championName)
        {
            this.championName = championName;
        }   
    }
}