using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects.MatchObjects
{
    class Frame
    {
        public List<Event> events { get; set; }
        public Dictionary<String, ParticipantFrame> participantFrames { get; set; }
        public long timestamp { get; set; }
    }
}
