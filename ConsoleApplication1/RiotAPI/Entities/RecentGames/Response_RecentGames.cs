using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects
{
    public class Response_RecentGames
    {
        public List<Game> games { get; set; }
        public long summonerId { get; set; }
    }
}
