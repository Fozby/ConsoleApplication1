using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleNS
{
    class GoogleRow
    {
        public long gameId { get; set; }
        public string player { get; set; }
        public string champion { get; set; }
        public bool win { get; set; }
        public long kills { get; set; }
        public long deaths { get; set; }
        public long assists { get; set; }
        public double killsAsPct { get; set; }
        public double deathsAsPct { get; set; }
        public double assistsAsPct { get; set; }
        public double killParticipation { get; set; }
        public double minionDmgAsPct { get; set; }
        public double minionKillsAsPct { get; set; }
        public double playerDmgAsPct { get; set; }
        public double dmgTakenAsPct { get; set; }
        public double goldEarnedAsPct { get; set; }
    }
}
