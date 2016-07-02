using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleAPI.Entities.Timeline
{
    public class TimelineStatsBuilder
    {
        int numGamesWithZeroToTen = 0;
        int numGamesWithTenToTwenty = 0;
        int numGamesWithTwentyToThirty = 0;
        int numGamesWithThirtyToEnd = 0;

        double totalPerMinZeroToTen = 0;
        double totalPerMinTenToTwenty = 0;
        double totalPerMinTwentyToThirty = 0;
        double totalPerMinThirtyToEnd = 0;

        public void AddZeroToTenValue(double value)
        {
            numGamesWithZeroToTen = value == 0.0 ? numGamesWithZeroToTen : numGamesWithZeroToTen + 1;
            totalPerMinZeroToTen += value;
        }

        public void AddTenToTwenty(double value)
        {
            numGamesWithTenToTwenty = value == 0.0 ? numGamesWithTenToTwenty : numGamesWithTenToTwenty + 1;
            totalPerMinTenToTwenty += value;
        }

        public void AddTwentyToThirty(double value)
        {
            numGamesWithTwentyToThirty = value == 0.0 ? numGamesWithTwentyToThirty : numGamesWithTwentyToThirty + 1;
            totalPerMinTwentyToThirty += value;
        }

        public void AddThirtyToEnd(double value)
        {
            numGamesWithThirtyToEnd = value == 0.0 ? numGamesWithThirtyToEnd : numGamesWithThirtyToEnd + 1;
            totalPerMinThirtyToEnd += value;
        }

        public TimelineStats build()
        {
            //Prevent divide by 0
            double avgZeroToTen = numGamesWithZeroToTen == 0 ? 0 : totalPerMinZeroToTen / numGamesWithZeroToTen;
            double avgTenToTwenty = numGamesWithTenToTwenty == 0 ? 0 : totalPerMinTenToTwenty / numGamesWithTenToTwenty;
            double avgTwentyToThirty = numGamesWithTwentyToThirty == 0 ? 0 : totalPerMinTwentyToThirty / numGamesWithTwentyToThirty;
            double avgThirtyToEnd = numGamesWithThirtyToEnd == 0 ? 0 : totalPerMinThirtyToEnd / numGamesWithThirtyToEnd;

            TimelineStat zeroToTen = new TimelineStat(numGamesWithZeroToTen, avgZeroToTen);
            TimelineStat tenToTwenty = new TimelineStat(numGamesWithTenToTwenty, avgTenToTwenty);
            TimelineStat twentyToThirty = new TimelineStat(numGamesWithTwentyToThirty, avgTwentyToThirty);
            TimelineStat thirtyToEnd = new TimelineStat(numGamesWithThirtyToEnd, avgThirtyToEnd);

            return new TimelineStats(zeroToTen, tenToTwenty, twentyToThirty, thirtyToEnd);
        }
    }

}
