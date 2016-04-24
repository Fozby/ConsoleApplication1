using System.Collections.Generic;

namespace ConsoleApplication1.RiotAPI.Entities.MatchObjects
{
    class Timeline
    {
        public long frameInterval { get; set; }
        public List<Frame> frames { get; set; }
    }
}
