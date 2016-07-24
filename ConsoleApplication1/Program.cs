using System;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.Database;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System.Collections.Generic;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using Newtonsoft.Json;
using System.Threading;
using System.Timers;
using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.GoogleAPI.DataObjects;
using ConsoleApplication1.Database.GoogleCache;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using ConsoleApplication1.RiotAPI.Exceptions;

namespace ConsoleApplication1
{
    class Program
    {
        static Riot riot = new Riot();
        static Mongo mongo = new Mongo();
        static GoogleSheets google = new GoogleSheets();
        static CompetitiveStatsBuilder converter = new CompetitiveStatsBuilder();
        static StatCollector collector = new StatCollector(mongo, riot, google);

        static void Main(string[] args)
        {
            mongo.ClearCache();

            //Console.ReadLine();
            Global.loadChampions(riot.getChampions()); //TODO store in mongodb to avoid an unnessary call

            double ONE_HOUR_TIMER = 60 * 60 * 1000;
            double FIVE_MINUTE_TIMER = 5 * 60 * 1000;
            double FIVE_HOUR_TIMER = 5 * 60 * 60 * 1000;

            System.Timers.Timer recentGamesTimer = new System.Timers.Timer();
            recentGamesTimer.Elapsed += new ElapsedEventHandler(CollectRecentGames);
            recentGamesTimer.Interval = ONE_HOUR_TIMER;
            recentGamesTimer.Enabled = true;

            System.Timers.Timer featuredGamesTimer = new System.Timers.Timer();
            featuredGamesTimer.Elapsed += new ElapsedEventHandler(CollectFeaturedGames);
            featuredGamesTimer.Interval = FIVE_MINUTE_TIMER;
            featuredGamesTimer.Enabled = true;

            System.Timers.Timer uploadTimer = new System.Timers.Timer();
            uploadTimer.Elapsed += new ElapsedEventHandler(UploadData);
            uploadTimer.Interval = FIVE_HOUR_TIMER;
            uploadTimer.Enabled = true;

            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Starting download for new Recent and Featured Games");
            collector.CollectRecentGames();
            collector.CollectFeaturedGames();
            Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Performing data cleanup");
            Cleanup();

            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Uploading data to google sheets");
            collector.UploadPlayerStats();
            collector.UploadCompetitiveChampionStats();

            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Starting periodic data retrieval timer");
            recentGamesTimer.Start();
            featuredGamesTimer.Start();
            uploadTimer.Start();
            Console.ReadLine();
        }

        private static void CollectRecentGames(object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Automatic task to download Recent games");
            collector.CollectRecentGames();
            Cleanup();
        }

        private static void CollectFeaturedGames(object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Automatic task to download Featured games");
            collector.CollectFeaturedGames();
        }

        private static void UploadData(object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Automatic task to upload data");
            collector.UploadCompetitiveChampionStats();
            Cleanup();
        }

        static void Cleanup()
        {
            CleanupRecentGames();
            CleanupFeaturedGames();
            AddSummonerIdsToMatches();
        }

        static void AddSummonerIdsToMatches()
        {
            List<RecentGame> flaggedGames = mongo.GetRecentGamesWithUninjectedSummonerId();

            foreach(RecentGame game in flaggedGames)
            {
                long gameId = game.gameId;
                int championId = game.championId;
                long summonerId = game.summonerId;

                mongo.InjectSummonerIntoMatch(gameId, championId, summonerId);
            }
        }

        static void CleanupRecentGames()
        {
            List<RecentGame> recentGames = mongo.GetUnflaggedRecentGames();

            foreach (RecentGame recentGame in recentGames)
            {
                MatchDetails match = mongo.GetMatch(recentGame.gameId);

                if (match == null)
                {
                    //TODO should only delete if GameDataNotFound error (shouldnt delete if server is down)
                    try
                    {
                        MatchDetails newMatch = riot.getMatch(recentGame.gameId);

                        if (newMatch != null)
                        {
                            mongo.insertMatch(newMatch);
                            mongo.FlagRecentGame(recentGame.gameId);
                        }
                    }
                    catch (DataNotFoundException e)
                    {
                        Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - No internal or riot record for {recentGame.gameId}. Deleting recent game");
                        //mongo.DeleteRecentGame(recentGame.gameId);
                    } 
                }
                else
                {
                    mongo.FlagRecentGame(recentGame.gameId);
                }
            }
        }

        static void CleanupFeaturedGames()
        {
            List<FeaturedGame> featuredGames = mongo.GetUnflaggedFeaturedGames();

            foreach (FeaturedGame featuredGame in featuredGames)
            {
                MatchDetails match = mongo.GetMatch(featuredGame.gameId);

                if (match == null)
                {
                    //TODO should only delete if GameDataNotFound error (shouldnt delete if server is down)
                    try
                    {
                        MatchDetails newMatch = riot.getMatch(featuredGame.gameId);

                        if (newMatch != null)
                        {
                            mongo.insertMatch(newMatch);
                            mongo.FlagFeaturedGame(featuredGame.gameId);
                        }
                    }
                    catch (DataNotFoundException e)
                    {
                        Console.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - No internal or riot record for {featuredGame.gameId}. Match probably not uploaded yet.");
                        //mongo.deleteFeaturedGame(featuredGame.gameId);
                    }
                }
                else
                {
                    mongo.FlagFeaturedGame(featuredGame.gameId);
                }
            }
        }
    }
}
