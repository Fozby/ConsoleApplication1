using ConsoleApplication1.JsonObjects;
using ConsoleApplication1.JsonObjects.MatchObjects;
using ConsoleApplication1.Objects;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Riot
    {
        private const string API_KEY = "defcd602-52e5-4349-817c-2b3cd73e32b5"; 
        //private const string API_KEY = "9b995c6c-7e5a-4c7a-b905-aab1928af045"; 

        private const string REGION = "oce";
        public const long SUMMONER_ID_ETARAM = 356367;
        public const long SUMMONER_ID_PHYTHE = 557862;
        public const long SUMMONER_ID_CELINAR = 692409;
        public const long SUMMONER_ID_MIROTICA = 464473;
        public const long SUMMONER_ID_SCORILOUS = 470159;
        public const long SUMMONER_ID_DRUZOR = 485547;
        public const long SUMMONER_ID_WART = 603309;
        public const long SUMMONER_ID_NEWBULA = 2120419;
        public const long SUMMONER_ID_MACABROS9 = 601322;
        public const long SUMMONER_ID_RISHVAS = 891580;
        public const long SUMMONER_ID_RISHMAU = 6160582;


        private const string BASE_URI = "https://" + REGION + ".api.pvp.net/";
        private const string RECENT_GAMES_RESOURCE = "api/lol/" + REGION + "/v1.3/game/by-summoner/{0}/recent?api_key=" + API_KEY;
        private const string MATCH_RESOURCE = "api/lol/" + REGION + "/v2.2/match/{0}?api_key=" + API_KEY;
        private const string CHAMPION_RESOURCE = "api/lol/static-data/" + REGION + "/v1.2/champion?api_key=" + API_KEY;

        public List<long> getAllSummoners()
        {
            List<long> summoners = new List<long>();
            summoners.Add(SUMMONER_ID_ETARAM);
            summoners.Add(SUMMONER_ID_PHYTHE);
            summoners.Add(SUMMONER_ID_CELINAR);
            summoners.Add(SUMMONER_ID_MIROTICA);
            summoners.Add(SUMMONER_ID_SCORILOUS);
            summoners.Add(SUMMONER_ID_DRUZOR);
            summoners.Add(SUMMONER_ID_WART);
            summoners.Add(SUMMONER_ID_NEWBULA);
            summoners.Add(SUMMONER_ID_MACABROS9);
            summoners.Add(SUMMONER_ID_RISHVAS);
            summoners.Add(SUMMONER_ID_RISHMAU);

            return summoners;
        }

        private RestClient myRestClient = new RestClient(BASE_URI);

        public List<Game> getRecentGames()
        {
            //default summonerid
            return getRecentGames(SUMMONER_ID_ETARAM);
        }

        public List<Game> getRecentGames(long summonerId)
        {
            string resource = String.Format(RECENT_GAMES_RESOURCE, summonerId);
            Response_RecentGames response = RiotApiRequest<Response_RecentGames>(resource);

            List<Game> games = response?.games ?? new List<Game>(); // don't die on null response
            foreach (Game game in games)
            {
                game.summonerId = summonerId;
            }

            return games;

        }

        public List<Game> getRecentGamesForAllPlayers()
        {
            List<Game> games = new List<Game>();

            foreach (long summonerId in getAllSummoners())
            {
                games.AddRange(getRecentGames(summonerId));
            }

            return games;
        }

        public MatchDetails getMatch(long gameId)
        {
            string resource = String.Format(MATCH_RESOURCE, gameId);
            return RiotApiRequest<MatchDetails>(resource);
        }
        public ChampionList getChampions()
        {
            return RiotApiRequest<ChampionList>(CHAMPION_RESOURCE);
        }

        private T RiotApiRequest<T>(string resource) where T : new()
        {
            Thread.Sleep(1100);

            RestRequest request = new RestRequest(resource, Method.GET);
            IRestResponse<T> response = myRestClient.Execute<T>(request);

            RiotHttpStatusCode statusCode = (RiotHttpStatusCode)response.StatusCode;
            switch (statusCode)
            {
                case RiotHttpStatusCode.OK: //200
                    {
                        return response.Data;
                    }
                case RiotHttpStatusCode.RateLimitExceeded:
                    {
                        string retry = response.Headers.Where(a => a.Name.Equals("Retry-After")).Select(b => b.Value.ToString()).FirstOrDefault();
                        int retrySeconds;
                        if (!int.TryParse(retry, out retrySeconds)) retrySeconds = 10;
                        ++retrySeconds;
                        Console.WriteLine($"You hit the hard limit, pause for {retrySeconds} seconds");
                        Thread.Sleep(retrySeconds * 1000);
                        break;
                    }
                default:
                    {
                        Console.Error.WriteLine($"Response returned status: {statusCode}");
                        break;
                    }
            }
            return default(T);
        }

    }
}
