using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleAPI.Entities
{
    public class PlayerStats : Stats
    {
        public string summonerName { get; set; }

        public int numFeaturedPossible { get; set; }
        public int numFeaturedGames { get; set; }
        public int numFeaturedWins { get; set; }

        public PlayerStats(string summonerName)
        {
            this.summonerName = summonerName;
        }
    }
}
