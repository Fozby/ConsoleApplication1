using ConsoleApplication1.GoogleAPI.Entities;
using ConsoleApplication1.GoogleAPI.Entities.Timeline;
using ConsoleApplication1.RiotAPI.Entities.MatchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleAPI.DataObjects
{
    public class MatchCollection
    {
        public List<MatchDetails> matches { get; }
        public int Count { get; }

        public MatchCollection(List<MatchDetails> matches)
        {
            this.matches = matches;
            this.Count = matches.Count;
        }

        public MatchCollection FindAll(List<long> matchIds)
        {
            return new MatchCollection(this.matches.FindAll(m => matchIds.Contains(m.matchId)));
        }

        //TODO flag on DB record whether gold data is recorded or not
        public TimelineStats buildGoldTimelineStats(int championId)
        {
            TimelineStatsBuilder builder = new TimelineStatsBuilder();

            foreach (MatchDetails match in matches)
            {
                ParticipantTimeline pTimeline = match.GetTimelineForChampion(championId);

                ParticipantTimelineData goldData = pTimeline.goldPerMinDeltas;

                if (goldData != null) //Riot previously had a bug where some data was not recorded, some of my old data therefore doesn't have a full data set
                {
                    builder.AddZeroToTenValue(goldData.zeroToTen);
                    builder.AddTenToTwenty(goldData.tenToTwenty);
                    builder.AddTwentyToThirty(goldData.twentyToThirty);
                    builder.AddThirtyToEnd(goldData.thirtyToEnd);
                }
            }

            return builder.build();
        }

        public TimelineStats buildCreepTimelineStats(int championId)
        {
            TimelineStatsBuilder builder = new TimelineStatsBuilder();

            foreach (MatchDetails match in matches)
            {
                ParticipantTimeline pTimeline = match.GetTimelineForChampion(championId);

                ParticipantTimelineData creepScoreData = pTimeline.creepsPerMinDeltas;

                if (creepScoreData != null) //Riot previously had a bug where some data was not recorded, some of my old data therefore doesn't have a full data set
                {
                    builder.AddZeroToTenValue(creepScoreData.zeroToTen);
                    builder.AddTenToTwenty(creepScoreData.tenToTwenty);
                    builder.AddTwentyToThirty(creepScoreData.twentyToThirty);
                    builder.AddThirtyToEnd(creepScoreData.thirtyToEnd);
                }
            }

            return builder.build();
        }
    }
}
