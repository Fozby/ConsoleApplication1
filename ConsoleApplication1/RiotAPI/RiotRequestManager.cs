using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using ConsoleApplication1.RiotAPI.Exceptions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1.RiotAPI
{
    class RiotRequestManager
    {
        private const string REGION = "oce";
        private const string API_KEY_1 = "defcd602-52e5-4349-817c-2b3cd73e32b5";
        private const string API_KEY_2 = "9b995c6c-7e5a-4c7a-b905-aab1928af045";
        private const string BASE_URI = "https://" + REGION + ".api.pvp.net/";
        private const string RECENT_GAMES_RESOURCE = "api/lol/" + REGION + "/v1.3/game/by-summoner/{0}/recent?api_key={1}";
        private const string MATCH_RESOURCE = "api/lol/" + REGION + "/v2.2/match/{0}?api_key={1}";

        private bool apiKeySwitch = true;

        private RestClient myRestClient = new RestClient(BASE_URI);

        public List<RecentGame> GetAllRecentGames(List<long> summonerIds)
        {
            Task<List<RecentGame>> t = GetRecentGames(summonerIds);

            return t.Result;
        }

        public MatchDetails getMatch(long gameId)
        {
            Task<MatchDetails> t = new Task<MatchDetails>(() =>
            {
                try
                {
                    return RiotApiRequest<MatchDetails>(GetMatchResource(gameId));
                }
                catch (DataNotFoundException e)
                {
                    return null;
                }
            });

            t.Start();
            MatchDetails result = t.Result;

            if (result == null)
            {
                throw new DataNotFoundException();
            }

            return result;
        }

        private string GetMatchResource(long gameId)
        {
            return String.Format(MATCH_RESOURCE, gameId, GetApiKey());
        }

        private string GetRecentGamesResource(long summonerId)
        {
            return String.Format(RECENT_GAMES_RESOURCE, summonerId, GetApiKey());
        }

        private string GetApiKey()
        {
            string apiKey;

            if (apiKeySwitch)
            {
                apiKey = API_KEY_1;
                apiKeySwitch = false;
            }
            else
            {
                apiKey = API_KEY_2;
                apiKeySwitch = true;
            }

            return apiKey;
        }

        private async Task<List<RecentGame>>GetRecentGames(List<long> summonerIds)
        {
            List<Task<Response_RecentGames>> tasks = new List<Task<Response_RecentGames>>();

            foreach (long summonerId in summonerIds)
            {
                Task<Response_RecentGames> t = new Task<Response_RecentGames>(() =>
                {
                    Response_RecentGames response = RiotApiRequest<Response_RecentGames>(GetRecentGamesResource(summonerId));

                    foreach (RecentGame g in response.games)
                    {
                        g.summonerId = summonerId;
                    }

                    return response;
                });

                tasks.Add(t);
                t.Start();
            }

            Response_RecentGames[] responses = await Task.WhenAll(tasks);

            List<RecentGame> games = new List<RecentGame>();

            foreach (Response_RecentGames response in responses)
            {
                games.AddRange(response.games);
            }

            return games;
        }

        private T RiotApiRequest<T>(string resource) where T : new()
        {
            RestRequest request = new RestRequest(resource, Method.GET);

            bool success = false;

            IRestResponse<T> response = null;
            while (!success)
            {
                response = myRestClient.Execute<T>(request);

                RiotHttpStatusCode statusCode = (RiotHttpStatusCode)response.StatusCode;
                switch (statusCode)
                {
                    case RiotHttpStatusCode.OK: //200
                    {
                        success = true;
                        break;
                    }
                    case RiotHttpStatusCode.GameDataNotFound:
                    {
                        throw new DataNotFoundException();
                    }
                    case RiotHttpStatusCode.RateLimitExceeded:
                    {
                        string retry = response.Headers.Where(a => a.Name.Equals("Retry-After")).Select(b => b.Value.ToString()).FirstOrDefault();
                        int retrySeconds;
                        if (!int.TryParse(retry, out retrySeconds))
                        {
                            retrySeconds = 10;
                        }

                        int millisToWait = retrySeconds * 1000 + 1000; //Add 1 second just in case
                        Thread.Sleep(millisToWait);
                        break;
                    }
                    default:
                    {
                        Console.Error.WriteLine($"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Response returned status: {statusCode}");
                        break;
                    }
                }
            }

            return response.Data;
        }
    }
}
