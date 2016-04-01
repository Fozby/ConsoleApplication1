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

namespace ConsoleApplication1
{
    class Program
    {
        private static string API_KEY = "9b995c6c-7e5a-4c7a-b905-aab1928af045";
        static void Main(string[] args)
        {
            string region = "oce";
            long summonerId = 356367;
            string baseUri = $"https://{region}.api.pvp.net/";
            string resource = $"api/lol/{region}/v1.3/game/by-summoner/{summonerId}/recent?api_key={API_KEY}";

            RestClient restClient = new RestClient(baseUri);
            RestRequest request = new RestRequest(resource, Method.GET);
            IRestResponse<Response_RecentGames> response = restClient.Execute<Response_RecentGames>(request);
            
            Game game = response.Data.games.FirstOrDefault();
            Stats s = game.stats;
            Console.WriteLine($"Kills: {s.championsKilled}, Deaths: {s.numDeaths}, Assists: {s.assists}");


            //Mongo mongo = new Mongo();

            //while (true)
            //{
            //    String input = Console.ReadLine();

            //    if (input == "get")
            //    {
            //        Task.Run(async () => await mongo.getRec());
            //    }
            //    if (input == "insert")
            //    {
            //        Task.Run(async () => await mongo.insertRec());
            //    }
            //    if (input == "count")
            //    {
            //        Task.Run(async () => await mongo.getCount());
            //    }
            //}
            Console.WriteLine("done");
            Console.ReadLine();
        }
    }

}
