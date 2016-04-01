﻿using MongoDB.Bson;
using System.Collections.Generic;

namespace ConsoleApplication1.JsonObjects
{
    public class Game
    {
        public ObjectId Id { get; set; }

        public List<FellowPlayer> fellowPlayers { get; set; }
        public string gameType { get; set; }
        public Stats stats { get; set; }
        public long gameId { get; set; }
    }
}
