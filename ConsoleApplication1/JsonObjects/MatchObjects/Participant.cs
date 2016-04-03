using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects.MatchObjects
{
    class Participant
    {
        public int championId { get; set; }
        public string highestAchievedSeasonTier { get; set; }
        public List<Mastery> masteries { get; set; }
        public int participantId { get; set; }
        public List<Rune> runes { get; set; }
        public int spell1Id { get; set; }
        public int spell2Id { get; set; }
        public ParticipantStats stats { get; set; }
        public int teamId { get; set; }
        public ParticipantTimeline timeline { get; set; }
    }
}
