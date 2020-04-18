using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteJournalReader;
using EliteJournalReader.Events;

namespace Elite
{
    public static class Cargo
    {
        public class CargoItem
        {
            public string Name { get; set; }

            public int Count { get; set; }
            public int Stolen { get; set; }

            public long MissionID { get; set; }

            public string MissionName { get; set; } // only filled in FipPanel.cs
            public string System { get; set; } // only filled in FipPanel.cs
            public string Station { get; set; } // only filled in FipPanel.cs

        }

        public static Dictionary<string, CargoItem> CargoList = new Dictionary<string, CargoItem>();
        
        public static void HandleCargoEvent(CargoEvent.CargoEventArgs info)
        {
            if (info.Vessel == "Ship")
            {
                CargoList = new Dictionary<string, CargoItem>();

                if (info.Inventory == null)
                {
                    info = App.watcher.ReadCargoJson();
                }

                if (info.Inventory != null)
                {
                    foreach (var e in info.Inventory)
                    {
                        CargoList.Add(e.Name + "_" + e.MissionID,
                            new CargoItem
                            {
                                Name = e.Name_Localised ??
                                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower()),
                                Count = e.Count,
                                Stolen = e.Stolen,
                                MissionID = e.MissionID
                            });
                    }
                }
            }
        }

        public static void HandleDiedEvent(DiedEvent.DiedEventArgs info)
        {
            CargoList = new Dictionary<string, CargoItem>();
        }

        public static void HandleEngineerContributionEvent(EngineerContributionEvent.EngineerContributionEventArgs info)
        {
            // gets handled in Cargo event

            //{ "timestamp":"2019-12-28T09:37:28Z", "event":"EngineerContribution", "Engineer":"Bill Turner", "EngineerID":300010, "Type":"Commodity", "Commodity":"bromellite", "Quantity":5, "TotalQuantity":5 }
            //{ "timestamp":"2019-12-28T09:37:31Z", "event":"Cargo", "Vessel":"Ship", "Count":100 }
        }

        public static void HandleEjectCargoEvent(EjectCargoEvent.EjectCargoEventArgs info)
        {
            // gets handled in Cargo event

            //{ "timestamp":"2020-01-25T09:45:40Z", "event":"EjectCargo", "Type":"biowaste", "Count":1, "Abandoned":false }
            //{ "timestamp":"2020-01-25T09:45:42Z", "event":"Cargo", "Vessel":"Ship", "Count":0 }
        }

        public static void HandleCollectCargoEvent(CollectCargoEvent.CollectCargoEventArgs info)
        {
            // gets handled in Cargo event

            //{ "timestamp":"2019-07-07T08:29:45Z", "event":"CollectCargo", "Type":"drones", "Type_Localised":"Limpet", "Stolen":false }
            //{ "timestamp":"2019-07-07T08:29:47Z", "event":"Cargo", "Vessel":"Ship", "Count":128 }
        }

        public static void HandleMarketBuyEvent(MarketBuyEvent.MarketBuyEventArgs info)
        {
            // gets handled in Cargo event
        }

        public static void HandleMiningRefinedEvent(MiningRefinedEvent.MiningRefinedEventArgs info)
        {
            // gets handled in Cargo event
        }

        public static void HandleMarketSellEvent(MarketSellEvent.MarketSellEventArgs info)
        {
            // gets handled in Cargo event
        }

        public static void HandleCargoDepotEvent(CargoDepotEvent.CargoDepotEventArgs info)
        {
            // gets handled in Cargo event
        }

        public static void HandleMissionsEvent(MissionsEvent.MissionsEventArgs info)
        {
            // gets handled in Cargo event
        }

        public static void HandleMissionAcceptedEvent(MissionAcceptedEvent.MissionAcceptedEventArgs info)
        {
            // gets handled in Cargo event

            //{ "timestamp":"2019-12-27T08:59:43Z", "event":"MissionAccepted", "Faction":"Alioth Independents", "Name":"Mission_Delivery_Boom", "LocalisedName":"Boom time delivery of 56 units of Biowaste", "Commodity":"$Biowaste_Name;", "Commodity_Localised":"Biowaste", "Count":56, "TargetFaction":"Sons of Icarus", "DestinationSystem":"He Bo", "DestinationStation":"Krylov Ring", "Expiry":"2019-12-28T08:54:38Z", "Wing":false, "Influence":"++", "Reputation":"++", "Reward":714796, "MissionID":533221880 }
            //{ "timestamp":"2019-12-27T08:59:48Z", "event":"CargoDepot", "MissionID":533221880, "UpdateType":"Collect", "CargoType":"Biowaste", "Count":56, "StartMarketID":128141304, "EndMarketID":3227901184, "ItemsCollected":56, "ItemsDelivered":0, "TotalItemsToDeliver":56, "Progress":1.000000 }
            //{ "timestamp":"2019-12-27T08:59:50Z", "event":"Cargo", "Vessel":"Ship", "Count":56 }
        }

        public static void HandleMissionCompletedEvent(MissionCompletedEvent.MissionCompletedEventArgs info)
        {
            // gets handled in Cargo event

            //{ "timestamp":"2019-12-27T09:07:41Z", "event":"CargoDepot", "MissionID":533221880, "UpdateType":"Deliver", "CargoType":"Biowaste", "Count":56, "StartMarketID":128141304, "EndMarketID":3227901184, "ItemsCollected":56, "ItemsDelivered":56, "TotalItemsToDeliver":56, "Progress":0.000000 }
            //{ "timestamp":"2019-12-27T09:07:44Z", "event":"Cargo", "Vessel":"Ship", "Count":0 }
            //{ "timestamp":"2019-12-27T09:07:52Z", "event":"MissionCompleted", "Faction":"Alioth Independents", "Name":"Mission_Delivery_Boom_name", "MissionID":533221880, "Commodity":"$Biowaste_Name;", "Commodity_Localised":"Biowaste", "Count":56, "TargetFaction":"Sons of Icarus", "DestinationSystem":"He Bo", "DestinationStation":"Krylov Ring", "Reward":254796, "MaterialsReward":[ { "Name":"MechanicalComponents", "Name_Localised":"Mechanical Components", "Category":"$MICRORESOURCE_CATEGORY_Manufactured;", "Category_Localised":"Manufactured", "Count":3 } ], "FactionEffects":[ { "Faction":"Sons of Icarus", "Effects":[ { "Effect":"$MISSIONUTIL_Interaction_Summary_Outbreak_up;", "Effect_Localised":"Without medical support, $#MinorFaction; may soon be forced to declare a state of outbreak in the $#System; system.", "Trend":"UpBad" } ], "Influence":[ { "SystemAddress":332023202996, "Trend":"UpGood", "Influence":"+" } ], "ReputationTrend":"UpGood", "Reputation":"++" }, { "Faction":"Alioth Independents", "Effects":[ { "Effect":"$MISSIONUTIL_Interaction_Summary_Outbreak_down;", "Effect_Localised":"With fewer reported cases of illness, $#MinorFaction; hope they have prevented a full outbreak in the $#System; system.", "Trend":"DownGood" } ], "Influence":[ { "SystemAddress":1109989017963, "Trend":"UpGood", "Influence":"++" } ], "ReputationTrend":"UpGood", "Reputation":"++" } ] }
        }

        public static void HandleMissionAbandonedEvent(MissionAbandonedEvent.MissionAbandonedEventArgs info)
        {
            // TODO ???????????

            //{ "timestamp":"2018-11-04T08:27:58Z", "event":"MissionAbandoned", "Name":"Mission_Delivery_Boom_name", "MissionID":430798877 }
        }

        public static void HandleMissionFailedEvent(MissionFailedEvent.MissionFailedEventArgs info)
        {
            // TODO ???????????

        }

        /*
        public static void HandleMissionExpiredEvent(MissionExpiredEvent.MissionExpiredEventArgs info)
        {
         
        }*/



    }
}
