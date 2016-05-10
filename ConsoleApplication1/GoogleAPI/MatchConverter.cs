using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.GoogleAPI
{
    class MatchConverter
    {
        public ComparativeChampionStats BuildCompetitiveChampionStats(int championId, List<MatchDetails> matches, Dictionary<long, List<GameStats>> playerGames)
        {
            ComparativeChampionStats stats = new ComparativeChampionStats();

            stats.championStats = BuildChampionStats(championId, matches);
            
            foreach(long summonerId in playerGames.Keys)
            {
                List<GameStats> recentGames = new List<GameStats>();
                playerGames.TryGetValue(summonerId, out recentGames);

                if (playerGames.Count > 0)
                {
                    PlayerStats playerStats = BuildPlayerStats(recentGames);
                    stats.playerStats.Add(Global.GetPlayerName(summonerId), playerStats);
                }
            }

            return stats;
        }

        public GameStats BuildGameStats(RecentGame game, MatchDetails match)
        {
            int teamId = game.teamId;
            TeamStats teamStats = BuildTeamStats(match, teamId);

            string playerName = game.summonerId.ToString();
            Global.players.TryGetValue(game.summonerId, out playerName);

            GameStats gameStats = new GameStats();

            gameStats.gameId = game.gameId;
            DateTime dt = FromUnixTime(game.createDate);
            gameStats.matchDate = dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            gameStats.player = playerName;
            gameStats.champion = Global.getChampionName(game.championId);
            gameStats.win = game.stats.win;
            gameStats.kills = game.stats.championsKilled;
            gameStats.deaths = game.stats.numDeaths;
            gameStats.assists = game.stats.assists;
            gameStats.killsAsPct = (double)gameStats.kills / (double)teamStats.kills;
            gameStats.deathsAsPct = (double)gameStats.deaths / (double)teamStats.deaths;
            gameStats.assistsAsPct = (double)gameStats.assists / (double)teamStats.assists;
            gameStats.killParticipation = (double)(gameStats.kills + gameStats.assists) / (double)teamStats.kills;
            gameStats.minionDmgAsPct = (double)(game.stats.totalDamageDealt - game.stats.totalDamageDealtToChampions) / (double)teamStats.minionDmg;
            gameStats.minionKillsAsPct = (double)game.stats.minionsKilled / (double)teamStats.minionKills;
            gameStats.playerDmgAsPct = (double)game.stats.totalDamageDealtToChampions / (double)teamStats.playerDmg;
            gameStats.dmgTakenAsPct = (double)game.stats.totalDamageTaken / (double)teamStats.dmgTaken;
            gameStats.goldEarnedAsPct = (double)game.stats.goldEarned / (double)teamStats.gold;

            double matchMins = match.matchDuration / 60.0;
            gameStats.playerDmgPerMin = (double)game.stats.totalDamageDealtToChampions / matchMins;
            gameStats.minionDmgPerMin = (double)(game.stats.totalDamageDealt - game.stats.totalDamageDealtToChampions) / matchMins;
            gameStats.minionKillsPerMin = (double)game.stats.minionsKilled / matchMins;
            gameStats.goldPerMin = (double)game.stats.goldEarned / matchMins;

            return gameStats;
        }

        public PlayerStats BuildPlayerStats(List<GameStats> games)
        {
            PlayerStats playerStats = new PlayerStats();

            playerStats.player = games.ElementAt(0).player;
            playerStats.numGames = games.Count;

            double totalWin = 0.0;
            double totalKills = 0.0;
            double totalDeaths = 0.0;
            double totalAssists = 0.0;
            double totalKillParticipation = 0.0;
            double totalKillsPct = 0.0;
            double totalDeathsPct = 0.0;
            double totalAssistsPct = 0.0;
            double totalMinionDmgPct = 0.0;
            double totalMinionKillsPct = 0.0;
            double totalPlayerDmgPct = 0.0;
            double totalDmgTakenPct = 0.0;
            double totalGoldPct = 0.0;
            double totalPlayerDmgPerMin = 0.0;
            double totalMinionDmgPerMin = 0.0;
            double totalMinionKillsPerMin = 0.0;
            double totalGoldPerMin = 0.0;

            foreach (GameStats game in games)
            {

                if (game.win)
                {
                    totalWin++;
                }

                totalKills += game.kills;
                totalDeaths += game.deaths;
                totalAssists += game.assists;
                totalKillParticipation += game.killParticipation;
                totalKillsPct += game.killsAsPct;
                totalDeathsPct += game.deathsAsPct;
                totalAssistsPct += game.assistsAsPct;
                totalMinionDmgPct += game.minionDmgAsPct;
                totalMinionKillsPct += game.minionKillsAsPct;
                totalPlayerDmgPct += game.playerDmgAsPct;
                totalDmgTakenPct += game.dmgTakenAsPct;
                totalGoldPct += game.goldEarnedAsPct;
                totalPlayerDmgPerMin += game.playerDmgPerMin;
                totalMinionDmgPerMin += game.minionDmgPerMin;
                totalMinionKillsPerMin += game.minionKillsPerMin;
                totalGoldPerMin += game.goldPerMin;
            }

            playerStats.win = totalWin / playerStats.numGames;
            playerStats.avgKills = totalKills / playerStats.numGames;
            playerStats.avgDeaths = totalDeaths / playerStats.numGames;
            playerStats.avgAssists = totalAssists / playerStats.numGames;
            playerStats.avgKillParticipation = totalKillParticipation / playerStats.numGames;
            playerStats.avgKillsPct = totalKillsPct / playerStats.numGames;
            playerStats.avgDeathsPct = totalDeathsPct / playerStats.numGames;
            playerStats.avgAssistsPct = totalAssistsPct / playerStats.numGames;
            playerStats.avgMinionDmgPct = totalMinionDmgPct / playerStats.numGames;
            playerStats.avgMinionKillsPct = totalMinionKillsPct / playerStats.numGames;
            playerStats.avgPlayerDmgPct = totalPlayerDmgPct / playerStats.numGames;
            playerStats.avgDmgTakenPct = totalDmgTakenPct / playerStats.numGames;
            playerStats.avgGoldPct = totalGoldPct / playerStats.numGames;
            playerStats.avgPlayerDmgPerMin = totalPlayerDmgPerMin / playerStats.numGames;
            playerStats.avgMinionDmgPerMin = totalMinionDmgPerMin / playerStats.numGames;
            playerStats.avgMinionKillsPerMin = totalMinionKillsPerMin / playerStats.numGames;
            playerStats.avgMinionKillsPerMin = totalMinionKillsPerMin / playerStats.numGames;
            playerStats.avgGoldPerMin = totalGoldPerMin / playerStats.numGames;

            return playerStats;
        }

        public ChampionStats BuildChampionStats(int championId, List<MatchDetails> matches)
        {
            ChampionStats championStats = new ChampionStats();

            championStats.champion = Global.getChampionName(championId);

            int numWin = 0;
            championStats.numGames = matches.Count;

            long individualKills = 0;
            long individualDeaths = 0;
            long individualAssists = 0;
            long individualGold = 0;
            long individualMinionKills = 0;
            long individualMinionDmg = 0;
            long individualPlayerDmg = 0;
            long individualDmgTaken = 0;
            double matchDurationMins = 0.0;

            long teamKills = 0;
            long teamDeaths = 0;
            long teamAssists = 0;
            long teamGold = 0;
            long teamMinionKills = 0;
            long teamMinionDmg = 0;
            long teamPlayerDmg = 0;
            long teamDmgTaken = 0;

            foreach (MatchDetails match in matches)
            {
                int teamId = match.GetTeamIdForChampion(championId);
                ParticipantStats individualStats = match.GetStatsForChampion(championId);
                TeamStats teamStats = BuildTeamStats(match, teamId);

                individualKills += individualStats.kills;
                individualDeaths += individualStats.deaths;
                individualAssists += individualStats.assists;
                individualGold += individualStats.goldEarned;
                individualMinionKills += individualStats.minionsKilled;
                individualMinionDmg += individualStats.totalDamageDealt - individualStats.totalDamageDealtToChampions;
                individualPlayerDmg += individualStats.totalDamageDealtToChampions;
                individualDmgTaken += individualStats.totalDamageTaken;

                teamKills += teamStats.kills;
                teamDeaths += teamStats.deaths;
                teamAssists += teamStats.assists;
                teamGold += teamStats.gold;
                teamMinionKills += teamStats.minionKills;
                teamMinionDmg += teamStats.minionDmg;
                teamPlayerDmg += teamStats.playerDmg;
                teamDmgTaken += teamStats.dmgTaken;

                if (teamId == match.GetWinningTeam())
                {
                    numWin += 1;
                }

                matchDurationMins += match.matchDuration / 60.0;
            }

            championStats.win = (double) numWin / (double) championStats.numGames;
            championStats.avgKills = individualKills / championStats.numGames;
            championStats.avgDeaths = individualDeaths / championStats.numGames;
            championStats.avgAssists = individualAssists / championStats.numGames;
            championStats.avgKillParticipation = (double)(individualKills + individualAssists) / (double)teamKills;
            championStats.avgKillsPct = (double)individualKills / (double)teamKills;
            championStats.avgDeathsPct = (double)individualDeaths / (double)teamDeaths;
            championStats.avgAssistsPct = (double)individualAssists / (double)teamAssists;
            championStats.avgMinionDmgPct = (double)individualMinionDmg / (double)teamMinionDmg;
            championStats.avgMinionKillsPct = (double)individualMinionKills / (double)teamMinionKills;
            championStats.avgPlayerDmgPct = (double)individualPlayerDmg / (double)teamPlayerDmg;
            championStats.avgDmgTakenPct = (double)individualDmgTaken / (double)teamDmgTaken;
            championStats.avgGoldPct = (double)individualGold / (double)teamGold;
            championStats.avgPlayerDmgPerMin = (double)individualPlayerDmg / matchDurationMins;
            championStats.avgMinionDmgPerMin = (double)individualMinionDmg / matchDurationMins;
            championStats.avgMinionKillsPerMin = (double)individualMinionKills / matchDurationMins;
            championStats.avgGoldPerMin = (double)individualGold / matchDurationMins;

            return championStats;
        }
    
        private TeamStats BuildTeamStats(MatchDetails match, int teamId)
        {
            TeamStats teamStats = new TeamStats();

            foreach (Participant participant in match.participants)
            {
                if (participant.teamId == teamId)
                {
                    teamStats.kills += participant.stats.kills;
                    teamStats.deaths += participant.stats.deaths;
                    teamStats.assists += participant.stats.assists;
                    teamStats.playerDmg += participant.stats.totalDamageDealtToChampions;
                    teamStats.minionDmg += (participant.stats.totalDamageDealt - participant.stats.totalDamageDealtToChampions);
                    teamStats.dmgTaken += participant.stats.totalDamageTaken;
                    teamStats.gold += participant.stats.goldEarned;
                    teamStats.minionKills += participant.stats.minionsKilled;
                }
            }
            return teamStats;
        }

        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }
    }
}
