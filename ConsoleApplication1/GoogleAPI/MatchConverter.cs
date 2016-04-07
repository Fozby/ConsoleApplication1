using ConsoleApplication1.JsonObjects;
using ConsoleApplication1.JsonObjects.MatchObjects;
using ConsoleApplication1.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleNS
{
    class MatchConverter
    {
        public static GoogleRow buildGoogleRow(Game game, TeamStats teamStats)
        {
            GoogleRow row = new GoogleRow();

            row.gameId = game.gameId;
            row.player = game.summonerId.ToString();
            row.champion = game.championId.ToString();
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

        public Dictionary<string, GoogleRow> convert(MatchDetails match)
        {
            Dictionary<string, GoogleRow> rows = new Dictionary<string, GoogleRow>();

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

                GoogleRow row = new GoogleRow();
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

    }
}
