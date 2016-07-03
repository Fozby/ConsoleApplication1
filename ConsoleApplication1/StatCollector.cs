using ConsoleApplication1.Database;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.GoogleAPI.DataObjects;
using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApplication1
{
    class StatCollector
    {
        private Riot riot;
        private Mongo mongo;
        private GoogleSheets google;
        private CompetitiveStatsBuilder converter = new CompetitiveStatsBuilder();

        public StatCollector(Mongo mongo, Riot riot, GoogleSheets google)
        {
            this.riot = riot;
            this.mongo = mongo;
            this.google = google;
        }

        public void CollectFeaturedGames()
        {
            Console.WriteLine("Querying featured games...");
            //while (true)
            //{
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
                Console.WriteLine($"[{DateTime.Now.ToString()}] Total featured games: {mongo.getFeaturedGameCount()}");
            //    Thread.Sleep(300000);
            //}
        }

        public void CollectRecentGames()
        {
            List<RecentGame> recentGames = new List<RecentGame>();

            try
            {
                recentGames = riot.getRecentGamesForAllPlayers();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception when querying for recent games: {e}");
                throw e;
            }

            int numAdded = mongo.insertGames(recentGames);
            Console.WriteLine($"Collected {numAdded} games. Total Recent Games: {mongo.getAllRecentGames().Count}");
        }

        public void PrintRatings()
        {
            Dictionary<string, string> ratings = new Dictionary<string, string>();

            foreach (long summonerId in Global.players.Keys)
            {
                List<RecentGame> games = mongo.getRecentGamesForPlayer(summonerId);

                long numGames = games.Count;
                long numFeatured = 0;

                long numValidGames = 0;

                foreach (RecentGame game in games)
                {
                    long gameId = game.gameId;

                    MatchDetails match = mongo.GetMatch(gameId);
                    
                    long start = game.createDate;
                    long end = start + (match.matchDuration * 1000); //match duration is in seconds, create date is milliseconds

                    if (mongo.isCorrectToCheckIfFeaturedGame(start, end))// && game.IsSolo())
                    {
                        numValidGames++;

                        if (mongo.isFeaturedGame(gameId))
                        {
                            numFeatured++;
                        }
                    }
                }

                double featuredPercent = (double)numFeatured / numValidGames;
                string rating = $"{numFeatured} / {numValidGames} ({numGames}) {String.Format("{0:0.00}", featuredPercent)}%";
                ratings.Add(Global.GetPlayerName(summonerId), rating);
            }

            foreach(KeyValuePair<string, string> kvp in ratings)
            {
                Console.WriteLine($"{kvp.Key} : {kvp.Value}");
            }
        }

        public void UploadCompetitiveChampionStats()
        {
            List<CompetitiveStats> statsList = new List<CompetitiveStats>();

            Console.WriteLine($"Starting {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            foreach (int championId in Global.champions.Keys)
            {
                statsList.Add(BuildCompetitiveChampionStats(championId));
            }
            Console.WriteLine($"Finished {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");


            statsList = statsList.OrderByDescending(c => c.totalCompetitiveGames).ToList();

            google.AddCompetitiveChampionStats2(statsList);

        }

        public CompetitiveStats BuildCompetitiveChampionStats(int championId)
        {
            MatchCollection matchCollection = GetMatchesWithChampion(championId);

            if (matchCollection.Count > 0)
            {
                ChampionStats cStats = mongo.GetChampionStats(championId, matchCollection.Count);

                if (cStats == null)
                {
                    cStats = converter.buildChampionStats(championId, matchCollection);
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
                        PlayerChampionStats pStats = mongo.GetPlayerChampionStats(summonerId, championId, playerMatches.Count);

                        if (pStats == null)
                        {
                            pStats = converter.buildPlayerStats(summonerId, championId, playerMatches);
                            mongo.InsertPlayerChampionStats(pStats);
                        }

                        pStatsList.Add(pStats);
                    }

                }

                return new CompetitiveStats(cStats, pStatsList);
            }

            return null;
        }


        private MatchCollection GetMatchesWithChampion(int championId)
        {
            List<MatchDetails> matchesWithChampion = new List<MatchDetails>();

            List<FeaturedGame> featuredGames = mongo.GetFeaturedGamesForChampion(championId);
            foreach (FeaturedGame featuredGame in featuredGames)
            {
                MatchDetails match = mongo.GetMatch(featuredGame.gameId);

                if (match!= null && !matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }
            }
            
            List<RecentGame> games = mongo.GetRecentGamesForChampion(championId);

            foreach (RecentGame game in games)
            {
                MatchDetails match = mongo.GetMatch(game.gameId);

                if (match != null &&!matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }
            }

            matchesWithChampion.RemoveAll(m => !m.IsValid());

            return new MatchCollection(matchesWithChampion);
        }
    }
}
