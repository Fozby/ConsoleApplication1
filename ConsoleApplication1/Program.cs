using System;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.Database;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System.Collections.Generic;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;

namespace ConsoleApplication1
{
    class Program
    {
        static Riot riot = new Riot();
        static Mongo mongo = new Mongo();
        static GoogleSheets google = new GoogleSheets();
        static MatchConverter converter = new MatchConverter();
        
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            StatCollector collector = new StatCollector(mongo, riot, google);
            Global.loadChampions(riot.getChampions()); //TODO store in mongodb to avoid an unnessary call

            Console.WriteLine("Adding recent games...");
            //collector.CollectRecentGames();
            collector.UploadCompetitiveChampionStats();

            Cleanup();

            while (true)
            {
                Console.WriteLine("Insert command");
                string input = Console.ReadLine();

                if (input == "featured")
                {
                    collector.CollectFeaturedGames();
                }
                else if (input == "player")
                {
                    collector.UploadPlayerStats();
                }
                else if (input == "champion")
                {
                    collector.UploadChampionStats();
                }
                else if (input == "competitive")
                {
                    collector.UploadCompetitiveChampionStats();
                }
            }
        }

        static void Cleanup()
        {
            CleanupRecentGames();
            CleanupFeaturedGames();
        }

        static void CleanupRecentGames()
        {
            List<RecentGame> recentGames = mongo.getAllRecentGames();

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
                        }
                    }
                    catch (DataNotFoundException e)
                    {
                        Console.WriteLine($"No internal or riot record for {recentGame.gameId}. Deleting game");
                        mongo.DeleteRecentGame(recentGame.gameId);
                    }
                }
            }
        }

        static void CleanupFeaturedGames()
        {
            List<FeaturedGame> featuredGames = mongo.GetAllFeaturedGames();

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
                        }
                    }
                    catch (DataNotFoundException e)
                    {
                        Console.WriteLine($"No internal or riot record for {featuredGame.gameId}. Deleting game");
                        mongo.deleteFeaturedGame(featuredGame.gameId);
                    }
                }
            }
        }
    }
}
