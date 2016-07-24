using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Objects
{
    public class RecentGameCollection
    {
        public List<RecentGame> recentGames { get; set; }

        public RecentGameCollection(List<RecentGame> games)
        {
            this.recentGames = games;
        }

        public int GetChampionIdForPlayerAndGame(long summonerId, long matchId)
        {
            RecentGame game = recentGames.Find(g => g.gameId == matchId && g.summonerId == summonerId);

            if (game == null)
            {
                return -1;
            }

            return game.championId;
        }
    }
}
