using ConsoleApplication1.GoogleNS;
using ConsoleApplication1.JsonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Objects
{
    public class PlayerStats
    {
        public string player { get; set; }
        public int numGames { get; set; }
        public double win { get; set; }
        public double avgKills { get; set; }
        public double avgDeaths { get; set; }
        public double avgAssists { get; set; }
        public double avgKillParticipation { get; set; }
        public double avgKillsPct { get; set; }
        public double avgDeathsPct { get; set; }
        public double avgAssistsPct { get; set; }
        public double avgMinionDmgPct { get; set; }
        public double avgMinionKillsPct { get; set; }
        public double avgPlayerDmgPct { get; set; }
        public double avgDmgTakenPct { get; set; }
        public double avgGoldPct { get; set; }

        //TODO dont do this...
        public PlayerStats(List<GameRow> games)
        {
            player = games.ElementAt(0).player;
            numGames = games.Count;

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

            win = totalWin / numGames;
            avgKills = totalKills / numGames;
            avgDeaths = totalDeaths / numGames;
            avgAssists = totalAssists / numGames;
            avgKillParticipation = totalKillParticipation / numGames;
            avgKillsPct = totalKillsPct / numGames;
            avgDeathsPct = totalDeathsPct / numGames;
            avgAssistsPct = totalAssistsPct / numGames;
            avgMinionDmgPct = totalMinionDmgPct / numGames;
            avgMinionKillsPct = totalMinionKillsPct / numGames;
            avgPlayerDmgPct = totalPlayerDmgPct / numGames;
            avgDmgTakenPct = totalDmgTakenPct / numGames;
            avgGoldPct = totalGoldPct / numGames;
        }
    }
}


