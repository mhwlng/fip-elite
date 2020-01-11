using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elite.RingBuffer;
using EliteAPI.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Elite
{
    public enum LCDTab
    {
        None = -1,
        Commander = 1,
        Rank = 2,
        Ship = 3,
        Navigation = 4,
        Events = 5
    }
    public class Commander
    {
        public string Name { get; set; } = "";
        public uint Credits { get; set; } = 0;

        //reputationInfo.Alliance
        //reputationInfo.Empire
        //reputationInfo.Federation
        //reputationInfo.Independent
    }

    public class Dock
    {
        public int LandingPad { get; set; } = -1;
        public string Type { get; set; } = "";
        public string Government { get; set; } = "";
        public string Allegiance { get; set; } = "";
        public string Services { get; set; } = "";

        //undockedInfo.StationName
        //dockingGrantedInfo.StationName

        //undockedInfo.StationType
        //dockingGrantedInfo.StationType
        //dockingRequestedInfo.StationType

        //dockedInfo.StationEconomies
        //dockedInfo.StationEconomyLocalised

        //dockedInfo.StationFaction

        //dockedInfo.DistFromStarLs

    }


    public class Ship
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";

        //loadGameInfo.FuelLevel
        //fsdJumpInfo.FuelLevel
        //fsdJumpInfo.FuelUsed

        //loadGameInfo.ShipIdent
        //loadGameInfo.ShipLocalised
        //setUserShipNameInfo.UserShipId
        //loadoutInfo.ShipIdent

        //loadoutInfo.Hot
        //loadoutInfo.HullHealth
        //shipTargetedInfo.HullHealth
        //loadoutInfo.HullValue
        //loadoutInfo.Modules
        //loadoutInfo.ModulesValue
        //loadoutInfo.Rebuy

        //fuelScoopInfo.Scooped
        //fuelScoopInfo.Total

        //underAttackInfo.Target

        //hullDamageInfo.Fighter
        //hullDamageInfo.PlayerPilot
        //hullDamageInfo.Health

        //shipTargetedInfo.Bounty
        //shipTargetedInfo.Faction
        //shipTargetedInfo.LegalStatus
        //shipTargetedInfo.PilotNameLocalised
        //shipTargetedInfo.PilotRank
        //shipTargetedInfo.ScanStage
        //shipTargetedInfo.ShieldHealth
        //shipTargetedInfo.ShipLocalised
        //shipTargetedInfo.SubsystemLocalised
        //shipTargetedInfo.TargetLocked
        //shipTargetedInfo.SubsystemHealth

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


    }

    public class Location
    {
        //locationInfo.SystemAllegiance
        //fsdJumpInfo.SystemAllegiance

        //locationInfo.SystemEconomyLocalised
        //fsdJumpInfo.SystemEconomyLocalised

        //locationInfo.SystemFaction
        //fsdJumpInfo.SystemFaction

        //locationInfo.SystemGovernmentLocalised
        //fsdJumpInfo.SystemGovernmentLocalised

        //locationInfo.SystemSecondEconomyLocalised
        //fsdJumpInfo.SystemSecondEconomyLocalised

        //locationInfo.SystemSecurityLocalised
        //fsdJumpInfo.SystemSecurityLocalised

        //dockedInfo.StarSystem
        //startJumpInfo.StarSystem
        //supercruiseEntryInfo.StarSystem
        //supercruiseExitInfo.StarSystem

        //startJumpInfo.StarClass

        //supercruiseExitInfo.Body

        //supercruiseExitInfo.BodyType

        //fsdJumpInfo.Population
        //fsdJumpInfo.PowerplayState
        //fsdJumpInfo.Powers
        //fsdJumpInfo.StarPos
        //fSDTargetInfo.Name
        //fsdJumpInfo.FactionState
        //fsdJumpInfo.Factions

        //approachSettlementInfo.Latitude
        //approachSettlementInfo.Longitude
        //approachSettlementInfo.NameLocalised

        //fSDTargetInfo.RemainingJumpsInRoute

        //startJumpInfo.JumpType

    }

    class FipPanel
    {
        private readonly object _refreshDevicePageLock = new object();


        private LCDTab _currenttab = LCDTab.None;
        private LCDTab _lasttab = LCDTab.None;

        public IntPtr FipDevicePointer;

        const int DEFAULT_PAGE = 0;
        private uint _currentPage = DEFAULT_PAGE;
        private uint _prevButtons;

        private List<uint> _pageList = new List<uint>();

        protected bool DoShutdown;
        //private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        //protected Thread GraphicsDrawingThread;

        private Font font = new Font("Console", 10f);
        private SolidBrush solidBrush = new SolidBrush(Color.White);

        protected DirectOutputClass.PageCallback PageCallbackDelegate;
        protected DirectOutputClass.SoftButtonCallback SoftButtonCallbackDelegate;

        protected EventHandler<dynamic> HandleEliteEventsDelegate;

        private RingBuffer<string> EventHistory = new RingBuffer<string>(13, true);

        private Commander Commander = new Commander();
        private Ship ShipExtra = new Ship();
        private Location LocationData = new Location();
        private Dock Dock = new Dock();


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

        private void SetTab(LCDTab tab)
        {
            _lasttab = _currenttab;
            _currenttab = tab;
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
            if (device == FipDevicePointer & (uint)buttons != _prevButtons)
            {
                uint button = (uint)buttons ^ _prevButtons;
                bool state = ((uint)buttons & button) == button;
                _prevButtons = (uint)buttons;

                //Console.WriteLine($"button {button}  state {state}");

                if (state)
                {
                    switch (button)
                    {
                        case 32:
                            SetTab(LCDTab.Commander);
                            break;
                        case 64:
                            SetTab(LCDTab.Rank);
                            break;
                        case 128:
                            SetTab(LCDTab.Ship);
                            break;
                        case 256:
                            SetTab(LCDTab.Navigation);
                            break;
                        case 512:
                            SetTab(LCDTab.Events);
                            break;
                    }

                    RefreshDevicePage(_currentPage);
                }

            }
        }

        public ReturnValues AddPage(uint pageNumber, bool setActive)
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
                using (var fipImage = new Bitmap(320, 240))
                {
                    using (var graphics = Graphics.FromImage(fipImage))
                    {
                        graphics.Clear(Color.Black);

                        var str = DateTime.Now.ToLongTimeString() + "\n";
                        
                        switch (_currenttab)
                        {
                            
                            case LCDTab.None:
                                str += "INIT\n";
                                break;

                            case LCDTab.Commander:
                                str += "CMDR\n";

                                str += "Commander : " + Commander.Name + "\n";
                                str += "Ship : " + ShipExtra.Name != "" ? ShipExtra.Name + "\n" : ShipExtra.Type + "\n";
                                str += "Credits : " + Commander.Credits.ToString("N0") + " CR\n";

                                break;
                            case LCDTab.Rank:
                                str += "RANK\n";

                                str += "Federation: ";
                                str += App.EliteApi.Commander.FederationRankLocalised + " " +
                                       App.EliteApi.Commander.FederationRankProgress + "%\n";

                                str += "Empire: ";
                                str += App.EliteApi.Commander.EmpireRankLocalised + " " +
                                       App.EliteApi.Commander.EmpireRankProgress + "%\n";

                                //str += "Alliance: ";
                                //str += Ranks.AllianceReputation + "%\n";

                                str += "Combat: ";
                                str += App.EliteApi.Commander.CombatRankLocalised + " " +
                                       App.EliteApi.Commander.CombatRankProgress + "%\n";

                                str += "Trade: ";
                                str += App.EliteApi.Commander.TradeRankLocalised + " " +
                                       App.EliteApi.Commander.TradeRankProgress + "%\n";

                                str += "Explorer: ";
                                str += App.EliteApi.Commander.ExplorationRankLocalised + " " +
                                       App.EliteApi.Commander.ExplorationRankProgress + "%\n";

                                str += "CQC: ";
                                str += App.EliteApi.Commander.CqcRank + " " +
                                       App.EliteApi.Commander.CqcRankProgress + "%\n";

                                break;
                            case LCDTab.Ship:
                                str += "SHIP\n";

                                str += "Ship: ";
                                str += ShipExtra.Name != "" ? ShipExtra.Name + "\n" : ShipExtra.Type + "\n";

                                str += "Shiptype: ";
                                str += ShipExtra.Type + "\n";

                                str += "Fuel Main : " + App.EliteApi.Status.Fuel.FuelMain + "\n";
                                str += "Fuel Reservoir : " + App.EliteApi.Status.Fuel.FuelReservoir + "\n";
                                str += "Fuel Max : " + App.EliteApi.Status.Fuel.MaxFuel + "\n";

                                break;

                            case LCDTab.Navigation:
                                str += "NAV\n";

                                str += "Star System: ";
                                str += App.EliteApi.Location.StarSystem + "\n";

                                str += "Body: ";
                                str += App.EliteApi.Location.Body + "\n";

                                str += "BodyType: ";
                                str += App.EliteApi.Location.BodyType + "\n";

                                str += "Station: ";
                                str += App.EliteApi.Location.Station + "\n";

                                if (App.EliteApi.Status.Docked)
                                {

                                    str += "Landingpad: ";
                                    str += Dock.LandingPad != -1 ? Dock.LandingPad.ToString() + "\n" : "-\n";

                                    str += "Stationtype: ";
                                    str += Dock.Type + "\n";

                                    str += "Government: ";
                                    str += Dock.Government + "\n";

                                    str += "Allegiance: ";
                                    str += Dock.Allegiance + "\n";

                                    //str += "Services: ";
                                    //str += Dock.Services + "\n";

                                }

                                break;

                            case LCDTab.Events:
                                str += "EVENTS\n";
                                foreach (var b in EventHistory)
                                {
                                    str += b + "\n";
                                }

                                break;
                        }


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
                                App.EliteApi.Status.Docked
                                App.EliteApi.Status.Landed
                                App.EliteApi.Status.Gear
                                App.EliteApi.Status.Supercruise
                                App.EliteApi.Status.FlightAssist
                                App.EliteApi.Status.Hardpoints
                                App.EliteApi.Status.Winging
                                App.EliteApi.Status.Lights
                                App.EliteApi.Status.CargoScoop
                                App.EliteApi.Status.Scooping

                                App.EliteApi.Status.Cargo

                                App.EliteApi.Status.LegalState
                                App.EliteApi.Status.GameMode
                                App.EliteApi.Status.MusicTrack

                                App.EliteApi.Status.FireGroup
                                App.EliteApi.Status.GuiFocus

                                App.EliteApi.Status.Fuel
                                App.EliteApi.Status.Pips
                                App.EliteApi.Status.JumpRange

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

                                    */


                        var point = new PointF(0, 0);
                        graphics.DrawString(str, font, solidBrush, point);

                        fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        SetImage(page, fipImage);

                        if (_lasttab > 0)
                        {

                            DirectOutputClass.SetLed(FipDevicePointer, _currentPage, (uint)_lasttab, false);
                        }

                        if (_currenttab > 0)
                        {
                            DirectOutputClass.SetLed(FipDevicePointer, _currentPage, (uint)_currenttab, true);
                        }

                    }
                }
            }
        }


        private void HandleEliteEvents(object sender, dynamic e)
        {

            if (!App.EliteApi.IsReady || DoShutdown)
            {
                return;
            }

            //Console.WriteLine(e.@event.ToString());

            EventHistory.Put(DateTime.Now.ToLongTimeString() + " : " + e.@event.ToString());

            switch (e.@event.ToString())
            {
                //----------- COMMANDER

                case "LoadGame":

                    LoadGameInfo loadGameInfo = e.ToObject<LoadGameInfo>();

                    //App.EliteApi.Status.Fuel.MaxFuel
                    //App.EliteApi.Status.GameMode
                    //App.EliteApi.Credits
                    //App.EliteApi.Commander

                    //loadGameInfo.FuelLevel

                    //loadGameInfo.Group
                    //loadGameInfo.Horizons
                    //loadGameInfo.Loan
                    //loadGameInfo.ShipIdent
                    //loadGameInfo.ShipLocalised

                    ShipExtra.Name = loadGameInfo.ShipName;
                    ShipExtra.Type = loadGameInfo.Ship;

                    Commander.Name = loadGameInfo.Commander;
                    Commander.Credits = Convert.ToUInt32(loadGameInfo.Credits);

                    break;

                case "Commander":

                    CommanderInfo commanderInfo = e.ToObject<CommanderInfo>();

                    Commander.Name = commanderInfo.Name;
                    SetTab(LCDTab.Commander);
                    break;

                case "SetUserShipName":

                    SetUserShipNameInfo setUserShipNameInfo = e.ToObject<SetUserShipNameInfo>();

                    //setUserShipNameInfo.UserShipId

                    ShipExtra.Name = setUserShipNameInfo.UserShipName;
                    ShipExtra.Type = setUserShipNameInfo.Ship;
                    break;

                case "Loadout":

                    LoadoutInfo loadoutInfo = e.ToObject<LoadoutInfo>();
                    //loadoutInfo.Hot
                    //loadoutInfo.HullHealth
                    //loadoutInfo.HullValue
                    //loadoutInfo.Modules
                    //loadoutInfo.ModulesValue
                    //loadoutInfo.Rebuy
                    //loadoutInfo.ShipIdent

                    ShipExtra.Name = loadoutInfo.ShipName;
                    ShipExtra.Type = loadoutInfo.Ship;
                    break;

                case "Rank":

                    RankInfo rankInfo = e.ToObject<RankInfo>();

                    //App.EliteApi.Commander.CombatRank
                    //App.EliteApi.Commander.TradeRank
                    //App.EliteApi.Commander.ExplorationRank

                    //App.EliteApi.Commander.CqcRank
                    //App.EliteApi.Commander.EmpireRank
                    //App.EliteApi.Commander.FederationRank

                    break;

                case "Reputation":

                    ReputationInfo reputationInfo = e.ToObject<ReputationInfo>();
                    //reputationInfo.Alliance
                    //reputationInfo.Empire
                    //reputationInfo.Federation
                    //reputationInfo.Independent

                    //Ranks.FederationReputation = Convert.ToInt32(reputationInfo.Federation);
                    //Ranks.EmpireReputation = Convert.ToInt32(reputationInfo.Empire);
                    //Ranks.AllianceReputation = Convert.ToInt32(reputationInfo.Alliance);
                    break;


                case "Progress":

                    ProgressInfo progressInfo = e.ToObject<ProgressInfo>();
                    //App.EliteApi.Commander.CombatRankProgress
                    //App.EliteApi.Commander.TradeRankProgress
                    //App.EliteApi.Commander.ExplorationRankProgress

                    //App.EliteApi.Commander.CqcRankProgress
                    //App.EliteApi.Commander.EmpireRankProgress
                    //App.EliteApi.Commander.FederationRankProgress

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

                    Commander.Credits -= Convert.ToUInt32(refuelAllInfo.Cost);
                    break;

                case "RepairAll":

                    RepairAllInfo repairAllInfo = e.ToObject<RepairAllInfo>();

                    Commander.Credits -= Convert.ToUInt32(repairAllInfo.Cost);
                    break;

                case "Repair":

                    RepairInfo repairInfo = e.ToObject<RepairInfo>();
                    //repairInfo.Item

                    Commander.Credits -= Convert.ToUInt32(repairInfo.Cost);
                    break;

                case "BuyTradeData":

                    BuyTradeDataInfo buyTradeDataInfo = e.ToObject<BuyTradeDataInfo>();
                    //buyTradeDataInfo.System

                    Commander.Credits -= Convert.ToUInt32(buyTradeDataInfo.Cost);
                    break;

                case "BuyExplorationData":

                    BuyExplorationDataInfo buyExplorationDataInfo = e.ToObject<BuyExplorationDataInfo>();
                    //buyExplorationDataInfo.System

                    Commander.Credits -= Convert.ToUInt32(buyExplorationDataInfo.Cost);
                    break;

                case "BuyDrones":

                    BuyDronesInfo buyDronesInfo = e.ToObject<BuyDronesInfo>();
                    //buyDronesInfo.BuyPrice
                    //buyDronesInfo.Count
                    //buyDronesInfo.Type

                    Commander.Credits -= Convert.ToUInt32(buyDronesInfo.TotalCost);
                    break;

                case "BuyAmmo":

                    BuyAmmoInfo buyAmmoInfo = e.ToObject<BuyAmmoInfo>();

                    Commander.Credits -= Convert.ToUInt32(buyAmmoInfo.Cost);
                    break;

                //------------- LOCATION

                case "Location":

                    LocationInfo locationInfo = e.ToObject<LocationInfo>();

                    //App.EliteApi.Location.Body
                    //App.EliteApi.Location.BodyType
                    //App.EliteApi.Location.StarSystem

                    //locationInfo.Docked
                    //locationInfo.Factions
                    //locationInfo.Population
                    //locationInfo.StarPos
                    //locationInfo.SystemAllegiance
                    //locationInfo.SystemEconomyLocalised
                    //locationInfo.SystemFaction
                    //locationInfo.SystemGovernmentLocalised
                    //locationInfo.SystemSecondEconomyLocalised
                    //locationInfo.SystemSecurityLocalised

                    break;

                case "ApproachBody":

                    ApproachBodyInfo approachBodyInfo = e.ToObject<ApproachBodyInfo>();

                    //App.EliteApi.Location.Body
                    //App.EliteApi.Location.StarSystem
                    //App.EliteApi.Location.BodyType="Planet"
                    break;

                case "ApproachSettlement":

                    ApproachSettlementInfo approachSettlementInfo = e.ToObject<ApproachSettlementInfo>();

                    //App.EliteApi.Location.Station 
                    //App.EliteApi.Location.BodyType="Planet"

                    //approachSettlementInfo.Latitude
                    //approachSettlementInfo.Longitude
                    //approachSettlementInfo.NameLocalised

                    break;

                case "LeaveBody":

                    LeaveBodyInfo leaveBodyInfo = e.ToObject<LeaveBodyInfo>();
                    //App.EliteApi.Location.Body
                    //App.EliteApi.Location.StarSystem
                    //App.EliteApi.Location.BodyType="Planet"

                    break;

                case "Undocked":

                    UndockedInfo undockedInfo = e.ToObject<UndockedInfo>();
                    //undockedInfo.StationName
                    //undockedInfo.StationType

                    Dock = new Dock();

                    break;

                case "Docked":

                    DockedInfo dockedInfo = e.ToObject<DockedInfo>();

                    //App.EliteApi.Location.Station

                    //dockedInfo.DistFromStarLs
                    //dockedInfo.StarSystem
                    //dockedInfo.StationEconomies
                    //dockedInfo.StationEconomyLocalised
                    //dockedInfo.StationFaction

                    Dock.Type = dockedInfo.StationType;
                    Dock.Government = dockedInfo.StationGovernmentLocalised;
                    Dock.Allegiance = dockedInfo.StationAllegiance;
                    Dock.Services = string.Join(", ", dockedInfo.StationServices);

                    break;

                case "DockingGranted":

                    DockingGrantedInfo dockingGrantedInfo = e.ToObject<DockingGrantedInfo>();
                    //dockingGrantedInfo.StationName
                    //dockingGrantedInfo.StationType

                    Dock.LandingPad = Convert.ToInt32(dockingGrantedInfo.LandingPad);
                    break;

                case "DockingRequested":

                    DockingRequestedInfo dockingRequestedInfo = e.ToObject<DockingRequestedInfo>();

                    //App.EliteApi.Location.Station

                    //dockingRequestedInfo.StationType
                    break;

                case "FSDJump":

                    FSDJumpInfo fsdJumpInfo = e.ToObject<FSDJumpInfo>();

                    //App.EliteApi.Location.StarSystem
                    //App.EliteApi.Status.JumpRange (fsdJumpInfo.JumpDist)

                    //fsdJumpInfo.FactionState
                    //fsdJumpInfo.Factions
                    //fsdJumpInfo.FuelLevel
                    //fsdJumpInfo.FuelUsed
                    //fsdJumpInfo.Population
                    //fsdJumpInfo.PowerplayState
                    //fsdJumpInfo.Powers
                    //fsdJumpInfo.StarPos
                    //fsdJumpInfo.SystemAllegiance
                    //fsdJumpInfo.SystemEconomyLocalised
                    //fsdJumpInfo.SystemSecurityLocalised
                    //fsdJumpInfo.SystemSecondEconomyLocalised
                    //fsdJumpInfo.SystemGovernmentLocalised
                    //fsdJumpInfo.SystemFaction

                    break;

                case "StartJump":

                    StartJumpInfo startJumpInfo = e.ToObject<StartJumpInfo>();

                    //startJumpInfo.JumpType
                    //startJumpInfo.StarClass
                    //startJumpInfo.StarSystem
                    break;

                case "FSDTarget":
                    FSDTargetInfo fSDTargetInfo = e.ToObject<FSDTargetInfo>();
                    //fSDTargetInfo.RemainingJumpsInRoute
                    //fSDTargetInfo.Name

                    break;

                case "SupercruiseEntry":

                    SupercruiseEntryInfo supercruiseEntryInfo = e.ToObject<SupercruiseEntryInfo>();

                    //supercruiseEntryInfo.StarSystem
                    break;

                case "SupercruiseExit":

                    SupercruiseExitInfo supercruiseExitInfo = e.ToObject<SupercruiseExitInfo>();
                    //supercruiseExitInfo.Body
                    //supercruiseExitInfo.BodyType
                    //supercruiseExitInfo.StarSystem

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

                    //case "MainMenu":
                    //case "DockingComputer":
                    //case "NoTrack":

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
                    //hullDamageInfo.Fighter
                    //hullDamageInfo.PlayerPilot
                    //hullDamageInfo.Health
                    break;

                case "ShipTargeted":
                    ShipTargetedInfo shipTargetedInfo = e.ToObject<ShipTargetedInfo>();
                    //shipTargetedInfo.Bounty
                    //shipTargetedInfo.Faction
                    //shipTargetedInfo.HullHealth
                    //shipTargetedInfo.LegalStatus
                    //shipTargetedInfo.PilotNameLocalised
                    //shipTargetedInfo.PilotRank
                    //shipTargetedInfo.ScanStage
                    //shipTargetedInfo.ShieldHealth
                    //shipTargetedInfo.ShipLocalised
                    //shipTargetedInfo.SubsystemLocalised
                    //shipTargetedInfo.TargetLocked
                    //shipTargetedInfo.SubsystemHealth

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

                    case "CargoDepot":
                        CargoDepotInfo cargoDepotInfo = e.ToObject<CargoDepotInfo>();
                        break;

                    case "Cargo":
                        CargoInfo cargoInfo = e.ToObject<CargoInfo>();
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

                    case "CollectCargo":
                        CollectCargoInfo collectCargoInfo = e.ToObject<CollectCargoInfo>();
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

                    case "EjectCargo":
                        EjectCargoInfo ejectCargoInfo = e.ToObject<EjectCargoInfo>();
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

                    case "FetchRemoteModule":
                        FetchRemoteModuleInfo fetchRemoteModuleInfo = e.ToObject<FetchRemoteModuleInfo>();
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

                    case "MassModuleStore":
                        MassModuleStoreInfo massModuleStoreInfo = e.ToObject<MassModuleStoreInfo>();
                        break;

                    case "MaterialDiscovered":
                        MaterialDiscoveredInfo materialDiscoveredInfo = e.ToObject<MaterialDiscoveredInfo>();
                        break;

                    case "MiningRefined":
                        MiningRefinedInfo miningRefinedInfo = e.ToObject<MiningRefinedInfo>();
                        break;

                    case "MissionAbandoned":
                        MissionAbandonedInfo missionAbandonedInfo = e.ToObject<MissionAbandonedInfo>();
                        break;

                    case "MissionAccepted":
                        MissionAcceptedInfo missionAcceptedInfo = e.ToObject<MissionAcceptedInfo>();
                        break;

                    case "MissionCompleted":
                        MissionCompletedInfo missionCompletedInfo = e.ToObject<MissionCompletedInfo>();
                        break;

                    case "MissionFailed":
                        MissionFailedInfo missionFailedInfo = e.ToObject<MissionFailedInfo>();
                        break;

                    case "MissionRedirected":
                        MissionRedirectedInfo missionRedirectedInfo = e.ToObject<MissionRedirectedInfo>();
                        break;

                    case "Missions":
                        MissionsInfo missionsInfo = e.ToObject<MissionsInfo>();
                        break;

                    case "ModuleBuy":
                        ModuleBuyInfo moduleBuyInfo = e.ToObject<ModuleBuyInfo>();
                        break;

                    case "ModuleRetrieve":
                        ModuleRetrieveInfo moduleRetrieveInfo = e.ToObject<ModuleRetrieveInfo>();
                        break;

                    case "ModuleSell":
                        ModuleSellInfo moduleSellInfo = e.ToObject<ModuleSellInfo>();
                        break;

                    case "ModuleSellRemote":
                        ModuleSellRemoteInfo moduleSellRemoteInfo = e.ToObject<ModuleSellRemoteInfo>();
                        break;

                    case "ModuleStore":
                        ModuleStoreInfo moduleStoreInfo = e.ToObject<ModuleStoreInfo>();
                        break;

                    case "ModuleSwap":
                        ModuleSwapInfo moduleSwapInfo = e.ToObject<ModuleSwapInfo>();
                        break;

                    case "MultiSellExplorationData":
                        MultiSellExplorationDataInfo multiSellExplorationDataInfo =
                            e.ToObject<MultiSellExplorationDataInfo>();
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

                    case "Passengers":
                        PassengersInfo passengersInfo = e.ToObject<PassengersInfo>();
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

                    case "StoredModules":
                        StoredModulesInfo storedModulesInfo = e.ToObject<StoredModulesInfo>();
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

                    */

            }

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
