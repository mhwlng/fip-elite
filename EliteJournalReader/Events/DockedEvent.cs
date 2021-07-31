using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //    When written: when landing at landing pad in a space station, outpost, or surface settlement
    //    Parameters:
    //•	StationName: name of station
    //•	MarketID
    //•	SystemAddress
    //•	StationType: type of station
    //•	StarSystem: name of system
    //•	CockpitBreach:true (only if landing with breached cockpit)
    //•	StationFaction: station’s controlling faction
    //•	FactionState
    //•	StationAllegiance
    //•	StationEconomy : (station's primary economy)
    //•	StationEconomies: (array of name and proportion values)
    //•	StationGovernment
    //•	DistFromStarLS
    //•	StationServices: (Array of strings)
    //•	Wanted: (only if docking when wanted locally)
    //•	ActiveFine: true (if any fine is active)
    //The ‘anonymous docking’ protocol comes into effect if you’re either Wanted(ie have a local bounty) or have an ActiveFine


    //{ "timestamp":"2021-07-03T08:06:07Z", "event":"Docked", "StationName":"Skovoroda Hospitality Site", "StationType":"OnFootSettlement", "Taxi":true, "Multicrew":false, "StarSystem":"Shinrarta Dezhra", "SystemAddress":3932277478106, "MarketID":3866946816, "StationFaction":{ "Name":"Future of Arro Naga" }, 
    //"StationGovernment":"$government_Democracy;", "StationGovernment_Localised":"Democracy", "StationAllegiance":"Federation", "StationServices":[ "dock", "autodock", "blackmarket", "commodities", "contacts", "missions", "crewlounge", "refuel", "repair", "engineer", "missionsgenerated", "flightcontroller", "stationoperations", "searchrescue", "stationMenu" ], 
    //"StationEconomy":"$economy_Tourism;", "StationEconomy_Localised":"Tourism",
    //"StationEconomies":[ { "Name":"$economy_Tourism;", "Name_Localised":"Tourism", "Proportion":1.000000 } ],
    //"DistFromStarLS":3462.796521,
    //"LandingPads":{ "Small":1, "Medium":0, "Large":1 } }

    public class DockedEvent : JournalEvent<DockedEvent.DockedEventArgs>
    {
        public DockedEvent() : base("Docked") { }

        public class DockedEventArgs : JournalEventArgs
        {
            public bool Taxi { get; set; }
            public bool Multicrew { get; set; }

            public string StarSystem { get; set; }
            public string StationName { get; set; }
            public long SystemAddress { get; set; }
            public long MarketID { get; set; }
            public string StationType { get; set; }
            public bool CockpitBreach { get; set; } = false;
            public Faction StationFaction { get; set; }
            public string StationAllegiance { get; set; }
            public string StationEconomy { get; set; }
            public string StationEconomy_Localised { get; set; }
            public Economy[] StationEconomies { get; set; }
            public string StationGovernment { get; set; }
            public string StationGovernment_Localised { get; set; }
            public double? DistFromStarLS { get; set; }
            public string[] StationServices { get; set; }
            public bool Wanted { get; set; } = false;
            public bool ActiveFine { get; set; } = false;
            public LandingPad LandingPads { get; set; }

            public struct Economy
            {
                public string Name { get; set; }
                public string Name_Localised { get; set; }
                public double Proportion { get; set; }
            }

            public struct LandingPad
            {
                public int Small { get; set; }
                public int Medium { get; set; }
                public int Large { get; set; }
            }


            public override JournalEventArgs Clone()
            {
                var clone = (DockedEventArgs)base.Clone();
                clone.StationFaction = StationFaction?.Clone();
                return clone;
            }
        }
    }
}
