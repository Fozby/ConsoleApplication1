using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication1.JsonObjects;
using RestSharp;
using MongoDB.Bson.Serialization;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {


                Riot riot = new Riot();

                List<Game> games = riot.getRecentGames();

                Console.WriteLine("Ready for commands...");


                Mongo mongo = new Mongo();

                while (true)
                {
                    string input = Console.ReadLine();

                    if (input == "add")
                    {
                        Task.Run(async () => await mongo.insertGame(games[0]));
                    }
                    if (input == "addAll")
                    {
                        Task.Run(async () => await mongo.insertGames(games));
                    }
                    if (input == "get")
                    {
                        Task.Run(async () => await mongo.getRec());
                    }
                    if (input == "count")
                    {
                        Task.Run(async () => await mongo.getCount());
                    }
                    if (input == "remove")
                    {
                        Task.Run(async () =>
                        {
                            await mongo.removeAll();
                            await mongo.getCount();
                        });
                    }
                }

            }

            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
    }

}
