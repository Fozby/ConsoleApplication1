using System;
using System.Collections.Generic;
using ConsoleApplication1.JsonObjects;
using ConsoleApplication1.JsonObjects.MatchObjects;
using Newtonsoft.Json;
using ConsoleApplication1.GoogleNS;
using ConsoleApplication1.Objects;
using System.Threading;
using ConsoleApplication1.JsonObjects.FeaturedGames;
using ConsoleApplication1.GoogleNS.Entities;

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
            Global.loadChampions(riot.getChampions());

            Console.WriteLine("Loading recent games...");
            List<Game> games = riot.getRecentGamesForAllPlayers();

            Console.WriteLine("Adding recent games...");
            mongo.insertGames(games);

            while (true)
            {
                Console.WriteLine("Insert command");
                string input = Console.ReadLine();

                if (input == "feat")
                {
                    Console.WriteLine("Starting 5 minute timer to query featured games...");
                    while (true)
                    {
                        List<FeaturedGame> featuredAram = riot.getFeaturedGames();
                        if (featuredAram != null && featuredAram.Count > 0)
                        {
                            mongo.insertFeaturedGames(featuredAram);
                        }
                        Console.WriteLine($"Total featured games: {mongo.getFeaturedGameCount()}");
                        Thread.Sleep(300000);
                    }
                }
                else if (input == "upload")
                {
                    List<Game> allGames = mongo.getARAMGames();

                    foreach (Game game in allGames)
                    {
                        uploadGame(game);
                    }
                }
                else if (input == "stats")
                {
                    addAllPlayerStats();
                }
                else if (input == "featured")
                {
                    mongo.getFeaturedPlayers().ForEach(Console.WriteLine);
                }
                else if (input == "foo")
                {
                    foo();
                }
                else
                {
                    Game g = mongo.getGame(long.Parse(input));
                    Console.WriteLine(JsonConvert.SerializeObject(g, Formatting.Indented));
                }
            }
        }

        private static void foo()
        {
            

            foreach (int championId in Global.champions.Keys)
            {
                Console.WriteLine($"Building featured champion statistics for champion: {championId}");
                List<MatchDetails> matches = getStatsForChampion(championId);

                if (matches.Count > 0)
                {
                    Console.WriteLine($"{matches.Count} found for champion {championId} aka {Global.getChampionName(championId)}");
                    ChampionStats stats = new ChampionStats(championId, matches);
                    google.addChampionStats(stats);
                }
                else
                {
                    Console.WriteLine($"No matches found for {championId} aka {Global.getChampionName(championId)}");
                }
            }

            throw new NotImplementedException();
        }

        private static void addAllPlayerStats()
        {
            foreach (long summonerId in Global.Summoners)
            {
                List<Game> statGames = mongo.getARAMGamesForPlayer(summonerId);
                PlayerStats stats = new PlayerStats(getGameRows(statGames));
                google.addPlayerStats(stats);
            }
        }

        private static List<GameRow> getGameRows(List<Game> games)
        {
            List<GameRow> gameRows = new List<GameRow>();

            foreach (Game game in games)
            {
                try
                {
                    gameRows.Add(getGameRow(game));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error getting gamerow for game: " + game.gameId + "player: " + game.summonerId);
                }
            }

            return gameRows;
        }

        private static void uploadGame(Game game)
        {
            GameRow row = getGameRow(game);
            google.addGame(row);
        }

        private static GameRow getGameRow(Game game)
        {
            long matchId = game.gameId;
            int teamId = game.teamId;

            MatchDetails match = mongo.getMatch(matchId);

            if (match == null)
            {
                match = riot.getMatch(matchId);
                mongo.addMatch(match);
            }

            return converter.buildGoogleRow(game, getTeamStatsForMatch(match, teamId));
        }

        //TODO move to instance method on MatchDetails
        public static TeamStats getTeamStatsForMatch(MatchDetails match, int teamId)
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

        private static List<MatchDetails> getStatsForChampion(int championId)
        {
            Console.WriteLine($"Finding games and matches with champion: {championId}");
            List<MatchDetails> matchesWithChampion = new List<MatchDetails>();

            List<FeaturedGame> featuredGames = mongo.getFeaturedGamesForChampion(championId);
            foreach (FeaturedGame featuredGame in featuredGames)
            {
                MatchDetails match = mongo.getMatch(featuredGame.gameId);

                if (match == null)
                {
                    Console.WriteLine($"Match not found in DB, querying Riot for match: {featuredGame.gameId}");
                    match = riot.getMatch(featuredGame.gameId);

                    if (match != null)
                    {
                        mongo.addMatch(match);
                    }
                    else
                    {
                        Console.WriteLine($"Riot failed to find match {featuredGame.gameId}");
                    }
                }

                if (match != null && !matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }
            }

            List<Game> games = mongo.getGamesForChampion(championId);

            foreach(Game game in games)
            {
                MatchDetails match = mongo.getMatch(game.gameId);

                if (match == null)
                {
                    Console.WriteLine($"Match not found in DB, querying Riot for match: {game.gameId}");
                    match = riot.getMatch(game.gameId);

                    if (match != null)
                    {
                        mongo.addMatch(match);
                    }
                    else
                    {
                        Console.WriteLine($"Riot failed to find match {game.gameId}");
                    }
                }

                if (match != null && !matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }

            }

            return matchesWithChampion;
        }
    }
}
