using System;
using System.Collections.Generic;
using ConsoleApplication1.JsonObjects;
using MongoDB.Bson;
using ConsoleApplication1.JsonObjects.MatchObjects;
using Newtonsoft.Json;
using ConsoleApplication1.GoogleNS;

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
            MatchConverter converter = new MatchConverter();
        
            Console.WriteLine("Loading recent games...");
            List<Game> games = riot.getRecentGamesForAllPlayers();

            Console.WriteLine("Adding recent games...");
            mongo.insertGames(games);

            Console.WriteLine("Starting 1 hour timer...");

            List<Game> allGames = mongo.getAllGames();

            foreach(Game game in allGames)
            {
                GoogleRow row = MatchConverter.buildGoogleRow(game, riot.getTeamStatsForMatch(game.gameId, game.teamId));
                //google.addGame(row);
                //Console.WriteLine(JsonConvert.SerializeObject(row, Formatting.Indented));
            }

            while (true)
            {
                Console.WriteLine("Insert command");
                string input = Console.ReadLine();

                if (input == "add")
                {
                    mongo.insertGame(games[0]);
                    Console.WriteLine("Inserted game: " + games[0].GetPK() + ", Count is now: " + mongo.getCount());
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
                if (input == "drop")
                {
                    mongo.dropAll();
                    Console.WriteLine("Count = " + mongo.getCount());
                }
                if (input == "google")
                {
                    Game g = mongo.getGame();
                    GoogleRow row = MatchConverter.buildGoogleRow(g, riot.getTeamStatsForMatch(g.gameId, g.teamId));
                    Console.WriteLine(JsonConvert.SerializeObject(row, Formatting.Indented));
                }
            }

        }
    }


}
