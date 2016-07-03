using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleAPI.Entities.Timeline
{
    public class TimelineStats
    {
        public TimelineStat zeroToTen { get; set; }
        public TimelineStat tenToTwenty { get; set; }
        public TimelineStat twentyToThirty { get; set; }
        public TimelineStat thirtyToEnd { get; set; }

        public TimelineStats(TimelineStat zeroToTen, TimelineStat tenToTwenty, TimelineStat twentyToThirty, TimelineStat thirtyToEnd)
        {
            this.zeroToTen = zeroToTen;
            this.tenToTwenty = tenToTwenty;
            this.twentyToThirty = twentyToThirty;
            this.thirtyToEnd = thirtyToEnd;  
        }
    }
}
