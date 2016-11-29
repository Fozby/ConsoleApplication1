using ConsoleApplication1.Database;
using ConsoleApplication1.Database.Exceptions;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.GoogleAPI.DataObjects;
using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.Objects;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace ConsoleApplication1
{
    class StatCollector
    {
        private Riot riot;
        private Mongo mongo;
        private GoogleSheets google;
        private CompetitiveStatsBuilder competitiveStatsBuilder = new CompetitiveStatsBuilder();
        private PlayerStatsBuilder playerStatsBuilder;
        public StatCollector(Mongo mongo, Riot riot, GoogleSheets google)
        {
            this.riot = riot;
            this.mongo = mongo;
            this.google = google;

            this.playerStatsBuilder = new PlayerStatsBuilder(mongo);
        }

        public void CollectFeaturedGames()
        {        
            List<FeaturedGame> featuredGames = new List<FeaturedGame>();

            try
            {
                featuredGames = riot.getFeaturedGames();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception when querying for featured games. {e}");
            }

            if (featuredGames.Count > 0)
            {
                mongo.insertFeaturedGames(featuredGames);
            }
            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Total featured games: {mongo.getFeaturedGameCount()}");

        }

        public void CollectRecentGames()
        {
            List<RecentGame> recentGames = new List<RecentGame>();

            try
            {
                recentGames = riot.getRecentGames(Global.players.Keys.ToList());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception when querying for recent games: {e}");
                throw e;
            }

            int numAdded = mongo.insertGames(recentGames);
            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Collected {numAdded} recent games. Total Recent Games: {mongo.getAllRecentGames().Count}");
        }

        public void UploadPlayerStats()
        {
            List<PlayerStats> playerStatsList = new List<PlayerStats>();

            foreach (int summonerId in Global.players.Keys)
            {
                List<MatchDetails> matches = mongo.GetARAMWithPlayer(summonerId);
                matches.RemoveAll(m => !m.IsValid());

                PlayerStats pStats = playerStatsBuilder.buildPlayerStats(summonerId, new MatchCollection(matches));
                playerStatsList.Add(pStats);
            }

            google.AddPlayerStats(playerStatsList);
        }

        public void UploadCompetitiveChampionStats()
        {
            List<CompetitiveStats> statsList = new List<CompetitiveStats>();

            List<MatchDetails> matches = mongo.GetAramMatches();
            matches.RemoveAll(m => !m.IsValid());


            foreach (int championId in Global.champions.Keys)
            {
                List<MatchDetails> championMatches = matches.FindAll(m => m.participants.Find(p => p.championId == championId) != null);

                MatchCollection mc = new MatchCollection(championMatches);

                CompetitiveStats stats = BuildCompetitiveChampionStats(championId, mc);

                //If no matches for a champion, null is returned
                if (stats != null)
                {
                    statsList.Add(stats);
                }

            }

            statsList = statsList.OrderByDescending(c => c.totalCompetitiveGames).ToList();

            int reduction = 30;
            int count = statsList.Count();
            statsList.RemoveRange(count - reduction, reduction);

            String s = Newtonsoft.Json.JsonConvert.SerializeObject(statsList);
            int size = s.Length;
            Console.WriteLine("Packet size: " + size);

            google.AddCompetitiveChampionStats(statsList);
        }

        public CompetitiveStats BuildCompetitiveChampionStats(int championId, MatchCollection matchCollection)
        {
            if (matchCollection.matches.Count > 0)
            {
                ChampionStats cStats;

                try
                {
                    cStats = mongo.GetChampionStats(championId, matchCollection.Count);
                }
                catch (CacheNotFoundException e)
                {
                    cStats = competitiveStatsBuilder.buildChampionStats(championId, matchCollection);
                    mongo.InsertChampionStats(cStats);
                }
                catch (StaleCacheException e)
                {
                    cStats = competitiveStatsBuilder.buildChampionStats(championId, matchCollection);

                    mongo.DeleteChampionStatsCache(championId);
                    mongo.InsertChampionStats(cStats);
                }

                List<PlayerChampionStats> pStatsList = new List<PlayerChampionStats>();
                foreach (int summonerId in Global.players.Keys)
                {
                    List<RecentGame> recentGames = mongo.GetRecentGamesForChampionAndSummoner(championId, summonerId);

                    List<long> recentGameIds = recentGames.ConvertAll<long>(g => g.gameId);

                    MatchCollection playerMatches = matchCollection.FindAll(recentGameIds);

                    if (playerMatches.Count > 0)
                    {
                        PlayerChampionStats pStats;

                        try
                        {
                            pStats = mongo.GetPlayerChampionStats(summonerId, championId, playerMatches.Count);
                        }
                        catch (CacheNotFoundException e)
                        {
                            pStats = competitiveStatsBuilder.buildPlayerChampionStats(summonerId, championId, playerMatches);
                            mongo.InsertPlayerChampionStats(pStats);
                        }
                        catch (StaleCacheException e)
                        {
                            pStats = competitiveStatsBuilder.buildPlayerChampionStats(summonerId, championId, playerMatches);

                            mongo.DeletePlayerChampionStatsCache(summonerId, championId);
                            mongo.InsertPlayerChampionStats(pStats);
                        }

                        pStatsList.Add(pStats);
                    }

                }

                return new CompetitiveStats(cStats, pStatsList);
            }

            return null;
        }
    }
}
