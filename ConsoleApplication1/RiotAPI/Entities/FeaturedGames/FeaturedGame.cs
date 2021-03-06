﻿using MongoDB.Bson;
using System.Collections.Generic;

namespace ConsoleApplication1.RiotAPI.Entities.FeaturedGames
{
    public class FeaturedGame
    {
        public ObjectId Id { get; set; }

        public long gameId { get; set; }
        public string gameMode { get; set; }
        public long gameStartTime { get; set; }
        public List<FeaturedGameParticipant> participants { get; set; }

        public bool hasMatch { get; set; } = false;
    }
}
