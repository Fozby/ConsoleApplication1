using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects.MatchObjects
{
    class MatchDetails
    {
        public ObjectId Id { get; set; }

        public int mapId { get; set; }
        public long matchCreation { get; set; }
        public int matchDuration { get; set; }
        public long matchId { get; set; }
        public string matchMode { get; set; }
        public string matchType { get; set; }
        public string matchVersion { get; set; }
        public List<ParticipantIdentity> participantIdentities { get; set; }
        public List<Participant> participants { get; set; }
        public string platformId { get; set; }
        public string queueType { get; set; }
        public string region { get; set; }
        public string season { get; set; }
        public List<Team> teams { get; set; }
        public Timeline timeline { get; set; }
    }
}
