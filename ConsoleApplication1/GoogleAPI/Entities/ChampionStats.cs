using ConsoleApplication1.JsonObjects.MatchObjects;
using ConsoleApplication1.Objects;
using System.Collections.Generic;

namespace ConsoleApplication1.GoogleNS.Entities
{
    class ChampionStats
    {
        public string champion { get; set; }
        public int numGames { get; set; }
        public double win { get; set; }
        public double avgKills { get; set; }
        public double avgDeaths { get; set; }
        public double avgAssists { get; set; }
        public double avgKillParticipation { get; set; }
        public double avgKillsPct { get; set; }
        public double avgDeathsPct { get; set; }
        public double avgAssistsPct { get; set; }
        public double avgMinionKillsPct { get; set; }
        public double avgMinionDmgPct { get; set; }
        public double avgPlayerDmgPct { get; set; }
        public double avgDmgTakenPct { get; set; }
        public double avgGoldPct { get; set; }

        //TODO dont do this...
        public ChampionStats(int championId, List<MatchDetails> matches)
        {
            champion = Global.getChampionName(championId);

            int numWin = 0;
            numGames = matches.Count;

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
                TeamStats teamStats = Program.getTeamStatsForMatch(match, teamId);

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

            win = (double) numWin / (double) numGames;
            avgKills = individualKills / numGames;
            avgDeaths = individualDeaths / numGames;
            avgAssists = individualAssists / numGames;
            avgKillParticipation = (double) (individualKills + individualAssists) / (double)teamKills;
            avgKillsPct = (double)individualKills / (double)teamKills;
            avgDeathsPct = (double)individualDeaths / (double)teamDeaths;
            avgAssistsPct = (double)individualAssists / (double)teamAssists;
            avgMinionDmgPct = (double)individualMinionDmg / (double)teamMinionDmg;
            avgMinionKillsPct = (double)individualMinionKills / (double)teamMinionKills;
            avgPlayerDmgPct = (double)individualPlayerDmg / (double)teamPlayerDmg;
            avgDmgTakenPct = (double)individualDmgTaken / (double)teamDmgTaken;
            avgGoldPct = (double)individualGold / (double)teamGold;

        }
    }
}
