using MongoDB.Bson;
using System.Collections.Generic;

namespace ConsoleApplication1.RiotAPI.Entities.RecentGames
{
    public class RecentGame
    {
        public ObjectId Id { get; set; }

        public long summonerId { get; set; }
        public List<FellowPlayer> fellowPlayers { get; set; }
        public string gameType { get; set; }
        public Stats stats { get; set; }
        public long gameId { get; set; }
        public int championId { get; set; }
        public long createDate { get; set; }
        public string gameMode { get; set; }
        public bool invalid { get; set; }
        public int ipEarned { get; set; }
        public int level { get; set; }
        public int mapId { get; set; }
        public int spell1 { get; set; }
        public int spell2 { get; set; }
        public string subType { get; set; }
        public int teamId { get; set; }
    }
}
