using System;
using System.Collections.Generic;
using System.Linq;
using EliteJournalReader.Events;

namespace Elite
{
    public static class Missions
    {
        public class Mission
        {
            public long MissionID { get; set; }
            public string Name { get; set; }
            public bool PassengerMission { get; set; }
            public DateTime? Expires { get; set; }

            public string System { get; set; }
            public string Station { get; set; }
            public long Reward { get; set; }
            public int? Passengers { get; set; }

            public string Faction { get; set; }
            public string Influence { get; set; } //    None/Low/Med/High
            public string Reputation { get; set; } //  None/Low/Med/High

            public string Commodity { get; set; }
            public string CommodityLocalised { get; set; }
            public int? Count { get; set; }

            public bool? PassengerViPs { get; set; }
            public bool? PassengerWanted { get; set; }
            public string PassengerType { get; set; }

            public bool Wing { get; set; }
            public string Target { get; set; }
            public string TargetType { get; set; }
            public string TargetFaction { get; set; }
            public int? KillCount { get; set; }
            public string Donation { get; set; }
            public int? Donated { get; set; }

        }

        public static List<Mission> MissionList = new List<Mission>();

        public static string GetMissionName(long missionID)
        {
            var missionName = string.Empty;
            if (missionID > 0)
            {
                missionName = MissionList.FirstOrDefault(x => x.MissionID == missionID)?.Name;

                if (string.IsNullOrEmpty(missionName))
                {
                    missionName = "Unknown";
                }
            }

            return missionName;
        }

        public static string GetMissionSystem(long missionID)
        {
            var missionSystem = string.Empty;
            if (missionID > 0)
            {
                missionSystem = MissionList.FirstOrDefault(x => x.MissionID == missionID)?.System;
            }

            return missionSystem;
        }

        public static string GetMissionStation(long missionId)
        {
            var missionStation = string.Empty;
            if (missionId > 0)
            {
                missionStation = MissionList.FirstOrDefault(x => x.MissionID == missionId)?.Station;
            }

            return missionStation;
        }

        public static void HandleMissionsEvent(MissionsEvent.MissionsEventArgs info)
        {
            if (info.Active?.Length > 0)
            {
                MissionList = info.Active.Select(x => new Mission
                {
                    MissionID = x.MissionID,
                    PassengerMission = x.PassengerMission,
                    Expires = (DateTime?)null,
                    Name = x.Name?.Replace("_name", " ").Replace("Mission_", " ").Replace("_EXPANSION", " ").Replace("_", " ")
                }).ToList();
            }
        }
        public static void HandleMissionAbandonedEvent(MissionAbandonedEvent.MissionAbandonedEventArgs info)
        {
            MissionList.RemoveAll(x => x.MissionID == info.MissionID);
        }

        public static void HandleMissionAcceptedEvent(MissionAcceptedEvent.MissionAcceptedEventArgs info)
        {
            MissionList.RemoveAll(x => x.MissionID == info.MissionID);

            MissionList.Add(new Mission
            {
                MissionID = info.MissionID,
                Name = info.LocalisedName?.Replace("_name", " ").Replace("Mission_", " ").Replace("_EXPANSION", " ").Replace("_", " "),
                Expires = info.Expiry,
                PassengerMission = info.PassengerCount > 0,
                System = info.DestinationSystem,
                Station = info.DestinationStation,
                Reward = info.Reward,
                Passengers = info.PassengerCount,

                Faction = info.Faction,
                Influence = info.Influence.ToString(), //    None/Low/Med/High
                Reputation = info.Reputation.ToString(), //  None/Low/Med/High

                Commodity = info.Commodity,
                CommodityLocalised = info.Commodity_Localised,
                Count = info.Count,

                PassengerViPs = info.PassengerVIPs,
                PassengerWanted = info.PassengerWanted,
                PassengerType = info.PassengerType,

                Wing = info.Wing,
                Target = info.Target,
                TargetType = info.TargetType,
                TargetFaction = info.TargetFaction,
                KillCount = info.KillCount,
                Donation = info.Donation,
                Donated = info.Donated
            });
        }

        public static void HandleMissionCompletedEvent(MissionCompletedEvent.MissionCompletedEventArgs info)
        {
            MissionList.RemoveAll(x => x.MissionID == info.MissionID);
        }

        public static void HandleMissionFailedEvent(MissionFailedEvent.MissionFailedEventArgs info)
        {
            MissionList.RemoveAll(x => x.MissionID == info.MissionID);
        }

    }
}
