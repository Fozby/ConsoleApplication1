using ConsoleApplication1.GoogleAPI.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using ConsoleApplication1.Database.GoogleCache;
using MongoDB.Bson;
using ConsoleApplication1.Database.Exceptions;

namespace ConsoleApplication1.Database
{
    class Mongo
    { 
        private MongoClient theClient;
        private IMongoDatabase theDataBase;
        private IMongoCollection<RecentGame> gameCollection;
        private IMongoCollection<MatchDetails> matchCollection;
        private IMongoCollection<FeaturedGame> featuredGameCollection;

        //Google entity caches
        private IMongoCollection<ChampionStatsCache> championStatsCollection;
        private IMongoCollection<PlayerChampionStatsCache> playerChampionStatsCollection;

        public Mongo()
        {
            theClient = new MongoClient("mongodb://localhost:27017");
            theDataBase = theClient.GetDatabase("Riot_1");
            gameCollection = theDataBase.GetCollection<RecentGame>("Game_1");
            matchCollection = theDataBase.GetCollection<MatchDetails>("Match_1");
            featuredGameCollection = theDataBase.GetCollection<FeaturedGame>("FeaturedGame_1");
            championStatsCollection = theDataBase.GetCollection<ChampionStatsCache>("ChampionStatsCollection_1");
            playerChampionStatsCollection = theDataBase.GetCollection<PlayerChampionStatsCache>("PlayerChampionStatsCollection_1");

            handleIndexes();
        }

        private void handleIndexes()
        {
            //Add gameId as a Unique Index (Primary Key)
            if (gameCollection.Indexes.List().ToList().Count == 1)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;

                IndexKeysDefinition<RecentGame> gameId = Builders<RecentGame>.IndexKeys.Ascending(_ => _.gameId);
                IndexKeysDefinition<RecentGame> summonerId = Builders<RecentGame>.IndexKeys.Ascending(_ => _.summonerId);

                gameCollection.Indexes.CreateOne(Builders<RecentGame>.IndexKeys.Combine(gameId, summonerId), cio);
            }

            //Add matchId as a Unique Index (Primary Key)
            if (matchCollection.Indexes.List().ToList().Count == 2) 
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;
                matchCollection.Indexes.CreateOne(Builders<MatchDetails>.IndexKeys.Ascending(_ => _.matchId), cio);


                cio = new CreateIndexOptions();
                IndexKeysDefinition<MatchDetails> championIds = Builders<MatchDetails>.IndexKeys.Ascending("participants.championId");
                IndexKeysDefinition<MatchDetails> matchMode = Builders<MatchDetails>.IndexKeys.Ascending(_ => _.matchMode);

                matchCollection.Indexes.CreateOne(Builders<MatchDetails>.IndexKeys.Combine(championIds, matchMode), cio);
            }
 
            //Add gameId as a Unique Index (Primary Key)
            if (featuredGameCollection.Indexes.List().ToList().Count == 1)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;
                featuredGameCollection.Indexes.CreateOne(Builders<FeaturedGame>.IndexKeys.Ascending(_ => _.gameId), cio);
            }

           
            //looks like mongodb has 1 index by default that cant be dropped, so count == 1 ... maybe not?
            if (championStatsCollection.Indexes.List().ToList().Count == 1)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;

                ChampionStatsCache cache = new ChampionStatsCache();
                ChampionStats stats = new ChampionStats("a");

                championStatsCollection.Indexes.CreateOne(Builders<ChampionStatsCache>.IndexKeys.Ascending(_ => _.championStats.championName), cio);
            }


            if (playerChampionStatsCollection.Indexes.List().ToList().Count == 1)
            {
                CreateIndexOptions cio = new CreateIndexOptions();
                cio.Unique = true;

                IndexKeysDefinition<PlayerChampionStatsCache> championName = Builders<PlayerChampionStatsCache>.IndexKeys.Ascending(_ => _.playerStats.championStats.championName);
                IndexKeysDefinition<PlayerChampionStatsCache> summonerName = Builders<PlayerChampionStatsCache>.IndexKeys.Ascending(_ => _.playerStats.summonerName);

                playerChampionStatsCollection.Indexes.CreateOne(Builders<PlayerChampionStatsCache>.IndexKeys.Combine(championName, summonerName), cio);
            }

        }

        public void ClearCache()
        {
            championStatsCollection.DeleteMany(Builders<ChampionStatsCache>.Filter.Empty);
            playerChampionStatsCollection.DeleteMany(Builders<PlayerChampionStatsCache>.Filter.Empty);
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

        public RecentGame GetRecentGame(long gameId)
        {
            var filter = Builders<RecentGame>.Filter.Eq("gameId", gameId);

            return gameCollection.Find(filter).First();
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

        public List<MatchDetails> GetMatchesWithChampion(int championId)
        {
            var filter = Builders<MatchDetails>.Filter.ElemMatch(match => match.participants, participant => participant.championId == championId);

            return matchCollection.Find(filter).ToList();
        }

        public List<MatchDetails> GetAramMatches()
        {
           return matchCollection.Find(Builders<MatchDetails>.Filter.Eq("matchMode", "ARAM")).ToList();

        }

        public List<MatchDetails> GetARAMWithChampion(int championId)
        {
            var builder = Builders<MatchDetails>.Filter;
            var filter = builder.ElemMatch(g => g.participants, g => g.championId == championId) &
                            builder.Eq("matchMode", "ARAM");

            return matchCollection.Find(filter).ToList();
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

        public bool InsertChampionStats(ChampionStats stats)
        {
            try
            {
                ChampionStatsCache cache = new ChampionStatsCache(stats);

                championStatsCollection.InsertOne(cache);

                return true;
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    //Comment out for now, too much spam
                    //Console.WriteLine("Error adding record");
                }
                else
                {
                    //Console.WriteLine($"Unhandled exception inserting Cache for champion: [{game.gameId}]. {e}");
                    throw e;
                }
            }

            return false;
        }

        public ChampionStats GetChampionStats(int championId, int count)
        {
            ChampionStatsCache cache = GetChampionStatsCache(championId);

            if (cache == null)
            {
                throw new CacheNotFoundException("no cache");
            }

            if (count > cache.edition)
            {

                throw new StaleCacheException("stale cache");
            }

            return cache.championStats;
        }

        public void DeleteChampionStatsCache(int championId)
        {
            string championName = Global.getChampionName(championId);

            var filter = Builders<ChampionStatsCache>.Filter.Eq("championStats.championName", championName);

            championStatsCollection.DeleteOne(filter);
        }

        private ChampionStatsCache GetChampionStatsCache(int championId)
        {
            string championName = Global.getChampionName(championId);

            var filter = Builders<ChampionStatsCache>.Filter.Eq("championStats.championName", championName);

            return championStatsCollection.Find(filter).FirstOrDefault();
        }

        public bool InsertPlayerChampionStats(PlayerChampionStats stats)
        {
            try
            {
                PlayerChampionStatsCache cache = new PlayerChampionStatsCache(stats);

                playerChampionStatsCollection.InsertOne(cache);

                return true;
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    //Comment out for now, too much spam
                    //Console.WriteLine("Error adding record");
                }
                else
                {
                    throw e;
                }
            }

            return false;
        }

        public PlayerChampionStats GetPlayerChampionStats(int summonerId, int championId, int count)
        {
            PlayerChampionStatsCache cache = GetPlayerChampionStatsCache(summonerId, championId);

            if (cache == null)
            {
                throw new CacheNotFoundException("no cache");
            }

            if (count > cache.edition)
            {
                throw new StaleCacheException("stale cache");
            }

            return cache.playerStats;
        }

        public void DeletePlayerChampionStatsCache(int summonerId, int championId)
        {
            string championName = Global.getChampionName(championId);
            string summonerName = Global.GetPlayerName(summonerId);

            var builder = Builders<PlayerChampionStatsCache>.Filter;
            var filter = builder.Eq("playerStats.summonerName", summonerName)
                            & builder.Eq("playerStats.championStats.championName", championName);

            playerChampionStatsCollection.DeleteOne(filter);
        }

        private PlayerChampionStatsCache GetPlayerChampionStatsCache(int summonerId, int championId)
        {
            string championName = Global.getChampionName(championId);
            string summonerName = Global.GetPlayerName(summonerId);

            var builder = Builders<PlayerChampionStatsCache>.Filter;
            var filter = builder.Eq("playerStats.summonerName", summonerName)
                            & builder.Eq("playerStats.championStats.championName", championName);

            return playerChampionStatsCollection.Find(filter).FirstOrDefault();
        }
    }
}
