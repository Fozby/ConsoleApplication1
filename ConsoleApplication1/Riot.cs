﻿using ConsoleApplication1.JsonObjects;
using ConsoleApplication1.JsonObjects.MatchObjects;
using ConsoleApplication1.Objects;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{ 
    class Riot
    {
        private const string API_KEY = "9b995c6c-7e5a-4c7a-b905-aab1928af045";
        private const string REGION = "oce";
        private const long SUMMONER_ID_ETARAM = 356367;
        private const long SUMMONER_ID_PHYTHE = 557862;
        private const long SUMMONER_ID_CELINAR = 692409;
        private const long SUMMONER_ID_MIROTICA = 464473;
        private const long SUMMONER_ID_SCORILOUS = 470159;
        private const long SUMMONER_ID_DRUZOR = 485547;

        private const string BASE_URI = "https://" + REGION + ".api.pvp.net/";
        private const string RECENT_GAMES_RESOURCE = "api/lol/" + REGION + "/v1.3/game/by-summoner/{0}/recent?api_key=" + API_KEY;
        private const string MATCH_RESOURCE = "api/lol/" + REGION + "/v2.2/match/{0}?api_key=" + API_KEY;


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

            RiotHttpStatusCode statusCode = (RiotHttpStatusCode)response.StatusCode;

            switch (statusCode)
            {
                case RiotHttpStatusCode.OK: //200
                {
                    List<Game> games = response.Data.games;
                    foreach (Game game in games)
                    {
                        game.summonerId = summonerId;
                    }

                    return games;
                }
                default:
                {
                    Console.Error.WriteLine($"Response returned status: {statusCode}");
                    break;
                }
            }
            return new List<Game>();
        }
        
        public List<Game> getRecentGamesForAllPlayers()
        {
            List<Game> games = getRecentGames(SUMMONER_ID_ETARAM);
            games.AddRange(getRecentGames(SUMMONER_ID_PHYTHE));
            games.AddRange(getRecentGames(SUMMONER_ID_MIROTICA));
            games.AddRange(getRecentGames(SUMMONER_ID_CELINAR));
            games.AddRange(getRecentGames(SUMMONER_ID_SCORILOUS));
            games.AddRange(getRecentGames(SUMMONER_ID_DRUZOR));

            return games;
        }


        public TeamStats getTeamStatsForMatch(long matchId, int teamId)
        {
            TeamStats teamStats = new TeamStats();
            MatchDetails match = getMatch(matchId);

            foreach (Participant participant in match.participants)
            {
                if (participant.teamId == teamId)
                {
                    teamStats.kills += participant.stats.kills;
                    teamStats.deaths += participant.stats.deaths;
                    teamStats.assists += participant.stats.assists;
                    teamStats.playerDmg += participant.stats.totalDamageDealtToChampions;
                    teamStats.minionDmg += (participant.stats.totalDamageDealt - participant.stats.totalDamageDealtToChampions);
                    teamStats.dmgTaken += participant.stats.totalDamageTaken;
                    teamStats.gold += participant.stats.goldEarned;
                    teamStats.minionKills += participant.stats.minionsKilled;
                }
            }
            return teamStats;
        }

        public MatchDetails getMatch(long gameId)
        {
            string resource = String.Format(MATCH_RESOURCE, gameId);

            RestRequest request = new RestRequest(resource, Method.GET);
            IRestResponse<MatchDetails> response = myRestClient.Execute<MatchDetails>(request);

            RiotHttpStatusCode statusCode = (RiotHttpStatusCode)response.StatusCode;

            switch (statusCode)
            {
                case RiotHttpStatusCode.OK: //200
                    {
                        return response.Data;
                    }
                default:
                    {
                        Console.Error.WriteLine($"Response returned status: {statusCode}");
                        break;
                    }
            }

            return null;
        }

    }
}
