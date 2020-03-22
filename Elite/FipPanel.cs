using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elite.RingBuffer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EliteJournalReader;
using EliteJournalReader.Events;
using TheArtOfDev.HtmlRenderer.WinForms;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;
using TheArtOfDev.HtmlRenderer.Core.Entities;

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
        POI = 6,

        //---------------

        Map = 7,
        //B = 8,
        //C = 9,
        //D = 10,
        //E = 11,
        Events = 12,

        //---------------

        G = 13,
        H = 14,
        I = 15,
        J = 16,
        K = 17,
        L = 18
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

        private readonly object _refreshDevicePageLock = new object();


        private LCDTab _currentTab = LCDTab.None;
        private LCDTab _lastTab = LCDTab.Init;
        const int DEFAULT_PAGE = 0;
        const int NO_PAGES = 2;
        private uint _currentPage = DEFAULT_PAGE;
        private string _settingsPath;

        private int[] _currentCard = new int[100];
        private int[] _currentZoomLevel = new int[100];


        private int CurrentLCDYOffset = 0;
        private int CurrentLCDHeight = 0;

        public IntPtr FipDevicePointer;

        private uint _prevButtons;

        private List<uint> _pageList = new List<uint>();

        private readonly Pen _scrollPen = new Pen(Color.FromArgb(0xff,0xFF,0xB0,0x00));
        private readonly Pen _whitePen = new Pen(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF),(float)0.1);
        private readonly SolidBrush _scrollBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xB0, 0x00));
        private readonly SolidBrush redBrush = new SolidBrush(Color.FromArgb(0xFF, 0x00, 0x00));

        private Image htmlImage = null;
        private Image menuHtmlImage = null;

        private const int HtmlMenuWindowWidth = 69;

        private const int HtmlWindowHeight = 240;

        private const int HtmlWindowXOffset = HtmlMenuWindowWidth + 1;
        private const int HtmlWindowWidth = 311- HtmlWindowXOffset;
        

        protected bool DoShutdown;

        protected DirectOutputClass.PageCallback PageCallbackDelegate;
        protected DirectOutputClass.SoftButtonCallback SoftButtonCallbackDelegate;


        public FipPanel(IntPtr devicePtr) 
        {
            FipDevicePointer = devicePtr;
        }

        public void Initalize()
        {
            // FIP = 3e083cd8-6a37-4a58-80a8-3d6a2c07513e

            // https://github.com/Raptor007/Falcon4toSaitek/blob/master/Raptor007's%20Falcon%204%20to%20Saitek%20Utility/DirectOutput.h
            //https://github.com/poiuqwer78/fip4j-core/tree/master/src/main/java/ch/poiuqwer/saitek/fip4j


            PageCallbackDelegate = PageCallback;
            SoftButtonCallbackDelegate = SoftButtonCallback;

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

            _currentPage = DEFAULT_PAGE;

            var returnValues3 = DirectOutputClass.GetSerialNumber(FipDevicePointer, out var serialNumber);
            if (returnValues3 != ReturnValues.S_OK)
            {
                App.log.Error("FipPanel failed to get Serial Number. " + returnValues1);
            }
            else
            {
                App.log.Info("FipPanel Serial Number " + serialNumber);

                _settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                "\\mhwlng\\fip-elite\\" + serialNumber;

                if (File.Exists(_settingsPath))
                {
                    _currentPage = uint.Parse(File.ReadAllText(_settingsPath));
                }
                else
                {
                    (new FileInfo(_settingsPath)).Directory?.Create();

                    File.WriteAllText(_settingsPath, _currentPage.ToString());
                }

                for (uint x = 0; x <= NO_PAGES; x++)
                {
                    if (x != _currentPage)
                    {
                        AddPage(x, false);
                    }
                }

                AddPage(_currentPage, true);

                RefreshDevicePage();
            }

        }

        public void Shutdown()
        {
            try
            {
                DoShutdown = true;

                if (_pageList.Count > 0)
                {
                    do
                    {
                        DirectOutputClass.RemovePage(FipDevicePointer, _pageList[0]);

                        _pageList.Remove(_pageList[0]);


                    } while (_pageList.Count > 0);
                }


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

                _currentPage = ((uint)_currentTab - 1) / 6;

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
                    _lastTab = LCDTab.Init;
                    _currentTab = LCDTab.None;

                    CurrentLCDYOffset = 0;

                    _currentPage = (uint)page;

                    File.WriteAllText(_settingsPath, _currentPage.ToString());


                    RefreshDevicePage();
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

                bool mustRender = true;

                switch (button)
                {
                    case 8: // scroll clockwise
                        if (state && (_currentTab == LCDTab.POI || _currentTab == LCDTab.Map || _currentTab == LCDTab.Ship))
                        {

                            _currentCard[(int) _currentTab]++;
                            _currentZoomLevel[(int) _currentTab]++;
                            CurrentLCDYOffset = 0;

                            mustRefresh = true;
                        }
                        break;
                    case 16: // scroll anti-clockwise

                        if (state && (_currentTab == LCDTab.POI || _currentTab == LCDTab.Map || _currentTab == LCDTab.Ship))
                        {
                            _currentCard[(int) _currentTab]--;
                            _currentZoomLevel[(int) _currentTab]--;
                            CurrentLCDYOffset = 0;

                            mustRefresh = true;
                        }
                        break;
                    case 2: // scroll clockwise
                        if (state)
                        {
                            CurrentLCDYOffset += 50;

                            mustRender = false;

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

                            mustRender = false;

                            mustRefresh = true;
                        }
                        break;
                }

                switch (_currentPage)
                {
                    case 0:
                        switch (button)
                        {
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
                                if (EliteData.TargetData.TargetLocked)
                                {
                                    mustRefresh = SetTab(LCDTab.Target);
                                }
                                break;
                            case 512:
                                if (EliteData.MissionData.Count > 0)
                                {
                                    mustRefresh = SetTab(LCDTab.Missions);
                                }
                                break;
                            case 1024:
                                mustRefresh = SetTab(LCDTab.POI);
                                break;
                        }

                        break;
                    case 1:
                        switch (button)
                        {
                            case 32:
                                mustRefresh = SetTab(LCDTab.Map);
                                break;
                            /*
                            case 64:
                                mustRefresh = SetTab(LCDTab.B);
                                break;
                            case 128:
                                mustRefresh = SetTab(LCDTab.C);
                                break;
                            case 256:
                                mustRefresh = SetTab(LCDTab.D);
                                break;
                            case 512:
                                mustRefresh = SetTab(LCDTab.E);
                                break;
                                */
                            case 1024:
                                mustRefresh = SetTab(LCDTab.Events);
                                break;
                        }
                        break;
                    case 2:
                        switch (button)
                        {
                            case 32:
                                mustRefresh = SetTab(LCDTab.G);
                                break;
                            case 64:
                                mustRefresh = SetTab(LCDTab.H);
                                break;
                            case 128:
                                mustRefresh = SetTab(LCDTab.I);
                                break;
                            case 256:
                                mustRefresh = SetTab(LCDTab.J);
                                break;
                            case 512:
                                mustRefresh = SetTab(LCDTab.K);
                                break;
                            case 1024:
                                mustRefresh = SetTab(LCDTab.L);
                                break;
                        }
                        break;
                }

                if (mustRefresh)
                {
                    RefreshDevicePage(mustRender);
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
        
        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            var markerWidth = 40.0;
            var markerHeight = 40.0;

            try
            {
                var image = Image.FromFile("Templates\\images\\" + e.Src);

                using (var graphics = Graphics.FromImage(image))
                {
                    if (e.Src.ToLower() == "galaxy.png" && _currentTab == LCDTab.Map)
                    {
                        graphics.DrawPolygon(_whitePen, EliteHistory.TravelHistoryPoints.ToArray());

                        var zoomLevel = _currentZoomLevel[(int)_currentTab] / 2.0 + 1;

                        if (zoomLevel > 1)
                        {
                            markerWidth /= zoomLevel;
                            markerHeight /= zoomLevel;
                        }

                        var zoomedXCenter = image.Width / 2.0;
                        var zoomedYCenter = image.Height / 2.0;

                        if (EliteData.LocationData?.StarPos?.Count == 3)
                        {
                            var spaceX = EliteData.LocationData.StarPos[0];
                            var spaceZ = EliteData.LocationData.StarPos[2];

                            var imgX = (spaceX - EliteHistory.SpaceMinX) / (EliteHistory.SpaceMaxX - EliteHistory.SpaceMinX) * image.Width;
                            var imgY = (EliteHistory.SpaceMaxZ - spaceZ) / (EliteHistory.SpaceMaxZ - EliteHistory.SpaceMinZ) * image.Height;

                            imgX -= markerWidth / 2.0;
                            imgY -= markerHeight / 2.0;

                            graphics.FillEllipse(redBrush, (float) imgX, (float) imgY, (float) markerWidth,
                                (float) markerWidth);

                            zoomedXCenter = imgX;
                            zoomedYCenter = imgY;

                        }

                        if (zoomLevel > 1)
                        {
                            var zoomedWidth = image.Width / zoomLevel;
                            var zoomedHeight = image.Height / zoomLevel;

                            var zoomedXTopLeft = zoomedXCenter - (zoomedWidth / 2.0);
                            var zoomedYTopLeft = zoomedYCenter - (zoomedHeight / 2.0);

                            if (zoomedXTopLeft + zoomedWidth > image.Width)
                            {
                                zoomedXTopLeft = image.Width - zoomedWidth;
                            }

                            if (zoomedYTopLeft + zoomedHeight > image.Height)
                            {
                                zoomedYTopLeft = image.Height - zoomedHeight;
                            }

                            e.Callback(image, zoomedXTopLeft, zoomedYTopLeft, zoomedWidth, zoomedHeight);
                            return;
                        }

                    }

                }

                e.Callback(image);
            }
            catch
            {
                var image = new Bitmap(1, 1);

                e.Callback(image);
            }
        }


        public void RefreshDevicePage(bool mustRender = true)
        {
            lock (_refreshDevicePageLock)
            {
                if (EliteData.MissionData.Count == 0 && _currentTab == LCDTab.Missions)
                {
                    SetTab(LCDTab.Navigation);
                }
                else if (!EliteData.TargetData.TargetLocked && _currentTab == LCDTab.Target)
                {
                    SetTab(LCDTab.Navigation);
                }

                using (var fipImage = new Bitmap(320, 240))
                {
                    using (var graphics = Graphics.FromImage(fipImage))
                    {
                        var menustr = "";

                        if (mustRender)
                        {
                            menustr =
                                Engine.Razor.Run("menu.cshtml", null, new
                                {
                                    CurrentTab = (int) _currentTab,
                                    CurrentPage = _currentPage,

                                    TargetLocked = EliteData.TargetData.TargetLocked,

                                    MissionCount = EliteData.MissionData.Count
                                });
                        }

                        var str = "";

                        switch (_currentTab)
                        {
                            case LCDTab.Ship:

                                if (_currentCard[(int)_currentTab] < 0)
                                {
                                    _currentCard[(int)_currentTab] = 1;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 1)
                                {
                                    _currentCard[(int)_currentTab] = 0;
                                }

                                break;

                            case LCDTab.POI:

                                if (_currentCard[(int)_currentTab] < 0)
                                {
                                    _currentCard[(int)_currentTab] = 6;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 6)
                                {
                                    _currentCard[(int)_currentTab] = 0;
                                }

                                break;

                            case LCDTab.Map:


                                if (_currentZoomLevel[(int)_currentTab] < 0)
                                {
                                    _currentZoomLevel[(int)_currentTab] = 0;
                                }
                                else
                                if (_currentZoomLevel[(int)_currentTab] > 15)
                                {
                                    _currentZoomLevel[(int)_currentTab] = 15;
                                }

                                break;

                        }

                        if (mustRender)
                        {

                            switch (_currentTab)
                            {
                                //----------------------

                                case LCDTab.None:

                                    str =
                                        Engine.Razor.Run("init.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage

                                        });

                                    break;

                                case LCDTab.Commander:

                                    str =
                                        Engine.Razor.Run("1.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            Commander = EliteData.CommanderData.Name,

                                            ShipName = EliteData.ShipData.Name?.Trim(),

                                            ShipType = EliteData.ShipData.Type?.Trim(),

                                            LegalState = EliteData.StatusData.LegalState,

                                            Credits = EliteData.CommanderData.Credits.ToString("N0"),

                                            Rebuy = EliteData.CommanderData.Rebuy.ToString("N0"),

                                            FederationRank = EliteData.CommanderData.FederationRank,
                                            FederationRankProgress = EliteData.CommanderData.FederationRankProgress,

                                            EmpireRank = EliteData.CommanderData.EmpireRank,
                                            EmpireRankProgress = EliteData.CommanderData.EmpireRankProgress,

                                            CombatRank = EliteData.CommanderData.CombatRank,
                                            CombatRankProgress = EliteData.CommanderData.CombatRankProgress,

                                            TradeRank = EliteData.CommanderData.TradeRank,
                                            TradeRankProgress = EliteData.CommanderData.TradeRankProgress,

                                            ExplorationRank = EliteData.CommanderData.ExplorationRank,
                                            ExplorationRankProgress = EliteData.CommanderData.ExplorationRankProgress,

                                            CqcRank = EliteData.CommanderData.CqcRank.ToString(),

                                            CqcRankProgress = EliteData.CommanderData.CqcRankProgress,

                                            FederationReputation = EliteData.CommanderData.FederationReputation,
                                            AllianceReputation = EliteData.CommanderData.AllianceReputation,
                                            EmpireReputation = EliteData.CommanderData.EmpireReputation,

                                            FederationReputationState = EliteData.CommanderData.FederationReputationState,
                                            AllianceReputationState = EliteData.CommanderData.AllianceReputationState,
                                            EmpireReputationState = EliteData.CommanderData.EmpireReputationState

                                        });

                                

                                    break;

                                case LCDTab.Ship:

                                    str =
                                        Engine.Razor.Run("2.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            CurrentCard = _currentCard[(int)_currentTab],

                                            ShipName = EliteData.ShipData.Name?.Trim(),

                                            ShipType = EliteData.ShipData.Type?.Trim(),

                                            ShipImage = EliteData.ShipData.Type?.Trim() + ".png",

                                            AutomaticDocking = EliteData.ShipData.AutomaticDocking,

                                            Docked = EliteData.StatusData.Docked,

                                            FuelMain = EliteData.StatusData.Fuel.FuelMain,

                                            FuelReservoir = EliteData.StatusData.Fuel.FuelReservoir,

                                            FuelCapacity = EliteData.StatusData.FuelCapacity,

                                            FuelPercent = EliteData.StatusData.FuelCapacity >  0 ?
                                                Convert.ToInt32(100 / EliteData.StatusData.FuelCapacity *
                                                                EliteData.StatusData.Fuel.FuelMain) : 0,

                                            LastJump = EliteData.StatusData.JumpRange,

                                            Cargo = EliteData.StatusData.Cargo,

                                            CargoCapacity = EliteData.ShipData.CargoCapacity,

                                            HullHealth = EliteData.ShipData.HullHealth,

                                            HullValue = EliteData.ShipData.HullValue, 
                                            ModulesValue= EliteData.ShipData.ModulesValue,

                                            UnladenMass= EliteData.ShipData.UnladenMass,
                                            MaxJumpRange= EliteData.ShipData.MaxJumpRange,

                                            Hot = EliteData.ShipData.Hot,

                                            StoredShips = EliteHistory.ShipsList.Where(x => x.Stored == true).OrderBy(x => x.Distance).ThenBy(x => x.ShipType).ToList()

                                        });
                                    
                                    break;
                                    
                                case LCDTab.Navigation:

                                    str =
                                        Engine.Razor.Run("3.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            StarSystem = EliteData.LocationData.StarSystem,

                                            Body = !string.IsNullOrEmpty(EliteData.LocationData.BodyType) &&
                                                   !string.IsNullOrEmpty(EliteData.LocationData.Body)
                                                ? EliteData.LocationData.Body
                                                : null,

                                            //"Station""Star""Planet""PlanetaryRing""StellarRing""AsteroidCluster"

                                            BodyType = EliteData.LocationData.BodyType,

                                            Station = EliteData.LocationData.BodyType == "Station" &&
                                                      !string.IsNullOrEmpty(EliteData.LocationData.Body)
                                                ? EliteData.LocationData.Body
                                                : EliteData.LocationData.Station,

                                            Docked = EliteData.StatusData.Docked,

                                            LandingPad = EliteData.DockData.LandingPad,

                                            StartJump = EliteData.LocationData.StartJump,

                                            JumpType = EliteData.LocationData.JumpType,

                                            JumpToSystem = EliteData.LocationData.JumpToSystem,

                                            JumpToStarClass = EliteData.LocationData.JumpToStarClass,

                                            RemainingJumpsInRoute = EliteData.LocationData.RemainingJumpsInRoute,

                                            FsdTargetName = EliteData.LocationData.FsdTargetName,

                                            Settlement = EliteData.LocationData.Settlement,

                                            HideBody = EliteData.LocationData.HideBody,

                                            StationType = EliteData.DockData.Type,

                                            Government = EliteData.DockData.Government,

                                            Allegiance = EliteData.DockData.Allegiance,

                                            Faction = EliteData.DockData.Faction,

                                            Economy = EliteData.DockData.Economy,

                                            DistFromStarLs = EliteData.DockData.DistFromStarLs,

                                            SystemAllegiance = EliteData.LocationData.SystemAllegiance,

                                            SystemFaction = EliteData.LocationData.SystemFaction,

                                            SystemSecurity = EliteData.LocationData.SystemSecurity,

                                            SystemEconomy = EliteData.LocationData.SystemEconomy,

                                            SystemGovernment = EliteData.LocationData.SystemGovernment,

                                            Population = EliteData.LocationData.Population

                                        });

                                    break;
                                    
                                case LCDTab.Target:

                                    str =
                                        Engine.Razor.Run("4.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            TargetLocked = EliteData.TargetData.TargetLocked,
                                            ScanStage = EliteData.TargetData.ScanStage,

                                            Bounty = EliteData.TargetData.Bounty,
                                            Faction = EliteData.TargetData.Faction,
                                            LegalStatus = EliteData.TargetData.LegalStatus,
                                            PilotNameLocalised = EliteData.TargetData.PilotNameLocalised,
                                            PilotRank = EliteData.TargetData.PilotRank,

                                            Power = EliteData.TargetData.Power,

                                            Ship = EliteData.TargetData.Ship?.Trim(),

                                            ShipImage = EliteData.TargetData.Ship?.Trim() + ".png",

                                            SubsystemLocalised = EliteData.TargetData.SubsystemLocalised,

                                            SubsystemHealth = EliteData.TargetData.SubsystemHealth,
                                            HullHealth = EliteData.TargetData.HullHealth,
                                            ShieldHealth = EliteData.TargetData.ShieldHealth,

                                        });

                                    break;
                                    
                                case LCDTab.Missions:

                                    str =
                                        Engine.Razor.Run("5.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            MissionData = EliteData.MissionData
                                        });

                                    break;
                                    
                                case LCDTab.POI:

                                    str =
                                        Engine.Razor.Run("6.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            CurrentCard = _currentCard[(int) _currentTab],

                                            CurrentPois = EliteData.CurrentPois, // 0

                                            CurrentInterStellarFactors = EliteData.CurrentInterStellarFactors, // 1
                                            CurrentRawMaterialTraders = EliteData.CurrentRawMaterialTraders, // 2
                                            CurrentManufacturedMaterialTraders =
                                                EliteData.CurrentManufacturedMaterialTraders, // 3
                                            CurrentEncodedDataTraders = EliteData.CurrentEncodedDataTraders, // 4
                                            CurrentHumanTechnologyBrokers = EliteData.CurrentHumanTechnologyBrokers, // 5
                                            CurrentGuardianTechnologyBrokers = EliteData.CurrentGuardianTechnologyBrokers // 6

                                        });

                                    break;

                                //----------------------

                                case LCDTab.Map:


                                    str =
                                        Engine.Razor.Run("7.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            _currentZoomLevel = _currentZoomLevel[(int) _currentTab],


                                        });

                                    break;

                                case LCDTab.Events:

                                    var eventlist = "";
                                    foreach (var b in EliteData.EventHistory)
                                    {
                                        eventlist += b + "<br/>";
                                    }

                                    str =
                                        Engine.Razor.Run("12.cshtml", null, new
                                        {
                                            CurrentTab = (int) _currentTab,
                                            CurrentPage = _currentPage,

                                            EventList = eventlist
                                        });

                                    break;

                            } 
                        }

                        graphics.Clear(Color.Black);

                        if (_currentTab >= 0)
                        {

                            if (mustRender)
                            {
                                var measureData =HtmlRender.Measure(graphics, str, HtmlWindowWidth, App.cssData);

                                CurrentLCDHeight = (int)measureData.Height;
                            }

                            CheckLcdOffset();

                            if (CurrentLCDHeight > 0)
                            {

                                if (mustRender)
                                {
                                    htmlImage = HtmlRender.RenderToImage(str,
                                        new Size(HtmlWindowWidth, CurrentLCDHeight + 20), Color.Black, App.cssData,
                                        null, OnImageLoad);
                                }

                                if (htmlImage != null)
                                {
                                    graphics.DrawImage(htmlImage, new Rectangle(new Point(HtmlWindowXOffset, 0),
                                            new Size(HtmlWindowWidth, HtmlWindowHeight + 20)),
                                        new Rectangle(new Point(0, CurrentLCDYOffset),
                                            new Size(HtmlWindowWidth, HtmlWindowHeight + 20)),
                                        GraphicsUnit.Pixel);
                                }
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

                        if (mustRender)
                        {
                            menuHtmlImage = HtmlRender.RenderToImage(menustr,
                                new Size(HtmlMenuWindowWidth, HtmlWindowHeight), Color.Black, App.cssData, null,
                                OnImageLoad);
                        }

                        if (menuHtmlImage != null)
                        {

                            graphics.DrawImage(menuHtmlImage, 0, 0);
                        }

#if DEBUG
                        fipImage.Save(@"screenshot"+(int)_currentTab+"_"+ _currentCard[(int)_currentTab] + ".png", ImageFormat.Png);
#endif

                        fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        SetImage(_currentPage, fipImage);

                        if (_lastTab > 0)
                        {
                            DirectOutputClass.SetLed(FipDevicePointer, _currentPage,
                                (uint) _lastTab - ((uint) _lastTab - 1) / 6 * 6, false);
                        }

                        if (_currentTab > 0)
                        {
                            DirectOutputClass.SetLed(FipDevicePointer, _currentPage,
                                (uint) _currentTab - ((uint) _currentTab - 1) / 6 * 6, true);
                        }

                    }
                }
            }
        }


    }
}
