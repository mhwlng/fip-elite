﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using EDEngineer.Models;
using EliteJournalReader;
using TheArtOfDev.HtmlRenderer.WinForms;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using Image = System.Drawing.Image;

// For extension methods.


// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Elite
{
    public enum JoystickButton
    {
        Up,
        Down,
        Left,
        Right,
        Push,
        Navigation,
        Target,
        Commander,
        Galnet,
        Missions,
        Chat,
        HWInfo,
        Ship,
        Materials,
        Cargo,
        Engineer,
        ShipLocker,
        BackPack,
        POI,
        Galaxy,
        Engineers,
        Powers,
        Mining
    }

    public enum LcdPage
    {
        Collapsed = -1,
        HomeMenu = 0,
        InfoMenu = 1,
        ShipMenu = 2,
        SuitMenu = 3,
        LocationsMenu = 4
    }

    public enum LcdTab
    {
        Init=-999,
        None = 0,

        Navigation = 1,
        Target = 2,
        InfoMenu = 3, // info ->
        ShipMenu = 4, // ship ->
        PersonMenu = 5, // suit ->
        LocationsMenu = 6, // locations ->

        //---------------

        InfoBack = 7,
        Commander = 8,
        Galnet = 9,
        Missions = 10,
        Chat = 11,
        HWInfo = 12,
        
        //---------------

        ShipBack = 13, // <- back
        Ship = 14,
        Materials = 15,
        Cargo = 16,
        Engineer = 17,
        // = 18

        //---------------

        SuitBack = 19, // <- back
        ShipLocker = 20,
        BackPack = 21,
        // = 22,
        // = 23,
        // = 24,

        //---------------

        LocationsBack = 25, // <- back
        POI = 26,
        Galaxy = 27,
        Engineers = 28,
        Powers = 29,
        Mining = 30

    }

    public static class TimeSpanFormattingExtensions
    {
        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalMinutes <= 1)
            {
                return $@"{Math.Ceiling(t.TotalSeconds)} seconds";
            }
            if (t.TotalHours <= 1)
            {
                return $@"{Math.Ceiling(t.TotalMinutes)} minutes";
            }
            if (t.TotalDays <= 1)
            {
                return $@"{Math.Ceiling(t.TotalHours)} hours";
            }

            return $@"{Math.Ceiling(t.TotalDays)} days";
        }
    }

    public class MyHtmlHelper
    {
        public IEncodedString MaterialsString(int i)
        {
            var ms = "";

            var materials = SystemInfo.SystemData.Data.bodies[i].materials;

            foreach (var m in materials)
            {
                SystemInfo.PeriodicElements.TryGetValue(m.Key, out var elem);
                if (!string.IsNullOrEmpty(elem))
                {
                    ms += elem;
                }
                else
                {
                    ms += m.Key;
                }
                ms +=  "&nbsp;(" + m.Value.ToString("N1") + "%) ";

            }

            return new RawString(ms.Trim());

        }

        public IEncodedString BodyTreeElement1(int i)
        {
            var parents = SystemInfo.SystemData.Data.bodies[i].parents;

            var colcount = parents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;

            int[] topLineHide = new int[100];

            for (int k = i ; k < SystemInfo.SystemData.Data.bodies.Count; k++)
            {
                var rowParents = SystemInfo.SystemData.Data.bodies[k].parents;
                var rowColcount = rowParents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;

                for (int j = 0; j < rowColcount; j++)
                {
                    if (topLineHide[j] != 2)
                    {
                        if (rowColcount > 0 && j < rowColcount - 1)
                        {
                            topLineHide[j] = 1;
                        }
                        else
                        {
                            topLineHide[j] = 2;
                        }
                    }
                }

                if (rowColcount == 0)
                    break;
            }

            var s = "";

            for (int j = 0; j < colcount; j++)
            {
                s += "<td style=\"";
                s += "width:10px;";
                s += "\">&nbsp;</td>";

                var leftLine = "border-left: 2px solid #ffffff;";

                if (j < colcount - 1 && i == SystemInfo.SystemData.Data.bodies.Count - 1)
                {
                    leftLine = "";
                }

                if (topLineHide[j] == 1)
                {
                    leftLine = "";
                }

                s += "<td style=\"";
                s += "width:10px;";
                s += leftLine;
                s += "\">&nbsp;</td>";
            }


            return new RawString(s);
        }

        public IEncodedString BodyTreeElement2(int i)
        {
            var parents = SystemInfo.SystemData.Data.bodies[i].parents;

            var colcount = parents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;
            var nextColcount = colcount;

            if (i < SystemInfo.SystemData.Data.bodies.Count - 1)
            {
                var nextParents = SystemInfo.SystemData.Data.bodies[i+1].parents;
                nextColcount = nextParents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;
            }

            int[] topLineHide = new int[100];

            for (int k = i + 1; k < SystemInfo.SystemData.Data.bodies.Count; k++)
            {
                var rowParents = SystemInfo.SystemData.Data.bodies[k].parents;
                var rowColcount = rowParents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;

                for (int j = 0; j < rowColcount; j++)
                {
                    if (topLineHide[j] != 2)
                    {
                        if (rowColcount > 0 && j < rowColcount - 1)
                        {
                            topLineHide[j] = 1;
                        }
                        else
                        {
                            topLineHide[j] = 2;
                        }
                    }
                }

                if (rowColcount == 0)
                    break;
            }

            var s = "";

            for (int j = 0; j < colcount; j++)
            {
                s += "<td style=\"";
                s += "width:10px;";
                s += "\">&nbsp;</td>";

                
                var topLine = "border-top: 2px solid #ffffff; ";
                if (colcount > 1 && j < colcount - 1)
                {
                    topLine = "";
                }

                var leftLine = "border-left: 2px solid #ffffff;";

                if (nextColcount == 0 || (j == colcount-1 && nextColcount < colcount) || i == SystemInfo.SystemData.Data.bodies.Count-1)
                {
                    leftLine = "";
                }

                if (topLineHide[j] == 1)
                {
                    leftLine = "";
                }


                s += "<td style=\"";
                s += "width:10px;";
                s += leftLine + topLine;
                s += "\">&nbsp;</td>";
            }

            return new RawString(s);
        }


        public IEncodedString BodyTreeElement3(int i)
        {
            var parents = SystemInfo.SystemData.Data.bodies[i].parents;

            var colcount = parents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;
            var nextColcount = colcount;

            if (i < SystemInfo.SystemData.Data.bodies.Count - 1)
            {
                var nextParents = SystemInfo.SystemData.Data.bodies[i + 1].parents;
                nextColcount = nextParents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;
            }

            int[] topLineHide = new int[100];

            for (int k = i + 1; k < SystemInfo.SystemData.Data.bodies.Count; k++)
            {
                var rowParents = SystemInfo.SystemData.Data.bodies[k].parents;
                var rowColcount = rowParents?.Where(x => !x.ContainsKey("Null")).ToList().Count ?? 0;

                for (int j = 0; j < rowColcount; j++)
                {
                    if (topLineHide[j] != 2)
                    {
                        if (rowColcount > 0 && j < rowColcount - 1)
                        {
                            topLineHide[j] = 1;
                        }
                        else
                        {
                            topLineHide[j] = 2;
                        }
                    }
                }

                if (rowColcount == 0)
                    break;
            }

            var s = "";

            for (int j = 0; j < colcount; j++)
            {
                s += "<td style=\"font-size: 3px; ";
                s += "width:10px;";
                s += "\">&nbsp;</td>";

                var leftLine = "border-left: 2px solid #ffffff;";

                if (nextColcount == 0 || (j == colcount - 1 && nextColcount < colcount) || i == SystemInfo.SystemData.Data.bodies.Count-1)
                {
                    leftLine = "";
                }

                if (topLineHide[j] == 1)
                {
                    leftLine = "";
                }

                s += "<td style=\"font-size: 3px; "; 
                s += "width:10px;";
                s += leftLine;
                s += "\">&nbsp;</td>";
            }

            return new RawString(s);
        }

        public IEncodedString SinceText(int agodec, DateTime updatedTime)
        {

            var ts = DateTime.Now - updatedTime.AddSeconds(-agodec);

            return new RawString(ts.ToHumanReadableString());
        }

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

    internal class FipPanel
    {

        private readonly object _refreshDevicePageLock = new object();

        private bool _initOk;

        public LcdTab CurrentTab = LcdTab.None;
        public int[] CurrentCard = new int[100];

        private LcdPage _currentPage = LcdPage.Collapsed;
        private LcdTab _currentTabCursor = LcdTab.None;
        private LcdTab _lastTab = LcdTab.Init;
        private const int DEFAULT_PAGE = 0;
        private string _settingsPath;

        private int[] _currentZoomLevel = new int[100];


        private DateTime _lastClockwiseScroll  = DateTime.Now;
        private DateTime _lastCounterClockwiseScroll = DateTime.Now;
        private int _ClockwiseScrollCount;
        private int _CounterClockwiseScrollCount;

        private int _fastScrollClickDelay = 250;
        private int _fastScrollClickCount = 4;
        private int _scrollIncrement = 50;
        private int _fastScrollIncrement = 200;

        
        private int _currentLcdYOffset;
        private int _currentLcdHeight;

        public IntPtr FipDevicePointer;
        public string SerialNumber;

        private uint _prevButtons;

        private bool[] _ledState = new bool[7];

        private List<uint> _pageList = new List<uint>();

        private readonly Pen _scrollPen = new Pen(Color.FromArgb(0xff,0xFF,0xB0,0x00));
        private readonly Pen _whitePen = new Pen(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF),(float)0.1);
        
        
        private readonly SolidBrush _scrollBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xB0, 0x00));
        private readonly SolidBrush _redBrush = new SolidBrush(Color.FromArgb(0xFF, 0x00, 0x00));
        private readonly SolidBrush _whiteBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF));

        private readonly Font _drawFont = new Font("Arial", 13, GraphicsUnit.Pixel);

        private Image _htmlImage;
        private Image _menuHtmlImage;
        private Image _cardcaptionHtmlImage;

        private const int HtmlMenuWindowWidth = 110; //69;
        private const int HtmlMenuWindowHeight = 259;
        private const int HtmlWindowXOffset = 1;

        private int _htmlWindowWidth = 320;
        private int _htmlWindowHeight = 240;

        private int HtmlWindowUsableWidth => _htmlWindowWidth - 9 - HtmlWindowXOffset;

        private double ScrollBarHeight => _htmlWindowHeight -7.0;

        private int GalaxyImageDisplayWidth => _htmlWindowWidth - 10;

        private int ChartImageDisplayWidth => _htmlWindowWidth - 25;

        private const int ChartImageDisplayHeight = 60;

        private int GalaxyImageDisplayHeight
        {
            get
            {
                double aspectRatio;

                if (_htmlWindowWidth >= _htmlWindowHeight)
                {
                    aspectRatio = History.GalaxyImageLHeight / History.GalaxyImageLWidth;
                }
                else
                {
                    aspectRatio = History.GalaxyImagePHeight / History.GalaxyImagePWidth;
                }

                return (int)(GalaxyImageDisplayWidth * aspectRatio);

            }
        }

        private string GalaxyImageFileName => _htmlWindowWidth >= _htmlWindowHeight ? "galaxyl.png" : "galaxyp.png";

        private int GalaxyImageMarginTop => (int)((_htmlWindowHeight - GalaxyImageDisplayHeight) / 2.0);

        private DirectOutputClass.PageCallback _pageCallbackDelegate;
        private DirectOutputClass.SoftButtonCallback _softButtonCallbackDelegate;

        private bool _blockNextUpState;

        private string _exePath;

        private string _autoActivateTarget;
        private DateTime _autoActivateTargetLastRefreshed = DateTime.Now;

        private string _autoActivateNavigation;
        private DateTime _autoActivateNavigationLastRefreshed = DateTime.Now;

        public FipPanel(IntPtr devicePtr) 
        {
            FipDevicePointer = devicePtr;
        }


        private void GetExePath()
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            _exePath = Path.GetDirectoryName(strExeFilePath);
        }


        private void InitFipPanelSerialNumber()
        {
            App.Log.Info("FipPanel Serial Number : " + SerialNumber);

            _settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            "\\mhwlng\\fip-elite\\" + SerialNumber;

            if (File.Exists(_settingsPath))
            {
                try
                {
                    CurrentTab = (LcdTab)uint.Parse(File.ReadAllText(_settingsPath));
                }
                catch
                {
                    CurrentTab = LcdTab.None;
                }
            }
            else
            {
                new FileInfo(_settingsPath).Directory?.Create();

                File.WriteAllText(_settingsPath, ((int)CurrentTab).ToString());
            }

            GetExePath();

            if (File.Exists(Path.Combine(_exePath, "panelSettings.config")) &&
                ConfigurationManager.GetSection("panelSettings") is NameValueCollection panelSection)
            {
                _autoActivateTarget = panelSection["AutoActivateTarget"];

                if (_autoActivateTarget?.ToLower() != SerialNumber.ToLower())
                {
                    _autoActivateTarget = string.Empty;
                }
                else
                {
                    App.Log.Info("FipPanel AutoActivateTarget : " + SerialNumber);
                    _autoActivateTargetLastRefreshed = DateTime.Now;
                }

                _autoActivateNavigation = panelSection["AutoActivateNavigation"];

                if (_autoActivateNavigation?.ToLower() != SerialNumber.ToLower())
                {
                    _autoActivateNavigation = string.Empty;
                }
                else
                {
                    App.Log.Info("FipPanel AutoActivateNavigation : " + SerialNumber);
                    _autoActivateNavigationLastRefreshed = DateTime.Now;
                }
            }
        }

        private void InitScrollConstants()
        {
            GetExePath();

            if (File.Exists(Path.Combine(_exePath, "panelSettings.config")) &&
                ConfigurationManager.GetSection("panelSettings") is NameValueCollection panelSection)
            {
                var fastScrollClickDelayString = panelSection["FastScrollClickDelay"];

                int.TryParse(fastScrollClickDelayString, out _fastScrollClickDelay);

                App.Log.Info("FipPanel FastScrollClickDelay : " + _fastScrollClickDelay);

                var fastScrollClickCountString = panelSection["FastScrollClickCount"];

                int.TryParse(fastScrollClickCountString, out _fastScrollClickCount);

                App.Log.Info("FipPanel FastScrollClickCount : " + _fastScrollClickCount);

                var scrollIncrementString = panelSection["ScrollIncrement"];

                int.TryParse(scrollIncrementString, out _scrollIncrement);

                App.Log.Info("FipPanel ScrollIncrement : " + _scrollIncrement);

                var fastScrollIncrementString = panelSection["FastScrollIncrement"];

                int.TryParse(fastScrollIncrementString, out _fastScrollIncrement);

                App.Log.Info("FipPanel FastScrollIncrement : " + _fastScrollIncrement);




            }
        }


        public void Initalize()
        {
            // FIP = 3e083cd8-6a37-4a58-80a8-3d6a2c07513e

            // https://github.com/Raptor007/Falcon4toSaitek/blob/master/Raptor007's%20Falcon%204%20to%20Saitek%20Utility/DirectOutput.h
            //https://github.com/poiuqwer78/fip4j-core/tree/master/src/main/java/ch/poiuqwer/saitek/fip4j


            _pageCallbackDelegate = PageCallback;
            _softButtonCallbackDelegate = SoftButtonCallback;

            var returnValues1 = DirectOutputClass.RegisterPageCallback(FipDevicePointer, _pageCallbackDelegate);
            if (returnValues1 != ReturnValues.S_OK)
            {
                App.Log.Error("FipPanel failed to init RegisterPageCallback. " + returnValues1);
            }
            var returnValues2 = DirectOutputClass.RegisterSoftButtonCallback(FipDevicePointer, _softButtonCallbackDelegate);
            if (returnValues2 != ReturnValues.S_OK)
            {
                App.Log.Error("FipPanel failed to init RegisterSoftButtonCallback. " + returnValues1);
            }

            var returnValues3 = DirectOutputClass.GetSerialNumber(FipDevicePointer, out SerialNumber);
            if (returnValues3 != ReturnValues.S_OK)
            {
                App.Log.Error("FipPanel failed to get Serial Number. " + returnValues1);
            }
            else
            {

                InitFipPanelSerialNumber();

                InitScrollConstants();

                _initOk = true;

                AddPage(DEFAULT_PAGE, true);

                RefreshDevicePage();

                
            }

        }

        public void InitalizeWindow(string serialNumber, int windowWidth, int windowHeight)
        {
            SerialNumber = serialNumber;
            _initOk = false;
            _htmlWindowWidth = windowWidth;
            _htmlWindowHeight = windowHeight;


            InitFipPanelSerialNumber();

            InitScrollConstants();

            RefreshDevicePage();

        }

        public void Shutdown()
        {
            try
            {
                if (_pageList.Count > 0)
                {
                    do
                    {
                        if (_initOk)
                        {
                            DirectOutputClass.RemovePage(FipDevicePointer, _pageList[0]);
                        }

                        _pageList.Remove(_pageList[0]);


                    } while (_pageList.Count > 0);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

        }

        private bool SetTab(LcdTab tab)
        {
            if (CurrentTab != tab)
            {
                _lastTab = CurrentTab;
                CurrentTab = tab;


                _currentLcdYOffset = 0;

                File.WriteAllText(_settingsPath, ((int)CurrentTab).ToString());
            }

            _currentPage = LcdPage.Collapsed;
            _currentTabCursor = LcdTab.None;

            App.PlayClickSound();

            return true;
        }

        private void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context)
        {
            if (device == FipDevicePointer)
            {
                if (bActivated != 0)
                {
                    RefreshDevicePage();
                }
            }
        }

        private uint CalculateButton (LcdTab tab)
        {
            var currentPage = ((int)tab - 1) / 6;

            return (uint)(1 << ((int)tab + 4 - currentPage * 6));
        }

        public void HandleJoystickButton(JoystickButton joystickButton, bool state, bool oldState)
        {
            uint buttons = 0;

            var currentPage = (LcdPage)(((uint)CurrentTab - 1) / 6);
            if (CurrentTab == LcdTab.None)
            {
                currentPage = LcdPage.HomeMenu;
            }

            if (state)
            {
                switch (joystickButton)
                {
                    case JoystickButton.Navigation:
                        if (oldState) return;

                        _currentPage = LcdPage.HomeMenu;

                        buttons = CalculateButton(LcdTab.Navigation);
                        break;
                    case JoystickButton.Target:
                        if (oldState) return;

                        _currentPage = LcdPage.HomeMenu;

                        buttons = CalculateButton(LcdTab.Target);
                        break;
                    case JoystickButton.Commander:
                        if (oldState) return;

                        _currentPage = LcdPage.InfoMenu;

                        buttons = CalculateButton(LcdTab.Commander);
                        break;
                    case JoystickButton.Galnet:
                        if (oldState) return;

                        _currentPage = LcdPage.InfoMenu;

                        buttons = CalculateButton(LcdTab.Galnet);
                        break;
                    case JoystickButton.Missions:
                        if (oldState) return;

                        _currentPage = LcdPage.InfoMenu;

                        buttons = CalculateButton(LcdTab.Missions);
                        break;
                    case JoystickButton.Chat:
                        if (oldState) return;

                        _currentPage = LcdPage.InfoMenu;

                        buttons = CalculateButton(LcdTab.Chat);
                        break;
                    case JoystickButton.HWInfo:
                        if (oldState) return;

                        _currentPage = LcdPage.InfoMenu;

                        buttons = CalculateButton(LcdTab.HWInfo);
                        break;
                    case JoystickButton.Ship:
                        if (oldState) return;

                        _currentPage = LcdPage.ShipMenu;

                        buttons = CalculateButton(LcdTab.Ship);
                        break;
                    case JoystickButton.Materials:
                        if (oldState) return;

                        _currentPage = LcdPage.ShipMenu;

                        buttons = CalculateButton(LcdTab.Materials);
                        break;
                    case JoystickButton.Cargo:
                        if (oldState) return;

                        _currentPage = LcdPage.ShipMenu;

                        buttons = CalculateButton(LcdTab.Cargo);
                        break;
                    case JoystickButton.Engineer:
                        if (oldState) return;

                        _currentPage = LcdPage.ShipMenu;

                        buttons = CalculateButton(LcdTab.Engineer);
                        break;
                    case JoystickButton.ShipLocker:
                        if (oldState) return;

                        _currentPage = LcdPage.SuitMenu;

                        buttons = CalculateButton(LcdTab.ShipLocker);
                        break;
                    case JoystickButton.BackPack:
                        if (oldState) return;

                        _currentPage = LcdPage.SuitMenu;

                        buttons = CalculateButton(LcdTab.BackPack);
                        break;
                    case JoystickButton.POI:
                        if (oldState) return;

                        _currentPage = LcdPage.LocationsMenu;

                        buttons = CalculateButton(LcdTab.POI);
                        break;
                    case JoystickButton.Galaxy:
                        if (oldState) return;

                        _currentPage = LcdPage.LocationsMenu;

                        buttons = CalculateButton(LcdTab.Galaxy);
                        break;
                    case JoystickButton.Engineers:
                        if (oldState) return;

                        _currentPage = LcdPage.LocationsMenu;

                        buttons = CalculateButton(LcdTab.Engineers);
                        break;
                    case JoystickButton.Powers:
                        if (oldState) return;

                        _currentPage = LcdPage.LocationsMenu;

                        buttons = CalculateButton(LcdTab.Powers);
                        break;
                    case JoystickButton.Mining:
                        if (oldState) return;

                        _currentPage = LcdPage.LocationsMenu;

                        buttons = CalculateButton(LcdTab.Mining);
                        break;
                    case JoystickButton.Up:
                        if (_currentPage == LcdPage.Collapsed)
                        {
                            buttons = 4;
                        }
                        else
                        {
                            if (oldState) return;

                            if (_currentTabCursor == LcdTab.None)
                            {
                                _currentTabCursor = (LcdTab)((int)currentPage * 6) + 1;
                            }

                            switch (_currentTabCursor)
                            {
                                case LcdTab.ShipBack:
                                    //Engineer >  Cargo > Materials > Ship >  ShipBack

                                    _currentTabCursor = LcdTab.Engineer;

                                    if (string.IsNullOrEmpty(Engineer.CommanderName))
                                    {
                                        _currentTabCursor -= 1;
                                    }

                                    break;

                                case LcdTab.Navigation:
                                    _currentTabCursor = LcdTab.LocationsMenu;
                                    break;
                                case LcdTab.InfoMenu:
                                    _currentTabCursor = LcdTab.Target;
                                    break;

                                case LcdTab.InfoBack:
                                    _currentTabCursor = LcdTab.HWInfo;

                                    if (!HWInfo.SensorData.Any())
                                    {
                                        _currentTabCursor -= 1;
                                    }
                                    break;
                                case LcdTab.LocationsBack:
                                    _currentTabCursor = LcdTab.Mining;
                                    break;

                                case LcdTab.SuitBack:
                                    _currentTabCursor = LcdTab.BackPack;
                                    break;

                                default:
                                    _currentTabCursor -= 1;
                                    break;
                            }

                            buttons = 2048;
                        }
                        break;
                    case JoystickButton.Down:
                        if (_currentPage == LcdPage.Collapsed)
                        {
                            buttons = 2;
                        }
                        else
                        {
                            if (oldState) return;

                            if (_currentTabCursor == LcdTab.None)
                            {
                                _currentTabCursor = (LcdTab)((int)currentPage * 6) + 1;
                            }

                            switch (_currentTabCursor)
                            {
                                case LcdTab.Engineer:
                                    _currentTabCursor = LcdTab.ShipBack;
                                    break;

                                case LcdTab.Cargo:
                                    //ShipBack > Ship > Materials > Cargo > Engineer

                                    if (!string.IsNullOrEmpty(Engineer.CommanderName))
                                        _currentTabCursor = LcdTab.Engineer;
                                    else
                                        _currentTabCursor = LcdTab.ShipBack;
                                    break;

                                case LcdTab.LocationsMenu:
                                    _currentTabCursor = LcdTab.Navigation;
                                    break;
                                case LcdTab.Target:
                                    _currentTabCursor = LcdTab.InfoMenu;
                                    break;

                                case LcdTab.HWInfo:
                                    _currentTabCursor = LcdTab.InfoBack;
                                    break;

                                case LcdTab.Chat:
                                    if (HWInfo.SensorData.Any())
                                        _currentTabCursor = LcdTab.HWInfo;
                                    else
                                        _currentTabCursor = LcdTab.InfoBack;
                                    break;
                                case LcdTab.Mining:
                                    _currentTabCursor = LcdTab.LocationsBack;
                                    break;

                                case LcdTab.BackPack:
                                    _currentTabCursor = LcdTab.SuitBack;
                                    break;

                                default:
                                    _currentTabCursor += 1;
                                    break;
                            }

                            buttons = 2048; // refresh
                        }
                        break;
                    case JoystickButton.Left:

                        if (_currentPage == LcdPage.Collapsed)
                        {
                            if (CurrentTab != LcdTab.Galaxy && oldState) return;

                            buttons = 16;
                        }
                        else
                        {
                            if (oldState) return;

                            if (_currentTabCursor == LcdTab.None)
                            {
                                _currentTabCursor = (LcdTab)((int)currentPage * 6) + 1;
                            }

                            switch (((int)_currentTabCursor - 1) / 6)
                            {
                                case (int)LcdPage.InfoMenu:
                                    buttons = 32; // back
                                    _currentTabCursor = LcdTab.InfoMenu;
                                    break;
                                case (int)LcdPage.ShipMenu:
                                    buttons = 32; // back
                                    _currentTabCursor = LcdTab.ShipMenu;
                                    break;
                                case (int)LcdPage.SuitMenu:
                                    buttons = 32; // back
                                    _currentTabCursor = LcdTab.PersonMenu;
                                    break;
                                case (int)LcdPage.LocationsMenu:
                                    buttons = 32; // back
                                    _currentTabCursor = LcdTab.LocationsMenu;
                                    break;

                            }
                        }
                        break;
                    case JoystickButton.Right:

                        if (_currentPage == LcdPage.Collapsed)
                        {
                            if (CurrentTab != LcdTab.Galaxy && oldState) return;

                            buttons = 8;
                        }
                        else
                        {
                            if (oldState) return;

                            if (_currentTabCursor == LcdTab.None)
                            {
                                _currentTabCursor = (LcdTab)((int)currentPage * 6) + 1;
                            }

                            switch (_currentTabCursor)
                            {
                                case LcdTab.InfoMenu:
                                    buttons = CalculateButton(_currentTabCursor);
                                    _currentTabCursor = LcdTab.InfoBack;
                                    break;

                                case LcdTab.ShipMenu:
                                    buttons = CalculateButton(_currentTabCursor);
                                    _currentTabCursor = LcdTab.ShipBack;
                                    break;
                                case LcdTab.PersonMenu:
                                    buttons = CalculateButton(_currentTabCursor);
                                    _currentTabCursor = LcdTab.SuitBack;
                                    break;
                                case LcdTab.LocationsMenu:
                                    buttons = CalculateButton(_currentTabCursor);
                                    _currentTabCursor = LcdTab.LocationsBack;
                                    break;
                            }
                        }
                        break;
                    case JoystickButton.Push:

                        if (oldState) return;

                        if (_currentPage == LcdPage.Collapsed)
                        {
                            if (_currentTabCursor == LcdTab.None)
                            {
                                if (CurrentTab != LcdTab.None)
                                {
                                    _currentTabCursor = CurrentTab;
                                }
                                else
                                {
                                    _currentTabCursor = (LcdTab)((int)currentPage * 6) + 1;
                                }
                            }

                            buttons = 32;
                        }
                        else
                        {
                            if (_currentTabCursor == LcdTab.None)
                            {
                                _currentTabCursor = (LcdTab)((int)currentPage * 6) + 1;
                            }

                            buttons = CalculateButton(_currentTabCursor);

                            switch (_currentTabCursor)
                            {
                                case LcdTab.InfoMenu:
                                    _currentTabCursor = LcdTab.InfoBack;
                                    break;
                                case LcdTab.InfoBack:
                                    _currentTabCursor = LcdTab.InfoMenu;
                                    break;

                                case LcdTab.ShipMenu:
                                    _currentTabCursor = LcdTab.ShipBack;
                                    break;
                                case LcdTab.ShipBack:
                                    _currentTabCursor = LcdTab.ShipMenu;
                                    break;

                                case LcdTab.PersonMenu:
                                    _currentTabCursor = LcdTab.SuitBack;
                                    break;
                                case LcdTab.SuitBack:
                                    _currentTabCursor = LcdTab.PersonMenu;
                                    break;

                                case LcdTab.LocationsMenu:
                                    _currentTabCursor = LcdTab.LocationsBack;
                                    break;
                                case LcdTab.LocationsBack:
                                    _currentTabCursor = LcdTab.LocationsMenu;
                                    break;
                            }
                        }

                        break;
                }
            }

            SoftButtonCallback(FipDevicePointer, (IntPtr) buttons, (IntPtr)null);

            if (!_initOk)
            {
                Thread.Sleep(30);
            }
        }


        private void HandleCardNavigationLeds()
        {
            var ledRight = false;
            var ledLeft = false;

            if (CurrentTab == LcdTab.POI || CurrentTab == LcdTab.Powers ||
                CurrentTab == LcdTab.Engineers || CurrentTab == LcdTab.ShipLocker ||
                CurrentTab == LcdTab.BackPack || CurrentTab == LcdTab.Chat ||
                CurrentTab == LcdTab.Materials || CurrentTab == LcdTab.Galaxy ||
                CurrentTab == LcdTab.Ship || CurrentTab == LcdTab.Mining ||
                CurrentTab == LcdTab.Navigation || CurrentTab == LcdTab.Engineer ||
                CurrentTab == LcdTab.Galnet || CurrentTab == LcdTab.HWInfo)
            {
                ledLeft = true;
                ledRight = true;

                if (CurrentTab == LcdTab.Galnet)
                {
                    var currentCard = CurrentCard[(int) CurrentTab];

                    var galNetCount = Galnet.GalnetList?.Count ?? 0;

                    ledLeft = currentCard > 0;

                    ledRight = currentCard < galNetCount - 1;
                }
                else if (CurrentTab == LcdTab.Galaxy)
                {
                    var currentZoomLevel = _currentZoomLevel[(int)CurrentTab];

                    ledLeft = currentZoomLevel > 0;

                    ledRight = currentZoomLevel < 15;

                }

            }

            SetLed(5, ledRight);
            SetLed(6, ledLeft);

        }

        private void SoftButtonCallback(IntPtr device, IntPtr buttons, IntPtr context)
        {
            if (device == FipDevicePointer & (uint) buttons != _prevButtons)
            {
                var button = (uint) buttons ^ _prevButtons;
                var state = ((uint) buttons & button) == button;
                _prevButtons = (uint) buttons;

                //Console.WriteLine($"button {button}  state {state}");

                var mustRefresh = false;

                var mustRender = true;

                switch (button)
                {
                    case 8: // scroll clockwise
                        if (state && (CurrentTab == LcdTab.POI || CurrentTab == LcdTab.Powers ||
                                      CurrentTab == LcdTab.Engineers || CurrentTab == LcdTab.ShipLocker ||
                                      CurrentTab == LcdTab.BackPack || CurrentTab == LcdTab.Chat ||
                                      CurrentTab == LcdTab.Materials || CurrentTab == LcdTab.Galaxy ||
                                      CurrentTab == LcdTab.Ship || CurrentTab == LcdTab.Mining || 
                                      CurrentTab == LcdTab.Navigation || CurrentTab == LcdTab.Engineer || 
                                      CurrentTab == LcdTab.Galnet || CurrentTab == LcdTab.HWInfo))
                        {

                            CurrentCard[(int) CurrentTab]++;
                            _currentZoomLevel[(int) CurrentTab]++;
                            _currentLcdYOffset = 0;

                            mustRefresh = true;

                            var playSound = true;
                            if (CurrentTab == LcdTab.Galnet)
                            {
                                var currentCard = CurrentCard[(int)CurrentTab];

                                var galNetCount = Galnet.GalnetList?.Count ?? 0;

                                playSound = currentCard < galNetCount;
                            }
                            else if (CurrentTab == LcdTab.Galaxy)
                            {
                                playSound = _currentZoomLevel[(int) CurrentTab] < 16;
                            }


                            if (playSound)
                            {
                                App.PlayClickSound();
                            }
                        }

                        break;
                    case 16: // scroll anti-clockwise

                        if (state && (CurrentTab == LcdTab.POI || CurrentTab == LcdTab.Powers ||
                                      CurrentTab == LcdTab.Engineers || CurrentTab == LcdTab.ShipLocker ||
                                      CurrentTab == LcdTab.BackPack || CurrentTab == LcdTab.Chat ||
                                      CurrentTab == LcdTab.Materials || CurrentTab == LcdTab.Galaxy ||
                                      CurrentTab == LcdTab.Ship || CurrentTab == LcdTab.Mining ||
                                      CurrentTab == LcdTab.Navigation || CurrentTab == LcdTab.Engineer || 
                                      CurrentTab == LcdTab.Galnet || CurrentTab == LcdTab.HWInfo))
                        {
                            CurrentCard[(int) CurrentTab]--;
                            _currentZoomLevel[(int) CurrentTab]--;
                            _currentLcdYOffset = 0;

                            mustRefresh = true;

                            var playSound = true;
                            if (CurrentTab == LcdTab.Galnet)
                            {
                                playSound = CurrentCard[(int)CurrentTab] >= 0;
                            }
                            else if (CurrentTab == LcdTab.Galaxy)
                            {
                                playSound = _currentZoomLevel[(int)CurrentTab] >= 0;
                            }

                            if (playSound)
                            {
                                App.PlayClickSound();
                            }
                        }

                        break;
                    case 2: // scroll clockwise
                        if (state && CurrentTab != LcdTab.Galaxy)
                        {
                            _lastCounterClockwiseScroll = DateTime.Now;
                            _CounterClockwiseScrollCount = 0;

                            if ((DateTime.Now - _lastClockwiseScroll).Milliseconds > _fastScrollClickDelay)
                            {
                                _ClockwiseScrollCount = 0;
                            }
                            else
                            {
                                _ClockwiseScrollCount ++;
                            }

                            _lastClockwiseScroll = DateTime.Now;

                            if (_ClockwiseScrollCount > _fastScrollClickCount)
                            {
                                _currentLcdYOffset += _fastScrollIncrement;
                            }
                            else
                            {
                                _currentLcdYOffset += _scrollIncrement;
                            }

                            mustRender = false;

                            mustRefresh = true;
                        }

                        break;
                    case 4: // scroll anti-clockwise

                        if (_currentLcdYOffset == 0) return;

                        if (state && CurrentTab != LcdTab.Galaxy)
                        {
                            _lastClockwiseScroll = DateTime.Now;
                            _ClockwiseScrollCount = 0;

                            if ((DateTime.Now - _lastCounterClockwiseScroll).Milliseconds > _fastScrollClickDelay)
                            {
                                _CounterClockwiseScrollCount = 0;
                            }
                            else
                            {
                                _CounterClockwiseScrollCount++;
                            }

                            _lastCounterClockwiseScroll = DateTime.Now;

                            if (_CounterClockwiseScrollCount > _fastScrollClickCount)
                            {
                                _currentLcdYOffset -= _fastScrollIncrement;
                            }
                            else
                            {
                                _currentLcdYOffset -= _scrollIncrement;
                            }

                            if (_currentLcdYOffset < 0)
                            {
                                _currentLcdYOffset = 0;
                            }

                            mustRender = false;

                            mustRefresh = true;
                        }

                        break;
                }

                if (!mustRefresh)
                {

                    switch (_currentPage)
                    {
                        case LcdPage.Collapsed:
                            if (state || !_blockNextUpState)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = (LcdPage) (((uint) CurrentTab - 1) / 6);
                                        if (CurrentTab == LcdTab.None)
                                        {
                                            _currentPage = LcdPage.HomeMenu;
                                        }

                                        _lastTab = LcdTab.Init;

                                        App.PlayClickSound();

                                        break;

                                    case 512:

                                        if (CurrentTab == LcdTab.POI || CurrentTab == LcdTab.Powers ||
                                            CurrentTab == LcdTab.Engineers || CurrentTab == LcdTab.ShipLocker ||
                                            CurrentTab == LcdTab.BackPack || CurrentTab == LcdTab.Chat ||
                                            CurrentTab == LcdTab.Materials || CurrentTab == LcdTab.Galaxy ||
                                            CurrentTab == LcdTab.Ship || CurrentTab == LcdTab.Mining ||
                                            CurrentTab == LcdTab.Navigation || CurrentTab == LcdTab.Engineer ||
                                            CurrentTab == LcdTab.Galnet || CurrentTab == LcdTab.HWInfo)
                                        {
                                            CurrentCard[(int)CurrentTab]++;
                                            _currentZoomLevel[(int)CurrentTab]++;
                                            _currentLcdYOffset = 0;

                                            mustRefresh = true;

                                            var playSound = true;
                                            if (CurrentTab == LcdTab.Galnet)
                                            {
                                                var currentCard = CurrentCard[(int)CurrentTab];

                                                var galNetCount = Galnet.GalnetList?.Count ?? 0;

                                                playSound = currentCard < galNetCount;
                                            }
                                            else if (CurrentTab == LcdTab.Galaxy)
                                            {
                                                playSound = _currentZoomLevel[(int)CurrentTab] < 16;
                                            }

                                            if (playSound)
                                            {
                                                App.PlayClickSound();
                                            }
                                        }

                                        break;
                                    case 1024:

                                        if (CurrentTab == LcdTab.POI || CurrentTab == LcdTab.Powers ||
                                            CurrentTab == LcdTab.Engineers || CurrentTab == LcdTab.ShipLocker ||
                                            CurrentTab == LcdTab.BackPack || CurrentTab == LcdTab.Chat ||
                                            CurrentTab == LcdTab.Materials || CurrentTab == LcdTab.Galaxy ||
                                            CurrentTab == LcdTab.Ship || CurrentTab == LcdTab.Mining ||
                                            CurrentTab == LcdTab.Navigation || CurrentTab == LcdTab.Engineer ||
                                            CurrentTab == LcdTab.Galnet || CurrentTab == LcdTab.HWInfo)
                                        {
                                            CurrentCard[(int)CurrentTab]--;
                                            _currentZoomLevel[(int)CurrentTab]--;
                                            _currentLcdYOffset = 0;

                                            mustRefresh = true;

                                            var playSound = true;
                                            if (CurrentTab == LcdTab.Galnet)
                                            {
                                                playSound = CurrentCard[(int)CurrentTab] >= 0;
                                            }
                                            else if (CurrentTab == LcdTab.Galaxy)
                                            {
                                                playSound = _currentZoomLevel[(int)CurrentTab] >= 0;

                                            }

                                            if (playSound)
                                            {
                                                App.PlayClickSound();
                                            }
                                        }

                                        break;
                                }
                            }

                            break;
                        case LcdPage.HomeMenu:
                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = SetTab(LcdTab.Navigation);
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LcdTab.Target);
                                        break;
                                    case 128:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.InfoMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 256:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.ShipMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 512:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.SuitMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 1024:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.LocationsMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        App.PlayClickSound();
                                        break;
                                }
                            }

                            break;

                        case LcdPage.InfoMenu:
                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.HomeMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LcdTab.Commander);
                                        break;
                                    case 128:
                                        mustRefresh = SetTab(LcdTab.Galnet);
                                        break;
                                    case 256:
                                        mustRefresh = SetTab(LcdTab.Missions);
                                        break;
                                    case 512:
                                        mustRefresh = SetTab(LcdTab.Chat);
                                        break;
                                    case 1024:
                                        if (HWInfo.SensorData.Any())
                                        {
                                            mustRefresh = SetTab(LcdTab.HWInfo);
                                        }
                                        else
                                        {
                                            mustRefresh = true;
                                            CurrentTab = LcdTab.None;
                                            _lastTab = LcdTab.Init;
                                            _currentTabCursor = LcdTab.None;
                                            _currentPage = LcdPage.Collapsed;
                                        }
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        App.PlayClickSound();
                                        break;
                                }
                            }

                            break;

                        case LcdPage.ShipMenu:
                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.HomeMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LcdTab.Ship);
                                        break;
                                    case 128:
                                        mustRefresh = SetTab(LcdTab.Materials);
                                        break;
                                    case 256:
                                        mustRefresh = SetTab(LcdTab.Cargo);
                                        break;
                                    case 512:
                                        if (!string.IsNullOrEmpty(Engineer.CommanderName))
                                        {
                                            Engineer.GetShoppingList();

                                            Engineer.GetBestSystems();

                                            mustRefresh = SetTab(LcdTab.Engineer);
                                        }
                                        else
                                        {
                                            mustRefresh = true;
                                            CurrentTab = LcdTab.None;
                                            _lastTab = LcdTab.Init;
                                            _currentTabCursor = LcdTab.None;
                                            _currentPage = LcdPage.Collapsed;
                                        }
                                        break;
                                    case 1024:
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        App.PlayClickSound();
                                        break;
                                }
                            }

                            break;

                        case LcdPage.SuitMenu:
                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.HomeMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LcdTab.ShipLocker);
                                        break;
                                    case 128:
                                        mustRefresh = SetTab(LcdTab.BackPack);
                                        break;
                                    case 256:
                                        break;
                                    case 512:
                                        break;
                                    case 1024:
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        App.PlayClickSound();
                                        break;
                                }
                            }

                            break;
                        case LcdPage.LocationsMenu:

                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = LcdPage.HomeMenu;
                                        _lastTab = LcdTab.Init;
                                        App.PlayClickSound();
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LcdTab.POI);
                                        break;
                                    case 128:
                                        mustRefresh = SetTab(LcdTab.Galaxy);
                                        break;
                                    case 256:
                                        mustRefresh = SetTab(LcdTab.Engineers);
                                        break;
                                    case 512:
                                        mustRefresh = SetTab(LcdTab.Powers);
                                        break;
                                    case 1024:
                                        mustRefresh = SetTab(LcdTab.Mining);
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        App.PlayClickSound();
                                        break;
                                }
                            }

                            break;
                    }
                }

                _blockNextUpState = state;

                if (mustRefresh)
                {
                    RefreshDevicePage(mustRender);
                }

            }
        }

        private void CheckLcdOffset()
        {
            if (_currentLcdHeight <= _htmlWindowHeight)
            {
                _currentLcdYOffset = 0;
            }

            if (_currentLcdYOffset + _htmlWindowHeight > _currentLcdHeight )
            {
                _currentLcdYOffset = _currentLcdHeight - _htmlWindowHeight + 4;
            }

            if (_currentLcdYOffset < 0) _currentLcdYOffset = 0;
        }

        private ReturnValues AddPage(uint pageNumber, bool setActive)
        {
            var result = ReturnValues.E_FAIL;

            if (_initOk)
            {
                try
                {
                    if (_pageList.Contains(pageNumber))
                    {
                        return ReturnValues.S_OK;
                    }

                    result = DirectOutputClass.AddPage(FipDevicePointer, (IntPtr) pageNumber, string.Concat("0x", FipDevicePointer.ToString(), " PageNo: ", pageNumber), setActive);
                    if (result == ReturnValues.S_OK)
                    {
                        App.Log.Info("Page: " + pageNumber + " added");

                        _pageList.Add(pageNumber);
                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex);
                }
            }

            return result;
        }

        private ReturnValues SendImageToFip(uint page, Bitmap fipImage)
        {

            if (_initOk)
            {
                if (fipImage == null)
                {
                    return ReturnValues.E_INVALIDARG;
                }

                try
                {
                    fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);

                    var bitmapData =
                        fipImage.LockBits(new Rectangle(0, 0, fipImage.Width, fipImage.Height),
                            ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    var intPtr = bitmapData.Scan0;
                    var local3 = bitmapData.Stride * fipImage.Height;
                    DirectOutputClass.SetImage(FipDevicePointer, page, 0, local3, intPtr);
                    fipImage.UnlockBits(bitmapData);
                    return ReturnValues.S_OK;
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex);
                }
            }

            return ReturnValues.E_FAIL;
        }
        
        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            var markerWidth = 40.0;
            var markerHeight = 40.0;

            try
            {
                if (CurrentTab == LcdTab.HWInfo)
                {
                    var image = new Bitmap(ChartImageDisplayWidth, ChartImageDisplayHeight);

                    using (var graphics = Graphics.FromImage(image))
                    {
                        if (HWInfo.SensorTrends.ContainsKey(e.Src))
                        {
                            graphics.DrawLines(_scrollPen, HWInfo.SensorTrends[e.Src].Read(ChartImageDisplayWidth, ChartImageDisplayHeight));
                        }

                        graphics.DrawRectangle(_whitePen,
                            new Rectangle(0, 0, ChartImageDisplayWidth - 1, ChartImageDisplayHeight - 1));

                        graphics.DrawString(HWInfo.SensorTrends[e.Src].MaxV(), _drawFont, _whiteBrush, (float)1, (float)1);


                        graphics.DrawString(HWInfo.SensorTrends[e.Src].MinV(), _drawFont, _whiteBrush, (float)1, (float)ChartImageDisplayHeight-17);


                    }

                    e.Callback(image);

                }
                else
                {
                    var image = Image.FromFile(Path.Combine(App.ExePath, "Templates\\images\\") + e.Src);

                    using (var graphics = Graphics.FromImage(image))
                    {
                        if (e.Src.ToLower().StartsWith("galaxy") && CurrentTab == LcdTab.Galaxy)
                        {
                            if (e.Src.ToLower().StartsWith("galaxyl"))
                            {
                                graphics.DrawPolygon(_whitePen, History.TravelHistoryPointsL.ToArray());
                            }
                            else
                            {
                                graphics.DrawPolygon(_whitePen, History.TravelHistoryPointsP.ToArray());
                            }

                            var zoomLevel = _currentZoomLevel[(int) CurrentTab] / 2.0 + 1;

                            if (zoomLevel > 1)
                            {
                                markerWidth /= zoomLevel;
                                markerHeight /= zoomLevel;
                            }

                            var zoomedXCenter = image.Width / 2.0;
                            var zoomedYCenter = image.Height / 2.0;

                            if (Data.LocationData?.StarPos?.Count == 3)
                            {
                                var spaceX = Data.LocationData.StarPos[0];
                                var spaceZ = Data.LocationData.StarPos[2];

                                var imgX = 0.0;
                                var imgY = 0.0;

                                if (e.Src.ToLower().StartsWith("galaxyl"))
                                {
                                    imgX = (spaceX - History.SpaceMinXL) / (History.SpaceMaxXL - History.SpaceMinXL) *
                                           image.Width;
                                    imgY = (History.SpaceMaxZL - spaceZ) / (History.SpaceMaxZL - History.SpaceMinZL) *
                                           image.Height;
                                }
                                else
                                {
                                    imgX = (spaceX - History.SpaceMinXP) / (History.SpaceMaxXP - History.SpaceMinXP) *
                                           image.Width;
                                    imgY = (History.SpaceMaxZP - spaceZ) / (History.SpaceMaxZP - History.SpaceMinZP) *
                                           image.Height;
                                }

                                imgX -= markerWidth / 2.0;
                                imgY -= markerHeight / 2.0;

                                graphics.FillEllipse(_redBrush, (float) imgX, (float) imgY, (float) markerWidth,
                                    (float) markerWidth);

                                zoomedXCenter = imgX;
                                zoomedYCenter = imgY;

                            }

                            if (zoomLevel > 1)
                            {
                                var zoomedWidth = image.Width / zoomLevel;
                                var zoomedHeight = image.Height / zoomLevel;

                                var zoomedXTopLeft = zoomedXCenter - zoomedWidth / 2.0;
                                var zoomedYTopLeft = zoomedYCenter - zoomedHeight / 2.0;

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
            }
            catch
            {
                var image = new Bitmap(1, 1);

                e.Callback(image);
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap src)
        {
            var ms = new MemoryStream();
            src.Save(ms, ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void RefreshMirrorWindow(Bitmap fipImage)
        {
            if (string.IsNullOrEmpty(App.FipSerialNumber) || SerialNumber == App.FipSerialNumber)
            {
                try
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action<Bitmap>(bitmap =>
                    {
                        var owner = System.Windows.Application.Current.MainWindow;

                        if (owner != null && owner.Visibility == System.Windows.Visibility.Visible)
                        {
                            var im = (System.Windows.Controls.Image)owner.FindName("im");

                            if (im != null)
                            {
                                im.Source = ConvertBitmapToBitmapImage(bitmap);
                            }
                        }
                    }), fipImage);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void SetLed(uint ledNumber, bool state)
        {
            if (_ledState[ledNumber] != state)
            {
                DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                    ledNumber, state);
                _ledState[ledNumber] = state;
            }
        }

        public void CheckCardSelectionLimits(int limit)
        {
            if (CurrentCard[(int)CurrentTab] < 0)
            {
                CurrentCard[(int)CurrentTab] = limit;
            }
            else
            if (CurrentCard[(int)CurrentTab] > limit)
            {
                CurrentCard[(int)CurrentTab] = 0;
            }
        }

        public void RefreshDevicePage(bool mustRender = true)
        {
            if (CurrentTab == LcdTab.Navigation && CurrentCard[(int)CurrentTab] == 2 && !string.IsNullOrEmpty(Data.LocationData.StarSystem))
            {
                SystemInfo.GetSystemData(Data.LocationData.StarSystem);
            }

            lock (_refreshDevicePageLock)
            {
                   
                if (Engineer.BlueprintShoppingList.Count == 0 && CurrentTab == LcdTab.Engineer)
                {
                    SetTab(LcdTab.Ship);
                }

                if (!string.IsNullOrEmpty(_autoActivateTarget) && Data.TargetData.Refreshed > _autoActivateTargetLastRefreshed)
                {
                    SetTab(LcdTab.Target);
                    _autoActivateTargetLastRefreshed = Data.TargetData.Refreshed;
                }

                if (!string.IsNullOrEmpty(_autoActivateNavigation) && Data.LocationData.Refreshed > _autoActivateNavigationLastRefreshed)
                {
                    SetTab(LcdTab.Navigation);
                    _autoActivateNavigationLastRefreshed = Data.LocationData.Refreshed;
                }

                using (var fipImage = new Bitmap(_htmlWindowWidth, _htmlWindowHeight))
                {
                    using (var graphics = Graphics.FromImage(fipImage))
                    {
                        var str = "";

                        switch (CurrentTab)
                        {
                            case LcdTab.Ship:
                                CheckCardSelectionLimits(2);
                                break;
                            case LcdTab.Navigation:
                                CheckCardSelectionLimits(4);
                                break;
                            case LcdTab.POI:
                                CheckCardSelectionLimits(7);
                                break;
                            case LcdTab.Engineer:
                                CheckCardSelectionLimits(2);
                                break;
                            case LcdTab.Powers:
                                CheckCardSelectionLimits(10);
                                break;
                            case LcdTab.Engineers:
                                CheckCardSelectionLimits(Data.EngineersList.Count-1);
                                break;
                            case LcdTab.Materials:
                                CheckCardSelectionLimits(2);
                                break;
                            case LcdTab.Mining:
                                CheckCardSelectionLimits(7);
                                break;
                            case LcdTab.HWInfo:
                                CheckCardSelectionLimits(1);
                                break;
                            case LcdTab.ShipLocker:
                                CheckCardSelectionLimits(3);
                                break;
                            case LcdTab.BackPack:
                                CheckCardSelectionLimits(3);
                                break;
                            case LcdTab.Chat:
                                CheckCardSelectionLimits(6);
                                break;
                            case LcdTab.Galaxy:

                                if (_currentZoomLevel[(int)CurrentTab] < 0)
                                {
                                    _currentZoomLevel[(int)CurrentTab] = 0;
                                }
                                else
                                if (_currentZoomLevel[(int)CurrentTab] > 15)
                                {
                                    _currentZoomLevel[(int)CurrentTab] = 15;
                                }

                                break;
                            case LcdTab.Galnet:

                                if (CurrentCard[(int)CurrentTab] < 0)
                                {
                                    CurrentCard[(int)CurrentTab] = 0;
                                }
                                else
                                if (CurrentCard[(int)CurrentTab] > (Galnet.GalnetList?.Count ?? 1) - 1)
                                {
                                    CurrentCard[(int)CurrentTab] = (Galnet.GalnetList?.Count ?? 1) - 1;
                                }

                                break;

                        }

                        if (mustRender)
                        {
                            try
                            {


                                switch (CurrentTab)
                                {
                                    //----------------------

                                    case LcdTab.None:

                                        str =
                                            Engine.Razor.Run("init.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage
                                            });

                                        break;

                                    case LcdTab.Commander:

                                        str =
                                            Engine.Razor.Run("commander.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,

                                                LegalState = Data.StatusData.LegalState,

                                                Commander = Data.CommanderData.Name,

                                                Credits = Data.CommanderData.Credits,

                                                FederationRank = Data.CommanderData.FederationRank,
                                                FederationRankProgress = Data.CommanderData.FederationRankProgress,

                                                EmpireRank = Data.CommanderData.EmpireRank,
                                                EmpireRankProgress = Data.CommanderData.EmpireRankProgress,

                                                CombatRank = Data.CommanderData.CombatRank,
                                                CombatRankProgress = Data.CommanderData.CombatRankProgress,

                                                TradeRank = Data.CommanderData.TradeRank,
                                                TradeRankProgress = Data.CommanderData.TradeRankProgress,

                                                ExplorationRank = Data.CommanderData.ExplorationRank,
                                                ExplorationRankProgress = Data.CommanderData.ExplorationRankProgress,

                                                SoldierRank = Data.CommanderData.SoldierRank,
                                                SoldierRankProgress = Data.CommanderData.SoldierRankProgress,

                                                ExobiologistRank = Data.CommanderData.ExobiologistRank,
                                                ExobiologistRankProgress = Data.CommanderData.ExobiologistRankProgress,

                                                CqcRank = Data.CommanderData.CqcRank?.Replace("SemiProfessional", "SemiPro"),

                                                CqcRankProgress = Data.CommanderData.CqcRankProgress,

                                                FederationReputation = Data.CommanderData.FederationReputation,
                                                AllianceReputation = Data.CommanderData.AllianceReputation,
                                                EmpireReputation = Data.CommanderData.EmpireReputation,

                                                FederationReputationState =
                                                    Data.CommanderData.FederationReputationState,
                                                AllianceReputationState = Data.CommanderData.AllianceReputationState,
                                                EmpireReputationState = Data.CommanderData.EmpireReputationState

                                            });



                                        break;

                                    case LcdTab.Ship:

                                        lock (Module.RefreshModuleLock)
                                        {
                                            var onFoot = Data.StatusData.OnFoot;

                                            //var shipData = Ships.GetCurrentShip() ?? new Ships.ShipData();

                                            str =
                                                Engine.Razor.Run("ship.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = CurrentCard[(int) CurrentTab],

                                                    OnFoot = onFoot,

                                                    CurrentShip =
                                                        Ships.ShipsList.FirstOrDefault(x => x.Stored == false),

                                                    StoredShips = Ships.ShipsList.Where(x => x.Stored)
                                                        .OrderBy(x => x.Distance).ThenBy(x => x.ShipType).ToList(),

                                                    StoredModules = Module.StoredModulesList.Values
                                                        .OrderBy(x => x.Distance).ToList()

                                                });
                                        }

                                        break;

                                    case LcdTab.Navigation:

                                        var currentShip = Ships.ShipsList.FirstOrDefault(x => x.Stored == false);

                                        lock (Route.RefreshRouteLock)
                                        {
                                            var routeList = new List<RouteItem>();

                                            var onFoot = Data.StatusData.OnFoot;

                                            if (!onFoot &&
                                                Data.LocationData.StarSystem != Data.LocationData.FsdTargetName &&
                                                Data.LocationData.RemainingJumpsInRoute > 0 && Route.RouteList.Count >=
                                                Data.LocationData.RemainingJumpsInRoute)
                                            {
                                                routeList = Route.RouteList
                                                    .Skip(Route.RouteList.Count -
                                                          Data.LocationData.RemainingJumpsInRoute)
                                                    .ToList();
                                            }

                                            var jumpDistance = 0.0;
                                            var fuelCost = 0.0;
                                            var fuelWarning = "";

                                            var fuelMain = currentShip?.CurrentFuelMain ?? 0;

                                            if (!onFoot && fuelMain > 0)
                                            {
                                                foreach (var r in routeList)
                                                {
                                                    r.FuelWarning = null;
                                                }

                                                for (var index = 0; index < routeList.Count; index++)
                                                {
                                                    var r = routeList[index];

                                                    if (fuelMain > 0)
                                                    {
                                                        r.FuelCost = Ships.GetFuelCostForNextJump(r.Distance, fuelMain);
                                                    }

                                                    if (index > 0 && (fuelMain <= 0 || r.FuelCost > fuelMain))
                                                    {
                                                        for (var index2 = index - 1; index2 >= 0; index2--)
                                                        {
                                                            var r2 = routeList[index2];

                                                            if (!string.IsNullOrEmpty(r2.IsFuelStar))
                                                            {
                                                                r2.FuelWarning = "Last chance to scoop fuel";
                                                                break;
                                                            }
                                                        }

                                                        break;
                                                    }

                                                    if (fuelMain > 0)
                                                    {
                                                        fuelMain -= r.FuelCost;
                                                    }
                                                }
                                            }

                                            if (!onFoot && routeList.Count >= 1)
                                            {
                                                var nextJump = routeList[0];
                                                if (nextJump.StarSystem == Data.LocationData.FsdTargetName)
                                                {
                                                    jumpDistance = nextJump.Distance;
                                                    fuelCost = nextJump.FuelCost;
                                                    fuelWarning = nextJump.FuelWarning;
                                                }
                                            }

                                            lock (App.RefreshSystemLock)
                                            {
                                                List<StationData> odysseySettlements = null;
                                                List<StationData> coloniaBridge = null;

                                                if (CurrentCard[(int)CurrentTab] == 3)
                                                {
                                                    Station.OdysseySettlements.TryGetValue(Data.LocationData.StarSystem,
                                                        out odysseySettlements);
                                                }
                                                else if (CurrentCard[(int)CurrentTab] == 4)
                                                {
                                                    coloniaBridge = Station.GetNearestColoniaBridge(Data.LocationData.StarPos,Station.ColoniaBridge);
                                                }

                                                str =
                                                    Engine.Razor.Run("navigation.cshtml", null, new
                                                    {
                                                        CurrentTab = CurrentTab,
                                                        CurrentPage = _currentPage,
                                                        CurrentCard = CurrentCard[(int) CurrentTab],

                                                        CurrentShip = currentShip ?? new Ships.ShipData(),

                                                        OnFoot = onFoot,

                                                        StarSystem = Data.LocationData.StarSystem,

                                                        SystemData = SystemInfo.SystemData,

                                                        Body = !string.IsNullOrEmpty(Data.LocationData.BodyType) &&
                                                               !string.IsNullOrEmpty(Data.LocationData.Body)
                                                            ? Data.LocationData.Body
                                                            : null,

                                                        //"Station""Star""Planet""PlanetaryRing""StellarRing""AsteroidCluster"

                                                        BodyType = Data.LocationData.BodyType,

                                                        Station = Data.LocationData.BodyType == "Station" &&
                                                                  !string.IsNullOrEmpty(Data.LocationData.Body)
                                                            ? Data.LocationData.Body
                                                            : Data.LocationData.Station,

                                                        Docked = Data.StatusData.Docked,

                                                        LandingPad = Data.DockData.LandingPad,

                                                        StationType = Data.DockData.Type,

                                                        Government = Data.DockData.Government,

                                                        Allegiance = Data.DockData.Allegiance,

                                                        Faction = Data.DockData.Faction,

                                                        Economy = Data.DockData.Economy,

                                                        DistFromStarLs = Data.DockData.DistFromStarLs,

                                                        StartJump = Data.LocationData.StartJump,

                                                        JumpType = Data.LocationData.JumpType,

                                                        JumpToSystem = Data.LocationData.JumpToSystem,

                                                        JumpToStarClass = Data.LocationData.JumpToStarClass,

                                                        RemainingJumpsInRoute = Data.LocationData.RemainingJumpsInRoute,

                                                        StarClass = Data.LocationData.StarClass,

                                                        IsFuelStar = Data.LocationData.IsFuelStar,

                                                        FsdTargetName = Data.LocationData.FsdTargetName,

                                                        Settlement = Data.LocationData.Settlement,

                                                        HideBody = Data.LocationData.HideBody,

                                                        SystemAllegiance = Data.LocationData.SystemAllegiance,

                                                        SystemFaction = Data.LocationData.SystemFaction,

                                                        SystemSecurity = Data.LocationData.SystemSecurity,

                                                        SystemEconomy = Data.LocationData.SystemEconomy,

                                                        SystemGovernment = Data.LocationData.SystemGovernment,

                                                        Population = Data.LocationData.Population,

                                                        PowerplayState = Data.LocationData.PowerplayState,
                                                        Powers = Data.LocationData.Powers,

                                                        SystemState = Data.LocationData.SystemState,

                                                        RouteList = routeList,
                                                        RouteListCount = routeList.Count,
                                                        RouteListDistance = routeList.Sum(x => x.Distance),
                                                        RouteDestination = routeList.LastOrDefault()?.StarSystem ?? "",
                                                        JumpDistance = jumpDistance,
                                                        FuelCost = fuelCost,
                                                        FuelWarning = fuelWarning,

                                                        OdysseySettlements = odysseySettlements,

                                                        ColoniaBridge = coloniaBridge

                                                    });
                                            }

                                        }

                                        break;

                                    case LcdTab.Target:

                                        str =
                                            Engine.Razor.Run("target.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,

                                                TargetLocked = Data.TargetData.TargetLocked,
                                                ScanStage = Data.TargetData.ScanStage,

                                                Bounty = Data.TargetData.Bounty,
                                                Faction = Data.TargetData.Faction,
                                                LegalStatus = Data.TargetData.LegalStatus,
                                                PilotNameLocalised = Data.TargetData.PilotNameLocalised,
                                                PilotRank = Data.TargetData.PilotRank,

                                                Power = Data.TargetData.Power,

                                                Ship = Data.TargetData.Ship?.Trim(),

                                                ShipImage = Data.TargetData.Ship?.Trim() + ".png",

                                                SubsystemLocalised = Data.TargetData.SubsystemLocalised,

                                                SubsystemHealth = Data.TargetData.SubsystemHealth,
                                                HullHealth = Data.TargetData.HullHealth,
                                                ShieldHealth = Data.TargetData.ShieldHealth

                                            });

                                        break;

                                    case LcdTab.Galnet:

                                        var currentCard = CurrentCard[(int)CurrentTab];

                                        if (Galnet.GalnetList?.Count <= currentCard -1)
                                        {
                                            currentCard = CurrentCard[(int) CurrentTab] = 0;
                                        }

                                        lock (App.RefreshJsonLock)
                                        {

                                            str =
                                            Engine.Razor.Run("galnet.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = currentCard,

                                                Galnet = Galnet.GalnetList.Skip(currentCard).FirstOrDefault().Value

                                            });
                                        }

                                        break;


                                    case LcdTab.POI:

                                        lock (App.RefreshJsonLock)
                                        {
                                            str =
                                                Engine.Razor.Run("poi.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = CurrentCard[(int) CurrentTab],

                                                    NearbyPoiList = Poi.NearbyPoiList,

                                                    NearbyCnbSystemsList = Data.NearbyCnbSystemsList,

                                                    NearbyStationList = Data.NearbyStationList

                                                });
                                        }

                                        break;

                                    //----------------------

                                    case LcdTab.Galaxy:


                                        str =
                                            Engine.Razor.Run("galaxy.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                GalaxyImageDisplayHeight = GalaxyImageDisplayHeight,
                                                GalaxyImageDisplayWidth = GalaxyImageDisplayWidth,
                                                GalaxyImageMarginTop = GalaxyImageMarginTop + "px",
                                                GalaxyImageFileName = GalaxyImageFileName,

                                                _currentZoomLevel = _currentZoomLevel[(int) CurrentTab]


                                            });

                                        break;

                                    case LcdTab.HWInfo:

                                        lock (HWInfo.RefreshHWInfoLock)
                                        {
                                            str =
                                                Engine.Razor.Run("hwinfo.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = CurrentCard[(int) CurrentTab],
                                                    
                                                    SensorCount = HWInfo.SensorData.Count,

                                                    SensorData = HWInfo.SensorData.Values.ToList(),

                                                    ChartImageDisplayWidth = ChartImageDisplayWidth,
                                                    ChartImageDisplayHeight = ChartImageDisplayHeight

                                                });
                                        }

                                        break;
                                    case LcdTab.Engineers:

                                        lock (App.RefreshJsonLock)
                                        {
                                            if (Data.EngineersList != null && CurrentCard[(int)CurrentTab] < Data.EngineersList.Count )
                                            {
                                                Engineer.EngineerBlueprints.TryGetValue(
                                                    Data.EngineersList[CurrentCard[(int) CurrentTab]].Faction,
                                                    out var blueprints);

                                                str =
                                                    Engine.Razor.Run("engineers.cshtml", null, new
                                                    {
                                                        CurrentTab = CurrentTab,
                                                        CurrentPage = _currentPage,
                                                        CurrentCard = CurrentCard[(int) CurrentTab],

                                                        Engineer = Data.EngineersList[CurrentCard[(int) CurrentTab]],

                                                        Blueprints = blueprints?.Where(x => x.Type != "Suit" && x.Type != "Weapon"),

                                                        SuitWeaponBlueprints = blueprints?.Where(x => x.Type == "Suit" || x.Type == "Weapon")

                                                    });
                                            }
                                        }

                                        break;


                                    case LcdTab.Powers:

                                        lock (App.RefreshJsonLock)
                                        {
                                            str =
                                                Engine.Razor.Run("powers.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = CurrentCard[(int) CurrentTab],

                                                    NearbyPowerStationList = Data.NearbyPowerStationList

                                                });
                                        }

                                        break;


                                    case LcdTab.Materials:

                                        str =
                                            Engine.Razor.Run("materials.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = CurrentCard[(int) CurrentTab],

                                                MaterialCount = Material.MaterialList.Count,

                                                Raw = Material.MaterialList.Where(x => x.Value.Category == "Raw")
                                                    .OrderBy(x => x.Value.Name),
                                                Manufactured = Material.MaterialList
                                                    .Where(x => x.Value.Category == "Manufactured")
                                                    .OrderBy(x => x.Value.Name),
                                                Encoded = Material.MaterialList
                                                    .Where(x => x.Value.Category == "Encoded")
                                                    .OrderBy(x => x.Value.Name)
                                            });

                                        break;

                                    case LcdTab.Cargo:

                                        lock (Cargo.RefreshCargoLock)
                                        {

                                            var cargo = Cargo.CargoList
                                                .Where(x => x.Value.Count > 0 && (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0" ))
                                                .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                            var missionCargo = Cargo.CargoList
                                                .Where(x => x.Value.Count > 0 && !string.IsNullOrEmpty(x.Value.MissionID)  && x.Value.MissionID != "0")
                                                .Select(x => new Cargo.CargoItem
                                                {
                                                    Count = x.Value.Count,
                                                    Name = x.Value.Name,
                                                    MissionID = x.Value.MissionID,
                                                    MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                    System = Missions.GetMissionSystem(x.Value.MissionID),
                                                    Station = Missions.GetMissionStation(x.Value.MissionID)
                                                })
                                                .OrderBy(x => x.Name)
                                                .GroupBy(x => x.MissionName)
                                                .Select(grp => grp.ToList())
                                                .ToList();


                                            var stolenCargo = Cargo.CargoList.Where(x => x.Value.Stolen > 0)
                                                .Select(x => x.Value)
                                                .OrderBy(x => x.Name).ToList();

                                            str =
                                                Engine.Razor.Run("cargo.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = CurrentCard[(int) CurrentTab],

                                                    Cargo = cargo,
                                                    CargoCount = cargo.Count,

                                                    MissionCargo = missionCargo,
                                                    MissionCargoCount = missionCargo.Count,

                                                    StolenCargo = stolenCargo,
                                                    StolenCargoCount = stolenCargo.Count,

                                                    CurrentShip = Ships.ShipsList.FirstOrDefault(x => x.Stored == false)

                                                });

                                        }

                                        break;

                                    case LcdTab.Mining:

                                        lock (App.RefreshJsonLock)
                                        {

                                            str =
                                                Engine.Razor.Run("mining.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = CurrentCard[(int)CurrentTab],

                                                    NearbyHotspotSystemsList = Data.NearbyHotspotSystemsList,

                                                    NearbyMiningStationsList = Data.NearbyMiningStationsList

                                                });
                                        }

                                        break;

                                    case LcdTab.Engineer:

                                        Engineer.RefreshMaterialList();

                                        str =
                                            Engine.Razor.Run("engineer.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = CurrentCard[(int) CurrentTab],

                                                Circuits = Engineer.IngredientShoppingList?
                                                                .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient &&
                                                                            x.EntryData.Group == Group.Circuits)
                                                                .OrderBy(x => x.Name).ToList() ??
                                                            new List<IngredientShoppingListItem>(),

                                                Chemicals = Engineer.IngredientShoppingList?
                                                           .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient &&
                                                                       x.EntryData.Group == Group.Chemicals)
                                                           .OrderBy(x => x.Name).ToList() ??
                                                       new List<IngredientShoppingListItem>(),

                                                Tech = Engineer.IngredientShoppingList?
                                                                .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient &&
                                                                            x.EntryData.Group == Group.Tech)
                                                                .OrderBy(x => x.Name).ToList() ??
                                                            new List<IngredientShoppingListItem>(),

                                                Data = Engineer.IngredientShoppingList?
                                                          .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient &&
                                                                      x.EntryData.Group == Group.Data)
                                                          .OrderBy(x => x.Name).ToList() ??
                                                      new List<IngredientShoppingListItem>(),

                                                Item = Engineer.IngredientShoppingList?
                                                           .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient &&
                                                                       x.EntryData.Group == Group.Item)
                                                           .OrderBy(x => x.Name).ToList() ??
                                                       new List<IngredientShoppingListItem>(),

                                                //---------------------

                                                Raw = Engineer.IngredientShoppingList?
                                                          .Where(x => x.EntryData.Subkind == Subkind.Raw)
                                                          .OrderBy(x => x.Name).ToList() ??
                                                      new List<IngredientShoppingListItem>(),

                                                Manufactured = Engineer.IngredientShoppingList?
                                                                   .Where(x => x.EntryData.Subkind ==
                                                                               Subkind.Manufactured)
                                                                    .OrderBy(x => x.Name).ToList() ??
                                                               new List<IngredientShoppingListItem>(),

                                                Encoded = Engineer.IngredientShoppingList?
                                                                .Where(x => x.EntryData.Kind == Kind.Data)
                                                                .OrderBy(x => x.Name).ToList() ??
                                                          new List<IngredientShoppingListItem>(),

                                                Commodity = Engineer.IngredientShoppingList?
                                                                .Where(x => x.EntryData.Kind == Kind.Commodity)
                                                                .OrderBy(x => x.Name).ToList() ??
                                                            new List<IngredientShoppingListItem>(),

                                                BlueprintShoppingList = Engineer.BlueprintShoppingList
                                                    .OrderBy (x => x.Blueprint.Type)
                                                    .ThenBy(x => x.Blueprint.BlueprintName)
                                                    .ThenBy(x => x.Blueprint.Grade)
                                                    .ToList(),

                                                MissingRaw = Engineer.IngredientShoppingList?
                                                                .Where(x => x.EntryData.Subkind == Subkind.Raw && x.RequiredCount > x.Inventory)
                                                                .OrderBy(x => x.Name).ToList() ??
                                                            new List<IngredientShoppingListItem>(),

                                                MissingManufactured = Engineer.IngredientShoppingList?
                                                                   .Where(x => x.EntryData.Subkind ==
                                                                               Subkind.Manufactured && x.RequiredCount > x.Inventory)
                                                                   .OrderBy(x => x.Name).ToList() ??
                                                               new List<IngredientShoppingListItem>(),

                                                MissingEncoded = Engineer.IngredientShoppingList?
                                                              .Where(x => x.EntryData.Kind == Kind.Data && x.RequiredCount > x.Inventory)
                                                              .OrderBy(x => x.Name).ToList() ??
                                                          new List<IngredientShoppingListItem>(),

                                                MissingCommodity = Engineer.IngredientShoppingList?
                                                                .Where(x => x.EntryData.Kind == Kind.Commodity && x.RequiredCount > x.Inventory)
                                                                .OrderBy(x => x.Name).ToList() ??
                                                            new List<IngredientShoppingListItem>(),

                                                //---------------------

                                                MissingCircuits = Engineer.IngredientShoppingList?
                                                                      .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient && x.EntryData.Group == Group.Circuits && x.RequiredCount > x.Inventory)
                                                                      .OrderBy(x => x.Name).ToList() ??
                                                                  new List<IngredientShoppingListItem>(),

                                                MissingChemicals = Engineer.IngredientShoppingList?
                                                                       .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient && x.EntryData.Group == Group.Chemicals && x.RequiredCount > x.Inventory)
                                                                       .OrderBy(x => x.Name).ToList() ??
                                                                   new List<IngredientShoppingListItem>(),

                                                MissingTech = Engineer.IngredientShoppingList?
                                                                  .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient && x.EntryData.Group == Group.Tech && x.RequiredCount > x.Inventory)
                                                                  .OrderBy(x => x.Name).ToList() ??
                                                              new List<IngredientShoppingListItem>(),

                                                MissingData = Engineer.IngredientShoppingList?
                                                                  .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient && x.EntryData.Group == Group.Data && x.RequiredCount > x.Inventory)
                                                                  .OrderBy(x => x.Name).ToList() ??
                                                              new List<IngredientShoppingListItem>(),

                                                MissingItem = Engineer.IngredientShoppingList?
                                                                  .Where(x => x.EntryData.Kind == Kind.OdysseyIngredient && x.EntryData.Group == Group.Item && x.RequiredCount > x.Inventory)
                                                                  .OrderBy(x => x.Name).ToList() ??
                                                              new List<IngredientShoppingListItem>()


                                            });

                                        break;

                                    case LcdTab.Missions:

                                        str =
                                            Engine.Razor.Run("missions.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,

                                                MissionList = Missions.MissionList
                                            });

                                        break;

                                    case LcdTab.ShipLocker:

                                        var shipLockerItems = Material.ShipLockerList.Where(x =>
                                                x.Value.Category == "Item" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionShipLockerItems = Material.ShipLockerList
                                            .Where(x => x.Value.Category == "Item" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        var shipLockerComponents = Material.ShipLockerList.Where(x =>
                                                x.Value.Category == "Component" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var shipLockerComponentsTech = shipLockerComponents
                                            .Where(x => x.Group == "Tech").ToList();
                                        var shipLockerComponentsCircuits = shipLockerComponents
                                            .Where(x => x.Group == "Circuits").ToList();
                                        var shipLockerComponentsChemicals = shipLockerComponents
                                            .Where(x => x.Group == "Chemicals").ToList();

                                        var missionShipLockerComponents = Material.ShipLockerList
                                            .Where(x => x.Value.Category == "Component" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        var shipLockerConsumables = Material.ShipLockerList.Where(x =>
                                                x.Value.Category == "Consumable" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionShipLockerConsumables = Material.ShipLockerList
                                            .Where(x => x.Value.Category == "Consumable" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        var shipLockerData = Material.ShipLockerList.Where(x =>
                                                x.Value.Category == "Data" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionShipLockerData = Material.ShipLockerList
                                            .Where(x => x.Value.Category == "Data" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        str =
                                            Engine.Razor.Run("shiplocker.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = CurrentCard[(int)CurrentTab],

                                                MaterialCount = Material.ShipLockerList.Count,

                                                Items = shipLockerItems,
                                                ItemsCount = shipLockerItems.Count,
                                                ComponentsCircuits = shipLockerComponentsCircuits,
                                                ComponentsChemicals = shipLockerComponentsChemicals,
                                                ComponentsTech = shipLockerComponentsTech,
                                                ComponentsCircuitsCount = shipLockerComponentsCircuits.Count,
                                                ComponentsChemicalsCount = shipLockerComponentsChemicals.Count,
                                                ComponentsTechCount = shipLockerComponentsTech.Count,
                                                Consumables = shipLockerConsumables,
                                                ConsumablesCount = shipLockerConsumables.Count,
                                                Data = shipLockerData,
                                                DataCount = shipLockerData.Count,

                                                MissionItems = missionShipLockerItems,
                                                MissionItemsCount = missionShipLockerItems.Count,
                                                MissionComponents = missionShipLockerComponents,
                                                MissionComponentsCount = missionShipLockerComponents.Count,
                                                MissionConsumables = missionShipLockerConsumables,
                                                MissionConsumablesCount = missionShipLockerConsumables.Count,
                                                MissionData = missionShipLockerData,
                                                MissionDataCount = missionShipLockerData.Count,

                                            });

                                        break;

                                    case LcdTab.BackPack:

                                        var backPackItems = Material.BackPackList.Where(x =>
                                                x.Value.Category == "Item" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionBackPackItems = Material.BackPackList
                                            .Where(x => x.Value.Category == "Item" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        var backPackComponents = Material.BackPackList.Where(x => 
                                                x.Value.Category == "Component" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var backPackComponentsTech = backPackComponents
                                            .Where(x => x.Group == "Tech").ToList();
                                        var backPackComponentsCircuits = backPackComponents
                                            .Where(x => x.Group == "Circuits").ToList();
                                        var backPackComponentsChemicals = backPackComponents
                                            .Where(x => x.Group == "Chemicals").ToList();

                                        var missionBackPackComponents = Material.BackPackList
                                            .Where(x => x.Value.Category == "Component" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        var backPackConsumables = Material.BackPackList.Where(x => 
                                                x.Value.Category == "Consumable" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionBackPackConsumables = Material.BackPackList
                                            .Where(x => x.Value.Category == "Consumable" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        var backPackData = Material.BackPackList.Where(x => 
                                                x.Value.Category == "Data" &&
                                                (string.IsNullOrEmpty(x.Value.MissionID) || x.Value.MissionID == "0"))
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionBackPackData = Material.BackPackList
                                            .Where(x => x.Value.Category == "Data" && (!string.IsNullOrEmpty(x.Value.MissionID) && x.Value.MissionID != "0"))
                                            .Select(x => new Material.MaterialItem
                                            {
                                                Count = x.Value.Count,
                                                Name = x.Value.Name,
                                                MissionID = x.Value.MissionID,
                                                MissionName = Missions.GetMissionName(x.Value.MissionID),
                                                System = Missions.GetMissionSystem(x.Value.MissionID),
                                                Station = Missions.GetMissionStation(x.Value.MissionID)
                                            })
                                            .OrderBy(x => x.Name)
                                            .GroupBy(x => x.MissionName)
                                            .Select(grp => grp.ToList())
                                            .ToList();

                                        str =
                                            Engine.Razor.Run("backpack.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = CurrentCard[(int)CurrentTab],

                                                MaterialCount = Material.BackPackList.Count,

                                                Items = backPackItems,
                                                ItemsCount = backPackItems.Count,
                                                ComponentsCircuits = backPackComponentsCircuits,
                                                ComponentsChemicals = backPackComponentsChemicals,
                                                ComponentsTech = backPackComponentsTech,
                                                ComponentsCircuitsCount = backPackComponentsCircuits.Count,
                                                ComponentsChemicalsCount = backPackComponentsChemicals.Count,
                                                ComponentsTechCount = backPackComponentsTech.Count,
                                                Consumables = backPackConsumables,
                                                ConsumablesCount = backPackConsumables.Count,
                                                Data = backPackData,
                                                DataCount = backPackData.Count,

                                                MissionItems = missionBackPackItems,
                                                MissionItemsCount = missionBackPackItems.Count,
                                                MissionComponents = missionBackPackComponents,
                                                MissionComponentsCount = missionBackPackComponents.Count,
                                                MissionConsumables = missionBackPackConsumables,
                                                MissionConsumablesCount = missionBackPackConsumables.Count,
                                                MissionData = missionBackPackData,
                                                MissionDataCount = missionBackPackData.Count,

                                            });

                                        break;

                                    case LcdTab.Chat:

                                        var chatCard = CurrentCard[(int) CurrentTab];

                                        TextChannel channel = TextChannel.Unknown;
                                        switch (chatCard)
                                        {
                                            case 0:
                                                channel = TextChannel.StarSystem;
                                                break;
                                            case 1:
                                                channel = TextChannel.Local;
                                                break;
                                            case 2:
                                                channel = TextChannel.Friend;
                                                break;
                                            case 3:
                                                channel = TextChannel.Player;
                                                break;
                                            case 4:
                                                channel = TextChannel.Wing;
                                                break;
                                            case 5:
                                                channel = TextChannel.Squadron;
                                                break;
                                            case 6:
                                                channel = TextChannel.VoiceChat;
                                                break;
                                        }

                                        var hist = Data.ChatHistory.Where(x => x.Channel == channel).Take(20).ToList();

                                        str =
                                            Engine.Razor.Run("chat.cshtml", null, new
                                            {
                                                CurrentTab = CurrentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = chatCard,

                                                ChatList = hist,

                                                ChatListCount = hist.Count
                                            });

                                        break;

                                        /*
                                        case LcdTab.Events:

                                            var eventlist = "";
                                            foreach (var b in Data.EventHistory)
                                            {
                                                eventlist += b + "<br/>";
                                            }

                                            str =
                                                Engine.Razor.Run("events.cshtml", null, new
                                                {
                                                    CurrentTab = CurrentTab,
                                                    CurrentPage = _currentPage,

                                                    EventList = eventlist
                                                });

                                            break;
                                        */
                                }


                            }
                            catch (Exception ex)
                            {
                                App.Log.Error(ex);
                            }
                        }

                        graphics.Clear(Color.Black);

                        if (CurrentTab >= 0)
                        {

                            if (mustRender)
                            {
                                var measureData =HtmlRender.Measure(graphics, str, HtmlWindowUsableWidth, App.CssData,null, OnImageLoad);

                                _currentLcdHeight = (int)measureData.Height;
                            }

                            CheckLcdOffset();

                            if (_currentLcdHeight > 0)
                            {

                                if (mustRender)
                                {
                                    _htmlImage = HtmlRender.RenderToImage(str,
                                        new Size(HtmlWindowUsableWidth, _currentLcdHeight + 20), Color.Black, App.CssData,
                                        null, OnImageLoad);
                                }

                                if (_htmlImage != null)
                                {
                                    graphics.DrawImage(_htmlImage, new Rectangle(new Point(HtmlWindowXOffset, 0),
                                            new Size(HtmlWindowUsableWidth, _htmlWindowHeight + 20)),
                                        new Rectangle(new Point(0, _currentLcdYOffset),
                                            new Size(HtmlWindowUsableWidth, _htmlWindowHeight + 20)),
                                        GraphicsUnit.Pixel);
                                }
                            }

                            if (_currentLcdHeight > _htmlWindowHeight && CurrentTab != LcdTab.Galaxy)
                            {
                                var scrollThumbHeight = _htmlWindowHeight / (double)_currentLcdHeight * ScrollBarHeight;
                                var scrollThumbYOffset = _currentLcdYOffset / (double)_currentLcdHeight * ScrollBarHeight;

                                graphics.DrawRectangle(_scrollPen, new Rectangle(new Point(_htmlWindowWidth - 9, 2),
                                                                   new Size(5, (int)ScrollBarHeight)));

                                graphics.FillRectangle(_scrollBrush, new Rectangle(new Point(_htmlWindowWidth - 9, 2 + (int)scrollThumbYOffset),
                                    new Size(5, 1 + (int)scrollThumbHeight)));

                            }
                        }


                        if (CurrentTab != LcdTab.Galaxy)
                        {
                            if (mustRender)
                            {
                                var currentCard = CurrentCard[(int) CurrentTab];

                                var galnetDate = "???";
                                var galnetCaption = "Galnet";

                                var galNetCount = Galnet.GalnetList?.Count ?? 0;

                                if (CurrentTab == LcdTab.Galnet && galNetCount > currentCard)
                                {
                                    galnetDate = Galnet.GalnetList.Skip(currentCard).FirstOrDefault().Value
                                        .FirstOrDefault()?.Date;
                                }

                                if (galnetDate.StartsWith("Community"))
                                {
                                    galnetCaption = "";
                                }
                                else
                                {
                                    galnetDate = " : " + galnetDate;
                                }

                                if (currentCard > 0)
                                {
                                    galnetCaption = "&#x25c0; " + galnetCaption;
                                }

                                galnetCaption += galnetDate;

                                if (currentCard < galNetCount -1)
                                {
                                    galnetCaption += " &#x25b6;";
                                }

                                var cardcaptionstr =
                                    Engine.Razor.Run("cardcaption.cshtml", null, new
                                    {
                                        CurrentTab = CurrentTab,
                                        CurrentPage = _currentPage,
                                        CurrentCard = currentCard,

                                        GalnetCaption = galnetCaption,

                                        EngineersList = Data.EngineersList
                                    });

                                _cardcaptionHtmlImage = HtmlRender.RenderToImage(cardcaptionstr,
                                    new Size(HtmlWindowUsableWidth, 26), Color.Black, App.CssData, null,
                                    null);
                            }

                            if (_cardcaptionHtmlImage != null)
                            {
                                graphics.DrawImage(_cardcaptionHtmlImage, HtmlWindowXOffset, 0);
                            }
                        }

                        if (_currentPage != LcdPage.Collapsed)
                        {
                            if (mustRender)
                            {
                                var menustr =
                                    Engine.Razor.Run("menu.cshtml", null, new
                                    {
                                        CurrentTab = CurrentTab,
                                        CurrentPage = _currentPage,
                                        Cursor = _currentTabCursor,

                                        TargetLocked = Data.TargetData.TargetLocked,

                                        MissionCount = Missions.MissionList.Count,

                                        MaterialCount = Material.MaterialList.Count,

                                        ShowEngineer = !string.IsNullOrEmpty(Engineer.CommanderName),
                                        
                                        ShowHWInfo = HWInfo.SensorData.Any()
                                    });

                                _menuHtmlImage = HtmlRender.RenderToImage(menustr,
                                    new Size(HtmlMenuWindowWidth, HtmlMenuWindowHeight), Color.Black, App.CssData, null,
                                    OnImageLoad);
                            }

                            if (_menuHtmlImage != null)
                            {
                                graphics.DrawImage(_menuHtmlImage, 0, 0);
                            }
                        }


#if DEBUG
                        fipImage.Save("screenshot"+ SerialNumber+"_"+(int)CurrentTab+"_"+ CurrentCard[(int)CurrentTab] + ".png", ImageFormat.Png);
#endif
                        RefreshMirrorWindow(fipImage);
                       
                        SendImageToFip(DEFAULT_PAGE, fipImage);

                        var tabPage = (LcdPage)(((uint)CurrentTab - 1) / 6);
                        if (CurrentTab == LcdTab.None)
                        {
                            tabPage = LcdPage.HomeMenu;
                        }

                        if (_initOk)
                        {
                            if (_currentPage == LcdPage.Collapsed)
                            {
                                for (uint i = 2; i <= 6; i++)
                                {
                                    SetLed(i, false);
                                }

                                SetLed(1, true);

                                HandleCardNavigationLeds();
                            }
                            else
                            {
                                for (uint i = 1; i <= 6; i++)
                                {
                                    if (_currentPage == LcdPage.InfoMenu && i == 6 && !HWInfo.SensorData.Any())
                                        SetLed(i, false);
                                    else if (_currentPage == LcdPage.ShipMenu && i == 5 && string.IsNullOrEmpty(Engineer.CommanderName))
                                        SetLed(i, false);
                                    else if (_currentPage == LcdPage.ShipMenu && i == 6)
                                        SetLed(i, false);
                                    else if (_currentPage == LcdPage.SuitMenu && i > 3) // change !!!!!!!!!!!!!
                                        SetLed(i, false);
                                    else
                                        SetLed(i, true);
                                }

                            }

                        }

                        _lastTab = CurrentTab;
                    }
                }
            }
        }


    }
}
