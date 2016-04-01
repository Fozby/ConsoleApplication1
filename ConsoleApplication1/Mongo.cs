using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private IMongoCollection<BsonDocument> theCollection;

        public Mongo()
        {
            theClient = new MongoClient("mongodb://localhost:27017");
            theDataBase = theClient.GetDatabase("foo");
            theCollection = theDataBase.GetCollection<BsonDocument>("bar");
        }

        public async Task insertRec()
        {
            var document = new BsonDocument
            {
                { "name", "MongoDB" },
                { "type", "Database" },
                { "count", 1 },
                { "info", new BsonDocument
                          {
                              { "x", 203 },
                              { "y", 102 }
                          }}
            };

            Console.WriteLine("About to add");
            await theCollection.InsertOneAsync(document);
            Console.WriteLine("Finished adding");
        }

        public async Task insertGame(Game game)
        {
            await theCollection.InsertOneAsync(game.ToBsonDocument());
        }

        public async Task insertGames(List<Game> games)
        {
            IEnumerable<BsonDocument> bsons = games.Select(a => a.ToBsonDocument());
            await theCollection.InsertManyAsync(bsons);
        }

        public async Task getRec()
        {
            var getDoc = await theCollection.Find(new BsonDocument()).FirstOrDefaultAsync();
            Console.WriteLine(getDoc.ToString());
        }

        public async Task getCount()
        {
            long count = await theCollection.CountAsync(new BsonDocument());
            Console.WriteLine(count.ToString());
        }

        public async Task removeAll()
        {
            await theCollection.DeleteManyAsync(new BsonDocument());
        }
    }
}
