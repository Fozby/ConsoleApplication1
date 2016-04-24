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
        //static Dictionary<long, string> players = new Dictionary<long, string>();

        public MatchConverter()
        {
            //players.Add(356367, "Etaram");
            //players.Add(557862, "Phythe");
            //players.Add(692409, "Celinar");
            //players.Add(464473, "Mirotica");
            //players.Add(470159, "Scorilous");
            //players.Add(485547, "Druzor");
            //players.Add(603309, "Wart");
            //players.Add(2120419, "NewBula");
            //players.Add(601322, "Macabros9");
            //players.Add(891580, "Rishvas");
            //players.Add(6160582, "Rishmau");
            //players.Add(Global.SUMMONER_ID_IGAR, "Igar");
        }

        public GameRow BuildGameStats(Game game, MatchDetails match, int teamId)
        {
            TeamStats teamStats = GetTeamStats(match, teamId);

            string playerName = game.summonerId.ToString();
            Global.players.TryGetValue(game.summonerId, out playerName);

            GameRow row = new GameRow();

            row.gameId = game.gameId;
            DateTime dt = FromUnixTime(game.createDate);
            row.matchDate = dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            row.player = playerName;
            row.champion = Global.getChampionName(game.championId);
            row.win = game.stats.win;
            row.kills = game.stats.championsKilled;
            row.deaths = game.stats.numDeaths;
            row.assists = game.stats.assists;
            row.killsAsPct = (double)row.kills / (double)teamStats.kills;
            row.deathsAsPct = (double)row.deaths / (double)teamStats.deaths;
            row.assistsAsPct = (double)row.assists / (double)teamStats.assists;
            row.killParticipation = (double)(row.kills + row.assists) / (double)teamStats.kills;
            row.minionDmgAsPct = (double)(game.stats.totalDamageDealt - game.stats.totalDamageDealtToChampions) / (double)teamStats.minionDmg;
            row.minionKillsAsPct = (double)game.stats.minionsKilled / (double)teamStats.minionKills;
            row.playerDmgAsPct = (double)game.stats.totalDamageDealtToChampions / (double)teamStats.playerDmg;
            row.dmgTakenAsPct = (double)game.stats.totalDamageTaken / (double)teamStats.dmgTaken;
            row.goldEarnedAsPct = (double)game.stats.goldEarned / (double)teamStats.gold;

            return row;
        }

        public PlayerStats BuildPlayerStats(List<GameRow> games)
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

            foreach (GameRow game in games)
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

            return playerStats;
        }

        public ChampionStats GetChampionStats(int championId, List<MatchDetails> matches)
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
                int teamId = match.getTeamIdForChampion(championId);
                ParticipantStats individualStats = match.getStatsForChampion(championId);
                TeamStats teamStats = GetTeamStats(match, teamId);

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

                if (teamId == match.getWinningTeam())
                {
                    numWin += 1;
                }
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

            return championStats;
        }
    

        private TeamStats GetTeamStats(MatchDetails match, int teamId)
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

        public Dictionary<string, GameRow> convert(MatchDetails match)
        {
            Dictionary<string, GameRow> rows = new Dictionary<string, GameRow>();

            long teamKills = 0;
            long teamDeaths = 0;
            long teamAssists = 0;
            long teamPlayerDmg = 0;
            long teamMinionDmg = 0;
            long teamDmgTaken = 0;
            long teamGold = 0;
            long teamMinionKills = 0;

            //participantId, summonerName
            Dictionary<int, string> map = match.participantIdentities.ToDictionary(k => k.participantId, v => v.player.summonerName);

            foreach (Participant participant in match.participants)
            {
                teamKills += participant.stats.kills;
                teamDeaths += participant.stats.deaths;
                teamAssists += participant.stats.assists;
                teamPlayerDmg += participant.stats.totalDamageDealtToChampions;
                teamMinionDmg += (participant.stats.totalDamageDealt - participant.stats.totalDamageDealtToChampions);
                teamDmgTaken += participant.stats.totalDamageTaken;
                teamGold += participant.stats.goldEarned;
                teamMinionKills += participant.stats.minionsKilled;
            }

            foreach (Participant participant in match.participants)
            {
                string player;
                map.TryGetValue(participant.participantId, out player);

                GameRow row = new GameRow();
                row.gameId = match.matchId;
                row.player = player;
                row.champion = participant.championId.ToString();
                row.win = participant.stats.winner;
                row.kills = participant.stats.kills;
                row.deaths = participant.stats.deaths;
                row.assists = participant.stats.assists;
                row.killsAsPct = row.kills / teamKills;
                row.deathsAsPct = row.deaths / teamDeaths;
                row.assistsAsPct = row.assists / teamAssists;
                row.killParticipation = (row.kills + row.assists) / teamKills;
                row.minionDmgAsPct = (participant.stats.totalDamageDealt - participant.stats.totalDamageDealtToChampions) / teamMinionDmg;
                row.minionKillsAsPct = participant.stats.minionsKilled / teamMinionKills;
                row.playerDmgAsPct = participant.stats.totalDamageDealtToChampions / teamPlayerDmg;
                row.dmgTakenAsPct = participant.stats.totalDamageTaken / teamDmgTaken;
                row.goldEarnedAsPct = participant.stats.goldEarned / teamGold;

                rows.Add(player, row);
            }

            return rows;
        }

        public DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

    }
}
