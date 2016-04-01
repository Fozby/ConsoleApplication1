using ConsoleApplication1.JsonObjects;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{ 
    class Riot
    {
        private const string API_KEY = "9b995c6c-7e5a-4c7a-b905-aab1928af045";
        private const string REGION = "oce";
        private const long SUMMONER_ID_ETARAM = 356367;

        private const string BASE_URI = "https://" + REGION + ".api.pvp.net/";
        private const string RECENT_GAMES_RESOURCE = "api/lol/" + REGION + "/v1.3/game/by-summoner/{0}/recent?api_key=" + API_KEY;

        private RestClient myRestClient = new RestClient(BASE_URI);
        
        public List<Game> getRecentGames()
        {
            //default summonerid
            return getRecentGames(SUMMONER_ID_ETARAM);
        }

        public List<Game> getRecentGames(long summonerId)
        {
            string resource = String.Format(RECENT_GAMES_RESOURCE, summonerId);

            RestRequest request = new RestRequest(resource, Method.GET);
            IRestResponse<Response_RecentGames> response = myRestClient.Execute<Response_RecentGames>(request);

            return response.Data.games;
        }
        


    }
}
