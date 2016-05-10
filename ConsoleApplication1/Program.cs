using System;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.Database;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System.Collections.Generic;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using Newtonsoft.Json;

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

            Cleanup();

            collector.PrintRatings();

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
                else if (input == "short")
                {
                    List<RecentGame> shortGames = mongo.GetShortGames();
                    Dictionary<string, int> foo = new Dictionary<string, int>();

                    foreach (RecentGame game in shortGames)
                    {
                        long summId = game.summonerId;
                        string name = Global.GetPlayerName(summId);

                        if (foo.ContainsKey(name))
                        {
                            int count;
                            foo.TryGetValue(name, out count);
                            count++;

                            foo.Remove(name);
                            foo.Add(name, count);
                        }
                        else
                        {
                            foo.Add(name, 1);
                        }

                        Console.WriteLine(JsonConvert.SerializeObject(game, Formatting.Indented));
                    }

                    foreach (string name in foo.Keys)
                    {
                        int count;
                        foo.TryGetValue(name, out count);
                        Console.WriteLine($"{name} : {count}");
                    }

                    Console.WriteLine($"{mongo.GetShortGames().Count}");
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
            List<RecentGame> recentGames = mongo.GetUnflaggedRecentGames();

            Console.WriteLine($"Found {recentGames.Count} unflagged recent games");

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
                        Console.WriteLine($"No internal or riot record for {recentGame.gameId}. Deleting recent game");
                        mongo.DeleteRecentGame(recentGame.gameId);
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

            Console.WriteLine($"Found {featuredGames.Count} unflagged featured games");

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
                        Console.WriteLine($"No internal or riot record for {featuredGame.gameId}. Match probably not uploaded yet.");
                        mongo.deleteFeaturedGame(featuredGame.gameId);
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
