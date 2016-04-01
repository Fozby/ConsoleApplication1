using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ConsoleApplication1.JsonObjects;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

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

        public async Task insertGame(Game game)
        {
            try
            {
                await theCollection.InsertOneAsync(game);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task insertGames(List<Game> games)
        {
            //IEnumerable<BsonDocument> bsons = games.Where(game => game.stats.win == true).Select(a => a);
          await theCollection.InsertManyAsync(games);
        }


        public async Task getRec()
        {
            var games = await theCollection.Find(Builders<Game>.Filter.Empty).ToListAsync();
            var game = games.ElementAt(0);
            Console.WriteLine(game.ToJson()); 
        }

        public async Task getCount()
        {
            long count = await theCollection.CountAsync(Builders<Game>.Filter.Empty);
            Console.WriteLine(count.ToString());
        }

        public async Task removeAll()
        {
            await theCollection.DeleteManyAsync(Builders<Game>.Filter.Empty);
        }
    }
}
