using ConsoleApplication1.GoogleAPI.DataObjects;
using ConsoleApplication1.GoogleAPI.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace ConsoleApplication1.RiotAPI.Entities.MatchObjects
{
    public class MatchDetails
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

        public override bool Equals(object obj)
        {
            var item = obj as MatchDetails;

            if (item == null)
            {
                return false;
            }

            return this.matchId.Equals(item.matchId);
        }

        public override int GetHashCode()
        {
            return this.matchId.GetHashCode();
        }

        public Data getChampionDataForPlayer(long summonerId)
        {
            Participant participant = participants.Find(p => p.summonerId == summonerId);

            return getChampionData(participant.championId);
        }

        public Data getTeamDataForPlayer(long summonerId)
        {
            Participant participant = participants.Find(p => p.summonerId == summonerId);

            return getTeamData(participant.championId);
        }

        public Data getChampionData(int championId)
        {
            ParticipantStats pStats = GetStatsForChampion(championId);

            return new Data(pStats);
        }

        public Data getTeamData(int championId)
        {
            int teamId = GetTeamIdForChampion(championId);

            List<ParticipantStats> teamStats = participants
                .FindAll(p => p.teamId == teamId)
                .ConvertAll(p => p.stats);

            Data teamData = new Data();

            foreach (ParticipantStats stats in teamStats)
            {
                teamData = teamData.add(new Data(stats));
            }

            return teamData;
        }

        public bool DidPlayerWin(long summonerId)
        {
            Participant participant = participants.Find(p => p.summonerId == summonerId);

            return participant.teamId == GetWinningTeam();
        }

        public int GetTeamIdForChampion(int championId)
        {
            foreach (Participant participant in participants)
            {
                if (participant.championId == championId)
                {
                    return participant.teamId;
                }
            }

            //TODO custom exceptions
            throw new ArgumentException($"championId not found in match: {matchId}");
        }

        public ParticipantStats GetStatsForPlayer(long summonerId)
        {
            Participant participant = participants.Find(p => p.summonerId == summonerId);

            return GetStatsForChampion(participant.championId);
        }

        public ParticipantStats GetStatsForChampion(int championId)
        {
            foreach (Participant participant in participants)
            {
                if (participant.championId == championId)
                {
                    return participant.stats;
                }
            }

            //TODO custom exceptions 
            throw new ArgumentException($"championId not found in match: {matchId}");
        }

        public ParticipantTimeline GetTimelineForPlayer(long summonerId)
        {
            foreach (Participant participant in participants)
            {
                if (participant.summonerId == summonerId)
                {
                    return participant.timeline;
                }
            }

            //TODO custom exceptions 
            throw new ArgumentException($"summonerId not found in match: {matchId}");
        }


        public ParticipantTimeline GetTimelineForChampion(int championId)
        {
            foreach (Participant participant in participants)
            {
                if (participant.championId == championId)
                {
                    return participant.timeline;
                }
            }

            //TODO custom exceptions 
            throw new ArgumentException($"championId not found in match: {matchId}");
        }

        public int GetWinningTeam()
        {
            foreach (Team team in teams)
            {
                if (team.winner)
                {
                    return team.teamId;
                }
            }

            return -1;
        }

        public Boolean IsValid()
        {
            foreach(Participant participant in participants)
            {
                if (participant.stats.totalDamageDealt == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
