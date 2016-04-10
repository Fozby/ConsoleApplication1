using System;
using System.Collections.Generic;
using ConsoleApplication1.JsonObjects;
using MongoDB.Bson;
using ConsoleApplication1.JsonObjects.MatchObjects;
using Newtonsoft.Json;
using ConsoleApplication1.GoogleNS;
using ConsoleApplication1.Objects;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        static Riot riot = new Riot();
        static Mongo mongo = new Mongo();
        static GoogleAPI google = new GoogleAPI();
        static MatchConverter converter = new MatchConverter();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            converter.loadChampions(riot.getChampions());
                    
            Console.WriteLine("Loading recent games...");
            List<Game> games = riot.getRecentGamesForAllPlayers();

            Console.WriteLine("Adding recent games...");
            mongo.insertGames(games);

            while (true)
            {
                Console.WriteLine("Insert command");
                string input = Console.ReadLine();

                if (input == "add")
                {
                    mongo.insertGame(games[0]);
                    Console.WriteLine("Inserted game: " + games[0].GetPK() + ", Count is now: " + mongo.getCount());
                }
                if (input == "addAll")
                {
                    mongo.insertGames(games);
                }
                if (input == "get")
                {
                    Console.WriteLine(mongo.getGame().ToJson());
                }
                if (input == "getMatch")
                {
                    mongo.getMatch(123);
                }
                if (input == "count")
                {
                    Console.WriteLine("Count = " + mongo.getCount());
                }
                if (input == "drop")
                {
                    mongo.dropAll();
                    Console.WriteLine("Count = " + mongo.getCount());
                }
                if (input == "upload")
                {
                    List<Game> allGames = mongo.getARAMGames();

                    foreach (Game game in allGames)
                    {
                        uploadGame(game);
                        Thread.Sleep(1100);
                    }
                }
            }

        }

        private static void uploadGame(Game game)
        {
            Console.WriteLine($"Uploading game {game.gameId} for player {game.summonerId}");
            long matchId = game.gameId;
            int teamId = game.teamId;

            MatchDetails match = mongo.getMatch(matchId);

            if (match == null)
            {
                Console.WriteLine($"Match Data was not found");
                match = riot.getMatch(matchId);
                mongo.addMatch(match);
                Console.WriteLine($"Queried and stored match {match.matchId}");
            }
            else
            {
                Console.WriteLine($"Match Data was found");
            }

            GoogleRow row = converter.buildGoogleRow(game, getTeamStatsForMatch(match, teamId));
            google.addGame(row);
        }

        private static TeamStats getTeamStatsForMatch(MatchDetails match, int teamId)
        {
            TeamStats teamStats = new TeamStats();

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
    }
}
