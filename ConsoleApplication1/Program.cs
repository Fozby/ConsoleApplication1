using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleApplication1.JsonObjects;
using MongoDB.Bson;

namespace ConsoleApplication1
{
    class Program
    {
        private void timerEvent()
        {

        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting...");
                Riot riot = new Riot();
                Mongo mongo = new Mongo();

                Console.WriteLine("Loading recent games (etaram)...");
                List<Game> games = riot.getRecentGames();

                Console.WriteLine("Adding recent games (others)...");
                mongo.insertGames(games);

                Console.WriteLine("Loading recent games (etaram)...");
                games = riot.getRecentGamesForOthers();

                Console.WriteLine("Adding recent games (others)...");
                mongo.insertGames(games);

                Console.WriteLine("Starting 1 hour timer...");

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
                    if (input == "remove")
                    {
                        mongo.removeAll();
                    }
                }
                

            }

            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
               
            }

            Console.ReadLine();

        }

    }

}
