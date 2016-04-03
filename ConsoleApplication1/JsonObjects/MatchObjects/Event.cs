using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects.MatchObjects
{
    class Event
    {
        public string ascendedType { get; set; }
        public List<int> assistingParticipantIds { get; set; }
        public string buildingType { get; set; }
        public int creatorId { get; set; }
        public string eventType { get; set; }
        public int itemAfter { get; set; }
        public int itemBefore { get; set; }
        public int itemId { get; set; }
        public int killerId { get; set; }
        public string laneType { get; set; }
        public string levelUpType { get; set; }
        public string monsterType { get; set; }
        public int participantId { get; set; }
        public string pointCaptured { get; set; }
        public Position position { get; set; }
        public int skillSlot { get; set; }
        public int teamId { get; set; }
        public long timestamp { get; set; }
        public string towerType { get; set; }
        public int victimId { get; set; }
        public string wardType { get; set; }
    }
}
