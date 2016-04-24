using ConsoleApplication1.JsonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Dictionary<int, string> champions = new Dictionary<int, string>();

        public static readonly List<long> Summoners = new List<long>
        (
            new List<long>
            {
             SUMMONER_ID_ETARAM,
             SUMMONER_ID_PHYTHE,
             SUMMONER_ID_CELINAR,
             SUMMONER_ID_MIROTICA,
             SUMMONER_ID_SCORILOUS,
             SUMMONER_ID_DRUZOR,
             SUMMONER_ID_WART,
             SUMMONER_ID_NEWBULA,
             SUMMONER_ID_MACABROS9,
             SUMMONER_ID_RISHMAU,
             SUMMONER_ID_IGAR
            }
        );

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

    }
}
