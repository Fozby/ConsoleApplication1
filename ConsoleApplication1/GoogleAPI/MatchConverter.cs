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
        static Dictionary<long, string> players = new Dictionary<long, string>();

        public MatchConverter()
        {
            players.Add(356367, "Etaram");
            players.Add(557862, "Phythe");
            players.Add(692409, "Celinar");
            players.Add(464473, "Mirotica");
            players.Add(470159, "Scorilous");
            players.Add(485547, "Druzor");
            players.Add(603309, "Wart");
            players.Add(2120419, "NewBula");
            players.Add(601322, "Macabros9");
            players.Add(891580, "Rishvas");
            players.Add(6160582, "Rishmau");
            players.Add(Global.SUMMONER_ID_IGAR, "Igar");

        }

        public GameRow buildGoogleRow(Game game, TeamStats teamStats)
        {
            string playerName = game.summonerId.ToString();
            players.TryGetValue(game.summonerId, out playerName);

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
