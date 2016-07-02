using ConsoleApplication1.RiotAPI.Entities.RecentGames;
using System.Collections.Generic;


namespace ConsoleApplication1
{
    class Global
    {
        private const long SUMMONER_ID_ETARAM = 356367;
        private const long SUMMONER_ID_PHYTHE = 557862;
        private const long SUMMONER_ID_CELINAR = 692409;
        private const long SUMMONER_ID_MIROTICA = 464473;
        private const long SUMMONER_ID_SCORILOUS = 470159;
        private const long SUMMONER_ID_DRUZOR = 485547;
        private const long SUMMONER_ID_WART = 603309;
        private const long SUMMONER_ID_NEWBULA = 2120419;
        private const long SUMMONER_ID_MACABROS9 = 601322;
        private const long SUMMONER_ID_RISHVAS = 891580;
        private const long SUMMONER_ID_RISHMAU = 6160582;
        public const long SUMMONER_ID_IGAR = 577854;
        public const long SUMMONER_ID_LOVELY = 1190141;
        public const long SUMMONER_ID_BUBULA = 921064;
        public const long SUMMONER_ID_REINGAR = 2691613;
        public const long SUMMONER_ID_LA_SQUID = 295097;
        public const long SUMMONER_ID_MEWBULA = 5934357;
     

        public static Dictionary<int, string> champions = new Dictionary<int, string>();
  
        public static readonly Dictionary<long, string> players = new Dictionary<long, string>
        { 
            {SUMMONER_ID_ETARAM, "Etaram" },
            {SUMMONER_ID_PHYTHE, "Phythe" },
            {SUMMONER_ID_CELINAR, "Celinar" },
            {SUMMONER_ID_MIROTICA, "Mirotica" },
            {SUMMONER_ID_SCORILOUS, "Scorilous" },
            {SUMMONER_ID_DRUZOR, "Druzor" },
            {SUMMONER_ID_WART, "Wart" },
            {SUMMONER_ID_NEWBULA, "NewBula" },
            {SUMMONER_ID_MACABROS9, "Macabros9" },
            {SUMMONER_ID_RISHMAU, "Rishmau" },
            {SUMMONER_ID_RISHVAS, "Rishvas" },
            {SUMMONER_ID_IGAR, "Igar" },
            {SUMMONER_ID_LOVELY, "Lovely" },
            {SUMMONER_ID_REINGAR, "Reingar" },
            {SUMMONER_ID_LA_SQUID, "La Squid" },
            {SUMMONER_ID_MEWBULA, "Mewbula" }
        };

        public static void loadChampions(ChampionList championList)
        {
            foreach (KeyValuePair<string, ChampionDto> entry in championList.data)
            {
                string championName = entry.Value.name;
                int championId = entry.Value.id;
                champions.Add(championId, championName);
            }
        }

        public static string getChampionName(int championId)
        {
            string championName = championId.ToString();
            champions.TryGetValue(championId, out championName);

            return championName;
        }

       public static string GetPlayerName(long summonerId)
        {
            string playerName = summonerId.ToString();
            players.TryGetValue(summonerId, out playerName);

            return playerName;
        }

    }
}
