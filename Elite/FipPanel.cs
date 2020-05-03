using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
    public enum JoystickButton
    {
        Up,
        Down,
        Left,
        Right,
        Push
    }

    public enum LCDPage
    {
        Collapsed = -1,
        HomeMenu = 0,
        ShipMenu = 1,
        LocationsMenu = 2
    }

    public enum LCDTab
    {
        Init=-999,
        None = 0,

        Commander = 1,
        Navigation = 2,
        Target = 3,
        Missions = 4,
        ShipMenu = 5, // ship ->
        LocationsMenu = 6, // locations ->

        //---------------

        ShipBack = 7, // <- back
        Ship = 8,
        Materials = 9,
        Cargo = 10,
        Events = 11,
        // empty

        //---------------

        LocationsBack = 13, // <- back
        POI = 14,
        Map = 15,
        Powers = 16,
        Mining = 17
        // empty

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
        public IEncodedString SinceText(int agodec, DateTime UpdatedTime)
        {

            var ts = DateTime.Now - UpdatedTime.AddSeconds(-agodec);

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

    class FipPanel
    {

        private readonly object _refreshDevicePageLock = new object();


        private LCDPage _currentPage = LCDPage.Collapsed;
        private LCDTab _currentTab = LCDTab.None;
        private LCDTab _currentTabCursor = LCDTab.None;
        private LCDTab _lastTab = LCDTab.Init;
        const int DEFAULT_PAGE = 0;
        const int NO_PAGES = 2;
        private string _settingsPath;

        private int[] _currentCard = new int[100];
        private int[] _currentZoomLevel = new int[100];


        private int CurrentLCDYOffset = 0;
        private int CurrentLCDHeight = 0;

        public IntPtr FipDevicePointer;
        public string SerialNumber;

        private uint _prevButtons;

        private List<uint> _pageList = new List<uint>();

        private readonly Pen _scrollPen = new Pen(Color.FromArgb(0xff,0xFF,0xB0,0x00));
        private readonly Pen _whitePen = new Pen(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF),(float)0.1);
        private readonly SolidBrush _scrollBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xB0, 0x00));
        private readonly SolidBrush redBrush = new SolidBrush(Color.FromArgb(0xFF, 0x00, 0x00));

        private Image htmlImage = null;
        private Image menuHtmlImage = null;
        private Image cardcaptionHtmlImage = null;

        private const int HtmlMenuWindowWidth = 110; //69;

        private const int HtmlWindowHeight = 240;

        private const int HtmlWindowXOffset = /*HtmlMenuWindowWidth*/ + 1;
        private const int HtmlWindowWidth = 311- HtmlWindowXOffset;


        protected bool DoShutdown;

        protected DirectOutputClass.PageCallback PageCallbackDelegate;
        protected DirectOutputClass.SoftButtonCallback SoftButtonCallbackDelegate;

        private bool blockNextUpState = false;

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

            var returnValues3 = DirectOutputClass.GetSerialNumber(FipDevicePointer, out SerialNumber);
            if (returnValues3 != ReturnValues.S_OK)
            {
                App.log.Error("FipPanel failed to get Serial Number. " + returnValues1);
            }
            else
            {
                App.log.Info("FipPanel Serial Number " + SerialNumber);

                _settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                "\\mhwlng\\fip-elite\\" + SerialNumber;

                
                if (File.Exists(_settingsPath))
                {
                    try
                    {
                        _currentTab = (LCDTab) uint.Parse(File.ReadAllText(_settingsPath));
                    }
                    catch
                    {
                        _currentTab = LCDTab.None;
                    }
                }
                else
                {
                    (new FileInfo(_settingsPath)).Directory?.Create();

                    File.WriteAllText(_settingsPath, ((int)_currentTab).ToString());
                }
                
                AddPage(DEFAULT_PAGE, true);

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

                _currentTabCursor = LCDTab.None;

                CurrentLCDYOffset = 0;

                File.WriteAllText(_settingsPath, ((int)_currentTab).ToString());
            }

            _currentPage = LCDPage.Collapsed;

            return true;
        }

        private void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context)
        {
            if (device == FipDevicePointer)
            {
                if (bActivated != 0)
                {
                    /*   
                    _lastTab = LCDTab.Init;
                    _currentTab = LCDTab.Map;
                    _currentPage = LCDPage.Collapsed;
                    CurrentLCDYOffset = 0;
                    File.WriteAllText(_settingsPath, ((int)_currentTab).ToString());
                   */

                    RefreshDevicePage();
                }
            }
        }

        public uint CalculateButton (LCDTab tab)
        {
            var currentPage = (((int)tab - 1) / 6);

            return (uint)(1 << ((int)tab + 4 - (currentPage * 6)));
        }

        public void HandleJoystickButton(JoystickButton joystickButton, bool state)
        {
            uint buttons = 0;

            var currentPage = (LCDPage)(((uint)_currentTab - 1) / 6);
            if (_currentTab == LCDTab.None)
            {
                currentPage = LCDPage.HomeMenu;
            }

            if (state)
            {
                switch (joystickButton)
                {
                    case JoystickButton.Up:
                        if (_currentPage == LCDPage.Collapsed)
                        {
                            buttons = 4;
                        }
                        else
                        {
                            if (_currentTabCursor == LCDTab.None)
                            {
                                _currentTabCursor = (LCDTab)((int)currentPage * 6) + 1;
                            }

                            switch (_currentTabCursor)
                            {
                                case LCDTab.Missions:
                                    _currentTabCursor = Data.TargetData.TargetLocked ? _currentTabCursor - 1 : _currentTabCursor - 2;
                                    break;
                                case LCDTab.ShipMenu:
                                    if (Missions.MissionList.Count > 0)
                                    {
                                        _currentTabCursor -= 1;
                                    }
                                    else
                                    {
                                        _currentTabCursor = Data.TargetData.TargetLocked ? _currentTabCursor -2 : _currentTabCursor - 3;
                                    }
                                    break;

                                case LCDTab.Cargo:
                                    _currentTabCursor = Material.MaterialList.Count > 0 ? _currentTabCursor - 1 : _currentTabCursor - 2;
                                    break;

                                case LCDTab.Events:
                                    if (Cargo.CargoList.Count > 0)
                                    {
                                        _currentTabCursor -= 1;
                                    }
                                    else
                                    {
                                        _currentTabCursor = Material.MaterialList.Count > 0 ? _currentTabCursor -2  : _currentTabCursor - 3;
                                    }
                                    break;

                                case LCDTab.Commander:
                                    _currentTabCursor = LCDTab.LocationsMenu;
                                    break;
                                case LCDTab.ShipBack:
                                    _currentTabCursor = LCDTab.Events;
                                    break;
                                case LCDTab.LocationsBack:
                                    _currentTabCursor = LCDTab.Mining;
                                    break;

                                case LCDTab.Navigation:
                                case LCDTab.Target:
                                case LCDTab.LocationsMenu:
                                case LCDTab.Ship:
                                case LCDTab.Materials:
                                case LCDTab.POI:
                                case LCDTab.Map:
                                case LCDTab.Powers:
                                case LCDTab.Mining:
                                    _currentTabCursor -= 1;
                                    break;
                            }

                            buttons = 2048;
                        }
                        break;
                    case JoystickButton.Down:
                        if (_currentPage == LCDPage.Collapsed)
                        {
                            buttons = 2;
                        }
                        else
                        {
                            if (_currentTabCursor == LCDTab.None)
                            {
                                _currentTabCursor = (LCDTab)((int)currentPage * 6) + 1;
                            }

                            switch (_currentTabCursor)
                            {
                                case LCDTab.Navigation:
                                    if (Data.TargetData.TargetLocked)
                                    {
                                        _currentTabCursor += 1;
                                    }
                                    else
                                    {
                                        _currentTabCursor = Missions.MissionList.Count > 0 ? _currentTabCursor + 2 : _currentTabCursor + 3;
                                    }
                                    break;
                                case LCDTab.Target:
                                    _currentTabCursor = Missions.MissionList.Count > 0 ? _currentTabCursor + 1 : _currentTabCursor + 2;
                                    break;

                                case LCDTab.Ship:
                                    if (Material.MaterialList.Count > 0)
                                    {
                                        _currentTabCursor += 1;
                                    }
                                    else
                                    {
                                        _currentTabCursor = Cargo.CargoList.Count > 0 ? _currentTabCursor + 2 : _currentTabCursor + 3;
                                    }
                                    break;

                                case LCDTab.Materials:
                                    _currentTabCursor = Cargo.CargoList.Count > 0 ? _currentTabCursor + 1 : _currentTabCursor + 2;
                                    break;

                                case LCDTab.LocationsMenu:
                                    _currentTabCursor = LCDTab.Commander;
                                    break;
                                case LCDTab.Events:
                                    _currentTabCursor = LCDTab.ShipBack;
                                    break;
                                case LCDTab.Mining:
                                    _currentTabCursor = LCDTab.LocationsBack;
                                    break;

                                case LCDTab.Commander:
                                case LCDTab.Missions:
                                case LCDTab.ShipMenu:
                                case LCDTab.ShipBack:
                                case LCDTab.Cargo:
                                case LCDTab.LocationsBack:
                                case LCDTab.POI:
                                case LCDTab.Map:
                                case LCDTab.Powers:
                                    _currentTabCursor += 1;
                                    break;
                            }

                            buttons = 2048; // refresh
                        }
                        break;
                    case JoystickButton.Left:
                        if (_currentPage == LCDPage.Collapsed)
                        {
                            buttons = 16;
                        }
                        else
                        {
                            if (_currentTabCursor == LCDTab.None)
                            {
                                _currentTabCursor = (LCDTab)((int)currentPage * 6) + 1;
                            }

                            switch (((int)_currentTabCursor - 1) / 6)
                            {
                                case 1:
                                    buttons = 32; // back
                                    _currentTabCursor = LCDTab.ShipMenu;
                                    break;
                                case 2:
                                    buttons = 32; // back
                                    _currentTabCursor = LCDTab.LocationsMenu;
                                    break;
                            }
                        }
                        break;
                    case JoystickButton.Right:
                        if (_currentPage == LCDPage.Collapsed)
                        {
                            buttons = 8;
                        }
                        else
                        {
                            if (_currentTabCursor == LCDTab.None)
                            {
                                _currentTabCursor = (LCDTab)((int)currentPage * 6) + 1;
                            }

                            switch (_currentTabCursor)
                            {
                                case LCDTab.ShipMenu:
                                    buttons = CalculateButton(_currentTabCursor);
                                    _currentTabCursor = LCDTab.ShipBack;
                                    break;
                                case LCDTab.LocationsMenu:
                                    buttons = CalculateButton(_currentTabCursor);
                                    _currentTabCursor = LCDTab.LocationsBack;
                                    break;
                            }
                        }
                        break;
                    case JoystickButton.Push:

                        if (_currentPage == LCDPage.Collapsed)
                        {
                            if (_currentTabCursor == LCDTab.None)
                            {
                                if (_currentTab != LCDTab.None)
                                {
                                    _currentTabCursor = _currentTab;
                                }
                                else
                                {
                                    _currentTabCursor = (LCDTab)((int)currentPage * 6) + 1;
                                }
                            }

                            buttons = 32;
                        }
                        else
                        {
                            if (_currentTabCursor == LCDTab.None)
                            {
                                _currentTabCursor = (LCDTab)((int)currentPage * 6) + 1;
                            }

                            buttons = CalculateButton(_currentTabCursor);

                            switch (_currentTabCursor)
                            {
                                case LCDTab.ShipMenu:
                                    _currentTabCursor = LCDTab.ShipBack;
                                    break;
                                case LCDTab.LocationsMenu:
                                    _currentTabCursor = LCDTab.LocationsBack;
                                    break;
                                case LCDTab.ShipBack:
                                    _currentTabCursor = LCDTab.ShipMenu;
                                    break;
                                case LCDTab.LocationsBack:
                                    _currentTabCursor = LCDTab.LocationsMenu;
                                    break;
                            }
                        }

                        break;
                }
            }

            SoftButtonCallback(FipDevicePointer, (IntPtr) buttons, (IntPtr)null);
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
                        if (state && (_currentTab == LCDTab.POI || _currentTab == LCDTab.Powers ||
                                      _currentTab == LCDTab.Materials || _currentTab == LCDTab.Map ||
                                      _currentTab == LCDTab.Ship || _currentTab == LCDTab.Mining))
                        {

                            _currentCard[(int) _currentTab]++;
                            _currentZoomLevel[(int) _currentTab]++;
                            CurrentLCDYOffset = 0;

                            mustRefresh = true;
                        }

                        break;
                    case 16: // scroll anti-clockwise

                        if (state && (_currentTab == LCDTab.POI || _currentTab == LCDTab.Powers ||
                                      _currentTab == LCDTab.Materials || _currentTab == LCDTab.Map ||
                                      _currentTab == LCDTab.Ship || _currentTab == LCDTab.Mining))
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

                if (!mustRefresh)
                {

                    switch (_currentPage)
                    {
                        case LCDPage.Collapsed:
                            if (state || !blockNextUpState)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = (LCDPage) (((uint) _currentTab - 1) / 6);
                                        if (_currentTab == LCDTab.None)
                                        {
                                            _currentPage = LCDPage.HomeMenu;
                                        }

                                        _lastTab = LCDTab.Init;

                                        break;
                                }
                            }

                            break;
                        case LCDPage.HomeMenu:
                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = SetTab(LCDTab.Commander);
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LCDTab.Navigation);
                                        break;
                                    case 128:
                                        if (Data.TargetData.TargetLocked)
                                        {
                                            mustRefresh = SetTab(LCDTab.Target);
                                        }

                                        break;
                                    case 256:
                                        if (Missions.MissionList.Count > 0)
                                        {
                                            mustRefresh = SetTab(LCDTab.Missions);
                                        }

                                        break;
                                    case 512:
                                        mustRefresh = true;
                                        _currentPage = LCDPage.ShipMenu;
                                        _lastTab = LCDTab.Init;
                                        break;
                                    case 1024:
                                        mustRefresh = true;
                                        _currentPage = LCDPage.LocationsMenu;
                                        _lastTab = LCDTab.Init;
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        break;
                                }
                            }

                            break;
                        case LCDPage.ShipMenu:
                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = LCDPage.HomeMenu;
                                        _lastTab = LCDTab.Init;
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LCDTab.Ship);
                                        break;
                                    case 128:
                                        if (Material.MaterialList.Count > 0)
                                        {
                                            mustRefresh = SetTab(LCDTab.Materials);
                                        }

                                        break;
                                    case 256:
                                        if (Cargo.CargoList.Count > 0)
                                        {
                                            mustRefresh = SetTab(LCDTab.Cargo);
                                        }
                                        break;
                                    case 512:
                                        mustRefresh = SetTab(LCDTab.Events);
                                        break;
                                    case 1024:
                                        // empty
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        break;
                                }
                            }

                            break;
                        case LCDPage.LocationsMenu:

                            if (state)
                            {
                                switch (button)
                                {
                                    case 32:
                                        mustRefresh = true;
                                        _currentPage = LCDPage.HomeMenu;
                                        _lastTab = LCDTab.Init;
                                        break;
                                    case 64:
                                        mustRefresh = SetTab(LCDTab.POI);
                                        break;
                                    case 128:
                                        mustRefresh = SetTab(LCDTab.Map);
                                        break;
                                    case 256:
                                        mustRefresh = SetTab(LCDTab.Powers);
                                        break;
                                    case 512:
                                        mustRefresh = SetTab(LCDTab.Mining);
                                        break;
                                    case 1024:
                                        // empty
                                        break;
                                    case 2048:
                                        mustRefresh = true;
                                        break;
                                }
                            }

                            break;
                    }
                }

                blockNextUpState = state;

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
                        graphics.DrawPolygon(_whitePen, History.TravelHistoryPoints.ToArray());

                        var zoomLevel = _currentZoomLevel[(int)_currentTab] / 2.0 + 1;

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

                            var imgX = (spaceX - History.SpaceMinX) / (History.SpaceMaxX - History.SpaceMinX) * image.Width;
                            var imgY = (History.SpaceMaxZ - spaceZ) / (History.SpaceMaxZ - History.SpaceMinZ) * image.Height;

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
                if (Missions.MissionList.Count == 0 && _currentTab == LCDTab.Missions)
                {
                    SetTab(LCDTab.Navigation);
                }
                else if (!Data.TargetData.TargetLocked && _currentTab == LCDTab.Target)
                {
                    SetTab(LCDTab.Navigation);
                }
                else if (Material.MaterialList.Count == 0 && _currentTab == LCDTab.Materials)
                {
                    SetTab(LCDTab.Map);
                }
                else if (Cargo.CargoList.Count == 0 && _currentTab == LCDTab.Cargo)
                {
                    SetTab(LCDTab.Map);
                }

                using (var fipImage = new Bitmap(320, 240))
                {
                    using (var graphics = Graphics.FromImage(fipImage))
                    {
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
                                    _currentCard[(int)_currentTab] = 7;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 7)
                                {
                                    _currentCard[(int)_currentTab] = 0;
                                }

                                break;

                            case LCDTab.Powers:

                                if (_currentCard[(int)_currentTab] < 0)
                                {
                                    _currentCard[(int)_currentTab] = 10;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 10)
                                {
                                    _currentCard[(int)_currentTab] = 0;
                                }

                                break;

                            case LCDTab.Materials:

                                if (_currentCard[(int)_currentTab] < 0)
                                {
                                    _currentCard[(int)_currentTab] = 2;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 2)
                                {
                                    _currentCard[(int)_currentTab] = 0;
                                }

                                break;

                            case LCDTab.Mining:

                                if (_currentCard[(int)_currentTab] < 0)
                                {
                                    _currentCard[(int)_currentTab] = 3;
                                }
                                else
                                if (_currentCard[(int)_currentTab] > 3)
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
                            try
                            {


                                switch (_currentTab)
                                {
                                    //----------------------

                                    case LCDTab.None:

                                        str =
                                            Engine.Razor.Run("init.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage
                                            });

                                        break;

                                    case LCDTab.Commander:

                                        str =
                                            Engine.Razor.Run("commander.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
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

                                                CqcRank = Data.CommanderData.CqcRank,

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

                                    case LCDTab.Ship:

                                        var shipData = Ships.GetCurrentShip() ?? new Ships.ShipData();

                                        str =
                                            Engine.Razor.Run("ship.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = _currentCard[(int) _currentTab],

                                                CurrentShip = Ships.ShipsList.FirstOrDefault(x => x.Stored == false),

                                                StoredShips = Ships.ShipsList.Where(x => x.Stored == true)
                                                    .OrderBy(x => x.Distance).ThenBy(x => x.ShipType).ToList()

                                            });

                                        break;

                                    case LCDTab.Navigation:

                                        str =
                                            Engine.Razor.Run("navigation.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,

                                                StarSystem = Data.LocationData.StarSystem,

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
                                                Powers = Data.LocationData.Powers


                                            });

                                        break;

                                    case LCDTab.Target:

                                        str =
                                            Engine.Razor.Run("target.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
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
                                                ShieldHealth = Data.TargetData.ShieldHealth,

                                            });

                                        break;

                                    case LCDTab.Missions:

                                        str =
                                            Engine.Razor.Run("missions.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,

                                                MissionList = Missions.MissionList
                                            });

                                        break;

                                    case LCDTab.POI:

                                        lock (App.RefreshJsonLock)
                                        {
                                            str =
                                                Engine.Razor.Run("poi.cshtml", null, new
                                                {
                                                    CurrentTab = _currentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = _currentCard[(int) _currentTab],

                                                    NearbyPoiList = Poi.NearbyPoiList,

                                                    NearbyCnbSystemsList = Data.NearbyCnbSystemsList,

                                                    NearbyStationList = Data.NearbyStationList

                                                });
                                        }

                                        break;

                                    //----------------------

                                    case LCDTab.Map:


                                        str =
                                            Engine.Razor.Run("map.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,

                                                _currentZoomLevel = _currentZoomLevel[(int) _currentTab],


                                            });

                                        break;


                                    case LCDTab.Powers:

                                        lock (App.RefreshJsonLock)
                                        {
                                            str =
                                                Engine.Razor.Run("powers.cshtml", null, new
                                                {
                                                    CurrentTab = _currentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = _currentCard[(int) _currentTab],

                                                    NearbyPowerStationList = Data.NearbyPowerStationList

                                                });
                                        }

                                        break;


                                    case LCDTab.Materials:

                                        str =
                                            Engine.Razor.Run("materials.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = _currentCard[(int) _currentTab],

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

                                    case LCDTab.Cargo:

                                        var cargo = Cargo.CargoList
                                            .Where(x => x.Value.Count > 0 && x.Value.MissionID == 0)
                                            .Select(x => x.Value).OrderBy(x => x.Name).ToList();

                                        var missionCargo = Cargo.CargoList
                                            .Where(x => x.Value.Count > 0 && x.Value.MissionID > 0)
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
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,
                                                CurrentCard = _currentCard[(int) _currentTab],

                                                Cargo = cargo,
                                                CargoCargoCount = cargo.Count,

                                                MissionCargo = missionCargo,
                                                MissionCargoCount = missionCargo.Count,

                                                StolenCargo = stolenCargo,
                                                StolenCargoCount = stolenCargo.Count
                                            });

                                        break;

                                    case LCDTab.Mining:

                                        lock (App.RefreshJsonLock)
                                        {

                                            str =
                                                Engine.Razor.Run("mining.cshtml", null, new
                                                {
                                                    CurrentTab = _currentTab,
                                                    CurrentPage = _currentPage,
                                                    CurrentCard = _currentCard[(int)_currentTab],

                                                    NearbyHotspotSystemsList = Data.NearbyHotspotSystemsList,

                                                    NearbyMiningStationsList = Data.NearbyMiningStationsList

                                                });
                                        }

                                        break;


                                    case LCDTab.Events:

                                        var eventlist = "";
                                        foreach (var b in Data.EventHistory)
                                        {
                                            eventlist += b + "<br/>";
                                        }

                                        str =
                                            Engine.Razor.Run("events.cshtml", null, new
                                            {
                                                CurrentTab = _currentTab,
                                                CurrentPage = _currentPage,

                                                EventList = eventlist
                                            });

                                        break;

                                }


                            }
                            catch (Exception ex)
                            {
                                App.log.Error(ex);
                            }
                        }

                        graphics.Clear(Color.Black);

                        if (_currentTab >= 0)
                        {

                            if (mustRender)
                            {
                                var measureData =HtmlRender.Measure(graphics, str, HtmlWindowWidth, App.cssData,null, OnImageLoad);

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


                        if (_currentTab != LCDTab.Map)
                        {
                            if (mustRender)
                            {
                                var cardcaptionstr =
                                    Engine.Razor.Run("cardcaption.cshtml", null, new
                                    {
                                        CurrentTab = _currentTab,
                                        CurrentPage = _currentPage,
                                        CurrentCard = _currentCard[(int)_currentTab]
                                    });

                                cardcaptionHtmlImage = HtmlRender.RenderToImage(cardcaptionstr,
                                    new Size(HtmlWindowWidth, 26), Color.Black, App.cssData, null,
                                    null);
                            }

                            if (cardcaptionHtmlImage != null)
                            {
                                graphics.DrawImage(cardcaptionHtmlImage, HtmlWindowXOffset, 0);
                            }
                        }

                        if (_currentPage != LCDPage.Collapsed)
                        {
                            if (mustRender)
                            {
                                var menustr =
                                    Engine.Razor.Run("menu.cshtml", null, new
                                    {
                                        CurrentTab = _currentTab,
                                        CurrentPage = _currentPage,
                                        Cursor = _currentTabCursor,

                                        TargetLocked = Data.TargetData.TargetLocked,

                                        MissionCount = Missions.MissionList.Count,

                                        MaterialCount = Material.MaterialList.Count,

                                        CargoCount = Cargo.CargoList.Count
                                    });

                                menuHtmlImage = HtmlRender.RenderToImage(menustr,
                                    new Size(HtmlMenuWindowWidth, HtmlWindowHeight), Color.Black, App.cssData, null,
                                    OnImageLoad);
                            }

                            if (menuHtmlImage != null)
                            {
                                graphics.DrawImage(menuHtmlImage, 0, 0);
                            }
                        }


#if DEBUG
                        fipImage.Save(@"screenshot"+(int)_currentTab+"_"+ _currentCard[(int)_currentTab] + ".png", ImageFormat.Png);
#endif

                        fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);


                        SetImage(DEFAULT_PAGE, fipImage);

                        var tabPage = (LCDPage)(((uint)_currentTab - 1) / 6);
                        if (_currentTab == LCDTab.None)
                        {
                            tabPage = LCDPage.HomeMenu;
                        }

                        if (_lastTab != _currentTab)
                        {
                            if (_currentPage == LCDPage.Collapsed)
                            {
                                if (_lastTab > 0)
                                {
                                    DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                        (uint) _lastTab - ((uint) _lastTab - 1) / 6 * 6, false);
                                }

                                if (_currentTab > 0)
                                {
                                    DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                        (uint) _currentTab - ((uint) _currentTab - 1) / 6 * 6, false);
                                }

                                DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                    1, true);

                            }
                            else if (tabPage != _currentPage)
                            {
                                if (_lastTab > 0)
                                {
                                    DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                        (uint) _lastTab - ((uint) _lastTab - 1) / 6 * 6, false);
                                }

                                if (_currentTab > 0)
                                {
                                    DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                        (uint) _currentTab - ((uint) _currentTab - 1) / 6 * 6, false);
                                }
                            }
                            else
                            {
                                DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                    1, false);

                                if (_currentTab > 0)
                                {
                                    DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                                        (uint) _currentTab - ((uint) _currentTab - 1) / 6 * 6, true);
                                }
                            }

                            _lastTab = _currentTab;
                        }

                    }
                }
            }
        }


    }
}
