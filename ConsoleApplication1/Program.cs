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
using Newtonsoft.Json;
using ConsoleApplication1.JsonObjects;

namespace ConsoleApplication1
{
    class Program
    {
        private static string API_KEY = "9b995c6c-7e5a-4c7a-b905-aab1928af045";
        static void Main(string[] args)
        {
            string region = "oce";
            long summonerId = 356367;
            string uri = $"https://{region}.api.pvp.net/api/lol/{region}/v1.3/game/by-summoner/{summonerId}/recent?api_key={API_KEY}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string output;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                output = reader.ReadToEnd();
            }

            output = ParseUnicodeEscapes(output);

            
            Response_RecentGames recentGames = JsonConvert.DeserializeObject<Response_RecentGames>(output);
            Stats s = recentGames.games.Select(a => a.stats).FirstOrDefault();
            Console.WriteLine($"Kills: {s.championsKilled}, Deaths: {s.numDeaths}, Assists: {s.assists}");
            Console.WriteLine(s.ToJson());


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

            Console.ReadLine();
        }

        public static string ParseUnicodeEscapes(string escapedString)
        {
            const string literalBackslashPlaceholder = "\uf00b";
            const string unicodeEscapeRegexString = @"(?:\\u([0-9a-fA-F]{4}))|(?:\\U([0-9a-fA-F]{8}))";
            // Replace escaped backslashes with something else so we don't
            // accidentally expand escaped unicode escapes.
            string workingString = escapedString.Replace("\\\\", literalBackslashPlaceholder);

            // Replace unicode escapes with actual unicode characters.
            workingString = new System.Text.RegularExpressions.Regex(unicodeEscapeRegexString).Replace(workingString,
                match => ((char)Int32.Parse(match.Value.Substring(2), System.Globalization.NumberStyles.HexNumber))
                .ToString(System.Globalization.CultureInfo.InvariantCulture));

            // Replace the escaped backslash placeholders with non-escaped literal backslashes.
            workingString = workingString.Replace(literalBackslashPlaceholder, "\\");
            return workingString;
        }
    }

}
