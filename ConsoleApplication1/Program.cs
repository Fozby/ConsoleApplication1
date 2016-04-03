using System;
using System.Collections.Generic;
using ConsoleApplication1.JsonObjects;
using ConsoleApplication1.GoogleNS;
using MongoDB.Bson;
using ConsoleApplication1.JsonObjects.MatchObjects;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            Riot riot = new Riot();
            Mongo mongo = new Mongo();
            GoogleAPI google = new GoogleAPI();

            MatchDetails match = riot.getMatch(128177983);
            Console.WriteLine($"Found match: [{ match.matchId}], [{match.Id}]");
            mongo.addMatch(match);
            MatchDetails insertedMatch = mongo.getMatch(match.matchId);
            Console.WriteLine($"Inserted match with id:[{insertedMatch.Id}]");
            Console.WriteLine($"Total matches in DB now:{mongo.getMatchCount()}");
            Console.ReadLine();
            return;

            Console.WriteLine("Loading recent games...");
            List<Game> games = riot.getRecentGamesForAllPlayers();

            Console.WriteLine("Adding recent games...");
            mongo.insertGames(games);

            Console.WriteLine("Starting 1 hour timer...");

            foreach (Game g in games)
            {
                google.addGame(g);
            }

            /*
            while (true)
            {
                Thread.Sleep(3600000);
                Console.WriteLine("Adding recent games...");
                mongo.insertGames(riot.getRecentGames());
            }
            */


            while (true)
            {
                string input = Console.ReadLine();

                if (input == "add")
                {
                    mongo.insertGame(games[0]);
                }
                if (input == "addAll")
                {
                    mongo.insertGames(games);
                }
                if (input == "get")
                {
                    Console.WriteLine(mongo.getGame().ToJson());
                }
                if (input == "count")
                {
                    Console.WriteLine("Count = " + mongo.getCount());
                }
            }

        }
    }


}
