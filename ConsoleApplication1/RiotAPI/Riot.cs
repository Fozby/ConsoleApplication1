using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using ConsoleApplication1.RiotAPI.Exceptions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApplication1.RiotAPI
{
    class Riot
    {
        private const string API_KEY = "defcd602-52e5-4349-817c-2b3cd73e32b5"; 
        private const string REGION = "oce";
        private const string BASE_URI = "https://" + REGION + ".api.pvp.net/";
        private const string CHAMPION_RESOURCE = "api/lol/static-data/" + REGION + "/v1.2/champion?api_key=" + API_KEY;
        private const string FEATURED_GAMES_RESOURCE = "observer-mode/rest/featured?api_key=" + API_KEY;

        private RestClient myRestClient = new RestClient(BASE_URI);


        /*
            Strangling Riot Requests to run through RiotRequestManager, supporting multithreaded async calls.
            Methods moved:
            - GetRecentGames()
            - GetMatch()

            Methods still to move:
            - GetFeaturedGames()
            - GetChampions()
            - GetSummoners() - not implemented yet
        */
        RiotRequestManager requestManager = new RiotRequestManager();

        public List<FeaturedGame> getFeaturedGames()
        {
            FeaturedGamesResponse response = RiotApiRequest<FeaturedGamesResponse>(FEATURED_GAMES_RESOURCE);

            if (response != null && response.gameList != null)
            {
                List<FeaturedGame> games = response.gameList;

                List<FeaturedGame> aramGames = games.FindAll(g => g.gameMode == "ARAM");

                return aramGames;
            }
            else
            {
                return new List<FeaturedGame>();
            }
        }

        public List<RecentGame> getRecentGames(List<long> summonerIds)
        {
            return requestManager.GetAllRecentGames(summonerIds);
        }

        public MatchDetails getMatch(long gameId)
        {
            return requestManager.getMatch(gameId);
        }

        public ChampionList getChampions()
        {
            return RiotApiRequest<ChampionList>(CHAMPION_RESOURCE);
        }

        private T RiotApiRequest<T>(string resource) where T : new()
        {
            RestRequest request = new RestRequest(resource, Method.GET);
            IRestResponse<T> response = myRestClient.Execute<T>(request);

            RiotHttpStatusCode statusCode = (RiotHttpStatusCode)response.StatusCode;
            switch (statusCode)
            {
                case RiotHttpStatusCode.OK: //200
                    {
                        return response.Data;
                    }
                case RiotHttpStatusCode.GameDataNotFound:
                   {
                        throw new DataNotFoundException();
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
