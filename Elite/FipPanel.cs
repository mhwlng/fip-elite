using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elite.RingBuffer;
using EliteAPI.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TheArtOfDev.HtmlRenderer.WinForms;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;

// For extension methods.


// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Elite
{
    public enum LCDTab
    {
        Init=-1,

        None = 0,
        Commander = 1,
        Ship = 2,
        Navigation = 3,
        Target = 4,
        Missions = 5,
        POI = 6
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
        public double DistFromStarLs { get; set; } = -1;
    }


    public class Ship
    {
        public bool AutomaticDocking { get; set; }

        public string Name { get; set; } = "";
        public string Type { get; set; } = "";

        public long CargoCapacity { get; set; }

        public double HullHealth { get; set; } = -1;

    }

    public class Mission
    {
        public long MissionId { get; set; }
        public string Name { get; set; }
        public bool PassengerMission { get; set; }
        public DateTime? Expires { get; set; } 

        public string System { get; set; }
        public long Reward { get; set; }
        public long Passengers { get; set; }

        public string Faction { get; set; }
        public string Influence { get; set; } //    None/Low/Med/High
        public string Reputation { get; set; } //  None/Low/Med/High

        public string CommodityLocalised { get; set; }
        public long Count { get; set; }

        public bool PassengerViPs { get; set; }
        public bool PassengerWanted { get; set; }
        public string PassengerType { get; set; }

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

        public string Body { get; set; }
        public string BodyType { get; set; }

        public string SystemAllegiance { get; set; }
        public string SystemFaction { get; set; }
        public string SystemSecurity { get; set; }
        public string SystemEconomy { get; set; }
        public string SystemGovernment { get; set; }

        public long Population { get; set; }

        public bool HideBody { get; set; } = false;

        public List<double> StarPos { get; set; } // array[x, y, z], in light years
    }

    public class MyHtmlHelper
    {
        public IEncodedString Raw(string rawString)
        {
            return new RawString(rawString);
        }
    }

    public abstract class HtmlSupportTemplateBase<T> : TemplateBase<T>
    {
        public HtmlSupportTemplateBase()
        {
            Html = new MyHtmlHelper();
        }

        public MyHtmlHelper Html { get; set; }
    }

    class FipPanel
    {

        private readonly Dictionary<string, string> ShipsByEliteID = new Dictionary<string, string>()
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


        private readonly object _refreshDevicePageLock = new object();


        private LCDTab _currentTab = LCDTab.None;
        private LCDTab _lastTab = LCDTab.Init;

        private int[] _currentCard = new int[10];


        private int CurrentLCDYOffset = 0;
        private int CurrentLCDHeight = 0;

        public IntPtr FipDevicePointer;

        const int DEFAULT_PAGE = 0;
        private uint _currentPage = DEFAULT_PAGE;
        private uint _prevButtons;

        private List<uint> _pageList = new List<uint>();

        private readonly Pen _scrollPen = new Pen(Color.FromArgb(0xff,0xFF,0xB0,0x00));
        private readonly SolidBrush _scrollBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xB0, 0x00));
        
        private const int HtmlMenuWindowWidth = 69;

        private const int HtmlWindowHeight = 240;

        private const int HtmlWindowXOffset = HtmlMenuWindowWidth + 1;
        private const int HtmlWindowWidth = 311- HtmlWindowXOffset;
        

        protected bool DoShutdown;
        //private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        //protected Thread GraphicsDrawingThread;

        protected DirectOutputClass.PageCallback PageCallbackDelegate;
        protected DirectOutputClass.SoftButtonCallback SoftButtonCallbackDelegate;

        protected EventHandler<dynamic> HandleEliteEventsDelegate;

        private RingBuffer<string> _eventHistory = new RingBuffer<string>(50, true);

        private Commander _commanderData = new Commander();
        private Ship _shipData = new Ship();
        private Target _targetData = new Target();
        private List<Mission> _missionData = new List<Mission>();
        private Location _locationData = new Location();
        private Dock _dockData = new Dock();

        private List<PoiItem> _currentPois = new List<PoiItem>();

        private List<StationData> _currentInterStellarFactors = new List<StationData>();
        private List<StationData> _currentRawMaterialTraders = new List<StationData>();
        private List<StationData> _currentManufacturedMaterialTraders = new List<StationData>();
        private List<StationData> _currentEncodedDataTraders = new List<StationData>();
        private List<StationData> _currentHumanTechnologyBrokers = new List<StationData>();
        private List<StationData> _currentGuardianTechnologyBrokers = new List<StationData>();

        public FipPanel(IntPtr devicePtr) 
        {
            FipDevicePointer = devicePtr;
        }

        public void Initalize()
        {
            // FIP = 3e083cd8-6a37-4a58-80a8-3d6a2c07513e

            // https://github.com/Raptor007/Falcon4toSaitek/blob/master/Raptor007's%20Falcon%204%20to%20Saitek%20Utility/DirectOutput.h
            //https://github.com/poiuqwer78/fip4j-core/tree/master/src/main/java/ch/poiuqwer/saitek/fip4j

            //GraphicsDrawingThread = new Thread(ThreadedImageGenerator);
            //GraphicsDrawingThread.Start();

            PageCallbackDelegate = PageCallback;
            SoftButtonCallbackDelegate = SoftButtonCallback;

            HandleEliteEventsDelegate = HandleEliteEvents;

            var returnValues1 = DirectOutputClass.RegisterPageCallback(FipDevicePointer, PageCallbackDelegate);
            if (returnValues1 != ReturnValues.S_OK)
            {
                App.log.Error("FipPanel failed to init RegisterPageCallback. " + returnValues1);
            }
            var returnValues2 = DirectOutputClass.RegisterSoftButtonCallback(FipDevicePointer, SoftButtonCallbackDelegate);
            if (returnValues2 != ReturnValues.S_OK)
            {
                App.log.Error("FipPanel failed to init RegisterSoftButtonCallback. " + returnValues1);
            }


            AddPage(DEFAULT_PAGE, true);
            //AddPage(1, false);

            RefreshDevicePage(DEFAULT_PAGE);

            App.EliteApi.Events.AllEvent += HandleEliteEventsDelegate;

        }

        public void Shutdown()
        {
            try
            {
                DoShutdown = true;

                App.EliteApi.Events.AllEvent -= HandleEliteEventsDelegate;

                if (_pageList.Count > 0)
                {
                    do
                    {
                        DirectOutputClass.RemovePage(FipDevicePointer, _pageList[0]);

                        _pageList.Remove(_pageList[0]);


                    } while (_pageList.Count > 0);
                    //Closed = true;
                }

                //_autoResetEvent.Set();

            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

        }

        private bool SetTab(LCDTab tab)
        {
            if (_currentTab != tab)
            {
                _lastTab = _currentTab;
                _currentTab = tab;

                CurrentLCDYOffset = 0;

                return true;
            }

            return false;
        }

        private void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context)
        {
            if (device == FipDevicePointer)
            {
                if (bActivated != 0)
                {
                    _currentPage = (uint)page;
                    RefreshDevicePage(_currentPage);
                }
            }
        }

        private void SoftButtonCallback(IntPtr device, IntPtr buttons, IntPtr context)
        {
            if (device == FipDevicePointer & (uint) buttons != _prevButtons)
            {
                uint button = (uint) buttons ^ _prevButtons;
                bool state = ((uint) buttons & button) == button;
                _prevButtons = (uint) buttons;

                //Console.WriteLine($"button {button}  state {state}");

                bool mustRefresh = false;

                switch (button)
                {
                    case 8: // scroll clockwise
                        if (state)
                        {
                            _currentCard[(int)_currentTab]++;
                            CurrentLCDYOffset = 0;

                            mustRefresh = true;
                        }
                        break;
                    case 16: // scroll anti-clockwise

                        if (state)
                        {
                            _currentCard[(int)_currentTab]--;
                            CurrentLCDYOffset = 0;

                            mustRefresh = true;
                        }
                        break;
                    case 2: // scroll clockwise
                        if (state)
                        {
                            CurrentLCDYOffset += 50;

                            mustRefresh = true;
                        }
                        break;
                    case 4: // scroll anti-clockwise

                        if (CurrentLCDYOffset == 0) return;

                        if (state)
                        {
                            CurrentLCDYOffset -= 50;
                            if (CurrentLCDYOffset < 0)
                            {
                                CurrentLCDYOffset = 0;
                            }

                            mustRefresh = true;
                        }
                        break;
                    case 32:
                        mustRefresh = SetTab(LCDTab.Commander);
                        break;
                    case 64:
                        mustRefresh = SetTab(LCDTab.Ship);
                        break;
                    case 128:
                        mustRefresh = SetTab(LCDTab.Navigation);
                        break;
                    case 256:
                        if (_targetData.TargetLocked)
                        {
                            mustRefresh = SetTab(LCDTab.Target);
                        }
                        break;
                    case 512:
                        if (_missionData.Count > 0)
                        {
                            mustRefresh = SetTab(LCDTab.Missions);
                        }
                        break;
                    case 1024:
                        mustRefresh = SetTab(LCDTab.POI);

                        break;
                }

                if (mustRefresh)
                {
                    RefreshDevicePage(_currentPage);
                }

            }
        }

        private void CheckLcdOffset()
        {
            if (CurrentLCDHeight <= HtmlWindowHeight)
            {
                CurrentLCDYOffset = 0;
            }

            if (CurrentLCDYOffset + HtmlWindowHeight > CurrentLCDHeight )
            {
                CurrentLCDYOffset = CurrentLCDHeight - HtmlWindowHeight + 4;
            }

            if (CurrentLCDYOffset < 0) CurrentLCDYOffset = 0;
        }

        private ReturnValues AddPage(uint pageNumber, bool setActive)
        {
            var result = ReturnValues.E_FAIL;
            try
            {
                if (_pageList.Contains(pageNumber))
                {
                    return ReturnValues.S_OK;
                }

                result = DirectOutputClass.AddPage(FipDevicePointer, (IntPtr)((long)pageNumber), string.Concat(new object[4]
                                                      {
                                                    "0x",
                                                    FipDevicePointer.ToString(),
                                                    " PageNo: ",
                                                    pageNumber
                                                      }), setActive);
                if (result == ReturnValues.S_OK)
                {
                    App.log.Info("Page: " + (pageNumber) + " added");
                    
                    _pageList.Add(pageNumber);
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }
            return result;
        }

        private ReturnValues SetImage(uint page, Bitmap fipImage)
        {
            if (fipImage == null)
            {
                return ReturnValues.E_INVALIDARG;
            }

            try
            {
                var bitmapData = fipImage.LockBits(new System.Drawing.Rectangle(0, 0, fipImage.Width, fipImage.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var intPtr = bitmapData.Scan0;
                var local3 = bitmapData.Stride * fipImage.Height;
                DirectOutputClass.SetImage(FipDevicePointer, page, 0, local3, intPtr);
                fipImage.UnlockBits(bitmapData);
                return ReturnValues.S_OK;
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }
            return ReturnValues.E_FAIL;
        }

        public void RefreshDevicePage(uint page)
        {
            lock (_refreshDevicePageLock)
            {
                if (_missionData.Count == 0 && _currentTab == LCDTab.Missions)
                {
                    SetTab(LCDTab.Navigation);
                }
                else if (!_targetData.TargetLocked && _currentTab == LCDTab.Target)
                {
                    SetTab(LCDTab.Navigation);
                }

                using (var fipImage = new Bitmap(320, 240))
                {
                    using (var graphics = Graphics.FromImage(fipImage))
                    {
                        var menustr =
                            Engine.Razor.Run("menu.cshtml", null, new
                            {
                                CurrentTab = (int)_currentTab,

                                TargetLocked = _targetData.TargetLocked,

                                MissionCount = _missionData.Count
                            });

                        var str = "";

                        switch (_currentTab)
                        {
                            case LCDTab.Commander:

                                str =
                                    Engine.Razor.Run("1.cshtml", null, new
                                    {
                                        CurrentTab = (int) _currentTab,

                                        Commander = _commanderData.Name,

                                        ShipName = _shipData.Name?.Trim(),

                                        ShipType = _shipData.Type?.Trim(),

                                        LegalState = App.EliteApi.Status.LegalState,

                                        Credits = _commanderData.Credits.ToString("N0"),

                                        Rebuy = _commanderData.Rebuy.ToString("N0"),

                                        FederationRank = App.EliteApi.Commander.FederationRankLocalised,
                                        FederationRankProgress = App.EliteApi.Commander.FederationRankProgress,

                                        EmpireRank = App.EliteApi.Commander.EmpireRankLocalised,
                                        EmpireRankProgress = App.EliteApi.Commander.EmpireRankProgress,

                                        CombatRank = App.EliteApi.Commander.CombatRankLocalised,
                                        CombatRankProgress = App.EliteApi.Commander.CombatRankProgress,

                                        TradeRank = App.EliteApi.Commander.TradeRankLocalised,
                                        TradeRankProgress = App.EliteApi.Commander.TradeRankProgress,

                                        ExplorationRank = App.EliteApi.Commander.ExplorationRankLocalised,
                                        ExplorationRankProgress = App.EliteApi.Commander.ExplorationRankProgress,

                                        CqcRank = App.EliteApi.Commander.CqcRank.ToString(),

                                        CqcRankProgress = App.EliteApi.Commander.CqcRankProgress,

                                        FederationReputation = _commanderData.FederationReputation,
                                        AllianceReputation = _commanderData.AllianceReputation,
                                        EmpireReputation = _commanderData.EmpireReputation,

                                        FederationReputationState = _commanderData.FederationReputationState,
                                        AllianceReputationState = _commanderData.AllianceReputationState,
                                        EmpireReputationState = _commanderData.EmpireReputationState

                                });

                                /*
                                App.EliteApi.Commander.Statistics.BankAccount
                                App.EliteApi.Commander.Statistics.Combat
                                App.EliteApi.Commander.Statistics.Crime
                                App.EliteApi.Commander.Statistics.Smuggling
                                App.EliteApi.Commander.Statistics.Trading
                                App.EliteApi.Commander.Statistics.Mining
                                App.EliteApi.Commander.Statistics.Exploration
                                App.EliteApi.Commander.Statistics.Passengers
                                App.EliteApi.Commander.Statistics.SearchAndRescue
                                App.EliteApi.Commander.Statistics.TgEncounters
                                App.EliteApi.Commander.Statistics.Crafting
                                App.EliteApi.Commander.Statistics.Crew
                                App.EliteApi.Commander.Statistics.Multicrew
                                App.EliteApi.Commander.Statistics.MaterialTraderStats
                                App.EliteApi.Commander.Statistics.Cqc

                                //reputationInfo.Alliance
                                //reputationInfo.Empire
                                //reputationInfo.Federation
                                //reputationInfo.Independent

                                */

                                break;

                            case LCDTab.Ship:

                                str =
                                    Engine.Razor.Run("2.cshtml", null, new
                                    {
                                        CurrentTab = (int) _currentTab,

                                        ShipName = _shipData.Name?.Trim(),

                                        ShipType = _shipData.Type,

                                        AutomaticDocking = _shipData.AutomaticDocking,

                                        Docked = App.EliteApi.Status.Docked,

                                        FuelMain = App.EliteApi.Status.Fuel.FuelMain,

                                        FuelReservoir = App.EliteApi.Status.Fuel.FuelReservoir,

                                        MaxFuel = App.EliteApi.Status.Fuel.MaxFuel,

                                        FuelPercent =
                                            Convert.ToInt32(100 / App.EliteApi.Status.Fuel.MaxFuel *
                                                            App.EliteApi.Status.Fuel.FuelMain),

                                        LastJump = App.EliteApi.Status.JumpRange,

                                        Cargo = App.EliteApi.Status.Cargo,

                                        CargoCapacity = _shipData.CargoCapacity,

                                        HullHealth = _shipData.HullHealth

                                    });

                                /*
                                App.EliteApi.Status.Shields
                                App.EliteApi.Status.LowFuel
                                App.EliteApi.Status.Overheating
                                App.EliteApi.Status.FsdCharging
                                App.EliteApi.Status.FsdCooldown
                                App.EliteApi.Status.MassLocked
                                App.EliteApi.Status.InDanger
                                App.EliteApi.Status.InInterdiction
                                App.EliteApi.Status.SrvHandbreak
                                App.EliteApi.Status.SrvNearShip
                                App.EliteApi.Status.SrvDriveAssist
                                App.EliteApi.Status.HasLatlong
                                App.EliteApi.Status.InMothership
                                App.EliteApi.Status.InFighter
                                App.EliteApi.Status.InSRV
                                App.EliteApi.Status.AnalysisMode
                                App.EliteApi.Status.NightVision
                                App.EliteApi.Status.InNoFireZone
                                App.EliteApi.Status.IsRunning
                                App.EliteApi.Status.SrvTurrent
                                App.EliteApi.Status.InMainMenu
                                App.EliteApi.Status.SilentRunning
                                App.EliteApi.Status.Flags
                                App.EliteApi.Status.Landed
                                App.EliteApi.Status.Gear
                                App.EliteApi.Status.Supercruise
                                App.EliteApi.Status.FlightAssist
                                App.EliteApi.Status.Hardpoints
                                App.EliteApi.Status.Winging
                                App.EliteApi.Status.Lights
                                App.EliteApi.Status.CargoScoop
                                App.EliteApi.Status.Scooping

                                App.EliteApi.Status.LegalState
                                App.EliteApi.Status.GameMode
                                App.EliteApi.Status.MusicTrack

                                App.EliteApi.Status.FireGroup
                                App.EliteApi.Status.GuiFocus

                                App.EliteApi.Status.Pips
                                */

                                //loadoutInfo.Hot
                                //loadoutInfo.HullValue
                                //loadoutInfo.Modules
                                //loadoutInfo.ModulesValue

                                //fuelScoopInfo.Scooped
                                //fuelScoopInfo.Total

                                //underAttackInfo.Target

                                //hullDamageInfo.Fighter
                                //hullDamageInfo.PlayerPilot

                                //interdictedInfo.Faction
                                //interdictedInfo.InterdictorLocalised
                                //interdictedInfo.IsPlayer
                                //interdictedInfo.Submitted
                                //interdictionInfo.Faction
                                //interdictionInfo.Interdicted
                                //interdictionInfo.IsPlayer
                                //interdictionInfo.Power
                                //interdictionInfo.Success

                                //scannedInfo.ScanType     

                                //loadGameInfo.ShipIdent
                                //loadGameInfo.Ship
                                //setUserShipNameInfo.UserShipId
                                //loadoutInfo.ShipIdent

                                break;

                            case LCDTab.Navigation:

                                str =
                                    Engine.Razor.Run("3.cshtml", null, new
                                    {
                                        CurrentTab = (int) _currentTab,

                                        StarSystem = App.EliteApi.Location.StarSystem,

                                        Body = !string.IsNullOrEmpty(_locationData.BodyType) && !string.IsNullOrEmpty(_locationData.Body)
                                            ? _locationData.Body
                                            : null, 

                                        /*
                                        "Station"
                                        "Star"
                                        "Planet"
                                        "PlanetaryRing"
                                        "StellarRing"                                        
                                        "AsteroidCluster"                                         
                                         */

                                        BodyType = _locationData.BodyType,

                                        Station = _locationData.BodyType == "Station" &&
                                                  !string.IsNullOrEmpty(_locationData.Body)
                                            ? _locationData.Body
                                            : App.EliteApi.Location.Station,

                                        Docked = App.EliteApi.Status.Docked,

                                        LandingPad = _dockData.LandingPad,

                                        StartJump = _locationData.StartJump,

                                        JumpType = _locationData.JumpType,

                                        JumpToSystem = _locationData.JumpToSystem,

                                        JumpToStarClass = _locationData.JumpToStarClass,

                                        RemainingJumpsInRoute = _locationData.RemainingJumpsInRoute,

                                        FsdTargetName = _locationData.FsdTargetName,

                                        Settlement = _locationData.Settlement,

                                        HideBody = _locationData.HideBody,

                                        StationType = _dockData.Type,

                                        Government = _dockData.Government,

                                        Allegiance = _dockData.Allegiance,

                                        Faction = _dockData.Faction,

                                        Economy = _dockData.Economy,

                                        DistFromStarLs = _dockData.DistFromStarLs,

                                        SystemAllegiance = _locationData.SystemAllegiance,

                                        SystemFaction = _locationData.SystemFaction,

                                        SystemSecurity = _locationData.SystemSecurity,

                                        SystemEconomy = _locationData.SystemEconomy,

                                        SystemGovernment = _locationData.SystemGovernment,

                                        Population = _locationData.Population

                                    });

                                //dockedInfo.StationEconomies

                                //locationInfo.SystemSecondEconomyLocalised
                                //fsdJumpInfo.SystemSecondEconomyLocalised

                                //fsdJumpInfo.PowerplayState
                                //fsdJumpInfo.Powers
                                //fsdJumpInfo.StarPos
                                //fsdJumpInfo.FactionState
                                //fsdJumpInfo.Factions

                                //approachSettlementInfo.Latitude
                                //approachSettlementInfo.Longitude
                                
                                break;

                            case LCDTab.Target:

                                str =
                                    Engine.Razor.Run("4.cshtml", null, new
                                    {
                                        CurrentTab = (int) _currentTab,

                                        TargetLocked = _targetData.TargetLocked,
                                        ScanStage = _targetData.ScanStage,

                                        Bounty = _targetData.Bounty,
                                        Faction = _targetData.Faction,
                                        LegalStatus = _targetData.LegalStatus,
                                        PilotNameLocalised = _targetData.PilotNameLocalised,
                                        PilotRank = _targetData.PilotRank,
                                        Ship = _targetData.Ship,
                                        SubsystemLocalised = _targetData.SubsystemLocalised,

                                        SubsystemHealth = _targetData.SubsystemHealth,
                                        HullHealth = _targetData.HullHealth,
                                        ShieldHealth = _targetData.ShieldHealth,

                                    });

                                break;

                            case LCDTab.Missions:

                                str =
                                    Engine.Razor.Run("5.cshtml", null, new
                                    {
                                        CurrentTab = (int)_currentTab,

                                        MissionData = _missionData
                                    });

                                break;

                            case LCDTab.POI:

                                if (_currentCard[(int) _currentTab] < 0)
                                {
                                    _currentCard[(int) _currentTab] = 6;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 6)
                                {
                                    _currentCard[(int)_currentTab] = 0;
                                }

                                str =
                                    Engine.Razor.Run("6.cshtml", null, new
                                    {
                                        CurrentTab = (int)_currentTab,

                                        CurrentCard = _currentCard[(int)_currentTab],

                                        CurrentPois = _currentPois, // 0

                                        CurrentInterStellarFactors= _currentInterStellarFactors, // 1
                                        CurrentRawMaterialTraders= _currentRawMaterialTraders, // 2
                                        CurrentManufacturedMaterialTraders= _currentManufacturedMaterialTraders, // 3
                                        CurrentEncodedDataTraders= _currentEncodedDataTraders, // 4
                                        CurrentHumanTechnologyBrokers= _currentHumanTechnologyBrokers, // 5
                                        CurrentGuardianTechnologyBrokers= _currentGuardianTechnologyBrokers // 6

                                });

                                break; 

                            /*
                            case LCDTab.Events:

                                var eventlist = "";
                                foreach (var b in EventHistory)
                                {
                                    eventlist += b + "<br/>";
                                }

                                str =
                                    Engine.Razor.Run("6.cshtml", null, new
                                    {
                                        CurrentTab = (int) _currenttab,

                                        EventList = eventlist
                                    });

                                break;*/

                        }


                        graphics.Clear(Color.Black);

                        if (_currentTab > 0)
                        {
                            var htmlSize = HtmlRender.Measure(graphics, str, HtmlWindowWidth, App.cssData);

                            CurrentLCDHeight = (int)htmlSize.Height; 

                            CheckLcdOffset();

                            if (CurrentLCDHeight > 0)
                            {
                                var image = HtmlRender.RenderToImage(str,
                                    new Size(HtmlWindowWidth, CurrentLCDHeight + 20), Color.Black, App.cssData);

                                graphics.DrawImage(image, new Rectangle(new Point(HtmlWindowXOffset,0),
                                                                                   new Size(HtmlWindowWidth, HtmlWindowHeight + 20) ),
                                                         new Rectangle(new Point(0, CurrentLCDYOffset),
                                                             new Size(HtmlWindowWidth, HtmlWindowHeight + 20)),
                                                         GraphicsUnit.Pixel);
                            }

                            if (CurrentLCDHeight > HtmlWindowHeight)
                            {
                                double scrollBarHeight = 233.0;
                                
                                double scrollThumbHeight = ((double)HtmlWindowHeight / (double)CurrentLCDHeight * (double)scrollBarHeight);
                                double scrollThumbYOffset = (double)CurrentLCDYOffset / (double)CurrentLCDHeight * scrollBarHeight;


                                graphics.DrawRectangle(_scrollPen, new Rectangle(new Point(320-9, 2),
                                                                   new Size(5, (int)scrollBarHeight)));

                                graphics.FillRectangle(_scrollBrush, new Rectangle(new Point(320 - 9, 2 + (int)scrollThumbYOffset),
                                    new Size(5, 1 + (int)scrollThumbHeight)));

                            }

                        }

                        var menuimage = HtmlRender.RenderToImage(menustr,
                            new Size(HtmlMenuWindowWidth, HtmlWindowHeight), Color.Black, App.cssData);

                        graphics.DrawImage(menuimage, 0,0);

#if DEBUG
                        fipImage.Save(@"screenshot"+(int)_currentTab+"_"+ _currentCard[(int)_currentTab] + ".png", ImageFormat.Png);
#endif

                        fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        SetImage(page, fipImage);

                        if (_lastTab > 0)
                        {
                            DirectOutputClass.SetLed(FipDevicePointer, _currentPage, (uint) _lastTab, false);
                        }

                        if (_currentTab > 0)
                        {
                            DirectOutputClass.SetLed(FipDevicePointer, _currentPage, (uint) _currentTab, true);
                        }

                    }
                }
            }
        }

        private int UpdateCargoCapacity(string item, int addremove)
        {
            if (item?.Contains("_cargorack_") == true)
            {

                var size = item.Substring(item.IndexOf("_size", StringComparison.OrdinalIgnoreCase) + 5);

                size = size.Substring(0, size.IndexOf("_", StringComparison.OrdinalIgnoreCase));

                return addremove * Convert.ToInt32(size);
            }

            return 0;
        }
        

        private string UpdateReputationState(double reputation)
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

        private void HandleEliteEvents(object sender, dynamic e)
        {

            if (!App.EliteApi.IsReady || DoShutdown)
            {
                return;
            }

            if (e != null && e.@event != null)
            {
                string evt = e.@event.ToString();

                if (evt != "FSSSignalDiscovered" && evt != "FSSDiscoveryScan")
                {
                    _eventHistory.Put(DateTime.Now.ToLongTimeString() + " : " + evt);
                }

                switch (evt)
                {
                    //----------- COMMANDER


                    case "LoadGame":

                        LoadGameInfo loadGameInfo = e.ToObject<LoadGameInfo>();

                        //App.EliteApi.Status.GameMode

                        //loadGameInfo.FuelLevel

                        //loadGameInfo.Group
                        //loadGameInfo.Horizons
                        //loadGameInfo.Loan
                        //loadGameInfo.ShipIdent
                        //loadGameInfo.Ship

                        _shipData.Name = loadGameInfo.ShipName;

                        ShipsByEliteID.TryGetValue(loadGameInfo.Ship?.ToLower() ?? "???", out var ship);
                        _shipData.Type = ship ?? loadGameInfo.Ship;

                        _commanderData.Name = loadGameInfo.Commander;
                        _commanderData.Credits = Convert.ToUInt32(loadGameInfo.Credits);

                        _locationData.Settlement = "";

                        break;

                    case "Commander":

                        CommanderInfo commanderInfo = e.ToObject<CommanderInfo>();

                        _commanderData.Name = commanderInfo.Name;

                        SetTab(LCDTab.Commander);
                        break;

                    case "SetUserShipName":

                        SetUserShipNameInfo setUserShipNameInfo = e.ToObject<SetUserShipNameInfo>();

                        //setUserShipNameInfo.UserShipId

                        _shipData.Name = setUserShipNameInfo.UserShipName;

                        ShipsByEliteID.TryGetValue(setUserShipNameInfo.Ship?.ToLower() ?? "???", out var tgtShip);
                        _shipData.Type = tgtShip ?? setUserShipNameInfo.Ship;

                        break;

                    case "Loadout":

                        LoadoutInfo loadoutInfo = e.ToObject<LoadoutInfo>();

                        _shipData.HullHealth = loadoutInfo.HullHealth * 100.0;

                        _commanderData.Rebuy = loadoutInfo.Rebuy;

                        //loadoutInfo.Hot
                        //loadoutInfo.HullValue
                        //loadoutInfo.Modules
                        //loadoutInfo.ModulesValue
                        //loadoutInfo.ShipIdent


                        //Ship:Python,
                        //Ident:MH-08P,
                        //Modules:31,
                        //Hull Health:100,0%,
                        //Hull:55.323.684 cr,
                        //Modules:32.315.494 cr,
                        //Rebuy:4.381.497 cr

                        _shipData.Name = loadoutInfo.ShipName;

                        ShipsByEliteID.TryGetValue(loadoutInfo.Ship?.ToLower() ?? "???", out var loadShip);
                        _shipData.Type = loadShip ?? loadoutInfo.Ship;

                        _shipData.CargoCapacity = 0;

                        foreach (var m in loadoutInfo.Modules.Where(x => x.Item.Contains("_cargorack_")))
                        {
                            _shipData.CargoCapacity += UpdateCargoCapacity(m.Item, 1);
                        }

                        break;

                    case "ModuleBuy":
                        ModuleBuyInfo moduleBuyInfo = e.ToObject<ModuleBuyInfo>();

                        _shipData.CargoCapacity += UpdateCargoCapacity(moduleBuyInfo.BuyItem, 1);
                        _shipData.CargoCapacity += UpdateCargoCapacity(moduleBuyInfo.SellItem, -1);

                        break;

                    case "ModuleSell":
                        ModuleSellInfo moduleSellInfo = e.ToObject<ModuleSellInfo>();

                        _shipData.CargoCapacity += UpdateCargoCapacity(moduleSellInfo.SellItem, -1);

                        break;

                    case "ModuleStore":
                        ModuleStoreInfo moduleStoreInfo = e.ToObject<ModuleStoreInfo>();

                        _shipData.CargoCapacity += UpdateCargoCapacity(moduleStoreInfo.StoredItem, -1);

                        break;

                    case "ModuleRetrieve":
                        ModuleRetrieveInfo moduleRetrieveInfo = e.ToObject<ModuleRetrieveInfo>();

                        _shipData.CargoCapacity += UpdateCargoCapacity(moduleRetrieveInfo.RetrievedItem, 1);

                        break;

                    case "MassModuleStore":
                        MassModuleStoreInfo massModuleStoreInfo = e.ToObject<MassModuleStoreInfo>();

                        foreach (var i in massModuleStoreInfo.Items)
                        {
                            _shipData.CargoCapacity += UpdateCargoCapacity(i.Name, -1);
                        }
                        break;

                    case "Rank":

                        RankInfo rankInfo = e.ToObject<RankInfo>();

                        break;

                    case "Reputation":

                        ReputationInfo reputationInfo = e.ToObject<ReputationInfo>();

                        //reputationInfo.Independent

                        _commanderData.FederationReputation = reputationInfo.Federation;
                        _commanderData.AllianceReputation = reputationInfo.Alliance;
                        _commanderData.EmpireReputation = reputationInfo.Empire;

                        _commanderData.FederationReputationState = UpdateReputationState(_commanderData.FederationReputation);
                        _commanderData.AllianceReputationState = UpdateReputationState(_commanderData.AllianceReputation);
                        _commanderData.EmpireReputationState = UpdateReputationState(_commanderData.EmpireReputation);

                        break;


                    case "Progress":

                        ProgressInfo progressInfo = e.ToObject<ProgressInfo>();

                        break;

                    case "Statistics":

                        StatisticsInfo statisticsInfo = e.ToObject<StatisticsInfo>();

                        //App.EliteApi.Commander.Statistics.Multicrew
                        //App.EliteApi.Commander.Statistics.Crew
                        //App.EliteApi.Commander.Statistics.Crafting
                        //App.EliteApi.Commander.Statistics.TgEncounters
                        //App.EliteApi.Commander.Statistics.SearchAndRescue
                        //App.EliteApi.Commander.Statistics.Passengers
                        //App.EliteApi.Commander.Statistics.Exploration
                        //App.EliteApi.Commander.Statistics.Mining
                        //App.EliteApi.Commander.Statistics.Trading
                        //App.EliteApi.Commander.Statistics.Smuggling
                        //App.EliteApi.Commander.Statistics.Crime
                        //App.EliteApi.Commander.Statistics.Combat
                        //App.EliteApi.Commander.Statistics.BankAccount
                        //App.EliteApi.Commander.Statistics.MaterialTraderStats
                        //App.EliteApi.Commander.Statistics.Cqc

                        break;

                    case "RefuelAll":

                        RefuelAllInfo refuelAllInfo = e.ToObject<RefuelAllInfo>();
                        //refuelAllInfo.Amount

                        _commanderData.Credits -= Convert.ToUInt32(refuelAllInfo.Cost);
                        break;

                    case "RepairAll":

                        RepairAllInfo repairAllInfo = e.ToObject<RepairAllInfo>();

                        _commanderData.Credits -= Convert.ToUInt32(repairAllInfo.Cost);
                        break;

                    case "Repair":

                        RepairInfo repairInfo = e.ToObject<RepairInfo>();
                        //repairInfo.Item

                        _commanderData.Credits -= Convert.ToUInt32(repairInfo.Cost);
                        break;

                    case "BuyTradeData":

                        BuyTradeDataInfo buyTradeDataInfo = e.ToObject<BuyTradeDataInfo>();
                        //buyTradeDataInfo.System

                        _commanderData.Credits -= Convert.ToUInt32(buyTradeDataInfo.Cost);
                        break;

                    case "BuyExplorationData":

                        BuyExplorationDataInfo buyExplorationDataInfo = e.ToObject<BuyExplorationDataInfo>();
                        //buyExplorationDataInfo.System

                        _commanderData.Credits -= Convert.ToUInt32(buyExplorationDataInfo.Cost);
                        break;

                    case "BuyDrones":

                        BuyDronesInfo buyDronesInfo = e.ToObject<BuyDronesInfo>();
                        //buyDronesInfo.BuyPrice
                        //buyDronesInfo.Count
                        //buyDronesInfo.Type

                        _commanderData.Credits -= Convert.ToUInt32(buyDronesInfo.TotalCost);
                        break;

                    case "BuyAmmo":

                        BuyAmmoInfo buyAmmoInfo = e.ToObject<BuyAmmoInfo>();

                        _commanderData.Credits -= Convert.ToUInt32(buyAmmoInfo.Cost);
                        break;

                    //------------- LOCATION

                    case "Location":

                        LocationInfo locationInfo = e.ToObject<LocationInfo>();

                        _locationData.StarPos = locationInfo.StarPos.ToList();

                        _currentPois = Poi.GetNearestPoiItems(_locationData.StarPos);

                        _currentInterStellarFactors = Station.GetNearestStationItems(_locationData.StarPos,App.InterStellarFactors);
                        _currentRawMaterialTraders = Station.GetNearestStationItems(_locationData.StarPos, App.RawMaterialTraders);
                        _currentManufacturedMaterialTraders = Station.GetNearestStationItems(_locationData.StarPos, App.ManufacturedMaterialTraders);
                        _currentEncodedDataTraders = Station.GetNearestStationItems(_locationData.StarPos, App.EncodedDataTraders);
                        _currentHumanTechnologyBrokers = Station.GetNearestStationItems(_locationData.StarPos, App.HumanTechnologyBrokers);
                        _currentGuardianTechnologyBrokers = Station.GetNearestStationItems(_locationData.StarPos, App.GuardianTechnologyBrokers);

                        _locationData.SystemAllegiance = locationInfo.SystemAllegiance;
                        _locationData.SystemFaction = locationInfo.SystemFaction?.Name;
                        _locationData.SystemSecurity = locationInfo.SystemSecurityLocalised;
                        _locationData.SystemEconomy = locationInfo.SystemEconomyLocalised;
                        _locationData.SystemGovernment = locationInfo.SystemGovernmentLocalised;
                        _locationData.Population = locationInfo.Population;
                        _locationData.Body = locationInfo.Body;
                        _locationData.BodyType = locationInfo.BodyType;

                        _locationData.HideBody = false;

                        //locationInfo.Docked
                        //locationInfo.Factions
                        //locationInfo.StarPos
                        //locationInfo.SystemSecondEconomyLocalised

                        break;

                    case "ApproachBody":

                        ApproachBodyInfo approachBodyInfo = e.ToObject<ApproachBodyInfo>();

                        _locationData.Body = approachBodyInfo.Body;
                        _locationData.BodyType = "Planet"; 

                        _locationData.HideBody = false;

                        break;

                    case "ApproachSettlement":

                        ApproachSettlementInfo approachSettlementInfo = e.ToObject<ApproachSettlementInfo>();

                        _locationData.Settlement = approachSettlementInfo.Name;

                        _locationData.BodyType = "Planet";

                        _locationData.HideBody = false;

                        //approachSettlementInfo.Latitude
                        //approachSettlementInfo.Longitude

                        break;

                    case "LeaveBody":

                        LeaveBodyInfo leaveBodyInfo = e.ToObject<LeaveBodyInfo>();

                        _locationData.Body = "";
                        _locationData.BodyType = "";

                        _locationData.HideBody = true;

                        break;

                    case "Undocked":

                        UndockedInfo undockedInfo = e.ToObject<UndockedInfo>();

                        //undockedInfo.StationName
                        //undockedInfo.StationType

                        _dockData = new Dock();

                        _locationData.Body = "";
                        _locationData.BodyType = "";

                        break;

                    case "Docked":

                        DockedInfo dockedInfo = e.ToObject<DockedInfo>();

                        //dockedInfo.StarSystem
                        //dockedInfo.StationEconomies

                        _shipData.AutomaticDocking = false;

                        _dockData.Type = dockedInfo.StationType;

                        _dockData.Government = dockedInfo.StationGovernmentLocalised;
                        _dockData.Allegiance = dockedInfo.StationAllegiance;
                        _dockData.Faction = dockedInfo.StationFaction?.Name;
                        _dockData.Economy = dockedInfo.StationEconomyLocalised;
                        _dockData.DistFromStarLs = dockedInfo.DistFromStarLs;

                        _dockData.Services = string.Join(", ", dockedInfo.StationServices);

                        _dockData.LandingPad = -1;

                        break;

                    case "DockingGranted":

                        DockingGrantedInfo dockingGrantedInfo = e.ToObject<DockingGrantedInfo>();

                        _locationData.Body = dockingGrantedInfo.StationName;
                        _locationData.BodyType = "Station";

                        _dockData.LandingPad = Convert.ToInt32(dockingGrantedInfo.LandingPad);
                        break;

                    case "DockingRequested":

                        DockingRequestedInfo dockingRequestedInfo = e.ToObject<DockingRequestedInfo>();

                        _locationData.Body = dockingRequestedInfo.StationName;
                        _locationData.BodyType = "Station";
                        break;

                    case "FSDJump":

                        FSDJumpInfo fsdJumpInfo = e.ToObject<FSDJumpInfo>();

                        _locationData.StarPos = fsdJumpInfo.StarPos.ToList();

                        _currentPois = Poi.GetNearestPoiItems(_locationData.StarPos);

                        _currentInterStellarFactors = Station.GetNearestStationItems(_locationData.StarPos, App.InterStellarFactors);
                        _currentRawMaterialTraders = Station.GetNearestStationItems(_locationData.StarPos, App.RawMaterialTraders);
                        _currentManufacturedMaterialTraders = Station.GetNearestStationItems(_locationData.StarPos, App.ManufacturedMaterialTraders);
                        _currentEncodedDataTraders = Station.GetNearestStationItems(_locationData.StarPos, App.EncodedDataTraders);
                        _currentHumanTechnologyBrokers = Station.GetNearestStationItems(_locationData.StarPos, App.HumanTechnologyBrokers);
                        _currentGuardianTechnologyBrokers = Station.GetNearestStationItems(_locationData.StarPos, App.GuardianTechnologyBrokers);

                        _locationData.StartJump = false;
                        _locationData.JumpToSystem = "";
                        _locationData.JumpToStarClass = "";
                        _locationData.JumpType = "";

                        _locationData.SystemAllegiance = fsdJumpInfo.SystemAllegiance;
                        _locationData.SystemFaction = fsdJumpInfo.SystemFaction?.Name;
                        _locationData.SystemSecurity = fsdJumpInfo.SystemSecurityLocalised;
                        _locationData.SystemEconomy = fsdJumpInfo.SystemEconomyLocalised;
                        _locationData.SystemGovernment = fsdJumpInfo.SystemGovernmentLocalised;
                        _locationData.Population = fsdJumpInfo.Population;

                        //fsdJumpInfo.JumpDist
                        //fsdJumpInfo.FactionState
                        //fsdJumpInfo.Factions
                        //fsdJumpInfo.FuelLevel
                        //fsdJumpInfo.FuelUsed
                        //fsdJumpInfo.Population
                        //fsdJumpInfo.PowerplayState
                        //fsdJumpInfo.Powers
                        //fsdJumpInfo.StarPos
                        //fsdJumpInfo.SystemSecondEconomyLocalised

                        break;

                    case "StartJump":

                        StartJumpInfo startJumpInfo = e.ToObject<StartJumpInfo>();

                        _locationData.StartJump = true;
                        _locationData.JumpType = startJumpInfo.JumpType;
                        _locationData.JumpToSystem = startJumpInfo.StarSystem;
                        _locationData.JumpToStarClass = startJumpInfo.StarClass;

                        _locationData.Body = "";
                        _locationData.BodyType = "";

                        break;

                    case "Status.FsdCooldown":

                        break;

                    case "Status.FsdCharging":

                        break;

                    case "FSDTarget":
                        FSDTargetInfo fSdTargetInfo = e.ToObject<FSDTargetInfo>();

                        _locationData.FsdTargetName = fSdTargetInfo.Name;

                        _locationData.RemainingJumpsInRoute = fSdTargetInfo.RemainingJumpsInRoute;

                        SetTab(LCDTab.Navigation);

                        break;

                    case "SupercruiseEntry":

                        SupercruiseEntryInfo supercruiseEntryInfo = e.ToObject<SupercruiseEntryInfo>();

                        _locationData.JumpToSystem = supercruiseEntryInfo.StarSystem;

                        _locationData.Body = "";
                        _locationData.BodyType = "";

                        _locationData.HideBody = true;

                        break;

                    case "SupercruiseExit":

                        SupercruiseExitInfo supercruiseExitInfo = e.ToObject<SupercruiseExitInfo>();

                        _locationData.StartJump = false;

                        _locationData.JumpToSystem = "";
                        _locationData.JumpToStarClass = "";
                        _locationData.JumpType = "";

                        _locationData.Body = supercruiseExitInfo.Body;
                        _locationData.BodyType = supercruiseExitInfo.BodyType;
                        _locationData.HideBody = false;


                        break;

                    //---------------- STATUS 

                    case "ReceiveText":


                        ReceiveTextInfo receiveTextInfo = e.ToObject<ReceiveTextInfo>();

                        //receiveTextInfo.Channel
                        //receiveTextInfo.From
                        //receiveTextInfo.FromLocalized
                        //receiveTextInfo.MessageLocalized

                        //Console.WriteLine(receiveTextInfo.Channel + " " + receiveTextInfo.From + " " + receiveTextInfo.Message);

                        //example : npc  $COMMS_entered:#name=Shinrarta Dezhra;

                        // copied from EddiJournalMonitor https://github.com/EDCD/EDDI

                        if (receiveTextInfo.From == string.Empty && receiveTextInfo.Channel == "npc" &&
                            (receiveTextInfo.Message.StartsWith("$COMMS_entered") ||
                             receiveTextInfo.Message.StartsWith("$CHAT_Intro")))
                        {
                            // We can safely ignore system messages that initialize the chat system or announce that we entered a receiveTextInfo.Channel - no event is needed. 
                            break;
                        }

                        if (
                            receiveTextInfo.Channel == "player" ||
                            receiveTextInfo.Channel == "wing" ||
                            receiveTextInfo.Channel == "friend" ||
                            receiveTextInfo.Channel == "voicechat" ||
                            receiveTextInfo.Channel == "local" ||
                            receiveTextInfo.Channel == "squadron" ||
                            receiveTextInfo.Channel == "starsystem" ||
                            receiveTextInfo.Channel == null
                        )
                        {
                            // Give priority to player messages
                            var source = receiveTextInfo.Channel == "squadron" ? "Squadron mate" :
                                receiveTextInfo.Channel == "wing" ? "Wing mate" :
                                receiveTextInfo.Channel == null ? "Crew mate" : "Commander";

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

                    case "Music":

                        MusicInfo musicInfo = e.ToObject<MusicInfo>();

                        //App.EliteApi.Status.MusicTrack

                        switch (musicInfo.MusicTrack)
                        {
                            case "MainMenu":
                                //TabLCDStartElite.Create();
                                break;

                            case "DockingComputer":
                                _shipData.AutomaticDocking = true;

                                //Tab.Refresh(LCDTab.Ship);
                                break;

                            case "NoTrack":
                                if (_shipData.AutomaticDocking)
                                {
                                    _shipData.AutomaticDocking = false;
                                    //Tab.Refresh(LCDTab.Ship);
                                }
                                break;

                            default:
                                return;
                        }

                        break;

                    case "FuelScoop":
                        FuelScoopInfo fuelScoopInfo = e.ToObject<FuelScoopInfo>();
                        //fuelScoopInfo.Scooped
                        //fuelScoopInfo.Total

                        break;

                    case "UnderAttack":
                        UnderAttackInfo underAttackInfo = e.ToObject<UnderAttackInfo>();

                        //underAttackInfo.Target
                        break;

                    case "HeatDamage":
                        HeatDamageInfo heatDamageInfo = e.ToObject<HeatDamageInfo>();

                        break;

                    case "HeatWarning":
                        HeatWarningInfo heatWarningInfo = e.ToObject<HeatWarningInfo>();

                        break;

                    case "HullDamage":
                        HullDamageInfo hullDamageInfo = e.ToObject<HullDamageInfo>();

                        _shipData.HullHealth = hullDamageInfo.Health * 100.0;

                        //hullDamageInfo.Fighter
                        //hullDamageInfo.PlayerPilot
                        break;

                    case "ShipTargeted":
                        ShipTargetedInfo shipTargetedInfo = e.ToObject<ShipTargetedInfo>();

                        _targetData.Bounty = shipTargetedInfo.Bounty;
                        _targetData.Faction = shipTargetedInfo.Faction;
                        _targetData.HullHealth = shipTargetedInfo.HullHealth;
                        _targetData.LegalStatus = shipTargetedInfo.LegalStatus;
                        _targetData.PilotNameLocalised = shipTargetedInfo.PilotNameLocalised;
                        _targetData.PilotRank = shipTargetedInfo.PilotRank;
                        _targetData.ScanStage = shipTargetedInfo.ScanStage;
                        _targetData.ShieldHealth = shipTargetedInfo.ShieldHealth;

                        ShipsByEliteID.TryGetValue(shipTargetedInfo.Ship?.ToLower() ?? "???", out var targetShip);

                        _targetData.Ship = shipTargetedInfo.ShipLocalised ?? targetShip ?? shipTargetedInfo.Ship;

                        _targetData.SubsystemLocalised = shipTargetedInfo.SubsystemLocalised;
                        _targetData.TargetLocked = shipTargetedInfo.TargetLocked;
                        _targetData.SubsystemHealth = shipTargetedInfo.SubsystemHealth;

                        if (_targetData.TargetLocked)
                        {
                            SetTab(LCDTab.Target);
                        }

                        break;

                    case "Interdicted":
                        InterdictedInfo interdictedInfo = e.ToObject<InterdictedInfo>();
                        //interdictedInfo.Faction
                        //interdictedInfo.InterdictorLocalised
                        //interdictedInfo.IsPlayer
                        //interdictedInfo.Submitted

                        break;

                    case "Interdiction":
                        InterdictionInfo interdictionInfo = e.ToObject<InterdictionInfo>();
                        //interdictionInfo.Faction
                        //interdictionInfo.Interdicted
                        //interdictionInfo.IsPlayer
                        //interdictionInfo.Power
                        //interdictionInfo.Success
                        break;

                    case "Scanned":
                        ScannedInfo scannedInfo = e.ToObject<ScannedInfo>();
                        //scannedInfo.ScanType
                        break;

                    case "Cargo":
                        //When written: at startup
                        CargoInfo cargoInfo = e.ToObject<CargoInfo>();
                        //cargoInfo.Vessel // Ship or SRV
                        //cargoInfo.Count
                        //cargoInfo.Inventory[0].Count
                        //cargoInfo.Inventory[0].NameLocalised
                        //cargoInfo.Inventory[0].Stolen
                        //cargoInfo.Inventory[0].MissionId  // missing in API !!!
                        break;

                    case "CollectCargo":
                        //When Written: when scooping cargo from space or planet surface 
                        CollectCargoInfo collectCargoInfo = e.ToObject<CollectCargoInfo>();
                        //collectCargoInfo.Stolen
                        //collectCargoInfo.TypeLocalised
                        //collectCargoInfo.MissionId // missing in API !!!
                        break;

                    case "EjectCargo":
                        EjectCargoInfo ejectCargoInfo = e.ToObject<EjectCargoInfo>();
                        //ejectCargoInfo.Abandoned
                        //ejectCargoInfo.Count
                        //ejectCargoInfo.TypeLocalised
                        //ejectCargoInfo.MissionId // missing in API !!!

                        break;

                    case "CargoDepot":
                        //When written: when collecting or delivering cargo for a wing mission
                        CargoDepotInfo cargoDepotInfo = e.ToObject<CargoDepotInfo>();
                        //cargoDepotInfo.CargoTypeLocalised
                        //cargoDepotInfo.Count
                        //cargoDepotInfo.EndMarketId
                        //cargoDepotInfo.ItemsCollected
                        //cargoDepotInfo.ItemsDelivered
                        //cargoDepotInfo.MissionId
                        //cargoDepotInfo.Progress
                        //cargoDepotInfo.StartMarketId
                        //cargoDepotInfo.TotalItemsToDeliver
                        //cargoDepotInfo.UpdateType "Collect", "Deliver", "WingUpdate"
                        break;

                    case "Passengers":
                        //When written: at startup
                        PassengersInfo passengersInfo = e.ToObject<PassengersInfo>();
                        //passengersInfo.Manifest[0].MissionId
                        //passengersInfo.Manifest[0].Count
                        //passengersInfo.Manifest[0].Type
                        //passengersInfo.Manifest[0].Vip
                        //passengersInfo.Manifest[0].Wanted
                        break;

                    case "Missions":
                        //When written: at startup
                        MissionsInfo missionsInfo = e.ToObject<MissionsInfo>();

                        if (missionsInfo.Active?.Count > 0)
                        {
                            _missionData = missionsInfo.Active.Select(x => new Mission
                            {
                                MissionId = x.MissionId,
                                PassengerMission = x.PassengerMission,
                                Expires = (DateTime?)null,
                                Name = x.Name
                            }).ToList();

                        }

                        //missionsInfo.Failed[0].MissionId
                        //missionsInfo.Failed[0].Name
                        //missionsInfo.Failed[0].PassengerMission
                        //missionsInfo.Failed[0].Expires time left in seconds 

                        //missionsInfo.Complete[0].MissionId
                        //missionsInfo.Complete[0].Name
                        //missionsInfo.Complete[0].PassengerMission
                        //missionsInfo.Complete[0].Expires

                        break;

                    case "MissionAccepted":
                        //When Written: when starting a mission 
                        MissionAcceptedInfo missionAcceptedInfo = e.ToObject<MissionAcceptedInfo>();

                        _missionData.RemoveAll(x => x.MissionId == missionAcceptedInfo.MissionId);

                        _missionData.Add(new Mission
                        {
                            MissionId = missionAcceptedInfo.MissionId,
                            Name = missionAcceptedInfo.LocalisedName,
                            Expires = missionAcceptedInfo.Expiry,
                            PassengerMission = missionAcceptedInfo.PassengerCount > 0,
                            System = missionAcceptedInfo.DestinationSystem,
                            Reward = missionAcceptedInfo.Reward,
                            Passengers  = missionAcceptedInfo.PassengerCount,

                            Faction =missionAcceptedInfo.Faction,
                            Influence = missionAcceptedInfo.Influence, //    None/Low/Med/High
                            Reputation = missionAcceptedInfo.Reputation, //  None/Low/Med/High

                            CommodityLocalised = missionAcceptedInfo.CommodityLocalised,
                            Count = missionAcceptedInfo.Count,

                            PassengerViPs = missionAcceptedInfo.PassengerViPs,
                            PassengerWanted = missionAcceptedInfo.PassengerWanted,
                            PassengerType = missionAcceptedInfo.PassengerType,

                            //Station = missionAcceptedInfo.DestinationStation,// does not exist in API ???
                            //NewDestinationSystem = missionAcceptedInfo.NewDestinationSystem,
                            //NewDestinationStation = missionAcceptedInfo.NewDestinationStation,
                            //Wing = missionAcceptedInfo.Wing, //  not in API ???
                            //Donation = missionAcceptedInfo.Donation, // not in API???
                            //Donated = missionAcceptedInfo.Donated, // not in API???
                            //Target = missionAcceptedInfo.Target,
                            //TargetType = missionAcceptedInfo.TargetType,
                            //TargetFaction = missionAcceptedInfo.TargetFaction,
                            //KillCount = missionAcceptedInfo.KillCount,


                        });

                        break;

                    case "MissionRedirected":
                        //When written: when a mission is updated with a new destination 

                        MissionRedirectedInfo missionRedirectedInfo = e.ToObject<MissionRedirectedInfo>();
                        //missionRedirectedInfo.Name
                        //missionRedirectedInfo.MissionId
                        //missionRedirectedInfo.NewDestinationStation
                        //missionRedirectedInfo.NewDestinationSystem
                        //missionRedirectedInfo.OldDestinationStation
                        //missionRedirectedInfo.OldDestinationSystem

                        break;

                    case "MissionCompleted":
                        //When Written: when a mission is completed

                        MissionCompletedInfo missionCompletedInfo = e.ToObject<MissionCompletedInfo>();

                        _commanderData.Credits += Convert.ToUInt32(missionCompletedInfo.Reward);

                        //missionCompletedInfo.Name
                        //missionCompletedInfo.Faction
                        //missionCompletedInfo.MissionId

                        //missionCompletedInfo.Target
                        //missionCompletedInfo.TargetTypeLocalised
                        //missionCompletedInfo.TargetFaction

                        //missionCompletedInfo.Donation
                        //missionCompletedInfo.Donated
                        //missionCompletedInfo.PermitsAwarded
                        //missionCompletedInfo.CommodityReward
                        //missionCompletedInfo.MaterialsReward

                        //missionCompletedInfo.FactionEffects

                        //missionCompletedInfo.DestinationStation
                        //missionCompletedInfo.NewDestinationStation
                        //missionCompletedInfo.DestinationSystem
                        //missionCompletedInfo.NewDestinationSystem

                        _missionData.RemoveAll(x => x.MissionId == missionCompletedInfo.MissionId);

                        break;
                    case "MissionAbandoned":
                        //When Written: when a mission has been abandoned 

                        MissionAbandonedInfo missionAbandonedInfo = e.ToObject<MissionAbandonedInfo>();
                        //missionAbandonedInfo.Name
                        //missionAbandonedInfo.MissionId
                        //missionAbandonedInfo.Fine // not in API

                        _missionData.RemoveAll(x => x.MissionId == missionAbandonedInfo.MissionId);

                        break;
                    case "MissionFailed":
                        //When Written: when a mission has failed 

                        MissionFailedInfo missionFailedInfo = e.ToObject<MissionFailedInfo>();
                        //missionFailedInfo.Name
                        //missionFailedInfo.MissionId

                        _missionData.RemoveAll(x => x.MissionId == missionFailedInfo.MissionId);

                        break;
                        /*

                            //---------------- MATERIAL 

                            case "Materials":
                                MaterialsInfo materialsInfo = e.ToObject<MaterialsInfo>();
                                break;

                            case "MaterialCollected":
                                MaterialCollectedInfo materialCollectedInfo = e.ToObject<MaterialCollectedInfo>();
                                break;

                            case "MaterialDiscarded":
                                MaterialDiscardedInfo materialDiscardedInfo = e.ToObject<MaterialDiscardedInfo>();
                                break;

                            case "MaterialTrade":
                                MaterialTradeInfo materialTradeInfo = e.ToObject<MaterialTradeInfo>();
                                break;

                            case "Synthesis":
                                SynthesisInfo synthesisInfo = e.ToObject<SynthesisInfo>();
                                break;

                            //------------------

                            case "ShieldState": // no longer works ?

                                ShieldStateInfo shieldStateInfo = e.ToObject<ShieldStateInfo>();
                                //shieldStateInfo.ShieldsUp

                                break;


                            case "Fileheader":
                                FileheaderInfo fileheaderInfo = e.ToObject<FileheaderInfo>();
                                //TabLCDStartElite.Create();
                                break;

                            case "Shutdown":
                                ShutdownInfo shutdownInfo = e.ToObject<ShutdownInfo>();
                                //EDLCD.Instance = new Instance();
                                //TabLCDStartup.Create();
                                break;

                            case "Scan":
                                ScanInfo scanInfo = e.ToObject<ScanInfo>();
                                //scanInfo.AxialTilt
                                //scanInfo.RotationPeriod
                                //scanInfo.OrbitalPeriod
                                //scanInfo.Periapsis
                                //scanInfo.OrbitalInclination
                                //scanInfo.Eccentricity
                                //scanInfo.SemiMajorAxis
                                //scanInfo.Composition
                                //scanInfo.Materials
                                //scanInfo.Landable
                                //scanInfo.SurfacePressure
                                //scanInfo.SurfaceTemperature
                                //scanInfo.SurfaceGravity
                                //scanInfo.Rings
                                //scanInfo.Radius
                                //scanInfo.Volcanism
                                //scanInfo.AtmosphereType
                                //scanInfo.Atmosphere
                                //scanInfo.PlanetClass
                                //scanInfo.TerraformState
                                //scanInfo.TidalLock
                                //scanInfo.DistanceFromArrivalLs
                                //scanInfo.Parents
                                //scanInfo.BodyId
                                //scanInfo.BodyName
                                //scanInfo.ScanType
                                //scanInfo.MassEm
                                //scanInfo.ReserveLevel

                                break;

                            case "AfmuRepairs":
                                AfmuRepairsInfo afmuRepairsInfo = e.ToObject<AfmuRepairsInfo>();
                                break;

                            case "AppliedToSquadron":
                                AppliedToSquadronInfo appliedToSquadronInfo = e.ToObject<AppliedToSquadronInfo>();
                                break;

                            case "AsteroidCracked":
                                AsteroidCrackedInfo asteroidCrackedInfo = e.ToObject<AsteroidCrackedInfo>();
                                break;

                            case "Bounty":
                                BountyInfo bountyInfo = e.ToObject<BountyInfo>();
                                break;

                            case "ChangeCrewRole":
                                ChangeCrewRoleInfo changeCrewRoleInfo = e.ToObject<ChangeCrewRoleInfo>();
                                break;

                            case "CockpitBreached":
                                CockpitBreachedInfo cockpitBreachedInfo = e.ToObject<CockpitBreachedInfo>();
                                break;

                            case "CodexEntry":
                                CodexEntryInfo codexEntryInfo = e.ToObject<CodexEntryInfo>();
                                break;


                            case "CommitCrime":
                                CommitCrimeInfo commitCrimeInfo = e.ToObject<CommitCrimeInfo>();
                                break;

                            case "CommunityGoalDiscard":
                                CommunityGoalDiscardInfo communityGoalDiscardInfo = e.ToObject<CommunityGoalDiscardInfo>();
                                break;

                            case "CommunityGoal":
                                CommunityGoalInfo communityGoalInfo = e.ToObject<CommunityGoalInfo>();
                                break;

                            case "CommunityGoalJoin":
                                CommunityGoalJoinInfo communityGoalJoinInfo = e.ToObject<CommunityGoalJoinInfo>();
                                break;

                            case "CommunityGoalReward":
                                CommunityGoalRewardInfo communityGoalRewardInfo = e.ToObject<CommunityGoalRewardInfo>();
                                break;

                            case "CrewAssign":
                                CrewAssignInfo crewAssignInfo = e.ToObject<CrewAssignInfo>();
                                break;

                            case "CrewFire":
                                CrewFireInfo crewFireInfo = e.ToObject<CrewFireInfo>();
                                break;

                            case "CrewHire":
                                CrewHireInfo crewHireInfo = e.ToObject<CrewHireInfo>();
                                break;

                            case "CrewLaunchFighter":
                                CrewLaunchFighterInfo crewLaunchFighterInfo = e.ToObject<CrewLaunchFighterInfo>();
                                break;

                            case "CrewMemberJoins":
                                CrewMemberJoinsInfo crewMemberJoinsInfo = e.ToObject<CrewMemberJoinsInfo>();
                                break;

                            case "CrewMemberQuits":
                                CrewMemberQuitsInfo crewMemberQuitsInfo = e.ToObject<CrewMemberQuitsInfo>();
                                break;

                            case "CrewMemberRoleChange":
                                CrewMemberRoleChangeInfo crewMemberRoleChangeInfo = e.ToObject<CrewMemberRoleChangeInfo>();
                                break;

                            case "CrimeVictim":
                                CrimeVictimInfo crimeVictimInfo = e.ToObject<CrimeVictimInfo>();
                                break;

                            case "DatalinkScan":
                                DatalinkScanInfo datalinkScanInfo = e.ToObject<DatalinkScanInfo>();
                                break;

                            case "DatalinkVoucher":
                                DatalinkVoucherInfo datalinkVoucherInfo = e.ToObject<DatalinkVoucherInfo>();
                                break;

                            case "DataScanned":
                                DataScannedInfo dataScannedInfo = e.ToObject<DataScannedInfo>();
                                break;

                            case "Died":
                                DiedInfo diedInfo = e.ToObject<DiedInfo>();
                                break;

                            case "DisbandedSquadron":
                                DisbandedSquadronInfo disbandedSquadronInfo = e.ToObject<DisbandedSquadronInfo>();
                                break;

                            case "DiscoveryScan":
                                DiscoveryScanInfo discoveryScanInfo = e.ToObject<DiscoveryScanInfo>();
                                break;

                            case "DockFighter":
                                DockFighterInfo dockFighterInfo = e.ToObject<DockFighterInfo>();
                                break;

                            case "DockingCancelled":
                                DockingCancelledInfo dockingCancelledInfo = e.ToObject<DockingCancelledInfo>();
                                break;

                            case "DockingDenied":
                                DockingDeniedInfo dockingDeniedInfo = e.ToObject<DockingDeniedInfo>();
                                break;

                            case "DockingTimeout":
                                DockingTimeoutInfo dockingTimeoutInfo = e.ToObject<DockingTimeoutInfo>();
                                break;

                            case "DockSRV":
                                DockSRVInfo dockSRVInfo = e.ToObject<DockSRVInfo>();
                                break;


                            case "EndCrewSession":
                                EndCrewSessionInfo endCrewSessionInfo = e.ToObject<EndCrewSessionInfo>();
                                break;

                            case "EngineerApply":
                                EngineerApplyInfo engineerApplyInfo = e.ToObject<EngineerApplyInfo>();
                                break;

                            case "EngineerContribution":
                                EngineerContributionInfo engineerContributionInfo = e.ToObject<EngineerContributionInfo>();
                                break;

                            case "EngineerCraft":
                                EngineerCraftInfo engineerCraftInfo = e.ToObject<EngineerCraftInfo>();
                                break;

                            case "EngineerProgress":
                                EngineerProgressInfo engineerProgressInfo = e.ToObject<EngineerProgressInfo>();
                                break;

                            case "EscapeInterdiction":
                                EscapeInterdictionInfo escapeInterdictionInfo = e.ToObject<EscapeInterdictionInfo>();
                                break;

                            case "FactionKillBond":
                                FactionKillBondInfo factionKillBondInfo = e.ToObject<FactionKillBondInfo>();
                                break;

                            case "MultiSellExplorationData":
                                MultiSellExplorationDataInfo multiSellExplorationDataInfo =
                                    e.ToObject<MultiSellExplorationDataInfo>();
                                break;


                            case "FighterDestroyed":
                                FighterDestroyedInfo fighterDestroyedInfo = e.ToObject<FighterDestroyedInfo>();
                                break;

                            case "FighterRebuilt":
                                FighterRebuiltInfo fighterRebuiltInfo = e.ToObject<FighterRebuiltInfo>();
                                break;

                            case "Friends":
                                FriendsInfo friendsInfo = e.ToObject<FriendsInfo>();
                                break;

                            case "FSSAllBodiesFound":
                                FSSAllBodiesFoundInfo fSSAllBodiesFoundInfo = e.ToObject<FSSAllBodiesFoundInfo>();
                                break;

                            case "FSSDiscoveryScan":
                                FSSDiscoveryScanInfo fSSDiscoveryScanInfo = e.ToObject<FSSDiscoveryScanInfo>();
                                break;

                            case "FSSSignalDiscovered":
                                FSSSignalDiscoveredInfo fSSSignalDiscoveredInfo = e.ToObject<FSSSignalDiscoveredInfo>();
                                break;

                            case "JetConeBoost":
                                JetConeBoostInfo jetConeBoostInfo = e.ToObject<JetConeBoostInfo>();
                                break;

                            case "JetConeDamage":
                                JetConeDamageInfo jetConeDamageInfo = e.ToObject<JetConeDamageInfo>();
                                break;

                            case "JoinACrew":
                                JoinACrewInfo joinACrewInfo = e.ToObject<JoinACrewInfo>();
                                break;

                            case "JoinedSquadron":
                                JoinedSquadronInfo joinedSquadronInfo = e.ToObject<JoinedSquadronInfo>();
                                break;

                            case "KickCrewMember":
                                KickCrewMemberInfo kickCrewMemberInfo = e.ToObject<KickCrewMemberInfo>();
                                break;

                            case "LaunchDrone":
                                LaunchDroneInfo launchDroneInfo = e.ToObject<LaunchDroneInfo>();
                                break;

                            case "LaunchFighter":
                                LaunchFighterInfo launchFighterInfo = e.ToObject<LaunchFighterInfo>();
                                break;

                            case "LaunchSRV":
                                LaunchSRVInfo launchSRVInfo = e.ToObject<LaunchSRVInfo>();
                                break;

                            case "LeftSquadron":
                                LeftSquadronInfo leftSquadronInfo = e.ToObject<LeftSquadronInfo>();
                                break;

                            case "Liftoff":
                                LiftoffInfo liftoffInfo = e.ToObject<LiftoffInfo>();
                                break;

                            case "MarketBuy":
                                MarketBuyInfo marketBuyInfo = e.ToObject<MarketBuyInfo>();
                                break;

                            case "Market":
                                MarketInfo marketInfo = e.ToObject<MarketInfo>();
                                break;

                            case "MarketSell":
                                MarketSellInfo marketSellInfo = e.ToObject<MarketSellInfo>();
                                break;

                            case "MaterialDiscovered":
                                MaterialDiscoveredInfo materialDiscoveredInfo = e.ToObject<MaterialDiscoveredInfo>();
                                break;

                            case "MiningRefined":
                                MiningRefinedInfo miningRefinedInfo = e.ToObject<MiningRefinedInfo>();
                                break;



                            case "NavBeaconScan":
                                NavBeaconScanInfo navBeaconScanInfo = e.ToObject<NavBeaconScanInfo>();
                                break;

                            case "NewCommander":
                                NewCommanderInfo newCommanderInfo = e.ToObject<NewCommanderInfo>();
                                break;

                            case "NpcCrewPaidWage":
                                NpcCrewPaidWageInfo npcCrewPaidWageInfo = e.ToObject<NpcCrewPaidWageInfo>();
                                break;

                            case "Outfitting":
                                OutfittingInfo outfittingInfo = e.ToObject<OutfittingInfo>();
                                break;


                            case "PayBounties":
                                PayBountiesInfo payBountiesInfo = e.ToObject<PayBountiesInfo>();
                                break;

                            case "PayFines":
                                PayFinesInfo payFinesInfo = e.ToObject<PayFinesInfo>();
                                break;

                            case "PayLegacyFines":
                                PayLegacyFinesInfo payLegacyFinesInfo = e.ToObject<PayLegacyFinesInfo>();
                                break;

                            case "PowerplayCollect":
                                PowerplayCollectInfo powerplayCollectInfo = e.ToObject<PowerplayCollectInfo>();
                                break;

                            case "PowerplayDefect":
                                PowerplayDefectInfo powerplayDefectInfo = e.ToObject<PowerplayDefectInfo>();
                                break;

                            case "PowerplayDeliver":
                                PowerplayDeliverInfo powerplayDeliverInfo = e.ToObject<PowerplayDeliverInfo>();
                                break;

                            case "PowerplayFastTrack":
                                PowerplayFastTrackInfo powerplayFastTrackInfo = e.ToObject<PowerplayFastTrackInfo>();
                                break;

                            case "Powerplay":
                                PowerplayInfo powerplayInfo = e.ToObject<PowerplayInfo>();
                                break;

                            case "PowerplayJoin":
                                PowerplayJoinInfo powerplayJoinInfo = e.ToObject<PowerplayJoinInfo>();
                                break;

                            case "PowerplayLeave":
                                PowerplayLeaveInfo powerplayLeaveInfo = e.ToObject<PowerplayLeaveInfo>();
                                break;

                            case "PowerplaySalary":
                                PowerplaySalaryInfo powerplaySalaryInfo = e.ToObject<PowerplaySalaryInfo>();
                                break;

                            case "PowerplayVote":
                                PowerplayVoteInfo powerplayVoteInfo = e.ToObject<PowerplayVoteInfo>();
                                break;

                            case "PowerplayVoucher":
                                PowerplayVoucherInfo powerplayVoucherInfo = e.ToObject<PowerplayVoucherInfo>();
                                break;

                            case "Promotion":
                                PromotionInfo promotionInfo = e.ToObject<PromotionInfo>();
                                break;

                            case "ProspectedAsteroid":
                                ProspectedAsteroidInfo prospectedAsteroidInfo = e.ToObject<ProspectedAsteroidInfo>();
                                break;

                            case "PVPKill":
                                PVPKillInfo pVPKillInfo = e.ToObject<PVPKillInfo>();
                                break;

                            case "QuitACrew":
                                QuitACrewInfo quitACrewInfo = e.ToObject<QuitACrewInfo>();
                                break;

                            case "RebootRepair":
                                RebootRepairInfo rebootRepairInfo = e.ToObject<RebootRepairInfo>();
                                break;

                            case "RedeemVoucher":
                                RedeemVoucherInfo redeemVoucherInfo = e.ToObject<RedeemVoucherInfo>();
                                break;

                            case "RepairDrone":
                                RepairDroneInfo repairDroneInfo = e.ToObject<RepairDroneInfo>();
                                break;

                            case "ReservoirReplenished":
                                ReservoirReplenishedInfo reservoirReplenishedInfo = e.ToObject<ReservoirReplenishedInfo>();
                                break;

                            case "RestockVehicle":
                                RestockVehicleInfo restockVehicleInfo = e.ToObject<RestockVehicleInfo>();
                                break;

                            case "Resurrect":
                                ResurrectInfo resurrectInfo = e.ToObject<ResurrectInfo>();
                                break;

                            case "SAAScanComplete":
                                SAAScanCompleteInfo sAAScanCompleteInfo = e.ToObject<SAAScanCompleteInfo>();
                                break;

                            case "ScientificResearch":
                                ScientificResearchInfo scientificResearchInfo = e.ToObject<ScientificResearchInfo>();
                                break;

                            case "Screenshot":
                                ScreenshotInfo screenshotInfo = e.ToObject<ScreenshotInfo>();
                                break;

                            case "SearchAndRescue":
                                SearchAndRescueInfo searchAndRescueInfo = e.ToObject<SearchAndRescueInfo>();
                                break;

                            case "SelfDestruct":
                                SelfDestructInfo selfDestructInfo = e.ToObject<SelfDestructInfo>();
                                break;

                            case "SellDrones":
                                SellDronesInfo sellDronesInfo = e.ToObject<SellDronesInfo>();
                                break;

                            case "SellExplorationData":
                                SellExplorationDataInfo sellExplorationDataInfo = e.ToObject<SellExplorationDataInfo>();
                                break;

                            case "SendText":
                                SendTextInfo sendTextInfo = e.ToObject<SendTextInfo>();
                                break;

                            case "ShipyardBuy":
                                ShipyardBuyInfo shipyardBuyInfo = e.ToObject<ShipyardBuyInfo>();
                                break;

                            case "Shipyard":
                                ShipyardInfo shipyardInfo = e.ToObject<ShipyardInfo>();
                                break;

                            case "ShipyardNew":
                                ShipyardNewInfo shipyardNewInfo = e.ToObject<ShipyardNewInfo>();
                                break;

                            case "ShipyardSell":
                                ShipyardSellInfo shipyardSellInfo = e.ToObject<ShipyardSellInfo>();
                                break;

                            case "ShipyardSwap":
                                ShipyardSwapInfo shipyardSwapInfo = e.ToObject<ShipyardSwapInfo>();
                                break;

                            case "ShipyardTransfer":
                                ShipyardTransferInfo shipyardTransferInfo = e.ToObject<ShipyardTransferInfo>();
                                break;

                            case "SquadronCreated":
                                SquadronCreatedInfo squadronCreatedInfo = e.ToObject<SquadronCreatedInfo>();
                                break;

                            case "SquadronStartup":
                                SquadronStartupInfo squadronStartupInfo = e.ToObject<SquadronStartupInfo>();
                                break;

                            case "SRVDestroyed":
                                SRVDestroyedInfo sRVDestroyedInfo = e.ToObject<SRVDestroyedInfo>();
                                break;

                            case "StoredShips":
                                StoredShipsInfo storedShipsInfo = e.ToObject<StoredShipsInfo>();
                                break;

                            case "TechnologyBroker":
                                TechnologyBrokerInfo technologyBrokerInfo = e.ToObject<TechnologyBrokerInfo>();
                                break;

                            case "Touchdown":
                                TouchdownInfo touchdownInfo = e.ToObject<TouchdownInfo>();
                                break;

                            case "USSDrop":
                                USSDropInfo uSSDropInfo = e.ToObject<USSDropInfo>();
                                break;

                            case "VehicleSwitch":
                                VehicleSwitchInfo vehicleSwitchInfo = e.ToObject<VehicleSwitchInfo>();
                                break;

                            case "WingAdd":
                                WingAddInfo wingAddInfo = e.ToObject<WingAddInfo>();
                                break;

                            case "WingInvite":
                                WingInviteInfo wingInviteInfo = e.ToObject<WingInviteInfo>();
                                break;

                            case "WingJoin":
                                WingJoinInfo wingJoinInfo = e.ToObject<WingJoinInfo>();
                                break;

                            case "WingLeave":
                                WingLeaveInfo wingLeaveInfo = e.ToObject<WingLeaveInfo>();
                                break;

                            case "ModuleSwap":
                                ModuleSwapInfo moduleSwapInfo = e.ToObject<ModuleSwapInfo>();
                                break;

                            case "StoredModules":
                                StoredModulesInfo storedModulesInfo = e.ToObject<StoredModulesInfo>();
                                break;

                            case "FetchRemoteModule":
                                FetchRemoteModuleInfo fetchRemoteModuleInfo = e.ToObject<FetchRemoteModuleInfo>();
                                break;

                            case "ModuleSellRemote":
                                ModuleSellRemoteInfo moduleSellRemoteInfo = e.ToObject<ModuleSellRemoteInfo>();
                                break;
                            */


                }
            }

            // scroll to the end
            /*
            if (_currenttab == LCDTab.Events && CurrentLCDYOffset > 0)
            {
                CurrentLCDYOffset = 99999999;
            }*/


            RefreshDevicePage(_currentPage);
            // _autoResetEvent.Set();

        }

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

        /*
    
        private void ThreadedImageGenerator()
        {
            while (!DoShutdown)
            {
                try
                {
                    RefreshDevicePage(_currentPage);
    
                    _autoResetEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    App.log.Error(ex);
                }
            }
        }*/


    }
}
