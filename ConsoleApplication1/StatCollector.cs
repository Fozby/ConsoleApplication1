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
            Console.WriteLine("Starting 5 minute timer to query featured games...");
            while (true)
            {
                List<FeaturedGame> featuredGames = new List<FeaturedGame>();

                try
                {
                    featuredGames = riot.getFeaturedGames();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception when querying for featured games. {e}");
                    throw e;
                }

                if (featuredGames.Count > 0)
                {
                    mongo.insertFeaturedGames(featuredGames);

                    foreach (FeaturedGame featuredGame in featuredGames)
                    {
                        try
                        {
                            MatchDetails match = riot.getMatch(featuredGame.gameId);
                            mongo.insertMatch(match);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Exception adding match for featured game {featuredGame.gameId}. {e}");
                            throw e;
                        }
                    }
                }
                Console.WriteLine($"[{DateTime.Now.ToString()}] Total featured games: {mongo.getFeaturedGameCount()}");
                Thread.Sleep(300000);
            }
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

        public void UploadPlayerStats()
        {
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

                if (matches.Count > 0)
                {
                    Dictionary<long, List<GameStats>> playerStats = new Dictionary<long, List<GameStats>>(); //<SummonerID, [List of Recent Games]

                    long totalFriendGames = 0;
                    foreach (int summonerId in Global.players.Keys)
                    {
                        List<RecentGame> recentGames = mongo.GetRecentGamesForChampionAndSummoner(championId, summonerId);

                        if (recentGames.Count > 0)
                        {
                            playerStats.Add(summonerId, BuildGameStats(recentGames));
                        }

                        totalFriendGames += recentGames.Count;
                    }

                    ComparativeChampionStats stats = converter.BuildCompetitiveChampionStats(championId, matches, playerStats);

                    statsList.Add(stats);
                }
            }

            statsList = statsList.OrderByDescending(c => c.getNumFriendlyGames()).ToList();

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

                if (!matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }
            }

            List<RecentGame> games = mongo.GetRecentGamesForChampion(championId);

            foreach (RecentGame game in games)
            {
                MatchDetails match = mongo.GetMatch(game.gameId);

                if (!matchesWithChampion.Contains(match))
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
