using System;
using System.Collections.Generic;

namespace ConsoleApplication1.RiotAPI.Entities.MatchObjects
{
    public class Frame
    {
        public List<Event> events { get; set; }
        public Dictionary<String, ParticipantFrame> participantFrames { get; set; }
        public long timestamp { get; set; }
    }
}
