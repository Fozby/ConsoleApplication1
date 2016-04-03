using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.JsonObjects.MatchObjects
{
    class Timeline
    {
        public long frameInterval { get; set; }
        public List<Frame> frames { get; set; }
    }
}
