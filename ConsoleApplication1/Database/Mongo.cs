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
        private IMongoCollection<Game> gameCollection;
        private IMongoCollection<MatchDetails> matchCollection;
        private IMongoCollection<FeaturedGame> featuredGameCollection;

        public Mongo()
        {
            theClient = new MongoClient("mongodb://localhost:27017");
            theDataBase = theClient.GetDatabase("Riot_1");
            gameCollection = theDataBase.GetCollection<Game>("Game_1");
            matchCollection = theDataBase.GetCollection<MatchDetails>("Match_1");
            featuredGameCollection = theDataBase.GetCollection<FeaturedGame>("FeaturedGame_1");

            handleIndexes();
        }

        public void dropAll()
        {
            gameCollection.DeleteMany(FilterDefinition<Game>.Empty);
        }

        private void handleIndexes()
        {
            //Add gameId as a Unique Index (Primary Key)
            if (gameCollection.Indexes.List().ToList().Count == 0)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;

                IndexKeysDefinition<Game> gameId = Builders<Game>.IndexKeys.Ascending(_ => _.gameId);
                IndexKeysDefinition<Game> summonerId = Builders<Game>.IndexKeys.Ascending(_ => _.summonerId);

                gameCollection.Indexes.CreateOne(Builders<Game>.IndexKeys.Combine(gameId, summonerId), cio);
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

        public bool insertGame(Game game)
        {
            try
            {
                gameCollection.InsertOne(game);
                return true;
            }
            catch (MongoWriteException e)
            {
                if(e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    //Comment out for now, too much spam
                    //Console.WriteLine("Error adding Game with GameID: [" + game.gameId + "]. Duplicate Game already exists.");
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

        public Game getGame(long gameId)
        {
            List<Game> games = gameCollection.Find(Builders<Game>.Filter.Eq("gameId", gameId)).ToList();
            var game = games.ElementAt(0);
            return game;
        }

        public List<Game> getAllGames()
        {
            var games = gameCollection.Find(Builders<Game>.Filter.Empty).ToList();
            return games;
        }

        public List<Game> getARAMGames()
        {
            var sort = Builders<Game>.Sort.Ascending("championId").Ascending("summonerId").Ascending("gameId");

            var games = gameCollection.Find(Builders<Game>.Filter.Eq("gameMode", "ARAM")).Sort(sort).ToList();
            return games;
        }

        public List<Game> getARAMGamesForPlayer(long summonerId)
        {
            var builder = Builders<Game>.Filter;
            var filter = builder.Eq("summonerId", summonerId) &
                         builder.Eq("gameMode", "ARAM");

            var games = gameCollection.Find(filter).ToList();

            return games;
        }

        public List<string> getFeaturedPlayers()
        {
            List<String> names = new List<String>();

            List<FeaturedGame> games = featuredGameCollection.Find(Builders<FeaturedGame>.Filter.Empty).ToList();

            foreach (FeaturedGame game in games)
            {
                foreach (FeaturedGameParticipant participant in game.participants)
                {
                    if (!names.Contains(participant.summonerName))
                    {
                        names.Add(participant.summonerName);
                    }
                }
            }

            return names;
        }

        public long getCount()
        {
            return gameCollection.Count(Builders<Game>.Filter.Empty);
        }

        public void addMatch(MatchDetails match)
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
                    Console.WriteLine("Unhandled exception inserting Match with matchId: [" + match.matchId + "]." + e);
                }
            }
        }

        public MatchDetails getMatch(long matchId)
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
                    Console.WriteLine("Unhandled exception inserting Featured Game with gameId: [" + game.gameId + "]." + e);
                }
            }
        }

        public long getFeaturedGameCount()
        {
            return featuredGameCollection.Count(Builders<FeaturedGame>.Filter.Empty);
        }

        public List<FeaturedGame> getFeaturedGamesForChampion(int championId)
        {
            var filter = Builders<FeaturedGame>.Filter.ElemMatch(g => g.participants, g => g.championId == championId);
            return featuredGameCollection.Find(filter).ToList();
        }

        public List<Game> getGamesForChampion(int championId)
        {
            var filter = Builders<Game>.Filter.Eq("championId", championId);
            return gameCollection.Find(filter).ToList();
        }
    }
}
