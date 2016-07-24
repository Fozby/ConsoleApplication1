using ConsoleApplication1.Database;
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
    public class PlayerStatsBuilder
    {
        private Mongo mongo;

        public PlayerStatsBuilder(Mongo mongo)
        {
            this.mongo = mongo;
        }

        public PlayerStats buildPlayerStats(long summonerId, MatchCollection matchCollection)
        {
            Data totalIndividualData = new Data();
            Data totalTeamData = new Data();

            int numFeaturedWins = 0;
            int numFeaturedGames = 0;
            int numFeaturedPossible = 0;

            foreach (MatchDetails match in matchCollection.matches)
            {
                Data individualData = match.getChampionDataForPlayer(summonerId);
                Data teamData = match.getTeamDataForPlayer(summonerId);

                totalIndividualData = totalIndividualData.add(individualData);
                totalTeamData = totalTeamData.add(teamData);

                long start = match.matchCreation;
                long end = start + (match.matchDuration * 1000); //match duration is in seconds, create date is milliseconds

                if (mongo.IsFeaturedGameRecorded(start, end))
                {
                    numFeaturedPossible++;

                    if (mongo.isFeaturedGame(match.matchId))
                    {
                        numFeaturedGames++;

                        if (match.DidPlayerWin(summonerId))
                        {
                            numFeaturedWins++;
                        }
                    }
            
                }
            }

            long totalMatchMins = matchCollection.GetTotalMatchMins();
            int totalWins = matchCollection.GetNumberOfWinsForPlayer(summonerId);

            PlayerStats stats = new PlayerStats(Global.GetPlayerName(summonerId));
           
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
            stats.pctPlayerDamage = ((double)totalIndividualData.playerDamage / totalTeamData.playerDamage) * 100;
            stats.pctDamageTaken = ((double)totalIndividualData.damageTaken / totalTeamData.damageTaken) * 100;
            stats.pctKills = ((double)totalIndividualData.kills / totalTeamData.kills) * 100;
            stats.pctDeaths = ((double)totalIndividualData.deaths / totalTeamData.deaths) * 100;
            stats.pctAssists = ((double)totalIndividualData.assists / totalTeamData.assists) * 100;
            stats.healing = totalIndividualData.healing / stats.numGames;
            stats.physPlayerDamage = totalIndividualData.physPlayerDamage / stats.numGames;
            stats.physCreepDamage = totalIndividualData.physCreepDamage / stats.numGames;
            stats.crowdControl = totalIndividualData.crowdControl / stats.numGames;
            stats.goldStats = matchCollection.buildGoldTimelineStatsForPlayer(summonerId);
            stats.creepStats = matchCollection.buildCreepTimelineStatsForPlayer(summonerId);
            stats.dmgPerDeath = totalIndividualData.damageTaken / totalIndividualData.deaths;
            stats.playerDmgPerMin = (double)totalIndividualData.playerDamage / totalMatchMins;
            stats.creepDmgPerMin = (double)totalIndividualData.creepDamage / totalMatchMins;

            stats.numFeaturedPossible = numFeaturedPossible;
            stats.numFeaturedGames = numFeaturedGames;
            stats.numFeaturedWins = numFeaturedWins;

            return stats;
        }
    }
}
