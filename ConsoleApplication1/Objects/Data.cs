using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleAPI.DataObjects
{
    public class Data
    {
        public long kills { get; set; } = 0;
        public long deaths { get; set; } = 0;
        public long assists { get; set; } = 0;
        public long takedowns { get; set; } = 0;
        public long playerDamage { get; set; } = 0;
        public long creepDamage { get; set; } = 0;
        public long damageTaken { get; set; } = 0;
        public long gold { get; set; } = 0;
        public long creepsKilled { get; set; } = 0;
        public long healing { get; set; } = 0;
        public long physPlayerDamage { get; set; } = 0;
        public long physCreepDamage { get; set; } = 0;
        public long crowdControl { get; set; } = 0;

        public Data()
        {

        }

        public Data add(Data other)
        {
            this.kills += other.kills;
            this.deaths += other.deaths;
            this.assists += other.assists;
            this.takedowns += other.takedowns;
            this.playerDamage += other.playerDamage;
            this.creepDamage += other.creepDamage;
            this.damageTaken += other.damageTaken;
            this.gold += other.gold;
            this.creepsKilled += other.creepsKilled;
            this.healing += other.healing;
            this.physPlayerDamage += other.physPlayerDamage;
            this.physCreepDamage += other.physCreepDamage;
            this.crowdControl += other.crowdControl;


            return this;
        }

        public Data(ParticipantStats stats)
        {
            this.kills = stats.kills;
            this.deaths = stats.deaths;
            this.assists = stats.assists;
            this.takedowns = this.kills + this.assists;
            this.playerDamage = stats.totalDamageDealtToChampions;
            this.creepDamage = stats.totalDamageDealt - this.playerDamage;
            this.damageTaken = stats.totalDamageTaken;
            this.gold = stats.goldEarned;
            this.creepsKilled = stats.minionsKilled;
            this.healing += stats.totalHeal;
            this.physPlayerDamage += stats.physicalDamageDealtToChampions;
            this.physCreepDamage += stats.physicalDamageDealt - stats.physicalDamageDealtToChampions;
            this.crowdControl += stats.totalTimeCrowdControlDealt;

        }
    }
}
