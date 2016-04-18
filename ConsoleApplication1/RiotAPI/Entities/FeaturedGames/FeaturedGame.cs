using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects.FeaturedGames
{
    class FeaturedGame
    {
        public ObjectId Id { get; set; }

        public long gameId { get; set; }
        public string gameMode { get; set; }
        public long gameStartTime { get; set; }
        public List<FeaturedGameParticipant> participants { get; set; }
    }
}
