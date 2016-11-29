using ConsoleApplication1.GoogleAPI.DataObjects;
using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.Objects;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.GoogleAPI
{
    class CompetitiveStatsBuilder
    {
        public PlayerChampionStats buildPlayerChampionStats(long summonerId, int championId, MatchCollection matchCollection)
        {
            ChampionStats cStats = buildChampionStats(championId, matchCollection);

            return new PlayerChampionStats(Global.GetPlayerName(summonerId), cStats);
        }

        public ChampionStats buildChampionStats(int championId, MatchCollection matchCollection)
        {
            Data totalIndividualData = new Data();
            Data totalTeamData = new Data();

            Double totalDmgPercent = 0;

            foreach (MatchDetails match in matchCollection.matches)
            {
                Data individualData = match.getChampionData(championId);
                Data teamData = match.getTeamData(championId);

                totalIndividualData = totalIndividualData.add(individualData);
                totalTeamData = totalTeamData.add(teamData);
                
                Double dmgPercent = ((Double) individualData.playerDamage / teamData.playerDamage) * 100;

                totalDmgPercent += dmgPercent;

            }

            totalDmgPercent = totalDmgPercent / matchCollection.Count;

            long totalMatchMins = matchCollection.GetTotalMatchMins();
            int totalWins = matchCollection.GetNumberOfWins(championId);

            ChampionStats stats = new ChampionStats(Global.getChampionName(championId));

            stats.numGames = matchCollection.Count;
            stats.win = ((double)totalWins / stats.numGames) * 100;
            stats.kills = (double)totalIndividualData.kills / stats.numGames;
            stats.deaths = (double)totalIndividualData.deaths / stats.numGames;
            stats.assists = (double)totalIndividualData.assists / stats.numGames;
            stats.goldPerMin = (double)totalIndividualData.gold / totalMatchMins;
            stats.creepsPerMin = (double)totalIndividualData.creepsKilled / totalMatchMins;
            stats.killParticipation = ((double)totalIndividualData.takedowns / totalTeamData.kills) * 100;
            stats.pctGold = ((double)totalIndividualData.gold / totalTeamData.gold) * 100;
            stats.pctCreepDamage = ((double)totalIndividualData.creepDamage / totalTeamData.creepDamage) * 100;
            stats.pctPlayerDamage = totalDmgPercent; /*((double)totalIndividualData.playerDamage / totalTeamData.playerDamage) * 100;*/
            stats.pctDamageTaken = ((double)totalIndividualData.damageTaken / totalTeamData.damageTaken) * 100;
            stats.pctKills = ((double)totalIndividualData.kills / totalTeamData.kills) * 100;
            stats.pctDeaths = ((double)totalIndividualData.deaths / totalTeamData.deaths) * 100;
            stats.pctAssists = ((double)totalIndividualData.assists / totalTeamData.assists) * 100;
            stats.healing = totalIndividualData.healing / stats.numGames;
            stats.physPlayerDamage = totalIndividualData.physPlayerDamage / stats.numGames;
            stats.physCreepDamage = totalIndividualData.physCreepDamage / stats.numGames;
            stats.crowdControl = totalIndividualData.crowdControl / stats.numGames;
            stats.goldStats = matchCollection.buildGoldTimelineStats(championId);
            stats.creepStats = matchCollection.buildCreepTimelineStats(championId);
            stats.dmgPerDeath = totalIndividualData.damageTaken / totalIndividualData.deaths;
            stats.playerDmgPerMin = (double)totalIndividualData.playerDamage / totalMatchMins;
            stats.creepDmgPerMin = (double)totalIndividualData.creepDamage / totalMatchMins;

            return stats;
        }
    }
}
