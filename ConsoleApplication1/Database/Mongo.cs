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
    public class Mongo
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

        public List<RecentGame> GetRecentGamesForChampionAndSummoner(int championId, long summonerId)
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Eq("championId", championId) &
                            builder.Eq("summonerId", summonerId) &
                            builder.Eq("gameMode", "ARAM");

            return gameCollection.Find(filter).ToList();
        }

        public List<RecentGame> GetRecentGamesSummoner(int championId, long summonerId)
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Eq("championId", championId) &
                            builder.Eq("summonerId", summonerId) &
                            builder.Eq("gameMode", "ARAM");

            return gameCollection.Find(filter).ToList();
        }

        public List<RecentGame> GetRecentGamesSummoner(long summonerId)
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Eq("summonerId", summonerId) &
                            builder.Eq("gameMode", "ARAM");

            return gameCollection.Find(filter).ToList();
        }

        public List<RecentGame> GetFlaggedRecentGames()
        {
            var filter = Builders<RecentGame>.Filter.Eq("hasMatch", true);

            var games = gameCollection.Find(filter).ToList();
            return games;
        }

        //Recent games have a mapping of summoner id to champion id
        //Match details do not contain summoner id's (riot privacy)
        //Therefore we use Recent Games to inject summoner id into match details
        //This is very DB intensive, so after we inject the id, we mark the recent game as to not try injecting again
        public List<RecentGame> GetRecentGamesWithUninjectedSummonerId()
        {
            var builder = Builders<RecentGame>.Filter;
            var filter = builder.Ne("injectedSummonerId", true) &
                            builder.Eq("gameMode", "ARAM");

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

        public void InjectSummonerIntoMatch(long matchId, int championId, long summonerId)
        {
            MatchDetails match = GetMatch(matchId);

            if (match == null)
            {
                return;
            }

            List<Participant> participants = match.participants;

            foreach (Participant p in participants)
            {
                if (p.championId == championId)
                {
                    p.summonerId = summonerId;
                    break;
                }
            }

            //Flag the recent game so that we dont bother trying to do this again
            var gameBuilder = Builders<RecentGame>.Filter;
            var gameFilter = gameBuilder.Eq("gameId", matchId) &
                            gameBuilder.Eq("summonerId", summonerId);

            var gameUpdate = Builders<RecentGame>.Update.Set("injectedSummonerId", true);
            gameCollection.UpdateOne(gameFilter, gameUpdate);

            //Inject summonerId into MatchDetails for easier data manipulation later
            var matchFilter = Builders<MatchDetails>.Filter.Eq("matchId", matchId);
            var matchUpdate = Builders<MatchDetails>.Update.Set("participants", participants);
            matchCollection.UpdateOne(matchFilter, matchUpdate);
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

        public List<MatchDetails> GetARAMWithPlayer(long summonerId)
        {
            var builder = Builders<MatchDetails>.Filter;
            var filter = builder.ElemMatch(g => g.participants, g => g.summonerId == summonerId) &
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

        public bool IsFeaturedGameRecorded(long gameStart, long gameEnd)
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
