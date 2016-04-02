using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ConsoleApplication1.JsonObjects;


namespace ConsoleApplication1
{
    class Mongo
    { 
        private MongoClient theClient;
        private IMongoDatabase theDataBase;
        private IMongoCollection<Game> theCollection;

        public Mongo()
        {
            theClient = new MongoClient("mongodb://localhost:27017");
            theDataBase = theClient.GetDatabase("Riot");
            theCollection = theDataBase.GetCollection<Game>("Games");
           
            handleIndexes();
        }

        private void handleIndexes()
        {
            //Add gameId as a Unique Index (Primary Key)
            if (theCollection.Indexes.List().ToList().Count == 0)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;
                theCollection.Indexes.CreateOneAsync(Builders<Game>.IndexKeys.Ascending(_ => _.gameId), cio);
            }
        }

        public bool insertGame(Game game)
        {
            try
            {
                theCollection.InsertOne(game);
                Console.WriteLine("Added game: " + game.gameId);
                return true;
            }
            catch (MongoWriteException e)
            {
                if(e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    Console.WriteLine("Error adding Game with GameID: [" + game.gameId + "]. Duplicate Game already exists.");
                }
                else
                {
                    Console.WriteLine("Unhandled exception inserting Game with GameID: [" + game.gameId + "]." + e);
                }
            }

            return false;
        }

        public void insertGames(List<Game> games)
        {
            int numAdded = 0;
            foreach(Game game in games)
            {
                if (insertGame(game))
                {
                    numAdded++;
                }
            }

            Console.WriteLine("Added " + numAdded + " / " + games.Count + " games");
        }

        public Game getGame()
        {
            var games = theCollection.Find(Builders<Game>.Filter.Empty).ToList();
            var game = games.ElementAt(0);
            return game;
        }

        public long getCount()
        {
            return theCollection.Count(Builders<Game>.Filter.Empty);
        }

        public void removeAll()
        {
            theCollection.DeleteMany(Builders<Game>.Filter.Empty);
            Console.WriteLine("Removed all Games from DB");
        }
    }
}
