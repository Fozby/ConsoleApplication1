using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects
{
    public class Game
    {
        public FellowPlayer[] fellowPlayers { get; set; }
        public string gameType { get; set; }
        public Stats stats { get; set; }
        public long gameId { get; set; }
    }
}
