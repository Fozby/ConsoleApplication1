using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;

namespace ConsoleApplication1.Database
{
    class Mongo
    { 
        private MongoClient theClient;
        private IMongoDatabase theDataBase;
        private IMongoCollection<RecentGame> gameCollection;
        private IMongoCollection<MatchDetails> matchCollection;
        private IMongoCollection<FeaturedGame> featuredGameCollection;

        public Mongo()
        {
            theClient = new MongoClient("mongodb://localhost:27017");
            theDataBase = theClient.GetDatabase("Riot_1");
            gameCollection = theDataBase.GetCollection<RecentGame>("Game_1");
            matchCollection = theDataBase.GetCollection<MatchDetails>("Match_1");
            featuredGameCollection = theDataBase.GetCollection<FeaturedGame>("FeaturedGame_1");

            handleIndexes();
        }

        private void handleIndexes()
        {
            //Add gameId as a Unique Index (Primary Key)
            if (gameCollection.Indexes.List().ToList().Count == 0)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;

                IndexKeysDefinition<RecentGame> gameId = Builders<RecentGame>.IndexKeys.Ascending(_ => _.gameId);
                IndexKeysDefinition<RecentGame> summonerId = Builders<RecentGame>.IndexKeys.Ascending(_ => _.summonerId);

                gameCollection.Indexes.CreateOne(Builders<RecentGame>.IndexKeys.Combine(gameId, summonerId), cio);
            }

            //Add matchId as a Unique Index (Primary Key)
            if (matchCollection.Indexes.List().ToList().Count == 0)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;
                matchCollection.Indexes.CreateOne(Builders<MatchDetails>.IndexKeys.Ascending(_ => _.matchId), cio);
            }

            //Add gameId as a Unique Index (Primary Key)
            if (featuredGameCollection.Indexes.List().ToList().Count == 0)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;
                featuredGameCollection.Indexes.CreateOne(Builders<FeaturedGame>.IndexKeys.Ascending(_ => _.gameId), cio);
            }
        }

        public bool insertGame(RecentGame game)
        {
            try
            {
                gameCollection.InsertOne(game);
                return true;
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    //Comment out for now, too much spam
                    //Console.WriteLine("Error adding Game with GameID: [" + game.gameId + "]. Duplicate Game already exists.");
                }
                else
                {
                    Console.WriteLine($"Unhandled exception inserting Game with GameID: [{game.gameId}]. {e}");
                    throw e;
                }
            }

            return false;
        }

        public void DeleteRecentGame(long gameId)
        {
            var filter = Builders<RecentGame>.Filter.Eq("gameId", gameId);
            gameCollection.DeleteOne(filter);
        }

        public List<RecentGame> GetShortGames()
        {
            var filter = Builders<RecentGame>.Filter.Lt(g => g.stats.timePlayed, 900);
            var sort = Builders<RecentGame>.Sort.Ascending("championId").Ascending("summonerId").Ascending("gameId");

            var games = gameCollection.Find(filter).Sort(sort).ToList();
            return games;
        }

        public int insertGames(List<RecentGame> games)
        {
            int numAdded = 0;
            foreach(RecentGame game in games)
            {
                if (insertGame(game))
                {
                    numAdded++;
                }
            }

            return numAdded;
        }

        public List<RecentGame> getAllRecentGames()
        {
            var sort = Builders<RecentGame>.Sort.Ascending("championId").Ascending("summonerId").Ascending("gameId");

            var games = gameCollection.Find(Builders<RecentGame>.Filter.Empty).Sort(sort).ToList();
            return games;
        }

        public List<RecentGame> getRecentGamesForPlayer(long summonerId)
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Eq("summonerId", summonerId);

            var games = gameCollection.Find(filter).ToList();

            return games;
        }

        public List<RecentGame> GetRecentGamesForChampion(int championId)
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Eq("championId", championId);

            return gameCollection.Find(filter).ToList();
        }

        public List<RecentGame> GetRecentGamesForChampionAndSummoner(int championId, long summonerId)
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Eq("championId", championId) &
                            builder.Eq("summonerId", summonerId);

            return gameCollection.Find(filter).ToList();
        }

        public List<RecentGame> GetFlaggedRecentGames()
        {
            var filter = Builders<RecentGame>.Filter.Eq("hasMatch", true);

            var games = gameCollection.Find(filter).ToList();
            return games;
        }

        public List<RecentGame> GetUnflaggedRecentGames()
        {
            var filter = Builders<RecentGame>.Filter.Ne("hasMatch", true);

            var games = gameCollection.Find(filter).ToList();
            return games;
        }

        public void FlagRecentGame(long gameId)
        {
            var filter = Builders<RecentGame>.Filter.Eq("gameId", gameId);
            var update = Builders<RecentGame>.Update.Set("hasMatch", true);

            List<RecentGame> games = gameCollection.Find(filter).ToList();

            UpdateResult result = gameCollection.UpdateMany(filter, update);
        }

        public void insertMatch(MatchDetails match)
        {
            try
            {
                matchCollection.InsertOne(match);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    //Comment out for now, too much spam
                    //Console.WriteLine("Error adding Match with matchId: [" + match.matchId + "]. Duplicate Game already exists.");
                }
                else
                {
                    Console.WriteLine($"Unhandled exception inserting Match with matchId: [{ match.matchId}]. {e}");
                }
            }
        }

        public MatchDetails GetMatch(long matchId)
        {
            var matches = matchCollection.Find(Builders<MatchDetails>.Filter.Eq("matchId", matchId)).ToList();

            if (matches.Count > 0)
            {
                var match = matches.ElementAt(0);
                return match;
            }

            return null;
        }

        public long getMatchCount()
        {
            return matchCollection.Count(Builders<MatchDetails>.Filter.Empty);
        }

        public void insertFeaturedGames(List<FeaturedGame> games)
        {
            foreach (FeaturedGame game in games)
            {
                insertFeaturedGame(game);
            }
        }

        public void insertFeaturedGame(FeaturedGame game)
        {
            try
            {
                featuredGameCollection.InsertOne(game);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    //Comment out for now, too much spam
                    //Console.WriteLine("Error adding Match with matchId: [" + match.matchId + "]. Duplicate Game already exists.");
                }
                else
                {
                    Console.WriteLine($"Unhandled exception inserting Featured Game with gameId: [{game.gameId}]. {e}");
                    throw e;
                }
            }
        }

        public long getFeaturedGameCount()
        {
            return featuredGameCollection.Count(Builders<FeaturedGame>.Filter.Empty);
        }

        public List<FeaturedGame> GetFeaturedGamesForChampion(int championId)
        {
            var builder = Builders<FeaturedGame>.Filter;
            var filter = builder.ElemMatch(g => g.participants, g => g.championId == championId) &
                            builder.Eq("hasMatch", true);

            return featuredGameCollection.Find(filter).ToList();
        }

        public void deleteFeaturedGame(long gameId)
        {
            var filter = Builders<FeaturedGame>.Filter.Eq("gameId", gameId);
            featuredGameCollection.DeleteOne(filter);
        }

        public void FlagFeaturedGame(long gameId)
        {
            var filter = Builders<FeaturedGame>.Filter.Eq("gameId", gameId);
            var update = Builders<FeaturedGame>.Update.Set("hasMatch", true);

            UpdateResult result = featuredGameCollection.UpdateMany(filter, update);
        }

        public bool isCorrectToCheckIfFeaturedGame(long gameStart, long gameEnd)
        {
            var builder = Builders<FeaturedGame>.Filter;
            var filter = builder.Gt(g => g.gameStartTime, gameStart) 
                            & builder.Lt(g => g.gameStartTime, gameEnd);

            return featuredGameCollection.Find(filter).ToList().Count > 0;
        }

        public bool isFeaturedGame(long gameId)
        {
            var filter = Builders<FeaturedGame>.Filter.Eq("gameId", gameId);

            return featuredGameCollection.Find(filter).ToList().Count > 0;
        }

        public List<FeaturedGame> GetUnflaggedFeaturedGames()
        {
            var builder = Builders<FeaturedGame>.Filter;
            var filter = builder.Ne("hasMatch", true);

            var games = featuredGameCollection.Find(filter).ToList();
            return games;
        }
    }
}
