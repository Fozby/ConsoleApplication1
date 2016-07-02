namespace ConsoleApplication1.RiotAPI.Entities.MatchObjects
{
    public class ParticipantTimeline
    {
        public ParticipantTimelineData ancientGolemAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData ancientGolemKillsPerMinCounts { get; set; }
        public ParticipantTimelineData assistedLaneDeathsPerMinDeltas { get; set; }
        public ParticipantTimelineData assistedLaneKillsPerMinDeltas { get; set; }
        public ParticipantTimelineData baronAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData baronKillsPerMinCounts { get; set; }
        public ParticipantTimelineData creepsPerMinDeltas { get; set; }
        public ParticipantTimelineData csDiffPerMinDeltas { get; set; }
        public ParticipantTimelineData damageTakenDiffPerMinDeltas { get; set; }
        public ParticipantTimelineData damageTakenPerMinDeltas { get; set; }
        public ParticipantTimelineData dragonAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData dragonKillsPerMinCounts { get; set; }
        public ParticipantTimelineData elderLizardAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData elderLizardKillsPerMinCounts { get; set; }
        public ParticipantTimelineData goldPerMinDeltas { get; set; }
        public ParticipantTimelineData inhibitorAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData inhibitorKillsPerMinCounts { get; set; }
        public string lane { get; set; }
        public string role { get; set; }
        public ParticipantTimelineData towerAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData towerKillsPerMinCounts { get; set; }
        public ParticipantTimelineData towerKillsPerMinDeltas { get; set; }
        public ParticipantTimelineData vilemawAssistsPerMinCounts { get; set; }
        public ParticipantTimelineData vilemawKillsPerMinCounts { get; set; }
        public ParticipantTimelineData wardsPerMinDeltas { get; set; }
        public ParticipantTimelineData xpDiffPerMinDeltas { get; set; }
        public ParticipantTimelineData xpPerMinDeltas { get; set; }
    }
}
