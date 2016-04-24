using System.Collections.Generic;

namespace ConsoleApplication1.RiotAPI.Entities.RecentGames
{
    public class Response_RecentGames
    {
        public List<Game> games { get; set; }
        public long summonerId { get; set; }
    }
}
