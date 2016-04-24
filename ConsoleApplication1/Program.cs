using System;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.Database;

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
            //Console.ReadLine();
            StatCollector collector = new StatCollector(mongo, riot, google);
           // Global.loadChampions(riot.getChampions()); //TODO store in mongodb to avoid an unnessary call

            Console.WriteLine("Adding recent games...");
            collector.CollectRecentGames();

            while (true)
            {
                Console.WriteLine("Insert command");
                string input = Console.ReadLine();

                if (input == "feat")
                {
                    collector.CollectFeaturedGames();
                }
                else if (input == "upload")
                {
                    //List<Game> allGames = mongo.getARAMGames();

                    //foreach (Game game in allGames)
                    //{
                    //    uploadGame(game);
                    //}
                }
                else if (input == "stats")
                {
                    collector.UploadPlayerStats();
                }
                else if (input == "foo")
                {
                    collector.UploadChampionStats();
                }
            }
        }
    }
}
