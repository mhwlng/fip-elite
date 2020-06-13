using System;
using System.Collections.Generic;
using System.Globalization;
using Elite.RingBuffer;
using EliteJournalReader;
using EliteJournalReader.Events;
// ReSharper disable StringLiteralTypo


namespace Elite
{
    public class Data
    {

        public static Dictionary<HotspotSystems.MaterialTypes, List<HotspotSystems.HotspotSystemData>> NearbyHotspotSystemsList = new Dictionary<HotspotSystems.MaterialTypes, List<HotspotSystems.HotspotSystemData>>
        {
            {HotspotSystems.MaterialTypes.Painite, new List<HotspotSystems.HotspotSystemData>()},
            {HotspotSystems.MaterialTypes.LTD, new List<HotspotSystems.HotspotSystemData>()}
        };


        public static Dictionary<MiningStations.MaterialTypes, List<MiningStations.MiningStationData>> NearbyMiningStationsList = new Dictionary<MiningStations.MaterialTypes, List<MiningStations.MiningStationData>>
        {
            {MiningStations.MaterialTypes.Painite, new List<MiningStations.MiningStationData>()},
            {MiningStations.MaterialTypes.LTD, new List<MiningStations.MiningStationData>()},
            {MiningStations.MaterialTypes.TritiumBuy, new List<MiningStations.MiningStationData>()},
            {MiningStations.MaterialTypes.TritiumSell, new List<MiningStations.MiningStationData>()}
        };

        public static Dictionary<Station.PoiTypes, List<StationData>> NearbyStationList = new Dictionary<Station.PoiTypes, List<StationData>>
        {
            {Station.PoiTypes.InterStellarFactors, new List<StationData>()},
            {Station.PoiTypes.RawMaterialTraders, new List<StationData>()},
            {Station.PoiTypes.ManufacturedMaterialTraders, new List<StationData>()},
            {Station.PoiTypes.EncodedDataTraders, new List<StationData>()},
            {Station.PoiTypes.HumanTechnologyBrokers, new List<StationData>()},
            {Station.PoiTypes.GuardianTechnologyBrokers, new List<StationData>()}
        };

        public static Dictionary<Station.PowerTypes, List<StationData>> NearbyPowerStationList = new Dictionary<Station.PowerTypes, List<StationData>>
        {
            {Station.PowerTypes.AislingDuval, new List<StationData>()},
            {Station.PowerTypes.ArchonDelaine, new List<StationData>()},
            {Station.PowerTypes.ArissaLavignyDuval, new List<StationData>()},
            {Station.PowerTypes.DentonPatreus, new List<StationData>()},
            {Station.PowerTypes.EdmundMahon, new List<StationData>()},
            {Station.PowerTypes.FeliciaWinters, new List<StationData>()},
            {Station.PowerTypes.LiYongRui, new List<StationData>()},
            {Station.PowerTypes.PranavAntal, new List<StationData>()},
            {Station.PowerTypes.YuriGrom, new List<StationData>()},
            {Station.PowerTypes.ZacharyHudson, new List<StationData>()},
            {Station.PowerTypes.ZeminaTorval, new List<StationData>()}
        };

        public static List<CnbSystems.CnbSystemData> NearbyCnbSystemsList = new List<CnbSystems.CnbSystemData>();


        public static  RingBuffer<string> EventHistory = new RingBuffer<string>(50, true);

        private static string npcSpeechBy(string from, string message)
        {
            string by;
            if (message.StartsWith("$AmbushedPilot_"))
            {
                by = "Ambushed pilot";
            }
            else if (message.StartsWith("$BountyHunter"))
            {
                by = "Bounty hunter";
            }
            else if (message.StartsWith("$CapShip") || message.StartsWith("$FEDCapShip"))
            {
                by = "Capital ship";
            }
            else if (message.StartsWith("$CargoHunter"))
            {
                by = "Cargo hunter"; // Mission specific
            }
            else if (message.StartsWith("$Commuter"))
            {
                by = "Civilian pilot";
            }
            else if (message.StartsWith("$ConvoyExplorers"))
            {
                by = "Exploration convoy";
            }
            else if (message.StartsWith("$ConvoyWedding"))
            {
                by = "Wedding convoy";
            }
            else if (message.StartsWith("$CruiseLiner"))
            {
                by = "Cruise liner";
            }
            else if (message.StartsWith("$Escort"))
            {
                by = "Escort";
            }
            else if (message.StartsWith("$Hitman"))
            {
                by = "Hitman";
            }
            else if (message.StartsWith("$Messenger"))
            {
                by = "Messenger";
            }
            else if (message.StartsWith("$Military"))
            {
                by = "Military";
            }
            else if (message.StartsWith("$Miner"))
            {
                by = "Miner";
            }
            else if (message.StartsWith("$PassengerHunter"))
            {
                by = "Passenger hunter"; // Mission specific
            }
            else if (message.StartsWith("$PassengerLiner"))
            {
                by = "Passenger liner";
            }
            else if (message.StartsWith("$Pirate"))
            {
                by = "Pirate";
            }
            else if (message.StartsWith("$Police"))
            {
                // Police messages appear to be re-used by bounty hunters.  Check from to see if it really is police
                by = from.Contains("Police") ? "Police" : "Bounty hunter";
            }
            else if (message.StartsWith("$PowersAssassin"))
            {
                by = "Rival power's agent"; // Power play specific
            }
            else if (message.StartsWith("$PowersPirate"))
            {
                by = "Rival power's agent"; // Power play specific
            }
            else if (message.StartsWith("$PowersSecurity"))
            {
                by = "Rival power's agent"; // Power play specific
            }
            else if (message.StartsWith("$Propagandist"))
            {
                by = "Propagandist";
            }
            else if (message.StartsWith("$Protester"))
            {
                by = "Protester";
            }
            else if (message.StartsWith("$Refugee"))
            {
                by = "Refugee";
            }
            else if (message.StartsWith("$Smuggler"))
            {
                by = "Civilian pilot"; // We shouldn't recognize a smuggler without a cargo scan
            }
            else if (message.StartsWith("$StarshipOne"))
            {
                by = "Starship One";
            }
            else if (message.Contains("_SearchandRescue_"))
            {
                by = "Search and rescue";
            }
            else
            {
                by = "NPC";
            }

            return by;
        }

        public static void HandleJson()
        {
            lock (App.RefreshJsonLock)
            {
                NearbyStationList[Station.PoiTypes.InterStellarFactors] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullStationList[Station.PoiTypes.InterStellarFactors]);
                NearbyStationList[Station.PoiTypes.RawMaterialTraders] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullStationList[Station.PoiTypes.RawMaterialTraders]);
                NearbyStationList[Station.PoiTypes.ManufacturedMaterialTraders] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullStationList[Station.PoiTypes.ManufacturedMaterialTraders]);
                NearbyStationList[Station.PoiTypes.EncodedDataTraders] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullStationList[Station.PoiTypes.EncodedDataTraders]);
                NearbyStationList[Station.PoiTypes.HumanTechnologyBrokers] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullStationList[Station.PoiTypes.HumanTechnologyBrokers]);
                NearbyStationList[Station.PoiTypes.GuardianTechnologyBrokers] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullStationList[Station.PoiTypes.GuardianTechnologyBrokers]);

                NearbyPowerStationList[Station.PowerTypes.AislingDuval] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.AislingDuval]);
                NearbyPowerStationList[Station.PowerTypes.ArchonDelaine] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.ArchonDelaine]);
                NearbyPowerStationList[Station.PowerTypes.ArissaLavignyDuval] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.ArissaLavignyDuval]);
                NearbyPowerStationList[Station.PowerTypes.DentonPatreus] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.DentonPatreus]);
                NearbyPowerStationList[Station.PowerTypes.EdmundMahon] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.EdmundMahon]);
                NearbyPowerStationList[Station.PowerTypes.FeliciaWinters] =
                    Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.FeliciaWinters]);
                NearbyPowerStationList[Station.PowerTypes.LiYongRui] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.LiYongRui]);
                NearbyPowerStationList[Station.PowerTypes.PranavAntal] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.PranavAntal]);
                NearbyPowerStationList[Station.PowerTypes.YuriGrom] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.YuriGrom]);
                NearbyPowerStationList[Station.PowerTypes.ZacharyHudson] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.ZacharyHudson]);
                NearbyPowerStationList[Station.PowerTypes.ZeminaTorval] = Station.GetNearestStations(LocationData.StarPos, Station.FullPowerStationList[Station.PowerTypes.ZeminaTorval]);

                NearbyCnbSystemsList = CnbSystems.GetNearestCnbSystems(LocationData.StarPos);

                NearbyHotspotSystemsList[HotspotSystems.MaterialTypes.Painite] = HotspotSystems.GetNearestHotspotSystems(LocationData.StarPos, HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.Painite]);
                NearbyHotspotSystemsList[HotspotSystems.MaterialTypes.LTD] = HotspotSystems.GetNearestHotspotSystems(LocationData.StarPos, HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.LTD]);

                NearbyMiningStationsList[MiningStations.MaterialTypes.Painite] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.Painite], true);
                NearbyMiningStationsList[MiningStations.MaterialTypes.LTD] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.LTD], true);
                NearbyMiningStationsList[MiningStations.MaterialTypes.TritiumBuy] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.TritiumBuy], false);
                NearbyMiningStationsList[MiningStations.MaterialTypes.TritiumSell] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.TritiumSell], true);

            }
        }

        public class Commander
        {
            public string Name { get; set; } = "";
            public uint Credits { get; set; }

            public double FederationReputation { get; set; } = -99999;
            public double AllianceReputation { get; set; } = -99999;
            public double EmpireReputation { get; set; } = -99999;
            public string FederationReputationState { get; set; }
            public string AllianceReputationState { get; set; }
            public string EmpireReputationState { get; set; }

            public string CqcRank { get; set; }
            public string EmpireRank { get; set; }
            public string FederationRank { get; set; }
            public string CombatRank { get; set; }
            public string TradeRank { get; set; }
            public string ExplorationRank { get; set; }

            public int EmpireRankProgress { get; set; }
            public int FederationRankProgress { get; set; }
            public int CombatRankProgress { get; set; }
            public int TradeRankProgress { get; set; }
            public int ExplorationRankProgress { get; set; }
            public int CqcRankProgress { get; set; }
        }

        public class Dock
        {
            public int LandingPad { get; set; } = -1;
            public string Type { get; set; } = "";
            public string Government { get; set; } = "";
            public string Allegiance { get; set; } = "";
            public string Faction { get; set; } = "";
            public string Economy { get; set; } = "";
            public string Services { get; set; } = "";
            public double? DistFromStarLs { get; set; } = -1;
        }

        public class Target
        {
            public long ScanStage { get; set; }
            public bool TargetLocked { get; set; }

            public string PilotNameLocalised { get; set; }
            public string PilotRank { get; set; }
            public string Ship { get; set; }
            public string Faction { get; set; }
            public string LegalStatus { get; set; }

            public string Power { get; set; }

            public string SubsystemLocalised { get; set; }

            public long Bounty { get; set; }
            public double HullHealth { get; set; }
            public double ShieldHealth { get; set; }
            public double SubsystemHealth { get; set; }

        }

        public class Location
        {
            public bool StartJump { get; set; }

            public string JumpType { get; set; }

            public string JumpToSystem { get; set; }

            public string JumpToStarClass { get; set; }

            public int RemainingJumpsInRoute { get; set; }
            public string StarClass { get; set; }

            public string FsdTargetName { get; set; }

            public string Settlement { get; set; }

            public string StarSystem { get; set; }
            public string Station { get; set; }

            public string Body { get; set; }
            public string BodyType { get; set; }

            public string SystemAllegiance { get; set; }
            public string SystemFaction { get; set; }
            public string SystemSecurity { get; set; }
            public string SystemEconomy { get; set; }
            public string SystemGovernment { get; set; }

            public long? Population { get; set; }

            public string PowerplayState { get; set; }

            public string Powers { get; set; }

            public bool HideBody { get; set; }

            public List<double> StarPos { get; set; } // array[x, y, z], in light years
        }

        public class Status
        {
            public bool Docked { get; set; }
            public bool Landed { get; set; }
            public bool LandingGearDown { get; set; }
            public bool ShieldsUp { get; set; }
            public bool Supercruise { get; set; }
            public bool FlightAssistOff { get; set; }
            public bool HardpointsDeployed { get; set; }
            public bool InWing { get; set; }
            public bool LightsOn { get; set; }
            public bool CargoScoopDeployed { get; set; }
            public bool SilentRunning { get; set; }
            public bool ScoopingFuel { get; set; }
            public bool SrvHandbrake { get; set; }
            public bool SrvTurret { get; set; }
            public bool SrvUnderShip { get; set; }
            public bool SrvDriveAssist { get; set; }
            public bool FsdMassLocked { get; set; }
            public bool FsdCharging { get; set; }
            public bool FsdCooldown { get; set; }
            public bool LowFuel { get; set; }
            public bool Overheating { get; set; }
            public bool HasLatLong { get; set; }
            public bool IsInDanger { get; set; }
            public bool BeingInterdicted { get; set; }
            public bool InMainShip { get; set; }
            public bool InFighter { get; set; }
            public bool InSRV { get; set; }
            public bool HudInAnalysisMode { get; set; }
            public bool NightVision { get; set; }
            public bool AltitudeFromAverageRadius { get; set; }
            public bool FsdJump { get; set; }
            public bool SrvHighBeam { get; set; }

            public StatusFuel Fuel { get; set; } = new StatusFuel();

        
            public double Cargo { get; set; }
            public string LegalState { get; set; }
            public double JumpRange { get; set; }

            public int Firegroup { get; set; }
            public string GuiFocus { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Altitude { get; set; }
            public double Heading { get; set; }
            public string BodyName { get; set; }
            public double PlanetRadius { get; set; }
        }


        public static Commander CommanderData = new Commander();
        public static Target TargetData = new Target();
        public static Location LocationData = new Location();
        public static Dock DockData = new Dock();

        public static Status StatusData = new Status();


        public static void HandleStatusEvents(object sender, StatusFileEvent evt)
        {
            StatusData.ShieldsUp = (evt.Flags & StatusFlags.ShieldsUp) != 0;
            StatusData.FlightAssistOff = (evt.Flags & StatusFlags.FlightAssistOff) != 0;
            StatusData.InWing = (evt.Flags & StatusFlags.InWing) != 0;
            StatusData.LightsOn = (evt.Flags & StatusFlags.LightsOn) != 0;
            StatusData.NightVision = (evt.Flags & StatusFlags.NightVision) != 0;
            StatusData.AltitudeFromAverageRadius = (evt.Flags & StatusFlags.AltitudeFromAverageRadius) != 0;
            StatusData.LowFuel = (evt.Flags & StatusFlags.LowFuel) != 0;
            StatusData.Overheating = (evt.Flags & StatusFlags.Overheating) != 0;
            StatusData.HasLatLong = (evt.Flags & StatusFlags.HasLatLong) != 0;
            StatusData.InMainShip = (evt.Flags & StatusFlags.InMainShip) != 0;
            StatusData.InFighter = (evt.Flags & StatusFlags.InFighter) != 0;
            StatusData.InSRV = (evt.Flags & StatusFlags.InSRV) != 0;
            StatusData.SrvDriveAssist = (evt.Flags & StatusFlags.SrvDriveAssist) != 0 && StatusData.InSRV;
            StatusData.SrvUnderShip = (evt.Flags & StatusFlags.SrvUnderShip) != 0 && StatusData.InSRV;
            StatusData.SrvTurret = (evt.Flags & StatusFlags.SrvTurret) != 0 && StatusData.InSRV;
            StatusData.SrvHandbrake = (evt.Flags & StatusFlags.SrvHandbrake) != 0 && StatusData.InSRV;
            StatusData.SrvHighBeam = (evt.Flags & StatusFlags.SrvHighBeam) != 0 && StatusData.InSRV;

            StatusData.Docked = (evt.Flags & StatusFlags.Docked) != 0;
            StatusData.Landed = (evt.Flags & StatusFlags.Landed) != 0;
            StatusData.LandingGearDown = (evt.Flags & StatusFlags.LandingGearDown) != 0;
            StatusData.CargoScoopDeployed = (evt.Flags & StatusFlags.CargoScoopDeployed) != 0;
            StatusData.SilentRunning = (evt.Flags & StatusFlags.SilentRunning) != 0;
            StatusData.ScoopingFuel = (evt.Flags & StatusFlags.ScoopingFuel) != 0;
            StatusData.IsInDanger = (evt.Flags & StatusFlags.IsInDanger) != 0;
            StatusData.BeingInterdicted = (evt.Flags & StatusFlags.BeingInterdicted) != 0;
            StatusData.HudInAnalysisMode = (evt.Flags & StatusFlags.HudInAnalysisMode) != 0;

            StatusData.FsdMassLocked = (evt.Flags & StatusFlags.FsdMassLocked) != 0;
            StatusData.FsdCharging = (evt.Flags & StatusFlags.FsdCharging) != 0;
            StatusData.FsdCooldown = (evt.Flags & StatusFlags.FsdCooldown) != 0;

            StatusData.Supercruise = (evt.Flags & StatusFlags.Supercruise) != 0;
            StatusData.FsdJump = (evt.Flags & StatusFlags.FsdJump) != 0;
            StatusData.HardpointsDeployed = (evt.Flags & StatusFlags.HardpointsDeployed) != 0 && !StatusData.Supercruise && !StatusData.FsdJump;

            StatusData.Fuel = evt.Fuel ?? new StatusFuel();

            StatusData.Cargo = evt.Cargo;

            StatusData.LegalState = evt.LegalState;

            StatusData.Firegroup = evt.Firegroup;
            StatusData.GuiFocus = evt.GuiFocus.ToString();
            StatusData.Latitude = evt.Latitude;
            StatusData.Longitude = evt.Longitude;
            StatusData.Altitude = evt.Altitude;
            StatusData.Heading = evt.Heading;
            StatusData.BodyName = evt.BodyName;
            StatusData.PlanetRadius = evt.PlanetRadius;


            var shipData = Ships.GetCurrentShip();
            if (shipData != null)
            {
                shipData.CurrentFuelMain = evt.Fuel?.FuelMain ?? 0;
                shipData.CurrentFuelReservoir = evt.Fuel?.FuelReservoir ?? 0;
                shipData.CurrentCargo = evt.Cargo;
            }

            App.FipHandler.RefreshDevicePages();
        }


        private static string UpdateReputationState(double reputation)
        {
            if (reputation >= -100 && reputation <= -91)
            {
                return "Hostile";
            }
            if (reputation >= -90 && reputation <= -36)
            {
                return "Unfriendly";
            }
            if (reputation >= -35 && reputation <= 3)
            {
                return "Neutral";
            }
            if (reputation >= 4 && reputation <= 34)
            {
                return "Cordial";
            }
            if (reputation >= 35 && reputation <= 89)
            {
                return "Friendly";
            }
            if (reputation >= 90 && reputation <= 100)
            {
                return "Allied";
            }

            return "";
        }

        public static void HandleNavRouteEvent(object sender, NavRouteEvent.NavRouteEventArgs e)
        {
            if (e?.Route == null) return;

            Route.HandleRouteEvent(e);
        }

        public static void HandleCargoEvent(object sender, CargoEvent.CargoEventArgs e)
        {
            if (e?.Inventory == null) return;

            Cargo.HandleCargoEvent(e);

        }

        public static void HandleEliteEvents(object sender, JournalEventArgs e)
        {
            var evt = e.OriginalEvent.Value<string>("event");

            if (string.IsNullOrWhiteSpace(evt))
            {
                return;
            }

            if (evt != "FSSSignalDiscovered" && evt != "FSSDiscoveryScan" && evt != "Music" && evt != "ReceiveText")
            {
                EventHistory.Put(DateTime.Now.ToLongTimeString() + " : " + evt);
            }

            var shipData = Ships.GetCurrentShip();

            switch (evt)
            {
                // ------ AT STARTUP -------------

                case "LoadGame":
                    //When written: at startup, when loading from main menu into game

                    var loadGameInfo = (LoadGameEvent.LoadGameEventArgs) e;

                    //ShipIdent

                    //FID
                    //Horizons
                    //StartLanded
                    //StartDead
                    //GameMode
                    //Group
                    //Loan

                    //FuelLevel

                    Ships.HandleLoadGame(loadGameInfo.ShipID, loadGameInfo.Ship, loadGameInfo.ShipName);

                    CommanderData.Name = loadGameInfo.Commander;
                    CommanderData.Credits = Convert.ToUInt32(loadGameInfo.Credits);

                    LocationData.Settlement = "";

                    break;

                case "Rank":
                    //When written: at startup
                    var rankInfo = (RankEvent.RankEventArgs) e;

                    CommanderData.CqcRank = rankInfo.CQC.ToString();

                    CommanderData.EmpireRank = rankInfo.Empire.ToString();
                    CommanderData.FederationRank = rankInfo.Federation.ToString();
                    CommanderData.CombatRank = rankInfo.Combat.ToString();
                    CommanderData.TradeRank = rankInfo.Trade.ToString();
                    CommanderData.ExplorationRank = rankInfo.Explore.ToString();

                    break;

                case "Reputation":
                    //When written: at startup(after Rank and Progress)

                    var reputationInfo = (ReputationEvent.ReputationEventArgs) e;

                    CommanderData.FederationReputation = reputationInfo.Federation;
                    CommanderData.AllianceReputation = reputationInfo.Alliance;
                    CommanderData.EmpireReputation = reputationInfo.Empire;

                    CommanderData.FederationReputationState =
                        UpdateReputationState(CommanderData.FederationReputation);
                    CommanderData.AllianceReputationState =
                        UpdateReputationState(CommanderData.AllianceReputation);
                    CommanderData.EmpireReputationState = UpdateReputationState(CommanderData.EmpireReputation);

                    break;

                case "Progress":
                    //When written: at startup

                    var progressInfo = (ProgressEvent.ProgressEventArgs) e;

                    CommanderData.EmpireRankProgress = progressInfo.Empire;
                    CommanderData.FederationRankProgress = progressInfo.Federation;
                    CommanderData.CombatRankProgress = progressInfo.Combat;
                    CommanderData.TradeRankProgress = progressInfo.Trade;
                    CommanderData.ExplorationRankProgress = progressInfo.Explore;

                    CommanderData.CqcRankProgress = progressInfo.CQC;

                    break;

                case "Commander":
                    //When written: at the start of the LoadGame process

                    var commanderInfo = (CommanderEvent.CommanderEventArgs) e;

                    //FID

                    CommanderData.Name = commanderInfo.Name;
                    break;

                case "SetUserShipName":
                    //When written: when assigning a name to the ship in Starport Services

                    var setUserShipNameInfo = (SetUserShipNameEvent.SetUserShipNameEventArgs) e;

                    //ShipID

                    Ships.HandleSetUserShipName(setUserShipNameInfo.ShipID, setUserShipNameInfo.UserShipName, setUserShipNameInfo.Ship);

                    break;

                case "Location":
                    //When written: at startup, or when being resurrected at a station

                    var locationInfo = (LocationEvent.LocationEventArgs)e;

                    Ships.HandleShipLocation(locationInfo.Docked, locationInfo.StarSystem, locationInfo.StationName, locationInfo.StarPos.ToList());

                    //Docked
                    //Latitude
                    //Longitude
                    //StationName
                    //StationType
                    //StationGovernment
                    //StationAllegiance
                    //StationServices
                    //SystemSecondEconomy_Localised
                    //Wanted
                    //Powers
                    //Factions
                    //Conflicts

                    LocationData.StarSystem = locationInfo.StarSystem;

                    LocationData.StarPos = locationInfo.StarPos.ToList();

                    Ships.HandleShipDistance(LocationData.StarPos);

                    Poi.NearbyPoiList = Poi.GetNearestPois(LocationData.StarPos);

                    HandleJson();

                    LocationData.SystemAllegiance = locationInfo.SystemAllegiance;
                    LocationData.SystemFaction = locationInfo.SystemFaction?.Name;
                    LocationData.SystemSecurity = locationInfo.SystemSecurity_Localised;
                    LocationData.SystemEconomy = locationInfo.SystemEconomy_Localised;
                    LocationData.SystemGovernment = locationInfo.SystemGovernment_Localised;
                    LocationData.Population = locationInfo.Population;
                    LocationData.Body = locationInfo.Body;
                    LocationData.BodyType = locationInfo.BodyType.ToString();

                    LocationData.PowerplayState = locationInfo.PowerplayState.ToString();
                    LocationData.Powers = string.Join(",", locationInfo.Powers);

                    LocationData.HideBody = false;

                    break;

                case "Missions":
                    //When written: at startup
                    var missionsInfo = (MissionsEvent.MissionsEventArgs)e;

                    Missions.HandleMissionsEvent(missionsInfo);

                    Cargo.HandleMissionsEvent(missionsInfo);

                    break;

                case "Loadout":
                    //When written: at startup, when loading from main menu, or when switching ships, 

                    var loadoutInfo = (LoadoutEvent.LoadoutEventArgs)e;

                    Ships.HandleLoadout(loadoutInfo);
                    Module.HandleLoadout(loadoutInfo);

                    break;

                case "ModuleRetrieve":
                    //When Written: when fetching a previously stored module
                    var moduleRetrieveInfo = (ModuleRetrieveEvent.ModuleRetrieveEventArgs)e;

                    Module.HandleModuleRetrieve(moduleRetrieveInfo);

                    break;

                case "ModuleBuy":
                    //When Written: when buying a module in outfitting
                    var moduleBuyInfo = (ModuleBuyEvent.ModuleBuyEventArgs) e;

                    Module.HandleModuleBuy(moduleBuyInfo);

                    break;

                case "ModuleSwap":
                    //When Written: when moving a module to a different slot on the ship
                    var moduleSwapInfo = (ModuleSwapEvent.ModuleSwapEventArgs)e;

                    Module.HandleModuleSwap(moduleSwapInfo);

                    break;

                case "ModuleSell":
                    //When Written: when selling a module in outfitting
                    var moduleSellInfo = (ModuleSellEvent.ModuleSellEventArgs) e;

                    Module.HandleModuleSell(moduleSellInfo);

                    break;

                case "ModuleSellRemote":
                    //When Written: when selling a module in outfitting
                    var moduleSellRemoteInfo = (ModuleSellRemoteEvent.ModuleSellRemoteEventArgs)e;

                    Module.HandleModuleSellRemote(moduleSellRemoteInfo);
                    
                    break;

                case "ModuleStore":
                    //When Written: when fetching a previously stored module
                    var moduleStoreInfo = (ModuleStoreEvent.ModuleStoreEventArgs) e;

                    Module.HandleModuleStore(moduleStoreInfo);

                    break;

                case "MassModuleStore":
                    //When written: when putting multiple modules into storage
                    var massModuleStoreInfo = (MassModuleStoreEvent.MassModuleStoreEventArgs)e;

                    Module.HandleMassModuleStore(massModuleStoreInfo);

                    break;

                case "RefuelAll":
                    //When Written: when refuelling (full tank)
                    var refuelAllInfo = (RefuelAllEvent.RefuelAllEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(refuelAllInfo.Cost);
                    break;

                case "RepairAll":
                    //When Written: when repairing the ship
                    var repairAllInfo = (RepairAllEvent.RepairAllEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(repairAllInfo.Cost);
                    break;

                case "Repair":
                    //When Written: when repairing the ship
                    var repairInfo = (RepairEvent.RepairEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(repairInfo.Cost);
                    break;

                case "BuyTradeData":
                    //When Written: when buying trade data in the galaxy map
                    var buyTradeDataInfo = (BuyTradeDataEvent.BuyTradeDataEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyTradeDataInfo.Cost);
                    break;

                case "BuyExplorationData":
                    //When Written: when buying system data via the galaxy map
                    var buyExplorationDataInfo = (BuyExplorationDataEvent.BuyExplorationDataEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyExplorationDataInfo.Cost);
                    break;

                case "BuyDrones":
                    //When Written: when purchasing drones
                    var buyDronesInfo = (BuyDronesEvent.BuyDronesEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyDronesInfo.TotalCost);
                    break;

                case "BuyAmmo":
                    //When Written: when purchasing ammunition
                    var buyAmmoInfo = (BuyAmmoEvent.BuyAmmoEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyAmmoInfo.Cost);
                    break;

                case "PayBounties":
                    //When written: when paying off bounties
                    var payBountiesInfo = (PayBountiesEvent.PayBountiesEventArgs)e;

                    // shipID

                    CommanderData.Credits -= Convert.ToUInt32(payBountiesInfo.Amount);
                    break;

                case "PayFines":
                    //When Written: when paying fines
                    var payFinesInfo = (PayFinesEvent.PayFinesEventArgs)e;

                    // shipID

                    CommanderData.Credits -= Convert.ToUInt32(payFinesInfo.Amount);
                    break;

                case "ApproachBody":
                    //    When written: when in Supercruise, and distance from planet drops to within the 'Orbital Cruise' zone
                    var approachBodyInfo = (ApproachBodyEvent.ApproachBodyEventArgs) e;

                    LocationData.StarSystem = approachBodyInfo.StarSystem;

                    LocationData.Body = approachBodyInfo.Body;
                    LocationData.BodyType = "Planet";

                    LocationData.HideBody = false;

                    break;

                case "ApproachSettlement":
                    //When written: when approaching a planetary settlement
                    var approachSettlementInfo = (ApproachSettlementEvent.ApproachSettlementEventArgs) e;

                    //Latitude
                    //Longitude

                    LocationData.Body = approachSettlementInfo.BodyName; 

                    LocationData.Station = approachSettlementInfo.Name;

                    LocationData.Settlement = approachSettlementInfo.Name;

                    LocationData.BodyType = "Planet";

                    LocationData.HideBody = false;


                    break;

                case "LeaveBody":
                    //When written: when flying away from a planet, and distance increases above the 'Orbital Cruise' altitude
                    var leaveBodyInfo = (LeaveBodyEvent.LeaveBodyEventArgs) e;

                    //Body

                    LocationData.StarSystem = leaveBodyInfo.StarSystem;

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    LocationData.HideBody = true;

                    break;

                case "Undocked":
                    //When written: liftoff from a landing pad in a station, outpost or settlement
                    //var undockedInfo = (UndockedEvent.UndockedEventArgs) e;

                    //StationName
                    //StationType

                    DockData = new Dock();

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    break;

                case "Docked":
                    //    When written: when landing at landing pad in a space station, outpost, or surface settlement
                    var dockedInfo = (DockedEvent.DockedEventArgs) e;

                    Ships.HandleShipDocked(dockedInfo.StarSystem, dockedInfo.StationName);

                    //CockpitBreach
                    //StationEconomies
                    //Wanted
                    //ActiveFine

                    LocationData.StarSystem = dockedInfo.StarSystem; 

                    LocationData.Station = dockedInfo.StationName;

                    DockData.Type = dockedInfo.StationType;

                    DockData.Government = dockedInfo.StationGovernment_Localised;
                    DockData.Allegiance = dockedInfo.StationAllegiance;
                    DockData.Faction = dockedInfo.StationFaction?.Name;
                    DockData.Economy = dockedInfo.StationEconomy_Localised;
                    DockData.DistFromStarLs = dockedInfo.DistFromStarLS;

                    DockData.Services = string.Join(", ", dockedInfo.StationServices);

                    DockData.LandingPad = -1;

                    if (shipData != null)
                    {
                        shipData.AutomaticDocking = false;
                    }

                    break;

                case "DockingGranted":
                    //When written: when a docking request is granted
                    var dockingGrantedInfo = (DockingGrantedEvent.DockingGrantedEventArgs) e;

                    DockData.Type = dockingGrantedInfo.StationType;

                    LocationData.Body = dockingGrantedInfo.StationName;
                    LocationData.BodyType = "Station";

                    DockData.LandingPad = Convert.ToInt32(dockingGrantedInfo.LandingPad);
                    break;

                case "DockingRequested":

                    //When written: when the player requests docking at a station
                    var dockingRequestedInfo = (DockingRequestedEvent.DockingRequestedEventArgs) e;

                    LocationData.Station = dockingRequestedInfo.StationName;

                    LocationData.Body = dockingRequestedInfo.StationName;
                    LocationData.BodyType = "Station";
                    break;

                case "FSDJump":
                    //When written: when jumping from one star system to another
                    var fsdJumpInfo = (FSDJumpEvent.FSDJumpEventArgs) e;

                    Ships.HandleShipFsdJump(fsdJumpInfo.StarSystem, fsdJumpInfo.StarPos.ToList());

                    //FuelUsed
                    //FuelLevel
                    //BoostUsed
                    //SystemSecondEconomy_Localised
                    //Wanted
                    //Powers

                    LocationData.Body = fsdJumpInfo.Body;  

                    LocationData.StarSystem = fsdJumpInfo.StarSystem;

                    if (StatusData.JumpRange < fsdJumpInfo.JumpDist)
                    {
                        StatusData.JumpRange = fsdJumpInfo.JumpDist;
                    }

                    LocationData.StarPos = fsdJumpInfo.StarPos.ToList();

                    Ships.HandleShipDistance(LocationData.StarPos);

                    History.AddTravelPos(LocationData.StarPos);

                    Poi.NearbyPoiList = Poi.GetNearestPois(LocationData.StarPos);

                    HandleJson();

                    LocationData.StartJump = false;
                    LocationData.JumpToSystem = "";
                    LocationData.JumpToStarClass = "";
                    LocationData.JumpType = "";

                    LocationData.PowerplayState = fsdJumpInfo.PowerplayState.ToString();
                    LocationData.Powers = string.Join(", ", fsdJumpInfo.Powers);

                    LocationData.SystemAllegiance = fsdJumpInfo.SystemAllegiance;
                    LocationData.SystemFaction = fsdJumpInfo.SystemFaction?.Name;
                    LocationData.SystemSecurity = fsdJumpInfo.SystemSecurity_Localised;
                    LocationData.SystemEconomy = fsdJumpInfo.SystemEconomy_Localised;
                    LocationData.SystemGovernment = fsdJumpInfo.SystemGovernment_Localised;
                    LocationData.Population = fsdJumpInfo.Population;

                    break;

                case "StartJump":

                    var startJumpInfo = (StartJumpEvent.StartJumpEventArgs) e;

                    LocationData.StartJump = true;
                    LocationData.JumpType = startJumpInfo.JumpType.ToString();
                    LocationData.JumpToSystem = startJumpInfo.StarSystem;
                    LocationData.JumpToStarClass = startJumpInfo.StarClass;

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    break;

                case "FSDTarget":
                    //When written: when selecting a star system to jump to
                    var fSdTargetInfo = (FSDTargetEvent.FSDTargetEventArgs) e;

                    LocationData.FsdTargetName = fSdTargetInfo.Name;

                    LocationData.RemainingJumpsInRoute = fSdTargetInfo.RemainingJumpsInRoute;

                    LocationData.StarClass = fSdTargetInfo.StarClass;

                    break;

                case "SupercruiseEntry":

                    //When written: entering supercruise from normal space
                    var supercruiseEntryInfo = (SupercruiseEntryEvent.SupercruiseEntryEventArgs) e;

                    LocationData.JumpToSystem = supercruiseEntryInfo.StarSystem;

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    LocationData.HideBody = true;

                    break;

                case "SupercruiseExit":
                    //When written: leaving supercruise for normal space
                    var supercruiseExitInfo = (SupercruiseExitEvent.SupercruiseExitEventArgs) e;

                    LocationData.StarSystem = supercruiseExitInfo.StarSystem; 

                    LocationData.StartJump = false;

                    LocationData.JumpToSystem = "";
                    LocationData.JumpToStarClass = "";
                    LocationData.JumpType = "";

                    LocationData.Body = supercruiseExitInfo.Body;
                    LocationData.BodyType = supercruiseExitInfo.BodyType.ToString();
                    LocationData.HideBody = false;

                    break;

                case "HullDamage":
                    //When written: player was HullDamage by player or npc
                    var hullDamageInfo = (HullDamageEvent.HullDamageEventArgs) e;

                    Ships.HandleHullDamage(hullDamageInfo.Health);
                   
                    break;

                case "ShipTargeted":
                    //    When written: when the current player selects a new target
                    var shipTargetedInfo = (ShipTargetedEvent.ShipTargetedEventArgs) e;

                    TargetData.Bounty = shipTargetedInfo.Bounty;
                    TargetData.Faction = shipTargetedInfo.Faction;
                    TargetData.HullHealth = shipTargetedInfo.HullHealth;
                    TargetData.LegalStatus = shipTargetedInfo.LegalStatus;
                    TargetData.PilotNameLocalised = shipTargetedInfo.PilotName_Localised;
                    TargetData.PilotRank = shipTargetedInfo.PilotRank.ToString();
                    TargetData.ScanStage = shipTargetedInfo.ScanStage;
                    TargetData.ShieldHealth = shipTargetedInfo.ShieldHealth;

                    TargetData.Power = shipTargetedInfo.Power;

                    Ships.ShipsByEliteId.TryGetValue(shipTargetedInfo.Ship?.ToLower() ?? "???", out var targetShip);

                    TargetData.Ship = shipTargetedInfo.Ship_Localised ?? targetShip ?? shipTargetedInfo.Ship;

                    TargetData.SubsystemLocalised = shipTargetedInfo.Subsystem_Localised;
                    TargetData.TargetLocked = shipTargetedInfo.TargetLocked;
                    TargetData.SubsystemHealth = shipTargetedInfo.SubSystemHealth;

                    break;

                case "MissionAccepted":
                    //When Written: when starting a mission 
                    var missionAcceptedInfo = (MissionAcceptedEvent.MissionAcceptedEventArgs) e;

                    Missions.HandleMissionAcceptedEvent(missionAcceptedInfo);

                    Cargo.HandleMissionAcceptedEvent(missionAcceptedInfo);

                    break;

                case "MissionAbandoned":
                    //When Written: when a mission has been abandoned 

                    var missionAbandonedInfo = (MissionAbandonedEvent.MissionAbandonedEventArgs) e;

                    Missions.HandleMissionAbandonedEvent(missionAbandonedInfo);

                    Cargo.HandleMissionAbandonedEvent(missionAbandonedInfo);

                    break;
                case "MissionFailed":
                    //When Written: when a mission has failed 

                    var missionFailedInfo = (MissionFailedEvent.MissionFailedEventArgs) e;

                    Missions.HandleMissionFailedEvent(missionFailedInfo);

                    Cargo.HandleMissionFailedEvent(missionFailedInfo);

                    break;

                case "MissionCompleted":
                    //When Written: when a mission is completed

                    var missionCompletedInfo = (MissionCompletedEvent.MissionCompletedEventArgs)e;

                    CommanderData.Credits += Convert.ToUInt32(missionCompletedInfo.Reward);

                    Missions.HandleMissionCompletedEvent(missionCompletedInfo);

                    Material.HandleMissionCompletedEvent(missionCompletedInfo);

                    Cargo.HandleMissionCompletedEvent(missionCompletedInfo);

                    break;

                case "Materials":

                    var materialsInfo = (MaterialsEvent.MaterialsEventArgs)e;

                    Material.HandleMaterialsEvent(materialsInfo);

                    break;
                case "MaterialCollected":

                    var materialCollectedInfo = (MaterialCollectedEvent.MaterialCollectedEventArgs)e;

                    Material.HandleMaterialCollectedEvent(materialCollectedInfo);

                    if (!string.IsNullOrEmpty(LocationData.StarSystem))
                    {
                        var name = (materialCollectedInfo.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(materialCollectedInfo.Name.ToLower())).Trim();

                        //TODO could possibly count some materials again, that were already added via GetEliteHistory() 

                        Material.AddHistory(name, LocationData.StarSystem, materialCollectedInfo.Count);
                    }

                    break;
                case "MaterialDiscarded":

                    var materialDiscardedInfo = (MaterialDiscardedEvent.MaterialDiscardedEventArgs)e;

                    Material.HandleMaterialDiscardedEvent(materialDiscardedInfo);

                    break;
                case "ScientificResearch":

                    var scientificResearchInfo = (ScientificResearchEvent.ScientificResearchEventArgs)e;

                    Material.HandleScientificResearchEvent(scientificResearchInfo);

                    break;
                case "MaterialTrade":

                    var materialTradeInfo = (MaterialTradeEvent.MaterialTradeEventArgs)e;

                    Material.HandleMaterialTradedEvent(materialTradeInfo);

                    break;
                case "Synthesis":

                    var synthesisInfo = (SynthesisEvent.SynthesisEventArgs)e;

                    Material.HandleSynthesisedEvent(synthesisInfo);

                    break;
                case "EngineerCraft":

                    var engineerCraftInfo = (EngineerCraftEvent.EngineerCraftEventArgs)e;

                    Material.HandleEngineerCraftEvent(engineerCraftInfo);

                    break;
                case "TechnologyBroker":

                    var technologyBrokerInfo = (TechnologyBrokerEvent.TechnologyBrokerEventArgs)e;

                    Material.HandleTechnologyBrokerEvent(technologyBrokerInfo);

                    break;
                case "EngineerContribution":

                    var engineerContributionInfo = (EngineerContributionEvent.EngineerContributionEventArgs)e;

                    Material.HandleEngineerContributionEvent(engineerContributionInfo);

                    Cargo.HandleEngineerContributionEvent(engineerContributionInfo);

                    break;

                case "Cargo":

                    var cargoInfo = (CargoEvent.CargoEventArgs)e;

                    //Cargo.HandleCargoEvent(cargoInfo);

                    break;

                case "CollectCargo":

                    var collectCargoInfo = (CollectCargoEvent.CollectCargoEventArgs)e;

                    Cargo.HandleCollectCargoEvent(collectCargoInfo);

                    break;

                case "EjectCargo":

                    var ejectCargoInfo = (EjectCargoEvent.EjectCargoEventArgs)e;

                    Cargo.HandleEjectCargoEvent(ejectCargoInfo);

                    break;

                case "MarketBuy":

                    var marketBuyInfo = (MarketBuyEvent.MarketBuyEventArgs)e;

                    Cargo.HandleMarketBuyEvent(marketBuyInfo);

                    break;

                case "MiningRefined":

                    var miningRefinedInfo = (MiningRefinedEvent.MiningRefinedEventArgs)e;

                    Cargo.HandleMiningRefinedEvent(miningRefinedInfo);

                    break;

                case "MarketSell":

                    var marketSellInfo = (MarketSellEvent.MarketSellEventArgs)e;

                    Cargo.HandleMarketSellEvent(marketSellInfo);

                    break;

                case "CargoDepot":

                    var cargoDepotInfo = (CargoDepotEvent.CargoDepotEventArgs)e;

                    Cargo.HandleCargoDepotEvent(cargoDepotInfo);

                    break;

                case "Died":

                    var diedInfo = (DiedEvent.DiedEventArgs)e;

                    Cargo.HandleDiedEvent(diedInfo);

                    break;

                case "ShipyardBuy":
                    //When Written: when buying a new ship in the shipyard
                    //Note: the new ship’s ShipID will be logged in a separate event after the purchase

                    var shipyardBuyInfo = (ShipyardBuyEvent.ShipyardBuyEventArgs)e;

                    Ships.HandleShipyardBuy(shipyardBuyInfo);

                    break;

                case "ShipyardSell":
                    //When Written: when selling a ship stored in the shipyard

                    var shipyardSellInfo = (ShipyardSellEvent.ShipyardSellEventArgs)e;

                    Ships.HandleShipyardSell(shipyardSellInfo);

                    break;

                case "ShipyardNew":
                    //When written: after a new ship has been purchased
                    var shipyardNewInfo = (ShipyardNewEvent.ShipyardNewEventArgs)e;

                    Ships.HandleShipyardNew(shipyardNewInfo);

                    break;

                case "ShipyardSwap":
                    //When Written: when switching to another ship already stored at this station
                    var shipyardSwapInfo = (ShipyardSwapEvent.ShipyardSwapEventArgs)e;

                    Ships.HandleShipyardSwap(shipyardSwapInfo);

                    break;

                case "ShipyardTransfer":
                    //When Written: when requesting a ship at another station be transported to this station
                    //var shipyardTransferInfo = (ShipyardTransferEvent.ShipyardTransferEventArgs)e;

                    //ShipID

                    break;

                case "StoredShips":
                    //    When written: when visiting shipyard
                    var storedShipsInfo = (StoredShipsEvent.StoredShipsEventArgs)e;

                    //ShipID

                    Ships.HandleStoredShips(storedShipsInfo);

                    break;

                case "Music":

                    var musicInfo = (MusicEvent.MusicEventArgs)e;
                    
                    switch (musicInfo.MusicTrack)
                    {
                        case "MainMenu":
                            //TabLCDStartElite.Create();
                            break;

                        case "DockingComputer":

                            if (shipData != null)
                            {
                                shipData.AutomaticDocking = true;
                            }

                            break;

                        case "NoTrack":
                            if (shipData != null)
                            {
                                if (shipData.AutomaticDocking)
                                {
                                    shipData.AutomaticDocking = false;
                                }
                            }

                            break;

                        default:
                            return;
                    }

                    break;

                case "ReceiveText":

                    var receiveTextInfo = (ReceiveTextEvent.ReceiveTextEventArgs) e;

                    var channel = receiveTextInfo.Channel.ToString();

                    // copied from EddiJournalMonitor https://github.com/EDCD/EDDI

                    if (receiveTextInfo.From == string.Empty && channel == "npc" &&
                        (receiveTextInfo.Message.StartsWith("$COMMS_entered") ||
                         receiveTextInfo.Message.StartsWith("$CHAT_Intro")))
                    {
                        // We can safely ignore system messages that initialize the chat system or announce that we entered a receiveTextInfo.Channel - no event is needed. 
                        break;
                    }

                    if (
                        channel == "player" ||
                        channel == "wing" ||
                        channel == "friend" ||
                        channel == "voicechat" ||
                        channel == "local" ||
                        channel == "squadron" ||
                        channel == "starsystem" ||
                        channel == null
                    )
                    {
                        // Give priority to player messages
                        //var source = channel == "squadron" ? "Squadron mate" :
                        //    channel == "wing" ? "Wing mate" :
                        //    channel == null ? "Crew mate" : "Commander";

                        //var channel = receiveTextInfo.Channel ?? "multicrew";

                    }
                    else
                    {
                        // This is NPC speech.  What's the source?
                        if (receiveTextInfo.From.Contains("npc_name_decorate"))
                        {
                            //var source = npcSpeechBy(receiveTextInfo.From, receiveTextInfo.Message);
                            //var from = receiveTextInfo.From.Replace("$npc_name_decorate:#name=", "").Replace(";", "");
                        }
                        else if (receiveTextInfo.From.Contains("ShipName_"))
                        {
                            //var source = npcSpeechBy(receiveTextInfo.From, receiveTextInfo.Message);
                            //var from = receiveTextInfo.FromLocalized;

                        }
                        else if (receiveTextInfo.Message.StartsWith("$STATION_") ||
                                 receiveTextInfo.Message.Contains("$Docking"))
                        {
                            //var source = "Station";
                        }
                        else
                        {
                            //var source = "NPC";
                        }

                        //var message =  receiveTextInfo.MessageLocalized

                        // See if we also want to spawn a specific event as well?
                        if (receiveTextInfo.Message == "$STATION_NoFireZone_entered;")
                        {
                            //events.Add(new StationNoFireZoneEnteredEvent(false) { raw = line, fromLoad = fromLogLoad });
                        }
                        else if (receiveTextInfo.Message == "$STATION_NoFireZone_entered_deployed;")
                        {
                            //events.Add(new StationNoFireZoneEnteredEvent(true) { raw = line, fromLoad = fromLogLoad });
                        }
                        else if (receiveTextInfo.Message == "$STATION_NoFireZone_exited;")
                        {
                            //events.Add(new StationNoFireZoneExitedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                        }
                        else if (receiveTextInfo.Message.Contains("_StartInterdiction"))
                        {
                            // Find out who is doing the interdicting
                            //string by = npcSpeechBy(receiveTextInfo.From, receiveTextInfo.Message);

                            //events.Add(new NPCInterdictionCommencedEvent(by) { raw = line, fromLoad = fromLogLoad });
                        }
                        else if (receiveTextInfo.Message.Contains("_Attack") ||
                                 receiveTextInfo.Message.Contains("_OnAttackStart") ||
                                 receiveTextInfo.Message.Contains("AttackRun") ||
                                 receiveTextInfo.Message.Contains("OnDeclarePiracyAttack"))
                        {
                            // Find out who is doing the attacking
                            //string by = npcSpeechBy(receiveTextInfo.From, receiveTextInfo.Message);
                            //events.Add(new NPCAttackCommencedEvent(by) { raw = line, fromLoad = fromLogLoad });
                        }
                        else if (receiveTextInfo.Message.Contains("_OnStartScanCargo"))
                        {
                            // Find out who is doing the scanning
                            //string by = npcSpeechBy(receiveTextInfo.From, receiveTextInfo.Message);
                            //events.Add(new NPCCargoScanCommencedEvent(by) { raw = line, fromLoad = fromLogLoad });
                        }
                    }


                    break;

            }

            App.FipHandler.RefreshDevicePages();

        }
    }
}
