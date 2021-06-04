using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using TheArtOfDev.HtmlRenderer.Core;
using EliteJournalReader;
using SharpDX.DirectInput;
using System.Collections.Specialized;
using System.Linq;


// ReSharper disable StringLiteralTypo

namespace Elite
{

    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        public static bool IsShuttingDown { get; set; }

        public static readonly object RefreshJsonLock = new object();
        public static readonly object RefreshSystemLock = new object();

        public static Task HWInfoTask;
        private static CancellationTokenSource _hwInfoTokenSource = new CancellationTokenSource();

        public static Task JsonTask;
        private static CancellationTokenSource _jsonTokenSource = new CancellationTokenSource();

        private static Task _joystickTask;
        private static CancellationTokenSource _joystickTokenSource = new CancellationTokenSource();

        private static Mutex _mutex;

        private TaskbarIcon _notifyIcon;

        public static readonly FipHandler FipHandler = new FipHandler();

        public static JournalWatcher journalWatcher;
        public static CargoWatcher cargoWatcher;
        public static NavRouteWatcher navRouteWatcher;
        public static BackPackWatcher backPackWatcher;

        private static StatusWatcher _statusWatcher;

        public static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CssData CssData;

        // Initialize DirectInput
        private static readonly DirectInput DirectInput = new DirectInput();

        private static Joystick _joystick;

        private static string _pid;
        private static string _vid;
        private static int _upButton;
        private static int _downButton;
        private static int _leftButton;
        private static int _rightButton;
        private static int _pushButton;

        private static int _navigationButton;
        private static int _targetButton;
        private static int _commanderButton;
        private static int _galnetButton;
        private static int _missionsButton;
        private static int _chatButton;
        private static int _hWInfoButton;
        private static int _shipButton;
        private static int _materialsButton;
        private static int _cargoButton;
        private static int _engineerButton;
        private static int _shipLockerButton;
        private static int _backPackButton;
        private static int _pOIButton;
        private static int _galaxyButton;
        private static int _engineersButton;
        private static int _powersButton;
        private static int _miningButton;

        public static int WindowWidth;
        public static int WindowHeight;

        public static string FipSerialNumber;

        private static bool[] _lastButtonState = new bool[256];

        public static string ExePath;

        private static CachedSound _clickSound = null;

        public static void PlayClickSound()
        {
            if (_clickSound != null)
            {
                try
                {
                    AudioPlaybackEngine.Instance.PlaySound(_clickSound);
                }
                catch (Exception ex)
                {
                    Log.Error( $"PlaySound: {ex}");
                }
            }
        }

        private static void GetExePath()
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            ExePath = Path.GetDirectoryName(strExeFilePath);
        }

        private static void RefreshJson(SplashScreenWindow splashScreen = null)
        {
            lock (RefreshJsonLock)
            {
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Inter Stellar Factors...");
                Station.FullStationList[Station.PoiTypes.InterStellarFactors] = Station.GetAllStations(@"Data\interstellarfactors.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Raw Material Traders...");
                Station.FullStationList[Station.PoiTypes.RawMaterialTraders] = Station.GetAllStations(@"Data\rawmaterialtraders.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Manufactured Material Traders...");
                Station.FullStationList[Station.PoiTypes.ManufacturedMaterialTraders] = Station.GetAllStations(@"Data\manufacturedmaterialtraders.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Encoded Data Traders..");
                Station.FullStationList[Station.PoiTypes.EncodedDataTraders] = Station.GetAllStations(@"Data\encodeddatatraders.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Human Technology Brokers...");
                Station.FullStationList[Station.PoiTypes.HumanTechnologyBrokers] = Station.GetAllStations(@"Data\humantechnologybrokers.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Guardian Technology Brokers...");
                Station.FullStationList[Station.PoiTypes.GuardianTechnologyBrokers] = Station.GetAllStations(@"Data\guardiantechnologybrokers.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Aisling Duval Stations...");
                Station.FullPowerStationList[Station.PowerTypes.AislingDuval] = Station.GetAllStations(@"Data\aislingduval.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Archon Delaine Stations...");
                Station.FullPowerStationList[Station.PowerTypes.ArchonDelaine] = Station.GetAllStations(@"Data\archondelaine.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Arissa Lavigny Duval Stations...");
                Station.FullPowerStationList[Station.PowerTypes.ArissaLavignyDuval] = Station.GetAllStations(@"Data\arissalavignyduval.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Denton Patreus Stations...");
                Station.FullPowerStationList[Station.PowerTypes.DentonPatreus] = Station.GetAllStations(@"Data\dentonpatreus.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Edmund Mahon Stations...");
                Station.FullPowerStationList[Station.PowerTypes.EdmundMahon] = Station.GetAllStations(@"Data\edmundmahon.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Felicia Winters Stations...");
                Station.FullPowerStationList[Station.PowerTypes.FeliciaWinters] = Station.GetAllStations(@"Data\feliciawinters.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Li Yong-Rui Stations...");
                Station.FullPowerStationList[Station.PowerTypes.LiYongRui] = Station.GetAllStations(@"Data\liyongrui.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Pranav Antal Stations...");
                Station.FullPowerStationList[Station.PowerTypes.PranavAntal] = Station.GetAllStations(@"Data\pranavantal.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Yuri Grom Stations...");
                Station.FullPowerStationList[Station.PowerTypes.YuriGrom] = Station.GetAllStations(@"Data\yurigrom.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Zachary Hudson Stations...");
                Station.FullPowerStationList[Station.PowerTypes.ZacharyHudson] = Station.GetAllStations(@"Data\zacharyhudson.json");
                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Zemina Torval Stations...");
                Station.FullPowerStationList[Station.PowerTypes.ZeminaTorval] = Station.GetAllStations(@"Data\zeminatorval.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading All System Stations ...");
                Station.SystemStations = Station.GetAllStations(@"Data\fullstationlist.json").GroupBy(x => x.SystemName)
                    .ToDictionary(x => x.Key, x => x.OrderBy(y => y.DistanceToArrival).ToList());

                Station.MarketIdStations = Station.GetAllStations(@"Data\fullstationlist.json").GroupBy(x => x.MarketId)
                    .ToDictionary(x => x.Key, x => x.FirstOrDefault());

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading CNB Systems...");
                CnbSystems.FullCnbSystemsList = CnbSystems.GetAllCnbSystems(@"Data\cnbsystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Populated Systems...");
                PopulatedSystems.SystemList = PopulatedSystems.GetAllPopupulatedSystems(@"Data\populatedsystemsEDDB.json").GroupBy(x => x.Name)
                    .ToDictionary(x => x.Key, x => x.First());

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Painite Hotspot Systems...");
                HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.Painite] = HotspotSystems.GetAllHotspotSystems(@"Data\painitesystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading LTD Hotspot Systems...");
                HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.LTD] = HotspotSystems.GetAllHotspotSystems(@"Data\ltdsystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Platinum Hotspot Systems...");
                HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.Platinum] = HotspotSystems.GetAllHotspotSystems(@"Data\platinumsystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Painite Mining Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.Painite] = MiningStations.GetAllMiningStations(@"Data\painitestations.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading LTD Mining Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.LTD] = MiningStations.GetAllMiningStations(@"Data\ltdstations.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Platinum Mining Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.Platinum] = MiningStations.GetAllMiningStations(@"Data\platinumstations.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Tritium Buy Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.TritiumBuy] = MiningStations.GetAllMiningStations(@"Data\tritiumbuystations.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Tritium Sell Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.TritiumSell] = MiningStations.GetAllMiningStations(@"Data\tritiumstations.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Community Goals...");
                var cg = CommunityGoals.GetCommunityGoals(@"Data\communitygoals.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Galnet News feed...");
                var galnet = Galnet.GetGalnet(@"Data\galnet.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Galnet Images...");
                Galnet.GetGalnetImages(galnet);

                Galnet.GalnetList = cg.Concat(galnet).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            }

            if (splashScreen == null)
            {
                Data.HandleJson();
            }

        }

        private static void RunProcess(string fileName)
        {
            var process = new Process();
            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = fileName;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
        }

        private static void HandleJoystickButton(JoystickState state, JoystickButton button, int buttonId)
        {
            if (buttonId > 0 && state.Buttons.Length >= buttonId)
            {
                var buttonState = state.Buttons[buttonId - 1];
                var oldButtonState = _lastButtonState[buttonId - 1];

                if (buttonState && buttonState == oldButtonState)
                {
                    FipHandler.HandleJoystickButton(button, false, false);
                }

                if (buttonState || buttonState != oldButtonState)
                {
                    FipHandler.HandleJoystickButton(button, buttonState, oldButtonState);
                }

                _lastButtonState[buttonId - 1] = buttonState;
            }

        }

        private static void MigrateSettings()
        {
            try
            {
                if (!Elite.Properties.Settings.Default.Upgraded)
                {
                    Elite.Properties.Settings.Default.Upgrade();
                    Elite.Properties.Settings.Default.Upgraded = true;
                    Elite.Properties.Settings.Default.Save();
                }

            }
            catch
            {
                // ignored
            }
        }

        protected override void OnStartup(StartupEventArgs evtArgs)
        {
            const string appName = "Fip-Elite";

            _mutex = new Mutex(true, appName, out var createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                Current.Shutdown();
            }

            GetExePath();

            base.OnStartup(evtArgs);

            log4net.Config.XmlConfigurator.Configure();

            MigrateSettings();

            
            _clickSound = null;

            if (File.Exists(Path.Combine(ExePath, "appSettings.config")) &&
                ConfigurationManager.GetSection("appSettings") is NameValueCollection appSection)
            {
                if (File.Exists(Path.Combine(ExePath, "Sounds", appSection["clickSound"])))
                {
                    try
                    {
                        _clickSound = new CachedSound(Path.Combine(ExePath, "Sounds", appSection["clickSound"]));
                    }
                    catch (Exception ex)
                    {
                        _clickSound = null;

                        Log.Error($"CachedSound: {ex}");
                    }

                }
            }

            var defaultFilter = @"Journal.*.log";
//#if DEBUG
            //defaultFilter = @"JournalAlpha.*.log";
//#endif

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            _notifyIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Elite;component/Hourglass.ico"));
            _notifyIcon.ToolTipText = "Elite Dangerous Flight Instrument Panel [WORKING]";

            var splashScreen = new SplashScreenWindow();
            splashScreen.Show();

            Task.Run(() =>
            {
                var config = new TemplateServiceConfiguration
                {
                    TemplateManager = new ResolvePathTemplateManager(new[] { Path.Combine(ExePath, "Templates") }),
                    DisableTempFileLocking = true,
                    BaseTemplateType = typeof(HtmlSupportTemplateBase<>) /*,
                    Namespaces = new HashSet<string>(){
                        "System",
                        "System.Linq",
                        "System.Collections",
                        "System.Collections.Generic"
                        }*/
                };

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading cshtml templates...");

                Engine.Razor = RazorEngineService.Create(config);

                Engine.Razor.Compile("menu.cshtml", null);
                Engine.Razor.Compile("cardcaption.cshtml", null);
                Engine.Razor.Compile("layout.cshtml", null);
                Engine.Razor.Compile("init.cshtml", null);

                Engine.Razor.Compile("commander.cshtml", null);
                Engine.Razor.Compile("ship.cshtml", null);
                Engine.Razor.Compile("navigation.cshtml", null);
                Engine.Razor.Compile("target.cshtml", null);
                Engine.Razor.Compile("galnet.cshtml", null);
                Engine.Razor.Compile("poi.cshtml", null);
                Engine.Razor.Compile("hwinfo.cshtml", null);

                Engine.Razor.Compile("galaxy.cshtml", null);
                Engine.Razor.Compile("engineers.cshtml", null);
                Engine.Razor.Compile("powers.cshtml", null);
                Engine.Razor.Compile("materials.cshtml", null);
                Engine.Razor.Compile("cargo.cshtml", null);
                Engine.Razor.Compile("engineer.cshtml", null);
                Engine.Razor.Compile("mining.cshtml", null);
                Engine.Razor.Compile("missions.cshtml", null);
                Engine.Razor.Compile("events.cshtml", null);
                Engine.Razor.Compile("chat.cshtml", null);

                Engine.Razor.Compile("backpack.cshtml", null);
                Engine.Razor.Compile("shiplocker.cshtml", null);

                CssData = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.ParseStyleSheet(
                    File.ReadAllText(Path.Combine(ExePath, "Templates\\styles.css")), true);

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Engineers...");
                Data.EngineersList = Station.GetEngineers(@"Data\engineers.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Engineering Materials...");
                (Engineer.EngineeringMaterials, Engineer.EngineeringMaterialsByKey) = Engineer.GetAllEngineeringMaterials(@"Data\entryData.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Blueprints...");
                Engineer.Blueprints = Engineer.GetAllBlueprints(@"Data\blueprints.json", Engineer.EngineeringMaterials);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Engineer Blueprints...");
                Engineer.EngineerBlueprints = Engineer.GetEngineerBlueprints(@"Data\blueprints.json", Engineer.EngineeringMaterials);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading POI Items...");
                Poi.FullPoiList = Poi.GetAllPois(); //?.GroupBy(x => x.System.Trim().ToLower()).ToDictionary(x => x.Key, x => x.ToList());

                RefreshJson(splashScreen);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading History...");
                var path = History.GetEliteHistory(defaultFilter);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Getting Shopping List from EDEngineer...");
                Engineer.GetCommanderName();
                Engineer.GetShoppingList();
                Engineer.GetBestSystems();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Getting sensor data from HWInfo...");

                HWInfo.ReadMem("HWINFO.INC");
                
                if (HWInfo.SensorData.Any())
                {
                    HWInfo.SaveDataToFile(@"Data\hwinfo.json");
                }

                if (File.Exists(Path.Combine(ExePath, "joystickSettings.config")) && ConfigurationManager.GetSection("joystickSettings") is NameValueCollection joystickSection)
                {
                    _pid = joystickSection["PID"];
                    _vid = joystickSection["VID"];
                    _upButton = Convert.ToInt32(joystickSection["UpButton"]);
                    _downButton = Convert.ToInt32(joystickSection["DownButton"]);
                    _leftButton = Convert.ToInt32(joystickSection["LeftButton"]);
                    _rightButton = Convert.ToInt32(joystickSection["RightButton"]);
                    _pushButton = Convert.ToInt32(joystickSection["PushButton"]);

                    _navigationButton = Convert.ToInt32(joystickSection["NavigationButton"]);
                    _targetButton = Convert.ToInt32(joystickSection["TargetButton"]);
                    _commanderButton = Convert.ToInt32(joystickSection["CommanderButton"]);
                    _galnetButton = Convert.ToInt32(joystickSection["GalnetButton"]);
                    _missionsButton = Convert.ToInt32(joystickSection["MissionsButton"]);
                    _chatButton = Convert.ToInt32(joystickSection["ChatButton"]);
                    _hWInfoButton = Convert.ToInt32(joystickSection["HWInfoButton"]);
                    _shipButton = Convert.ToInt32(joystickSection["ShipButton"]);
                    _materialsButton = Convert.ToInt32(joystickSection["MaterialsButton"]);
                    _cargoButton = Convert.ToInt32(joystickSection["CargoButton"]);
                    _engineerButton = Convert.ToInt32(joystickSection["EngineerButton"]);
                    _shipLockerButton = Convert.ToInt32(joystickSection["ShipLockerButton"]);
                    _backPackButton = Convert.ToInt32(joystickSection["BackPackButton"]);
                    _pOIButton = Convert.ToInt32(joystickSection["POIButton"]);
                    _galaxyButton = Convert.ToInt32(joystickSection["GalaxyButton"]);
                    _engineersButton = Convert.ToInt32(joystickSection["EngineersButton"]);
                    _powersButton = Convert.ToInt32(joystickSection["PowersButton"]);
                    _miningButton = Convert.ToInt32(joystickSection["MiningButton"]);

                    if (!string.IsNullOrEmpty(_pid) && !string.IsNullOrEmpty(_vid) && _upButton > 0 && _downButton > 0 && _leftButton > 0 && _rightButton > 0 && _pushButton > 0)
                    {
                        splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Looking for Joystick...");

                        FipSerialNumber = joystickSection["FipSerialNumber"];

                        if (!string.IsNullOrEmpty(FipSerialNumber))
                        {
                            Log.Info($"Sending joystick button presses to FIP panel with serial number : {FipSerialNumber}");

                            if (FipSerialNumber.ToLower().Contains("window"))
                            {
                                WindowWidth = Convert.ToInt32(joystickSection["WindowWidth"]);
                                WindowHeight = Convert.ToInt32(joystickSection["WindowHeight"]);

                                FipHandler.AddWindow("window",(IntPtr)0, WindowWidth, WindowHeight);
                            }
                        }

                        Log.Info($"Looking for directinput devices with PID={_pid} and VID={_vid}");

                        Log.Info($"Button numbers : Up={_upButton} Down={_downButton} Left={_leftButton} Right={_rightButton} Push={_pushButton}");

                        foreach (var deviceInstance in DirectInput.GetDevices())
                        {

                            //Device = 17,
                            //Mouse = 18,
                            //Keyboard = 19,

                            //Joystick = 20,
                            //Gamepad = 21,
                            //Driving = 22,
                            //Flight = 23,
                            //FirstPerson = 24,

                            //ControlDevice = 25,
                            //ScreenPointer = 26,
                            //Remote = 27,
                            //Supplemental = 28

                            if (deviceInstance.Type >= DeviceType.Joystick &&
                                deviceInstance.Type <= DeviceType.FirstPerson)
                            {
                                Log.Info("PID:" + deviceInstance.ProductGuid.ToString().Substring(0, 4) + " - VID:" +
                                         deviceInstance.ProductGuid.ToString().Substring(4, 4) + " - " +
                                         deviceInstance.Type.ToString().PadRight(11) + " - " +
                                         deviceInstance.ProductGuid + " - " + deviceInstance.InstanceGuid + " - " +
                                         deviceInstance.InstanceName.Trim().Replace("\0", ""));

                                if (_joystick == null &&
                                    deviceInstance.ProductGuid.ToString().ToUpper().StartsWith(_pid.ToUpper() + _vid.ToUpper()))
                                {
                                    Log.Info(
                                        $"Using Joystick {deviceInstance.InstanceName.Trim().Replace("\0", "")} with Instance Guid {deviceInstance.InstanceGuid}");

                                    _joystick = new Joystick(DirectInput, deviceInstance.InstanceGuid);

                                    //joystick.Properties.BufferSize = 128;

                                    /*
                                    joystick.SetCooperativeLevel(wih,
                                      CooperativeLevel.Background | CooperativeLevel.NonExclusive);*/

                                    _joystick.Acquire();

                                    var joystickToken = _joystickTokenSource.Token;

                                    _joystickTask = Task.Run(async () =>
                                    {
                                        Log.Info("joystick task started");

                                        while (true)
                                        {
                                            if (joystickToken.IsCancellationRequested)
                                            {
                                                joystickToken.ThrowIfCancellationRequested();
                                            }

                                            _joystick.Poll();

                                            var state = _joystick.GetCurrentState();

                                            HandleJoystickButton(state, JoystickButton.Up, _upButton);
                                            HandleJoystickButton(state, JoystickButton.Down, _downButton);
                                            HandleJoystickButton(state, JoystickButton.Left, _leftButton);
                                            HandleJoystickButton(state, JoystickButton.Right, _rightButton);
                                            HandleJoystickButton(state, JoystickButton.Push, _pushButton);

                                            HandleJoystickButton(state, JoystickButton.Navigation, _navigationButton);
                                            HandleJoystickButton(state, JoystickButton.Target, _targetButton);
                                            HandleJoystickButton(state, JoystickButton.Commander, _commanderButton);
                                            HandleJoystickButton(state, JoystickButton.Galnet, _galnetButton);
                                            HandleJoystickButton(state, JoystickButton.Missions, _missionsButton);
                                            HandleJoystickButton(state, JoystickButton.Chat, _chatButton);
                                            HandleJoystickButton(state, JoystickButton.HWInfo, _hWInfoButton);
                                            HandleJoystickButton(state, JoystickButton.Ship, _shipButton);
                                            HandleJoystickButton(state, JoystickButton.Materials, _materialsButton);
                                            HandleJoystickButton(state, JoystickButton.Cargo, _cargoButton);
                                            HandleJoystickButton(state, JoystickButton.Engineer, _engineerButton);
                                            HandleJoystickButton(state, JoystickButton.ShipLocker, _shipLockerButton);
                                            HandleJoystickButton(state, JoystickButton.BackPack, _backPackButton);
                                            HandleJoystickButton(state, JoystickButton.POI, _pOIButton);
                                            HandleJoystickButton(state, JoystickButton.Galaxy, _galaxyButton);
                                            HandleJoystickButton(state, JoystickButton.Engineers, _engineersButton);
                                            HandleJoystickButton(state, JoystickButton.Powers, _powersButton);
                                            HandleJoystickButton(state, JoystickButton.Mining, _miningButton);

                                            /*
                                             TODO 
                                            var pov = state.PointOfViewControllers;
                                            for (var j = 0; j < pov.Length; j++)
                                            {
                                                // 0 up
                                                // 9000 right
                                                // 18000 down
                                                // 27000 left
                                            } */

                                            /*
                                            var bufferedData = joystick.GetBufferedData();
                                            foreach (var data in bufferedData)
                                            {
                                                if (data.Offset >= JoystickOffset.Buttons0 &&
                                                    data.Offset <= JoystickOffset.Buttons127)
                                                {
                                                    var button = data.Offset - JoystickOffset.Buttons0 + 1;
                                                    var state = data.Value > 0;
                                                    if (button == UpButton)
                                                        fipHandler.HandleJoystickButton(JoystickButton.Up, state);
                                                    else if (button == DownButton)
                                                        fipHandler.HandleJoystickButton(JoystickButton.Down, state);
                                                    else if (button == LeftButton)
                                                        fipHandler.HandleJoystickButton(JoystickButton.Left, state);
                                                    else if (button == RightButton)
                                                        fipHandler.HandleJoystickButton(JoystickButton.Right, state);
                                                    else if (button == PushButton)
                                                        fipHandler.HandleJoystickButton(JoystickButton.Push, state);
                                                }
                                            }*/

                                            await Task.Delay(50, _jsonTokenSource.Token);
                                        }

                                    }, joystickToken);
                                }
                            }
                        }

                        if (_joystick == null)
                        {
                            Log.Info($"No joystick found with PID={_pid} and VID={_vid}");
                        }
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    var window = Current.MainWindow = new MainWindow();
                    window.ShowActivated = false;
                    var im = (System.Windows.Controls.Image)window.FindName("im");
                    if (im != null && WindowWidth > 0 && WindowHeight > 0)
                    {
                        im.Width = WindowWidth;
                        im.Height = WindowHeight;
                    }
                });

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Journal Status Watcher...");
                _statusWatcher = new StatusWatcher(path);

                _statusWatcher.StatusUpdated += Data.HandleStatusEvents;

                _statusWatcher.StartWatching();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Journal Watcher...");

                journalWatcher = new JournalWatcher(path, defaultFilter);

                journalWatcher.AllEventHandler += Data.HandleEliteEvents;

                journalWatcher.StartWatching().Wait();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Cargo Watcher...");
                cargoWatcher = new CargoWatcher(path);

                cargoWatcher.CargoUpdated += Data.HandleCargoEvent;

                cargoWatcher.StartWatching();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite NavRoute Watcher...");
                navRouteWatcher = new NavRouteWatcher(path);

                navRouteWatcher.NavRouteUpdated += Data.HandleNavRouteEvent;

                navRouteWatcher.StartWatching();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite BackPack Watcher...");
                backPackWatcher = new BackPackWatcher(path);

                backPackWatcher.BackPackUpdated += Data.HandleBackPackEvent;

                backPackWatcher.StartWatching();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Initializing FIP...");
                if (!FipHandler.Initialize())
                {
                    Current.Shutdown();
                }

                Log.Info("Fip-Elite started");

                Dispatcher.Invoke(() =>
                {
                    var window = Current.MainWindow;

                    if (evtArgs.Args.Length >= 1 && evtArgs.Args[0].ToLower().Contains("mirror") || !FipHandler.InitOk)
                    {
                        Elite.Properties.Settings.Default.Visible = true;
                        Elite.Properties.Settings.Default.Save();
                    }

                    if (window != null && Elite.Properties.Settings.Default.Visible)
                    {
                        window.Show();
                        FipHandler.RefreshDevicePages();
                    }
                    else
                        window.Hide();
                });

                Dispatcher.Invoke(() => { splashScreen.Close(); });

                var jsonToken = _jsonTokenSource.Token;

                JsonTask = Task.Run(async () =>
                {
                    Log.Info("json task started");

                    while (true)
                    {
                        if (jsonToken.IsCancellationRequested)
                        {
                            jsonToken.ThrowIfCancellationRequested();
                        }

                        Dispatcher.Invoke(() =>
                        {
                            _notifyIcon.IconSource =
                                new BitmapImage(new Uri("pack://application:,,,/Elite;component/Hourglass.ico"));

                            _notifyIcon.ToolTipText = "Elite Dangerous Flight Instrument Panel [WORKING]";
                        });

                        RunProcess(Path.Combine(ExePath, "ImportData.exe"));

                        RefreshJson();

                        Dispatcher.Invoke(() =>
                        {
                            _notifyIcon.IconSource =
                                new BitmapImage(new Uri("pack://application:,,,/Elite;component/Elite.ico"));

                            _notifyIcon.ToolTipText = "Elite Dangerous Flight Instrument Panel";
                        });

                        await Task.Delay(30 * 60 * 1000, _jsonTokenSource.Token); // repeat every 30 minutes
                    }

                }, jsonToken);

                var hwInfoToken = _hwInfoTokenSource.Token;

                HWInfoTask = Task.Run(async () =>
                {
                    var result = await MQTT.Connect();
                    
                    Log.Info("HWInfo task started");

                    while (true)
                    {
                        if (hwInfoToken.IsCancellationRequested)
                        {
                            hwInfoToken.ThrowIfCancellationRequested();
                        }

                        HWInfo.ReadMem("HWINFO.INC");

                        FipHandler.RefreshHWInfoPages();

                        await Task.Delay(5 * 1000, _hwInfoTokenSource.Token); // repeat every 5 seconds
                    }

                }, hwInfoToken);

            });

        }
      

        protected override void OnExit(ExitEventArgs e)
        {
            Elite.Properties.Settings.Default.Save();

            _statusWatcher.StatusUpdated -= Data.HandleStatusEvents;

            _statusWatcher.StopWatching();

            journalWatcher.AllEventHandler -= Data.HandleEliteEvents;

            journalWatcher.StopWatching();

            cargoWatcher.CargoUpdated -= Data.HandleCargoEvent;

            cargoWatcher.StopWatching();

            navRouteWatcher.NavRouteUpdated -= Data.HandleNavRouteEvent;

            navRouteWatcher.StopWatching();

            backPackWatcher.BackPackUpdated -= Data.HandleBackPackEvent;

            backPackWatcher.StopWatching();

            FipHandler.Close();

            _notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner

            _hwInfoTokenSource.Cancel();

            var hwInfoToken = _hwInfoTokenSource.Token;

            try
            {
                HWInfoTask?.Wait(hwInfoToken);
            }
            catch (OperationCanceledException)
            {
                Log.Info("HWInfo background task ended");
            }
            finally
            {
                _hwInfoTokenSource.Dispose();
            }

            _jsonTokenSource.Cancel();

            var jsonToken = _jsonTokenSource.Token;

            try
            {
                JsonTask?.Wait(jsonToken);
            }
            catch (OperationCanceledException)
            {
                Log.Info("json background task ended");
            }
            finally
            {
                _jsonTokenSource.Dispose();
            }

            if (_joystick != null)
            {
                _joystickTokenSource.Cancel();

                var joystickToken = _joystickTokenSource.Token;

                try
                {
                    _joystickTask?.Wait(joystickToken);
                }
                catch (OperationCanceledException)
                {
                    Log.Info("joystick background task ended");
                }
                finally
                {
                    _joystickTokenSource.Dispose();

                    _joystick?.Unacquire();
                    _joystick?.Dispose();
                }
            }

            Log.Info("exiting");

            base.OnExit(e);
        }
    }
}
