using ConsoleApplication1.Database;
using ConsoleApplication1.GoogleAPI;
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
        private MatchConverter converter = new MatchConverter();

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

        public void UploadPlayerStats()
        {
            google.ClearPlayerStats();
            foreach (long summonerId in Global.players.Keys)
            {
                List<RecentGame> statGames = mongo.getRecentGamesForPlayer(summonerId);
                PlayerStats stats = converter.BuildPlayerStats(BuildGameStats(statGames));
                google.AddPlayerStats(stats);
            }
        }

        public void UploadChampionStats()
        {
            foreach (int championId in Global.champions.Keys)
            {
                List<MatchDetails> matches = GetMatchesWithChampion(championId);

                if (matches.Count > 0)
                {
                    ChampionStats stats = converter.BuildChampionStats(championId, matches);
                    google.AddChampionStats(stats);
                }

                Console.WriteLine($"{matches.Count} found for champion {championId} aka {Global.getChampionName(championId)}");
            }
        }

        public void UploadCompetitiveChampionStats()
        {
            List<ComparativeChampionStats> statsList = new List<ComparativeChampionStats>();

            foreach (int championId in Global.champions.Keys)
            {
                List<MatchDetails> matches = GetMatchesWithChampion(championId);

                matches.RemoveAll(m => !m.IsValid());

                if (matches.Count > 0)
                {
                    Dictionary<long, List<GameStats>> playerStats = new Dictionary<long, List<GameStats>>(); //<SummonerID, [List of Recent Games]

                    foreach (int summonerId in Global.players.Keys)
                    {
                        List<RecentGame> recentGames = mongo.GetRecentGamesForChampionAndSummoner(championId, summonerId);

                        recentGames.RemoveAll(g =>
                        {
                            long gameId = g.gameId;
                            MatchDetails match = mongo.GetMatch(gameId);

                            if (match == null || !match.IsValid())
                            {
                                return true;
                            }

                            return false;
                        });

                        if (recentGames.Count > 0)
                        {
                            playerStats.Add(summonerId, BuildGameStats(recentGames));
                        }
                    }

                    ComparativeChampionStats stats = converter.BuildCompetitiveChampionStats(championId, matches, playerStats);

                    statsList.Add(stats);
                }
            }

            statsList = statsList.OrderByDescending(c => c.getNumFriendlyGames()).ToList();

            google.ClearCompetitiveStats();
            foreach (ComparativeChampionStats stats in statsList)
            {
                google.AddCompetitiveChampionStats(stats);
            }

        }

        private List<MatchDetails> GetMatchesWithChampion(int championId)
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

            return matchesWithChampion;
        }

        private List<GameStats> BuildGameStats(List<RecentGame> games)
        {
            List<GameStats> gameRows = new List<GameStats>();

            foreach (RecentGame game in games)
            {
                MatchDetails match = mongo.GetMatch(game.gameId);
                gameRows.Add(converter.BuildGameStats(game, match));
            }

            return gameRows;
        }
    }
}
