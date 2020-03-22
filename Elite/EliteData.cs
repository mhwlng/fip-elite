using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Elite.RingBuffer;
using EliteJournalReader;
using EliteJournalReader.Events;


namespace Elite
{
    public class EliteData
    {
        public static List<PoiItem> CurrentPois = new List<PoiItem>();
        public static List<StationData> CurrentInterStellarFactors = new List<StationData>();
        public static List<StationData> CurrentRawMaterialTraders = new List<StationData>();
        public static List<StationData> CurrentManufacturedMaterialTraders = new List<StationData>();
        public static List<StationData> CurrentEncodedDataTraders = new List<StationData>();
        public static List<StationData> CurrentHumanTechnologyBrokers = new List<StationData>();
        public static List<StationData> CurrentGuardianTechnologyBrokers = new List<StationData>();

        public static  RingBuffer<string> EventHistory = new RingBuffer<string>(50, true);

        public static readonly Dictionary<string, string> ShipsByEliteID = new Dictionary<string, string>()
        {
            {
                "sidewinder", "Sidewinder"
            },
            {
                "eagle", "Eagle"
            },
            {
                "hauler", "Hauler"
            },
            {
                "adder", "Adder"
            },
            {
                "viper", "Viper Mk III"
            },
            {
                "cobramkiii", "Cobra Mk III"
            },
            {
                "type6", "Type-6 Transporter"
            },
            {
                "dolphin", "Dolphin"
            },
            {
                "type7", "Type-7 Transporter"
            },
            {
                "asp", "Asp Explorer"
            },
            {
                "vulture", "Vulture"
            },
            {
                "empire_trader", "Imperial Clipper"
            },
            {
                "federation_dropship", "Federal Dropship"
            },
            {
                "orca", "Orca"
            },
            {
                "type9", "Type-9 Heavy"
            },
            {
                "type9_military", "Type-10 Defender"
            },
            {
                "python", "Python"
            },
            {
                "belugaliner", "Beluga Liner"
            },
            {
                "ferdelance", "Fer-de-Lance"
            },
            {
                "anaconda", "Anaconda"
            },
            {
                "federation_corvette", "Federal Corvette"
            },
            {
                "cutter", "Imperial Cutter"
            },
            {
                "diamondback", "Diamondback Scout"
            },
            {
                "empire_courier", "Imperial Courier"
            },
            {
                "diamondbackxl", "Diamondback Explorer"
            },
            {
                "empire_eagle", "Imperial Eagle"
            },
            {
                "federation_dropship_mkii", "Federal Assault Ship"
            },
            {
                "federation_gunship", "Federal Gunship"
            },
            {
                "viper_mkiv", "Viper Mk IV"
            },
            {
                "cobramkiv", "Cobra Mk IV"
            },
            {
                "independant_trader", "Keelback"
            },
            {
                "asp_scout", "Asp Scout"
            },
            {
                "testbuggy", "SRV"
            },
            {
                "typex", "Alliance Chieftain"
            },
            {
                "typex_2", "Alliance Crusader"
            },
            {
                "typex_3", "Alliance Challenger"
            },
            {
                "krait_mkii", "Krait Mk II"
            },
            {
                "krait_light", "Krait Phantom"
            },
            {
                "mamba", "Mamba"
            },
            {
                "empire_fighter", "Imperial Fighter"
            },
            {
                "federation_fighter", "F63 Condor"
            },
            {
                "independent_fighter", "Taipan Fighter"
            },
            {
                "gdn_hybrid_fighter_v1", "Trident"
            },
            {
                "gdn_hybrid_fighter_v2", "Javelin"
            },
            {
                "gdn_hybrid_fighter_v3", "Lance"
            },
            {
                "unknown", "Unknown"
            },
            {
                "unknown (crew)", "Unknown (crew)"
            },
            {
                "unknown (captain)", "Unknown (captain)"
            }
        };


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
                if (from.Contains("Police"))
                {
                    by = "Police";
                }
                else
                {
                    by = "Bounty hunter";
                }
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

        public class Commander
        {
            public string Name { get; set; } = "";
            public uint Credits { get; set; } = 0;
            public long Rebuy { get; set; } = 0;

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

        public class Ship
        {
            public bool AutomaticDocking { get; set; }

            public string Name { get; set; } = "";
            public string Type { get; set; } = "";

            public long CargoCapacity { get; set; }

            public double HullHealth { get; set; } = -1;

            public int HullValue { get; set; } = -1;
            public int ModulesValue { get; set; } = -1;

            public double UnladenMass { get; set; } = -1;
            public double MaxJumpRange { get; set; } = -1;

            public bool Hot { get; set; }

        }

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

            public bool HideBody { get; set; } = false;

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

            public double FuelCapacity { get; set; }
        
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
        public static Ship ShipData = new Ship();
        public static Target TargetData = new Target();
        public static Location LocationData = new Location();
        public static Dock DockData = new Dock();

        public static Status StatusData = new Status();

        public static List<Mission> MissionData = new List<Mission>();

        public static void HandleStatusEvents(object sender, StatusFileEvent evt)
        {
            StatusData.Docked = (evt.Flags & StatusFlags.Docked) != 0;
            StatusData.Landed = (evt.Flags & StatusFlags.Landed) != 0;
            StatusData.LandingGearDown = (evt.Flags & StatusFlags.LandingGearDown) != 0;
            StatusData.ShieldsUp = (evt.Flags & StatusFlags.ShieldsUp) != 0;
            StatusData.Supercruise = (evt.Flags & StatusFlags.Supercruise) != 0;
            StatusData.FlightAssistOff = (evt.Flags & StatusFlags.FlightAssistOff) != 0;
            StatusData.HardpointsDeployed = (evt.Flags & StatusFlags.HardpointsDeployed) != 0;
            StatusData.InWing = (evt.Flags & StatusFlags.InWing) != 0;
            StatusData.LightsOn = (evt.Flags & StatusFlags.LightsOn) != 0;
            StatusData.CargoScoopDeployed = (evt.Flags & StatusFlags.CargoScoopDeployed) != 0;
            StatusData.SilentRunning = (evt.Flags & StatusFlags.SilentRunning) != 0;
            StatusData.ScoopingFuel = (evt.Flags & StatusFlags.ScoopingFuel) != 0;
            StatusData.SrvHandbrake = (evt.Flags & StatusFlags.SrvHandbrake) != 0;
            StatusData.SrvTurret = (evt.Flags & StatusFlags.SrvTurret) != 0;
            StatusData.SrvUnderShip = (evt.Flags & StatusFlags.SrvUnderShip) != 0;
            StatusData.SrvDriveAssist = (evt.Flags & StatusFlags.SrvDriveAssist) != 0;
            StatusData.FsdMassLocked = (evt.Flags & StatusFlags.FsdMassLocked) != 0;
            StatusData.FsdCharging = (evt.Flags & StatusFlags.FsdCharging) != 0;
            StatusData.FsdCooldown = (evt.Flags & StatusFlags.FsdCooldown) != 0;
            StatusData.LowFuel = (evt.Flags & StatusFlags.LowFuel) != 0;
            StatusData.Overheating = (evt.Flags & StatusFlags.Overheating) != 0;
            StatusData.HasLatLong = (evt.Flags & StatusFlags.HasLatLong) != 0;
            StatusData.IsInDanger = (evt.Flags & StatusFlags.IsInDanger) != 0;
            StatusData.BeingInterdicted = (evt.Flags & StatusFlags.BeingInterdicted) != 0;
            StatusData.InMainShip = (evt.Flags & StatusFlags.InMainShip) != 0;
            StatusData.InFighter = (evt.Flags & StatusFlags.InFighter) != 0;
            StatusData.InSRV = (evt.Flags & StatusFlags.InSRV) != 0;
            StatusData.HudInAnalysisMode = (evt.Flags & StatusFlags.HudInAnalysisMode) != 0;
            StatusData.NightVision = (evt.Flags & StatusFlags.NightVision) != 0;
            StatusData.AltitudeFromAverageRadius = (evt.Flags & StatusFlags.AltitudeFromAverageRadius) != 0;
            StatusData.FsdJump = (evt.Flags & StatusFlags.FsdJump) != 0;
            StatusData.SrvHighBeam = (evt.Flags & StatusFlags.SrvHighBeam) != 0;

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

            App.fipHandler.RefreshDevicePages();
        }

        private static int UpdateCargoCapacity(string item, int addremove)
        {
            if (item?.Contains("_cargorack_") == true)
            {
                var size = item.Substring(item.IndexOf("_size", StringComparison.OrdinalIgnoreCase) + 5);

                size = size.Substring(0, size.IndexOf("_", StringComparison.OrdinalIgnoreCase));

                return addremove * Convert.ToInt32(size);
            }

            return 0;
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

        public static void HandleEliteEvents(object sender, JournalEventArgs e)
        {
            var evt = ((JournalEventArgs) e).OriginalEvent.Value<string>("event");

            if (string.IsNullOrWhiteSpace(evt))
            {
                return;
            }

            if (evt != "FSSSignalDiscovered" && evt != "FSSDiscoveryScan")
            {
                EventHistory.Put(DateTime.Now.ToLongTimeString() + " : " + evt);
            }

            switch (evt)
            {
                // ------ AT STARTUP -------------

                case "LoadGame":
                    //When written: at startup, when loading from main menu into game

                    var loadGameInfo = (LoadGameEvent.LoadGameEventArgs) e;

                    //FID
                    //Horizons
                    //StartLanded
                    //StartDead
                    //GameMode
                    //Group
                    //Loan
                    //ShipIdent
                    //FuelLevel

                    ShipData.Name = loadGameInfo.ShipName;

                    ShipsByEliteID.TryGetValue(loadGameInfo.Ship?.ToLower() ?? "???", out var ship);
                    ShipData.Type = ship ?? loadGameInfo.Ship;

                    CommanderData.Name = loadGameInfo.Commander;
                    CommanderData.Credits = Convert.ToUInt32(loadGameInfo.Credits);

                    StatusData.FuelCapacity = loadGameInfo.FuelCapacity;

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

                    var reputationInfo = (ReputationEvent.ReputationEventArgs)e;

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

                    ShipData.Name = setUserShipNameInfo.UserShipName;

                    ShipsByEliteID.TryGetValue(setUserShipNameInfo.Ship?.ToLower() ?? "???", out var tgtShip);
                    ShipData.Type = tgtShip ?? setUserShipNameInfo.Ship;

                    break;

                case "Location":
                    //When written: at startup, or when being resurrected at a station

                    var locationInfo = (LocationEvent.LocationEventArgs)e;

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
                    //PowerplayState
                    //Factions
                    //Conflicts

                    LocationData.StarSystem = locationInfo.StarSystem;

                    LocationData.StarPos = locationInfo.StarPos.ToList();

                    CurrentPois = Poi.GetNearestPoiItems(LocationData.StarPos);

                    CurrentInterStellarFactors =
                        Station.GetNearestStationItems(LocationData.StarPos, App.InterStellarFactors);
                    CurrentRawMaterialTraders =
                        Station.GetNearestStationItems(LocationData.StarPos, App.RawMaterialTraders);
                    CurrentManufacturedMaterialTraders =
                        Station.GetNearestStationItems(LocationData.StarPos, App.ManufacturedMaterialTraders);
                    CurrentEncodedDataTraders =
                        Station.GetNearestStationItems(LocationData.StarPos, App.EncodedDataTraders);
                    CurrentHumanTechnologyBrokers =
                        Station.GetNearestStationItems(LocationData.StarPos, App.HumanTechnologyBrokers);
                    CurrentGuardianTechnologyBrokers =
                        Station.GetNearestStationItems(LocationData.StarPos, App.GuardianTechnologyBrokers);

                    LocationData.SystemAllegiance = locationInfo.SystemAllegiance;
                    LocationData.SystemFaction = locationInfo.SystemFaction?.Name;
                    LocationData.SystemSecurity = locationInfo.SystemSecurity_Localised;
                    LocationData.SystemEconomy = locationInfo.SystemEconomy_Localised;
                    LocationData.SystemGovernment = locationInfo.SystemGovernment_Localised;
                    LocationData.Population = locationInfo.Population;
                    LocationData.Body = locationInfo.Body;
                    LocationData.BodyType = locationInfo.BodyType.ToString();

                    LocationData.HideBody = false;

                    break;

                case "Missions":
                    //When written: at startup
                    var missionsInfo = (MissionsEvent.MissionsEventArgs)e;

                    if (missionsInfo.Active?.Length > 0)
                    {
                        MissionData = missionsInfo.Active.Select(x => new Mission
                        {
                            MissionID = x.MissionID,
                            PassengerMission = x.PassengerMission,
                            Expires = (DateTime?)null, 
                            Name = x.Name
                        }).ToList();

                    }

                    break;


                case "Loadout":
                    //When written: at startup, when loading from main menu, or when switching ships, 

                    var loadoutInfo = (LoadoutEvent.LoadoutEventArgs)e;

                    //ShipID
                    //ShipIdent

                    ShipData.HullHealth = loadoutInfo.HullHealth * 100.0;

                    CommanderData.Rebuy = loadoutInfo.Rebuy;

                    //Ship:Python,
                    //Ident:MH-08P,
                    //Modules:31,
                    //Hull Health:100,0%,
                    //Hull:55.323.684 cr,
                    //Modules:32.315.494 cr,
                    //Rebuy:4.381.497 cr

                    ShipData.Name = loadoutInfo.ShipName;

                    ShipsByEliteID.TryGetValue(loadoutInfo.Ship?.ToLower() ?? "???", out var loadShip);
                    ShipData.Type = loadShip ?? loadoutInfo.Ship;

                    StatusData.FuelCapacity = loadoutInfo.FuelCapacity.Main;

                    ShipData.HullValue = loadoutInfo.HullValue; 
                    ShipData.ModulesValue = loadoutInfo.ModulesValue;
                    ShipData.UnladenMass = loadoutInfo.UnladenMass;
                    ShipData.MaxJumpRange = loadoutInfo.MaxJumpRange;
                    ShipData.Hot = loadoutInfo.Hot;

                    ShipData.CargoCapacity = 0;

                    foreach (var m in loadoutInfo.Modules.Where(x => x.Item.Contains("_cargorack_")))
                    {
                        ShipData.CargoCapacity += UpdateCargoCapacity(m.Item, 1);
                    }

                    break;

                case "ModuleBuy":
                    var moduleBuyInfo = (ModuleBuyEvent.ModuleBuyEventArgs) e;

                    ShipData.CargoCapacity += UpdateCargoCapacity(moduleBuyInfo.BuyItem, 1);
                    ShipData.CargoCapacity += UpdateCargoCapacity(moduleBuyInfo.SellItem, -1);

                    break;

                case "ModuleSell":
                    var moduleSellInfo = (ModuleSellEvent.ModuleSellEventArgs) e;

                    ShipData.CargoCapacity += UpdateCargoCapacity(moduleSellInfo.SellItem, -1);

                    break;

                case "ModuleStore":
                    var moduleStoreInfo = (ModuleStoreEvent.ModuleStoreEventArgs) e;

                    ShipData.CargoCapacity += UpdateCargoCapacity(moduleStoreInfo.StoredItem, -1);

                    break;

                case "ModuleRetrieve":
                    var moduleRetrieveInfo = (ModuleRetrieveEvent.ModuleRetrieveEventArgs) e;

                    ShipData.CargoCapacity += UpdateCargoCapacity(moduleRetrieveInfo.RetrievedItem, 1);

                    break;

                case "MassModuleStore":
                    var massModuleStoreInfo = (MassModuleStoreEvent.MassModuleStoreEventArgs) e;

                    foreach (var i in massModuleStoreInfo.Items)
                    {
                        ShipData.CargoCapacity += UpdateCargoCapacity(i.Name, -1);
                    }

                    break;

                case "RefuelAll":

                    var refuelAllInfo = (RefuelAllEvent.RefuelAllEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(refuelAllInfo.Cost);
                    break;

                case "RepairAll":

                    var repairAllInfo = (RepairAllEvent.RepairAllEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(repairAllInfo.Cost);
                    break;

                case "Repair":

                    var repairInfo = (RepairEvent.RepairEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(repairInfo.Cost);
                    break;

                case "BuyTradeData":

                    var buyTradeDataInfo = (BuyTradeDataEvent.BuyTradeDataEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyTradeDataInfo.Cost);
                    break;

                case "BuyExplorationData":

                    var buyExplorationDataInfo = (BuyExplorationDataEvent.BuyExplorationDataEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyExplorationDataInfo.Cost);
                    break;

                case "BuyDrones":

                    var buyDronesInfo = (BuyDronesEvent.BuyDronesEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyDronesInfo.TotalCost);
                    break;

                case "BuyAmmo":

                    var buyAmmoInfo = (BuyAmmoEvent.BuyAmmoEventArgs) e;

                    CommanderData.Credits -= Convert.ToUInt32(buyAmmoInfo.Cost);
                    break;

                case "ApproachBody":

                    var approachBodyInfo = (ApproachBodyEvent.ApproachBodyEventArgs) e;

                    LocationData.StarSystem = approachBodyInfo.StarSystem;

                    LocationData.Body = approachBodyInfo.Body;
                    LocationData.BodyType = "Planet";

                    LocationData.HideBody = false;

                    break;

                case "ApproachSettlement":

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

                    var leaveBodyInfo = (LeaveBodyEvent.LeaveBodyEventArgs) e;

                    //Body

                    LocationData.StarSystem = leaveBodyInfo.StarSystem;

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    LocationData.HideBody = true;

                    break;

                case "Undocked":

                    var undockedInfo = (UndockedEvent.UndockedEventArgs) e;

                    //StationName
                    //StationType

                    DockData = new Dock();

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    break;

                case "Docked":

                    var dockedInfo = (DockedEvent.DockedEventArgs) e;

                    //CockpitBreach
                    //StationEconomies
                    //Wanted
                    //ActiveFine

                    LocationData.StarSystem = dockedInfo.StarSystem; 

                    LocationData.Station = dockedInfo.StationName;

                    ShipData.AutomaticDocking = false;

                    DockData.Type = dockedInfo.StationType;

                    DockData.Government = dockedInfo.StationGovernment_Localised;
                    DockData.Allegiance = dockedInfo.StationAllegiance;
                    DockData.Faction = dockedInfo.StationFaction?.Name;
                    DockData.Economy = dockedInfo.StationEconomy_Localised;
                    DockData.DistFromStarLs = dockedInfo.DistFromStarLS;

                    DockData.Services = string.Join(", ", dockedInfo.StationServices);

                    DockData.LandingPad = -1;

                    break;

                case "DockingGranted":

                    var dockingGrantedInfo = (DockingGrantedEvent.DockingGrantedEventArgs) e;

                    DockData.Type = dockingGrantedInfo.StationType;

                    LocationData.Body = dockingGrantedInfo.StationName;
                    LocationData.BodyType = "Station";

                    DockData.LandingPad = Convert.ToInt32(dockingGrantedInfo.LandingPad);
                    break;

                case "DockingRequested":

                    var dockingRequestedInfo = (DockingRequestedEvent.DockingRequestedEventArgs) e;

                    LocationData.Station = dockingRequestedInfo.StationName;

                    LocationData.Body = dockingRequestedInfo.StationName;
                    LocationData.BodyType = "Station";
                    break;

                case "FSDJump":

                    var fsdJumpInfo = (FSDJumpEvent.FSDJumpEventArgs) e;

                    //FuelUsed
                    //FuelLevel
                    //BoostUsed
                    //SystemSecondEconomy_Localised
                    //Wanted
                    //Powers
                    //PowerplayState

                    LocationData.Body = fsdJumpInfo.Body;  

                    LocationData.StarSystem = fsdJumpInfo.StarSystem;

                    if (StatusData.JumpRange < fsdJumpInfo.JumpDist)
                    {
                        StatusData.JumpRange = fsdJumpInfo.JumpDist;
                    }

                    LocationData.StarPos = fsdJumpInfo.StarPos.ToList();

                    CurrentPois = Poi.GetNearestPoiItems(LocationData.StarPos);

                    TravelHistory.AddTravelPos(LocationData.StarPos);

                    CurrentInterStellarFactors =
                        Station.GetNearestStationItems(LocationData.StarPos, App.InterStellarFactors);
                    CurrentRawMaterialTraders =
                        Station.GetNearestStationItems(LocationData.StarPos, App.RawMaterialTraders);
                    CurrentManufacturedMaterialTraders =
                        Station.GetNearestStationItems(LocationData.StarPos, App.ManufacturedMaterialTraders);
                    CurrentEncodedDataTraders =
                        Station.GetNearestStationItems(LocationData.StarPos, App.EncodedDataTraders);
                    CurrentHumanTechnologyBrokers =
                        Station.GetNearestStationItems(LocationData.StarPos, App.HumanTechnologyBrokers);
                    CurrentGuardianTechnologyBrokers =
                        Station.GetNearestStationItems(LocationData.StarPos, App.GuardianTechnologyBrokers);

                    LocationData.StartJump = false;
                    LocationData.JumpToSystem = "";
                    LocationData.JumpToStarClass = "";
                    LocationData.JumpType = "";

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
                    var fSdTargetInfo = (FSDTargetEvent.FSDTargetEventArgs) e;

                    LocationData.FsdTargetName = fSdTargetInfo.Name;

                    LocationData.RemainingJumpsInRoute = fSdTargetInfo.RemainingJumpsInRoute;

                    break;

                case "SupercruiseEntry":

                    var supercruiseEntryInfo = (SupercruiseEntryEvent.SupercruiseEntryEventArgs) e;

                    LocationData.JumpToSystem = supercruiseEntryInfo.StarSystem;

                    LocationData.Body = "";
                    LocationData.BodyType = "";

                    LocationData.HideBody = true;

                    break;

                case "SupercruiseExit":

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
                    var hullDamageInfo = (HullDamageEvent.HullDamageEventArgs) e;

                    ShipData.HullHealth = hullDamageInfo.Health * 100.0;

                    break;

                case "ShipTargeted":
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

                    ShipsByEliteID.TryGetValue(shipTargetedInfo.Ship?.ToLower() ?? "???", out var targetShip);

                    TargetData.Ship = shipTargetedInfo.Ship_Localised ?? targetShip ?? shipTargetedInfo.Ship;

                    TargetData.SubsystemLocalised = shipTargetedInfo.Subsystem_Localised;
                    TargetData.TargetLocked = shipTargetedInfo.TargetLocked;
                    TargetData.SubsystemHealth = shipTargetedInfo.SubSystemHealth;

                    break;

                case "MissionAccepted":
                    //When Written: when starting a mission 
                    var missionAcceptedInfo = (MissionAcceptedEvent.MissionAcceptedEventArgs) e;

                    MissionData.RemoveAll(x => x.MissionID == missionAcceptedInfo.MissionID);

                    MissionData.Add(new Mission
                    {
                        MissionID = missionAcceptedInfo.MissionID,
                        Name = missionAcceptedInfo.LocalisedName,
                        Expires = missionAcceptedInfo.Expiry,
                        PassengerMission = missionAcceptedInfo.PassengerCount > 0,
                        System = missionAcceptedInfo.DestinationSystem,
                        Station = missionAcceptedInfo.DestinationStation,
                        Reward = missionAcceptedInfo.Reward,
                        Passengers = missionAcceptedInfo.PassengerCount,

                        Faction = missionAcceptedInfo.Faction,
                        Influence = missionAcceptedInfo.Influence.ToString(), //    None/Low/Med/High
                        Reputation = missionAcceptedInfo.Reputation.ToString(), //  None/Low/Med/High

                        CommodityLocalised = missionAcceptedInfo.Commodity_Localised,
                        Count = missionAcceptedInfo.Count,

                        PassengerViPs = missionAcceptedInfo.PassengerVIPs,
                        PassengerWanted = missionAcceptedInfo.PassengerWanted,
                        PassengerType = missionAcceptedInfo.PassengerType,

                        Wing = missionAcceptedInfo.Wing,
                        Target = missionAcceptedInfo.Target,
                        TargetType = missionAcceptedInfo.TargetType,
                        TargetFaction = missionAcceptedInfo.TargetFaction,
                        KillCount = missionAcceptedInfo.KillCount,
                        Donation = missionAcceptedInfo.Donation,
                        Donated = missionAcceptedInfo.Donated,

                    });

                    break;

                case "MissionCompleted":
                    //When Written: when a mission is completed

                    var missionCompletedInfo = (MissionCompletedEvent.MissionCompletedEventArgs) e;

                    CommanderData.Credits += Convert.ToUInt32(missionCompletedInfo.Reward);

                    MissionData.RemoveAll(x => x.MissionID == missionCompletedInfo.MissionID);

                    break;
                case "MissionAbandoned":
                    //When Written: when a mission has been abandoned 

                    var missionAbandonedInfo = (MissionAbandonedEvent.MissionAbandonedEventArgs) e;

                    MissionData.RemoveAll(x => x.MissionID == missionAbandonedInfo.MissionID);

                    break;
                case "MissionFailed":
                    //When Written: when a mission has failed 

                    var missionFailedInfo = (MissionFailedEvent.MissionFailedEventArgs) e;

                    MissionData.RemoveAll(x => x.MissionID == missionFailedInfo.MissionID);

                    break;

                case "Music":

                    var musicInfo = (MusicEvent.MusicEventArgs)e;

                    switch (musicInfo.MusicTrack)
                    {
                        case "MainMenu":
                            //TabLCDStartElite.Create();
                            break;

                        case "DockingComputer":
                            ShipData.AutomaticDocking = true;

                            break;

                        case "NoTrack":
                            if (ShipData.AutomaticDocking)
                            {
                                ShipData.AutomaticDocking = false;
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
                        var source = channel == "squadron" ? "Squadron mate" :
                            channel == "wing" ? "Wing mate" :
                            channel == null ? "Crew mate" : "Commander";

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
                        else if ((receiveTextInfo.Message.StartsWith("$STATION_")) ||
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

            App.fipHandler.RefreshDevicePages();

        }

    }
}
