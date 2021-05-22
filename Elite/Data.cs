using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using Elite.RingBuffer;
using EliteJournalReader;
using EliteJournalReader.Events;
using NAudio.Utils;

// ReSharper disable StringLiteralTypo


namespace Elite
{
    public class Data
    {

        public static Dictionary<HotspotSystems.MaterialTypes, List<HotspotSystems.HotspotSystemData>> NearbyHotspotSystemsList = new Dictionary<HotspotSystems.MaterialTypes, List<HotspotSystems.HotspotSystemData>>
        {
            {HotspotSystems.MaterialTypes.Painite, new List<HotspotSystems.HotspotSystemData>()},
            {HotspotSystems.MaterialTypes.LTD, new List<HotspotSystems.HotspotSystemData>()},
            {HotspotSystems.MaterialTypes.Platinum, new List<HotspotSystems.HotspotSystemData>()}
        };


        public static Dictionary<MiningStations.MaterialTypes, List<MiningStations.MiningStationData>> NearbyMiningStationsList = new Dictionary<MiningStations.MaterialTypes, List<MiningStations.MiningStationData>>
        {
            {MiningStations.MaterialTypes.Painite, new List<MiningStations.MiningStationData>()},
            {MiningStations.MaterialTypes.LTD, new List<MiningStations.MiningStationData>()},
            {MiningStations.MaterialTypes.Platinum, new List<MiningStations.MiningStationData>()},
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


        public class EngineerData : StationData
        {
            public int? Rank { get; set; }
            public string Progress { get; set; }

        }

        public static List<EngineerData> EngineersList = new List<EngineerData>();

        public static List<CnbSystems.CnbSystemData> NearbyCnbSystemsList = new List<CnbSystems.CnbSystemData>();

        public static  RingBuffer<string> EventHistory = new RingBuffer<string>(50, true);

        public static RingBuffer<ReceiveTextEvent.ReceiveTextEventArgs> ChatHistory = new RingBuffer<ReceiveTextEvent.ReceiveTextEventArgs>(300, true);

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
                NearbyHotspotSystemsList[HotspotSystems.MaterialTypes.Platinum] = HotspotSystems.GetNearestHotspotSystems(LocationData.StarPos, HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.Platinum]);

                NearbyMiningStationsList[MiningStations.MaterialTypes.Painite] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.Painite], true);
                NearbyMiningStationsList[MiningStations.MaterialTypes.LTD] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.LTD], true);
                NearbyMiningStationsList[MiningStations.MaterialTypes.Platinum] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.Platinum], true);
                NearbyMiningStationsList[MiningStations.MaterialTypes.TritiumBuy] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.TritiumBuy], false);
                NearbyMiningStationsList[MiningStations.MaterialTypes.TritiumSell] = MiningStations.GetNearestMiningStations(LocationData.StarPos, MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.TritiumSell], true);

                EngineersList = Station.UpdateEngineersLocation(LocationData.StarPos, EngineersList);
               
            }
        }

        public class Commander
        {
            public string Name { get; set; } = "";
            public long Credits { get; set; }

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

            public string SoldierRank { get; set; }
            public string ExobiologistRank { get; set; }

            public int EmpireRankProgress { get; set; }
            public int FederationRankProgress { get; set; }
            public int CombatRankProgress { get; set; }
            public int TradeRankProgress { get; set; }
            public int ExplorationRankProgress { get; set; }
            public int CqcRankProgress { get; set; }

            public int SoldierRankProgress { get; set; }
            public int ExobiologistRankProgress { get; set; }
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

            public DateTime Refreshed { get; set; }

        }

        public class Location
        {
            public bool StartJump { get; set; }

            public string JumpType { get; set; }

            public string JumpToSystem { get; set; }

            public string JumpToStarClass { get; set; }

            public int RemainingJumpsInRoute { get; set; }
            public string StarClass { get; set; }

            public string IsFuelStar
            {
                get
                {
                    string[] fuelStars = { "K", "G", "B", "F", "O", "A", "M" };

                    return fuelStars.Contains(StarClass) ? "(Fuel Star)" : "";
                }
            }

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

            public DateTime Refreshed { get; set; }

            public string SystemState { get; set; }


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
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Altitude { get; set; }
            public double Heading { get; set; }
            public string BodyName { get; set; }
            public double PlanetRadius { get; set; }

            public StatusGuiFocus GuiFocus { get; set; }

            public int[] Pips { get; set; } = new int[3];

            public bool OnFoot { get; set; }
            public bool InTaxi { get; set; }
            public bool InMulticrew { get; set; }
            public bool OnFootInStation { get; set; }
            public bool OnFootOnPlanet { get; set; }
            public bool AimDownSight { get; set; }
            public bool LowOxygen { get; set; }
            public bool LowHealth { get; set; }
            public bool Cold { get; set; }
            public bool Hot { get; set; }
            public bool VeryCold { get; set; }
            public bool VeryHot { get; set; }
            public bool GlideMode { get; set; }
            public bool OnFootInHangar { get; set; }
            public bool OnFootSocialSpace { get; set; }
            public bool OnFootExterior { get; set; }
            public bool BreathableAtmosphere { get; set; }
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

            //!!!!!!StatusData.Docked = (evt.Flags & StatusFlags.Docked) != 0;
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
            StatusData.GuiFocus = evt.GuiFocus;

            StatusData.Latitude = evt.Latitude;
            StatusData.Longitude = evt.Longitude;
            StatusData.Altitude = evt.Altitude;
            StatusData.Heading = evt.Heading;
            StatusData.BodyName = evt.BodyName;
            StatusData.PlanetRadius = evt.PlanetRadius;

            StatusData.Pips[0] = evt.Pips.System;
            StatusData.Pips[1] = evt.Pips.Engine;
            StatusData.Pips[2] = evt.Pips.Weapons;

            StatusData.OnFoot = (evt.Flags2 & StatusFlags2.OnFoot) != 0;
            StatusData.InTaxi = (evt.Flags2 & StatusFlags2.InTaxi) != 0;
            StatusData.InMulticrew = (evt.Flags2 & StatusFlags2.InMulticrew) != 0;
            StatusData.OnFootInStation = (evt.Flags2 & StatusFlags2.OnFootInStation) != 0;
            StatusData.OnFootOnPlanet = (evt.Flags2 & StatusFlags2.OnFootOnPlanet) != 0;
            StatusData.AimDownSight = (evt.Flags2 & StatusFlags2.AimDownSight) != 0;
            StatusData.LowOxygen = (evt.Flags2 & StatusFlags2.LowOxygen) != 0;
            StatusData.LowHealth = (evt.Flags2 & StatusFlags2.LowHealth) != 0;
            StatusData.Cold = (evt.Flags2 & StatusFlags2.Cold) != 0;
            StatusData.Hot = (evt.Flags2 & StatusFlags2.Hot) != 0;
            StatusData.VeryCold = (evt.Flags2 & StatusFlags2.VeryCold) != 0;
            StatusData.VeryHot = (evt.Flags2 & StatusFlags2.VeryHot) != 0;

            StatusData.GlideMode = (evt.Flags2 & StatusFlags2.GlideMode) != 0;
            StatusData.OnFootInHangar = (evt.Flags2 & StatusFlags2.OnFootInHangar) != 0;
            StatusData.OnFootSocialSpace = (evt.Flags2 & StatusFlags2.OnFootSocialSpace) != 0;
            StatusData.OnFootExterior = (evt.Flags2 & StatusFlags2.OnFootExterior) != 0;
            StatusData.BreathableAtmosphere = (evt.Flags2 & StatusFlags2.BreathableAtmosphere) != 0;

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

        public static void HandleBackPackEvent(object sender, BackPackEvent.BackPackEventArgs e)
        {
            if (e?.Components == null) return;

            Material.HandleBackPackEvent(e);
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

        private static void UpdateEngineerProgress(string engineer, string engineerID, int? rank, string progress)
        {
            var engineerItem = EngineersList.FirstOrDefault(x => x.Faction == engineer);

            if (engineerItem != null)
            {
                engineerItem.Progress = progress?.Trim();
                engineerItem.Rank = rank;
            }
        }

        public static void HandleEngineerProgressEvent(EngineerProgressEvent.EngineerProgressEventArgs info)
        {
            if (info?.Engineers?.Any() == true)
            {
                foreach (var engineer in info?.Engineers)
                {
                    UpdateEngineerProgress(engineer.Engineer, engineer.EngineerID, engineer.Rank, engineer.Progress);
                }
            }

            if (!string.IsNullOrEmpty(info?.Engineer))
            {
                UpdateEngineerProgress(info.Engineer, info.EngineerID, info.Rank, info.Progress);
            }
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

                    if (!string.IsNullOrEmpty(loadGameInfo.Ship))
                    {
                        Ships.HandleLoadGame(loadGameInfo.ShipID, loadGameInfo.Ship, loadGameInfo.ShipName);
                    }

                    CommanderData.Name = loadGameInfo.Commander;
                    CommanderData.Credits = loadGameInfo.Credits;

                    LocationData.Settlement = "";

                    break;

                case "Rank":
                    //When written: at startup
                    var rankInfo = (RankEvent.RankEventArgs) e;

                    CommanderData.CqcRank = rankInfo.CQC.StringValue();

                    CommanderData.EmpireRank = rankInfo.Empire.StringValue();
                    CommanderData.FederationRank = rankInfo.Federation.StringValue();
                    CommanderData.CombatRank = rankInfo.Combat.StringValue();
                    CommanderData.TradeRank = rankInfo.Trade.StringValue();
                    CommanderData.ExplorationRank = rankInfo.Explore.StringValue();

                    CommanderData.SoldierRank = rankInfo.Soldier.StringValue();
                    CommanderData.ExobiologistRank = rankInfo.Exobiologist.StringValue();

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

                    CommanderData.SoldierRankProgress = progressInfo.Soldier;
                    CommanderData.ExobiologistRankProgress = progressInfo.Exobiologist;

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

                    StatusData.Docked = locationInfo.Docked;
                    StatusData.InTaxi = locationInfo.Taxi;
                    StatusData.InMulticrew = locationInfo.Multicrew;
                    StatusData.InSRV = locationInfo.InSRV;
                    StatusData.OnFoot = locationInfo.OnFoot;


                    //Docked
                    //Latitude
                    //Longitude
                    //StationName
                    //StationType
                    //StationGovernment
                    //StationAllegiance
                    //StationServices
                    //Wanted
                    //Powers
                    //Factions
                    //Conflicts

                    LocationData.StarSystem = locationInfo.StarSystem;

                    LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

                    LocationData.StarPos = locationInfo.StarPos.ToList();

                    Ships.HandleShipDistance(LocationData.StarPos);
                    Module.HandleModuleDistance(LocationData.StarPos);

                    Poi.NearbyPoiList = Poi.GetNearestPois(LocationData.StarPos);

                    HandleJson();

                    LocationData.SystemAllegiance = locationInfo.SystemAllegiance;
                    LocationData.SystemFaction = locationInfo.SystemFaction?.Name;
                    LocationData.SystemSecurity = locationInfo.SystemSecurity_Localised;

                    LocationData.SystemEconomy = locationInfo.SystemEconomy_Localised;
                    if (!string.IsNullOrEmpty(locationInfo.SystemSecondEconomy_Localised))
                    {
                        LocationData.SystemEconomy += "," + locationInfo.SystemSecondEconomy_Localised;
                    }

                    LocationData.SystemGovernment = locationInfo.SystemGovernment_Localised;
                    LocationData.Population = locationInfo.Population;
                    LocationData.Body = locationInfo.Body;
                    LocationData.BodyType = locationInfo.BodyType.StringValue();

                    LocationData.PowerplayState = locationInfo.PowerplayState;
                    LocationData.Powers = locationInfo.Powers != null ? string.Join(",", locationInfo.Powers) : "";

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

                case "RefuelAll":
                    //When Written: when refuelling (full tank)
                    var refuelAllInfo = (RefuelAllEvent.RefuelAllEventArgs) e;

                    CommanderData.Credits -= refuelAllInfo.Cost;
                    break;

                case "RefuelPartial":
                    //When Written: when refuelling (10%)
                    var RefuelPartialInfo = (RefuelPartialEvent.RefuelPartialEventArgs)e;

                    CommanderData.Credits -= RefuelPartialInfo.Cost;
                    break;

                case "RestockVehicle":
                    //When Written: when purchasing an SRV or Fighter
                    var restockVehicleInfo = (RestockVehicleEvent.RestockVehicleEventArgs)e;

                    CommanderData.Credits -= restockVehicleInfo.Cost;
                    break;

                case "Resurrect":
                    //When Written: when purchasing an SRV or Fighter
                    var ResurrectInfo = (ResurrectEvent.ResurrectEventArgs)e;

                    CommanderData.Credits -= ResurrectInfo.Cost;
                    break;

                case "RepairAll":
                    //When Written: when repairing the ship
                    var repairAllInfo = (RepairAllEvent.RepairAllEventArgs) e;

                    CommanderData.Credits -= repairAllInfo.Cost;
                    break;

                case "Repair":
                    //When Written: when repairing the ship
                    var repairInfo = (RepairEvent.RepairEventArgs) e;

                    CommanderData.Credits -= repairInfo.Cost;
                    break;

                case "BuyTradeData":
                    //When Written: when buying trade data in the galaxy map
                    var buyTradeDataInfo = (BuyTradeDataEvent.BuyTradeDataEventArgs) e;

                    CommanderData.Credits -= buyTradeDataInfo.Cost;
                    break;

                case "BuyExplorationData":
                    //When Written: when buying system data via the galaxy map
                    var buyExplorationDataInfo = (BuyExplorationDataEvent.BuyExplorationDataEventArgs) e;

                    CommanderData.Credits -= buyExplorationDataInfo.Cost;
                    break;

                case "BuyDrones":
                    //When Written: when purchasing drones
                    var buyDronesInfo = (BuyDronesEvent.BuyDronesEventArgs) e;

                    CommanderData.Credits -= buyDronesInfo.TotalCost;
                    break;

                case "BuyAmmo":
                    //When Written: when purchasing ammunition
                    var buyAmmoInfo = (BuyAmmoEvent.BuyAmmoEventArgs) e;

                    CommanderData.Credits -= buyAmmoInfo.Cost;
                    break;

                case "PayBounties":
                    //When written: when paying off bounties
                    var payBountiesInfo = (PayBountiesEvent.PayBountiesEventArgs)e;

                    // shipID

                    CommanderData.Credits -= payBountiesInfo.Amount;
                    break;

                case "RedeemVoucher":
                    var redeemVoucherInfo = (RedeemVoucherEvent.RedeemVoucherEventArgs)e;

                    CommanderData.Credits += redeemVoucherInfo.Amount;

                    break;

                case "MarketSell":
                    //When Written: when selling goods in the market

                    var marketSellInfo = (MarketSellEvent.MarketSellEventArgs)e;

                    CommanderData.Credits += marketSellInfo.TotalSale;

                    Cargo.HandleMarketSellEvent(marketSellInfo);

                    break;

                case "MarketBuy":

                    var marketBuyInfo = (MarketBuyEvent.MarketBuyEventArgs)e;

                    CommanderData.Credits -= marketBuyInfo.TotalCost;

                    Cargo.HandleMarketBuyEvent(marketBuyInfo);

                    break;

                case "SellDrones":
                    //When Written: when selling unwanted drones back to the market

                    var sellDronesInfo = (SellDronesEvent.SellDronesEventArgs)e;

                    CommanderData.Credits += sellDronesInfo.TotalSale;

                    break;

                case "PayFines":
                    //When Written: when paying fines
                    var payFinesInfo = (PayFinesEvent.PayFinesEventArgs)e;

                    // shipID

                    CommanderData.Credits -= payFinesInfo.Amount;
                    break;

                case "ModuleBuy":
                    //When Written: when buying a module in outfitting
                    var moduleBuyInfo = (ModuleBuyEvent.ModuleBuyEventArgs)e;

                    CommanderData.Credits -= moduleBuyInfo.BuyPrice + (moduleBuyInfo.SellPrice ?? 0);

                    Module.HandleModuleBuy(moduleBuyInfo);

                    break;

                case "ModuleSell":
                    //When Written: when selling a module in outfitting
                    var moduleSellInfo = (ModuleSellEvent.ModuleSellEventArgs)e;

                    CommanderData.Credits += moduleSellInfo.SellPrice;

                    Module.HandleModuleSell(moduleSellInfo);

                    break;

                case "ModuleSellRemote":
                    //When Written: when selling a module in outfitting
                    var moduleSellRemoteInfo = (ModuleSellRemoteEvent.ModuleSellRemoteEventArgs)e;

                    CommanderData.Credits += moduleSellRemoteInfo.SellPrice;

                    Module.HandleModuleSellRemote(moduleSellRemoteInfo);

                    break;

                case "ModuleRetrieve":
                    //When Written: when fetching a previously stored module
                    var moduleRetrieveInfo = (ModuleRetrieveEvent.ModuleRetrieveEventArgs)e;

                    //CommanderData.Credits -= moduleRetrieveInfo.Cost; ???????????????

                    Module.HandleModuleRetrieve(moduleRetrieveInfo);

                    break;

                case "ModuleStore":
                    //When Written: when fetching a previously stored module
                    var moduleStoreInfo = (ModuleStoreEvent.ModuleStoreEventArgs)e;

                    //CommanderData.Credits -= moduleStoreInfo.Cost; ???????????????

                    Module.HandleModuleStore(moduleStoreInfo);

                    break;

                case "ModuleSwap":
                    //When Written: when moving a module to a different slot on the ship
                    var moduleSwapInfo = (ModuleSwapEvent.ModuleSwapEventArgs)e;

                    Module.HandleModuleSwap(moduleSwapInfo);

                    break;

                case "MassModuleStore":
                    //When written: when putting multiple modules into storage
                    var massModuleStoreInfo = (MassModuleStoreEvent.MassModuleStoreEventArgs)e;

                    Module.HandleMassModuleStore(massModuleStoreInfo);

                    break;

                case "FetchRemoteModule":
                    //    When written: when requesting a module is transferred from storage at another station 

                    var fetchRemoteModuleInfo = (FetchRemoteModuleEvent.FetchRemoteModuleEventArgs)e;

                    CommanderData.Credits -= fetchRemoteModuleInfo.TransferCost;

                    Module.HandleFetchRemoteModule(fetchRemoteModuleInfo);

                    break;

                case "PowerplayFastTrack":
                    //When written: when paying to fast-track allocation of commodities

                    var powerplayFastTrackInfo = (PowerplayFastTrackEvent.PowerplayFastTrackEventArgs)e;

                    CommanderData.Credits -= powerplayFastTrackInfo.Cost;

                    break;

                case "StoredModules":
                    //    When written: when first visiting Outfitting, and when the set of stored modules has changed 

                    var storedModulesInfo = (StoredModulesEvent.StoredModulesEventArgs)e;

                    Module.HandleStoredModules(storedModulesInfo);

                    break;

                case "SellShipOnRebuy":
                    //When written: When selling a stored ship to raise funds when on insurance/rebuy screen
                    var SellShipOnRebuyInfo = (SellShipOnRebuyEvent.SellShipOnRebuyEventArgs)e;

                    CommanderData.Credits += SellShipOnRebuyInfo.ShipPrice;

                    break;

                case "ShipyardBuy":
                    //When Written: when buying a new ship in the shipyard
                    //Note: the new ship’s ShipID will be logged in a separate event after the purchase

                    var shipyardBuyInfo = (ShipyardBuyEvent.ShipyardBuyEventArgs)e;

                    CommanderData.Credits -= shipyardBuyInfo.ShipPrice + (shipyardBuyInfo.SellPrice ?? 0);

                    Ships.HandleShipyardBuy(shipyardBuyInfo);

                    break;

                case "ShipyardSell":
                    //When Written: when selling a ship stored in the shipyard

                    var shipyardSellInfo = (ShipyardSellEvent.ShipyardSellEventArgs)e;

                    CommanderData.Credits += shipyardSellInfo.ShipPrice;

                    Ships.HandleShipyardSell(shipyardSellInfo);

                    break;

                case "ShipyardSwap":
                    //When Written: when switching to another ship already stored at this station
                    var shipyardSwapInfo = (ShipyardSwapEvent.ShipyardSwapEventArgs)e;

                    CommanderData.Credits += shipyardSwapInfo.SellPrice ?? 0;

                    Ships.HandleShipyardSwap(shipyardSwapInfo);

                    break;

                case "ShipyardTransfer":
                    //When Written: when requesting a ship at another station be transported to this station

                    var shipyardTransferInfo = (ShipyardTransferEvent.ShipyardTransferEventArgs)e;

                    CommanderData.Credits -= shipyardTransferInfo.TransferPrice;

                    //ShipID

                    break;

                case "ShipyardNew":
                    //When written: after a new ship has been purchased
                    var shipyardNewInfo = (ShipyardNewEvent.ShipyardNewEventArgs)e;

                    Ships.HandleShipyardNew(shipyardNewInfo);

                    break;

                case "StoredShips":
                    //    When written: when visiting shipyard
                    var storedShipsInfo = (StoredShipsEvent.StoredShipsEventArgs)e;

                    //ShipID

                    Ships.HandleStoredShips(storedShipsInfo);

                    break;

                case "CrewHire":
                    //When Written: when engaging a new member of crew
                    var crewHireInfo = (CrewHireEvent.CrewHireEventArgs)e;

                    CommanderData.Credits -= crewHireInfo.Cost;

                    break;


                case "ApproachBody":
                    //    When written: when in Supercruise, and distance from planet drops to within the 'Orbital Cruise' zone
                    var approachBodyInfo = (ApproachBodyEvent.ApproachBodyEventArgs) e;

                    LocationData.StarSystem = approachBodyInfo.StarSystem;

                    LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

                    LocationData.Body = approachBodyInfo.Body;
                    LocationData.BodyType = "Planet";

                    LocationData.HideBody = false;

                    LocationData.Refreshed = DateTime.Now;

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

                    LocationData.Refreshed = DateTime.Now;

                    break;

                case "LeaveBody":
                    //When written: when flying away from a planet, and distance increases above the 'Orbital Cruise' altitude
                    var leaveBodyInfo = (LeaveBodyEvent.LeaveBodyEventArgs) e;

                    //Body

                    LocationData.StarSystem = leaveBodyInfo.StarSystem;

                    LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

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

                    StatusData.Docked = false;

                    break;

                case "Docked":
                    //    When written: when landing at landing pad in a space station, outpost, or surface settlement
                    var dockedInfo = (DockedEvent.DockedEventArgs) e;

                    StatusData.Docked = true;

                    Ships.HandleShipDocked(dockedInfo.StarSystem, dockedInfo.StationName);

                    //CockpitBreach
                    //StationEconomies
                    //Wanted
                    //ActiveFine

                    LocationData.StarSystem = dockedInfo.StarSystem;

                    LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

                    LocationData.Station = dockedInfo.StationName;

                    DockData.Type = dockedInfo.StationType;

                    DockData.Government = dockedInfo.StationGovernment_Localised;
                    DockData.Allegiance = dockedInfo.StationAllegiance;
                    DockData.Faction = dockedInfo.StationFaction?.Name;
                    DockData.Economy = string.Join(",", dockedInfo.StationEconomies?.Select(x => x.Name_Localised) ?? new List<string>() { dockedInfo.StationEconomy_Localised });
                    DockData.DistFromStarLs = dockedInfo.DistFromStarLS;

                    DockData.Services = string.Join(", ", dockedInfo.StationServices);

                    DockData.LandingPad = -1;

                    if (shipData != null)
                    {
                        shipData.AutomaticDocking = false;
                    }

                    LocationData.Refreshed = DateTime.Now;

                    break;

                case "DockingGranted":
                    //When written: when a docking request is granted
                    var dockingGrantedInfo = (DockingGrantedEvent.DockingGrantedEventArgs) e;

                    DockData.Type = dockingGrantedInfo.StationType;

                    LocationData.Body = dockingGrantedInfo.StationName;
                    LocationData.BodyType = "Station";

                    DockData.LandingPad = Convert.ToInt32(dockingGrantedInfo.LandingPad);

                    LocationData.Refreshed = DateTime.Now;

                    break;

                case "DockingRequested":

                    //When written: when the player requests docking at a station
                    var dockingRequestedInfo = (DockingRequestedEvent.DockingRequestedEventArgs) e;

                    LocationData.Station = dockingRequestedInfo.StationName;

                    LocationData.Body = dockingRequestedInfo.StationName;
                    LocationData.BodyType = "Station";

                    LocationData.Refreshed = DateTime.Now;

                    break;

                case "CarrierBuy":
                    //When Written: Player has bought a fleet carrier
                    var CarrierBuyInfo = (CarrierBuyEvent.CarrierBuyEventArgs)e;

                    CommanderData.Credits -= CarrierBuyInfo.Price;

                    break;

                case "CarrierJump":
                    //When written: when jumping from one star system to another when docked on a a carrier
                    var carrierJumpInfo = (CarrierJumpEvent.CarrierJumpEventArgs)e;

                    if (carrierJumpInfo.Docked)
                    {
                        Ships.HandleShipFsdJump(carrierJumpInfo.StarSystem, carrierJumpInfo.StarPos.ToList());

                        LocationData.Body = carrierJumpInfo.Body;

                        LocationData.StarSystem = carrierJumpInfo.StarSystem;

                        LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

                        LocationData.StarPos = carrierJumpInfo.StarPos.ToList();

                        Ships.HandleShipDistance(LocationData.StarPos);
                        Module.HandleModuleDistance(LocationData.StarPos);
                        
                        History.AddTravelPos(LocationData.StarPos);

                        Poi.NearbyPoiList = Poi.GetNearestPois(LocationData.StarPos);

                        HandleJson();

                        LocationData.StartJump = false;
                        LocationData.JumpToSystem = "";
                        LocationData.JumpToStarClass = "";
                        LocationData.JumpType = "";

                        LocationData.PowerplayState = carrierJumpInfo.PowerplayState;
                        LocationData.Powers = carrierJumpInfo.Powers != null ? string.Join(",", carrierJumpInfo.Powers) : "";

                        LocationData.SystemAllegiance = carrierJumpInfo.SystemAllegiance;
                        LocationData.SystemFaction = carrierJumpInfo.SystemFaction?.Name;
                        LocationData.SystemSecurity = carrierJumpInfo.SystemSecurity_Localised;

                        LocationData.SystemEconomy = carrierJumpInfo.SystemEconomy_Localised;
                        if (!string.IsNullOrEmpty(carrierJumpInfo.SystemSecondEconomy_Localised))
                        {
                            LocationData.SystemEconomy += "," + carrierJumpInfo.SystemSecondEconomy_Localised;
                        }

                        LocationData.SystemGovernment = carrierJumpInfo.SystemGovernment_Localised;
                        LocationData.Population = carrierJumpInfo.Population;

                        LocationData.Refreshed = DateTime.Now;
                    }

                    break;

                case "FSDJump":
                    //When written: when jumping from one star system to another
                    var fsdJumpInfo = (FSDJumpEvent.FSDJumpEventArgs) e;

                    Ships.HandleShipFsdJump(fsdJumpInfo.StarSystem, fsdJumpInfo.StarPos.ToList());

                    //FuelUsed
                    //FuelLevel
                    //BoostUsed
                    //Wanted
                    //Powers

                    LocationData.Body = fsdJumpInfo.Body;  

                    LocationData.StarSystem = fsdJumpInfo.StarSystem;

                    LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

                    if (StatusData.JumpRange < fsdJumpInfo.JumpDist)
                    {
                        StatusData.JumpRange = fsdJumpInfo.JumpDist;
                    }

                    LocationData.StarPos = fsdJumpInfo.StarPos.ToList();

                    Ships.HandleShipDistance(LocationData.StarPos);
                    Module.HandleModuleDistance(LocationData.StarPos);
                    
                    History.AddTravelPos(LocationData.StarPos);

                    Poi.NearbyPoiList = Poi.GetNearestPois(LocationData.StarPos);

                    HandleJson();

                    LocationData.StartJump = false;
                    LocationData.JumpToSystem = "";
                    LocationData.JumpToStarClass = "";
                    LocationData.JumpType = "";


                    LocationData.PowerplayState = fsdJumpInfo.PowerplayState;
                    LocationData.Powers = fsdJumpInfo.Powers != null ? string.Join(",", fsdJumpInfo.Powers) : "";

                    LocationData.SystemAllegiance = fsdJumpInfo.SystemAllegiance;
                    LocationData.SystemFaction = fsdJumpInfo.SystemFaction?.Name;
                    LocationData.SystemSecurity = fsdJumpInfo.SystemSecurity_Localised;

                    LocationData.SystemEconomy = fsdJumpInfo.SystemEconomy_Localised;
                    if (!string.IsNullOrEmpty(fsdJumpInfo.SystemSecondEconomy_Localised))
                    {
                        LocationData.SystemEconomy += "," + fsdJumpInfo.SystemSecondEconomy_Localised;
                    }

                    LocationData.SystemGovernment = fsdJumpInfo.SystemGovernment_Localised;
                    LocationData.Population = fsdJumpInfo.Population;

                    LocationData.Refreshed = DateTime.Now;

                    break;

                case "StartJump":

                    var startJumpInfo = (StartJumpEvent.StartJumpEventArgs) e;

                    LocationData.StartJump = true;
                    LocationData.JumpType = startJumpInfo.JumpType.StringValue();
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

                    LocationData.SystemState = PopulatedSystems.GetSystemState(LocationData.StarSystem);

                    LocationData.StartJump = false;

                    LocationData.JumpToSystem = "";
                    LocationData.JumpToStarClass = "";
                    LocationData.JumpType = "";

                    LocationData.Body = supercruiseExitInfo.Body;
                    LocationData.BodyType = supercruiseExitInfo.BodyType.StringValue();
                    LocationData.HideBody = false;

                    LocationData.Refreshed = DateTime.Now;

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
                    TargetData.PilotRank = shipTargetedInfo.PilotRank.StringValue();
                    TargetData.ScanStage = shipTargetedInfo.ScanStage;
                    TargetData.ShieldHealth = shipTargetedInfo.ShieldHealth;

                    TargetData.Power = shipTargetedInfo.Power;

                    Ships.ShipsByEliteId.TryGetValue(shipTargetedInfo.Ship?.ToLower() ?? "???", out var targetShip);

                    TargetData.Ship = shipTargetedInfo.Ship_Localised ?? targetShip ?? shipTargetedInfo.Ship;

                    TargetData.SubsystemLocalised = shipTargetedInfo.Subsystem_Localised;
                    TargetData.TargetLocked = shipTargetedInfo.TargetLocked;
                    TargetData.SubsystemHealth = shipTargetedInfo.SubSystemHealth;

                    TargetData.Refreshed = DateTime.Now;

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

                    CommanderData.Credits += missionCompletedInfo.Reward;

                    Missions.HandleMissionCompletedEvent(missionCompletedInfo);

                    Material.HandleMissionCompletedEvent(missionCompletedInfo);

                    Cargo.HandleMissionCompletedEvent(missionCompletedInfo);

                    break;

                case "SearchAndRescue":
                    //When written: when contributing materials to a "research" community goal
                    var searchAndRescueInfo = (SearchAndRescueEvent.SearchAndRescueEventArgs)e;

                    CommanderData.Credits += searchAndRescueInfo.Reward;

                    break;
                case "FactionKillBond":
                    //When written: Player rewarded for taking part in a combat zone

                    var factionKillBondInfo = (FactionKillBondEvent.FactionKillBondEventArgs)e;

                    CommanderData.Credits += factionKillBondInfo.Reward;

                    break;
                case "DatalinkVoucher":

                    //When written: when scanning a datalink generates a reward

                    var datalinkVoucherInfo = (DatalinkVoucherEvent.DatalinkVoucherEventArgs)e;

                    CommanderData.Credits += datalinkVoucherInfo.Reward;

                    break;
                case "CommunityGoalReward":

                    //When Written: when receiving a reward for a community goal

                    var communityGoalRewardInfo = (CommunityGoalRewardEvent.CommunityGoalRewardEventArgs)e;

                    CommanderData.Credits += communityGoalRewardInfo.Reward;

                    break;
                case "CapShipBond":

                    //When written: The player has been rewarded for a capital ship combat

                    var capShipBondInfo = (CapShipBondEvent.CapShipBondEventArgs)e;

                    CommanderData.Credits += capShipBondInfo.Reward;

                    break;
                case "Bounty":

                    //    When written: player is awarded a bounty for a kill

                    var bountyInfo = (BountyEvent.BountyEventArgs)e;

                    CommanderData.Credits += bountyInfo.TotalReward;

                    break;

                case "MultiSellExplorationData":

                    //When Written: when selling exploration data in Cartographics

                    var multiSellExplorationDataInfo = (MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs)e;

                    CommanderData.Credits += multiSellExplorationDataInfo.TotalEarnings;

                    break;

                case "SellExplorationData":

                    //When Written: when selling exploration data in Cartographics

                    var sellExplorationDataInfo = (SellExplorationDataEvent.SellExplorationDataEventArgs)e;

                    CommanderData.Credits += sellExplorationDataInfo.TotalEarnings;

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

                case "EngineerProgress":

                    var engineerProgressInfo = (EngineerProgressEvent.EngineerProgressEventArgs)e;

                    HandleEngineerProgressEvent(engineerProgressInfo);

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


                case "MiningRefined":

                    var miningRefinedInfo = (MiningRefinedEvent.MiningRefinedEventArgs)e;

                    Cargo.HandleMiningRefinedEvent(miningRefinedInfo);

                    break;

                case "CargoDepot":

                    var cargoDepotInfo = (CargoDepotEvent.CargoDepotEventArgs)e;

                    Cargo.HandleCargoDepotEvent(cargoDepotInfo);

                    break;

                case "Died":

                    var diedInfo = (DiedEvent.DiedEventArgs)e;

                    Cargo.HandleDiedEvent(diedInfo);

                    break;


                case "Promotion":
                    var promotionInfo = (PromotionEvent.PromotionEventArgs)e;

                    if (promotionInfo.CQC != null)
                    {
                        CommanderData.CqcRank = promotionInfo.CQC.StringValue();
                    }
                    if (promotionInfo.Empire != null)
                    {
                        CommanderData.EmpireRank = promotionInfo.Empire.StringValue();
                    }
                    if (promotionInfo.Federation != null)
                    {
                        CommanderData.FederationRank = promotionInfo.Federation.StringValue();
                    }
                    if (promotionInfo.Combat != null)
                    {
                        CommanderData.CombatRank = promotionInfo.Combat.StringValue();
                    }
                    if (promotionInfo.Explore != null)
                    {
                        CommanderData.ExplorationRank = promotionInfo.Explore.StringValue();
                    }
                    if (promotionInfo.Trade != null)
                    {
                        CommanderData.TradeRank = promotionInfo.Trade.StringValue();
                    }

                    if (promotionInfo.Soldier != null)
                    {
                        CommanderData.SoldierRank = promotionInfo.Soldier.StringValue();
                    }
                    if (promotionInfo.Exobiologist != null)
                    {
                        CommanderData.ExobiologistRank = promotionInfo.Exobiologist.StringValue();
                    }

                    break;

                case "BuySuit":
                    var buySuitInfo = (BuySuitEvent.BuySuitEventArgs)e;

                    CommanderData.Credits -= buySuitInfo.Price;

                    break;
                case "SellSuit":
                    var sellSuitInfo = (SellSuitEvent.SellSuitEventArgs)e;

                    CommanderData.Credits += sellSuitInfo.Price;

                    break;
                case "UpgradeSuit":
                    var upgradeSuitInfo = (UpgradeSuitEvent.UpgradeSuitEventArgs)e;

                    CommanderData.Credits -= upgradeSuitInfo.Cost;

                    break;

                case "DeleteSuitLoadout":
                    var deleteSuitLoadoutInfo = (DeleteSuitLoadoutEvent.DeleteSuitLoadoutEventArgs)e;

                    break;
                case "SwitchSuitLoadout":
                    var switchSuitLoadoutInfo = (SwitchSuitLoadoutEvent.SwitchSuitLoadoutEventArgs)e;

                    break;

                case "SuitLoadout":
                    var suitLoadoutInfo = (SuitLoadoutEvent.SuitLoadoutEventArgs)e;

                    break;

                case "CreateSuitLoadout":
                    var createSuitLoadoutInfo = (CreateSuitLoadoutEvent.CreateSuitLoadoutEventArgs)e;

                    break;
                case "RenameSuitLoadout":
                    var renameSuitLoadoutInfo = (RenameSuitLoadoutEvent.RenameSuitLoadoutEventArgs)e;

                    break;

                case "BuyWeapon":
                    var buyWeaponInfo = (BuyWeaponEvent.BuyWeaponEventArgs)e;

                    CommanderData.Credits -= buyWeaponInfo.Price;

                    break;
                case "SellWeapon":
                    var sellWeaponInfo = (SellWeaponEvent.SellWeaponEventArgs)e;

                    CommanderData.Credits += sellWeaponInfo.Price;

                    break;
                case "UpgradeWeapon":
                    var upgradeWeaponInfo = (UpgradeWeaponEvent.UpgradeWeaponEventArgs)e;

                    CommanderData.Credits -= upgradeWeaponInfo.Cost;

                    break;

                case "ShipLockerMaterials":
                    var shipLockerMaterialsInfo = (ShipLockerMaterialsEvent.ShipLockerMaterialsEventArgs)e;

                    Material.HandleShipLockerMaterialsEvent(shipLockerMaterialsInfo);

                    break;
                case "BuyMicroResources":
                    var buyMicroResourcesInfo = (BuyMicroResourcesEvent.BuyMicroResourcesEventArgs)e;

                    Material.HandleBuyMicroResourcesEvent(buyMicroResourcesInfo);

                    CommanderData.Credits -= buyMicroResourcesInfo.Price;

                    break;
                case "SellMicroResources":
                    var sellMicroResourcesInfo = (SellMicroResourcesEvent.SellMicroResourcesEventArgs)e;

                    Material.HandleSellMicroResourcesEvent(sellMicroResourcesInfo);

                    CommanderData.Credits += sellMicroResourcesInfo.Price;

                    break;

                case "TransferMicroResources":

                    var transferMicroResourcesInfo = (TransferMicroResourcesEvent.TransferMicroResourcesEventArgs)e;

                    Material.HandleTransferMicroResourcesEvent(transferMicroResourcesInfo);

                    break;

                case "TradeMicroResources":
                    var tradeMicroResourcesInfo = (TradeMicroResourcesEvent.TradeMicroResourcesEventArgs)e;

                    Material.HandleTradeMicroResourcesEvent(tradeMicroResourcesInfo);

                    break;
                /* handled back backpack.json

                case "BackPackChange": already handled in backpack.json
                    var backPackChangeInfo = (BackPackChangeEvent.BackPackChangeEventArgs)e;
                    Material.HandleBackPackChangeEvent(backPackChangeInfo);
                    break;

                case "DropItems":
                    var dropItemsInfo = (DropItemsEvent.DropItemsEventArgs)e;

                    Material.HandleDropItemsEvent(dropItemsInfo);

                    break;
                case "CollectItems":
                    var collectItemsInfo = (CollectItemsEvent.CollectItemsEventArgs)e;

                    Material.HandleCollectItemsEvent(collectItemsInfo);

                    break;
                
                case "UseConsumable":
                    var useConsumableInfo = (UseConsumableEvent.UseConsumableEventArgs)e;

                    Material.HandleUseConsumableEvent(useConsumableInfo);

                    break;*/

                case "SellOrganicData":
                    var sellOrganicDataInfo = (SellOrganicDataEvent.SellOrganicDataEventArgs)e;

                    if (sellOrganicDataInfo.BioData?.Any() == true)
                    {
                        foreach (var b in sellOrganicDataInfo.BioData)
                        {
                            CommanderData.Credits += b.Bonus + b.Value;
                        }
                    }

                    break;
                case "ScanOrganic":
                    var scanOrganicInfo = (ScanOrganicEvent.ScanOrganicEventArgs)e;

                    break;

                case "BookTaxi":
                    var bookTaxiInfo = (BookTaxiEvent.BookTaxiEventArgs)e;

                    CommanderData.Credits -= bookTaxiInfo.Cost;

                    break;
                case "BookDropship":
                    var bookDropshipInfo = (BookDropshipEvent.BookDropshipEventArgs)e;

                    CommanderData.Credits -= bookDropshipInfo.Cost;

                    break;

                case "CancelTaxi":
                    var cancelTaxiInfo = (CancelTaxiEvent.CancelTaxiEventArgs)e;

                    CommanderData.Credits += cancelTaxiInfo.Refund;

                    break;
                case "CancelDropship":
                    var cancelDropshipInfo = (CancelDropshipEvent.CancelDropshipEventArgs)e;

                    CommanderData.Credits += cancelDropshipInfo.Refund;

                    break;

                case "DropShipDeploy":
                    var dropShipDeployInfo = (DropShipDeployEvent.DropShipDeployEventArgs)e;

                    break;
                case "LoadoutRemoveModule":
                    var loadoutRemoveModuleInfo = (LoadoutRemoveModuleEvent.LoadoutRemoveModuleEventArgs)e;

                    break;
                case "LoadoutEquipModule":
                    var loadoutEquipModuleInfo = (LoadoutEquipModuleEvent.LoadoutEquipModuleEventArgs)e;

                    break;
                case "Embark":
                    var embarkInfo = (EmbarkEvent.EmbarkEventArgs)e;

                    break;
                case "Disembark":
                    var disembarkInfo = (DisembarkEvent.DisembarkEventArgs)e;

                    break;


                case "Music":

                    var musicInfo = (MusicEvent.MusicEventArgs)e;
                    
                    switch (musicInfo.MusicTrack)
                    {
                        case "SystemMap":
                        case "GalaxyMap":
                        case "DestinationFromHyperspace":
                        case "Supercruise":
                        case "DestinationFromSupercruise":
                        case "Starport":
                            break;

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

                    if (!string.IsNullOrEmpty(receiveTextInfo.From) && receiveTextInfo.Channel != TextChannel.NPC)
                    {
                        ChatHistory.PutLifo(receiveTextInfo);
                    }

                    // copied from EddiJournalMonitor https://github.com/EDCD/EDDI

                    if (receiveTextInfo.From == string.Empty && receiveTextInfo.Channel == TextChannel.NPC &&
                        (receiveTextInfo.Message.StartsWith("$COMMS_entered") ||
                         receiveTextInfo.Message.StartsWith("$CHAT_Intro")))
                    {
                        // We can safely ignore system messages that initialize the chat system or announce that we entered a receiveTextInfo.Channel - no event is needed. 
                        break;
                    }

                    if (
                        receiveTextInfo.Channel == TextChannel.Player ||
                        receiveTextInfo.Channel == TextChannel.Wing ||
                        receiveTextInfo.Channel == TextChannel.Friend ||
                        receiveTextInfo.Channel == TextChannel.VoiceChat ||
                        receiveTextInfo.Channel == TextChannel.Local ||
                        receiveTextInfo.Channel == TextChannel.Squadron ||
                        receiveTextInfo.Channel == TextChannel.StarSystem 
                    )
                    {
                        // Give priority to player messages
                        //var source = receiveTextInfo.Channel == TextChannel.Squadron ? "Squadron mate" :
                        //    receiveTextInfo.Channel == TextChannel.Wing ? "Wing mate" :
                        //    receiveTextInfo.Channel == null ? "Crew mate" : "Commander";

                        //var receiveTextInfo.Channel = receiveTextInfo.Channel ?? "multicrew";

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

            if (evt != "FSSSignalDiscovered" && evt != "FSSDiscoveryScan" && evt != "Music" && evt != "ReceiveText" && evt != "FuelScoop" && evt != "ReservoirReplenished")
            {
                App.FipHandler.RefreshDevicePages();
            }

        }
    }
}
