using ConsoleApplication1.Database;
using ConsoleApplication1.GoogleAPI;
using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.RiotAPI;
using ConsoleApplication1.RiotAPI.Entities.FeaturedGames;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApplication1
{
    class StatCollector
    {
        private Riot riot;
        private Mongo mongo;
        private GoogleSheets google;
        private MatchConverter converter = new MatchConverter();

        public StatCollector(Mongo mongo, Riot riot, GoogleSheets google)
        {
            this.riot = riot;
            this.mongo = mongo;
            this.google = google;
        }

        public void CollectFeaturedGames()
        {
            Console.WriteLine("Starting 5 minute timer to query featured games...");
            while (true)
            {
                List<FeaturedGame> featuredAramGames = riot.getFeaturedGames();
                if (featuredAramGames != null && featuredAramGames.Count > 0)
                {
                    mongo.insertFeaturedGames(featuredAramGames);

                    foreach (FeaturedGame aramGame in featuredAramGames)
                    {
                        MatchDetails match = riot.getMatch(aramGame.gameId);
                        mongo.insertMatch(match);
                    }
                }
                Console.WriteLine($"[{DateTime.Now.ToString()}] Total featured games: {mongo.getFeaturedGameCount()}");
                Thread.Sleep(300000);
            }
        }

        public void CollectRecentGames()
        {
            List<Game> games = riot.getRecentGamesForAllPlayers();
            int numAdded = mongo.insertGames(games);
            Console.WriteLine($"Collected {numAdded} games. Total Recent Games: {mongo.getARAMGames().Count}");
        }

        public void UploadPlayerStats()
        {
            foreach (long summonerId in Global.players.Keys)
            {
                List<Game> statGames = mongo.getARAMGamesForPlayer(summonerId);
                PlayerStats stats = converter.BuildPlayerStats(getGameRows(statGames));
                google.addPlayerStats(stats);
            }
        }

        public void UploadChampionStats()
        {
            foreach (int championId in Global.champions.Keys)
            {
                List<MatchDetails> matches = getStatsForChampion(championId);

                if (matches.Count > 0)
                {
                    ChampionStats stats = converter.GetChampionStats(championId, matches);
                    google.addChampionStats(stats);
                }

                Console.WriteLine($"{matches.Count} found for champion {championId} aka {Global.getChampionName(championId)}");
            }
        }

        private List<MatchDetails> getStatsForChampion(int championId)
        {
            List<MatchDetails> matchesWithChampion = new List<MatchDetails>();

            List<FeaturedGame> featuredGames = mongo.getFeaturedGamesForChampion(championId);
            foreach (FeaturedGame featuredGame in featuredGames)
            {
                MatchDetails match = mongo.getMatch(featuredGame.gameId);

                if (!matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }
            }

            List<Game> games = mongo.getGamesForChampion(championId);

            foreach (Game game in games)
            {
                MatchDetails match = mongo.getMatch(game.gameId);

                if (!matchesWithChampion.Contains(match))
                {
                    matchesWithChampion.Add(match);
                }
            }

            return matchesWithChampion;
        }

        private List<GameRow> getGameRows(List<Game> games)
        {
            List<GameRow> gameRows = new List<GameRow>();

            foreach (Game game in games)
            { 
                gameRows.Add(getGameRow(game));
            }

            return gameRows;
        }

        private GameRow getGameRow(Game game)
        {
            long matchId = game.gameId;
            int teamId = game.teamId;
            MatchDetails match = mongo.getMatch(matchId);
         
            return converter.BuildGameStats(game, match, teamId);
        }
    }
}
