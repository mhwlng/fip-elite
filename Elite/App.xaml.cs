using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using TheArtOfDev.HtmlRenderer.Core;
using EliteJournalReader;
using EliteJournalReader.Events;
using log4net.Repository.Hierarchy;
using SharpDX.DirectInput;
using System.Collections.Specialized;


// ReSharper disable StringLiteralTypo

namespace Elite
{

    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        public static readonly object RefreshJsonLock = new object();

        public static Task jsonTask;
        public static CancellationTokenSource jsonTokenSource = new CancellationTokenSource();

        public static Task joystickTask;
        public static CancellationTokenSource joystickTokenSource = new CancellationTokenSource();

        private static Mutex _mutex = null;

        private TaskbarIcon notifyIcon;

        public static FipHandler fipHandler = new FipHandler();

        public static JournalWatcher watcher;

        public static StatusWatcher statusWatcher;

        public static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CssData cssData;

        // Initialize DirectInput
        public static DirectInput directInput = new DirectInput();

        public static Joystick joystick = null;

        private static string PID;
        private static string VID;
        private static int UpButton;
        private static int DownButton;
        private static int LeftButton;
        private static int RightButton;
        private static int PushButton;
        public static string FipSerialNumber;

        private static bool[] lastButtonState = new bool[256];

        public static void RefreshJson(SplashScreenWindow splashScreen = null)
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

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading CNB Systems...");
                CnbSystems.FullCnbSystemsList = CnbSystems.GetAllCnbSystems(@"Data\cnbsystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Painite Hotspot Systems...");
                HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.Painite] = HotspotSystems.GetAllHotspotSystems(@"Data\painitesystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading LTD Hotspot Systems...");
                HotspotSystems.FullHotspotSystemsList[HotspotSystems.MaterialTypes.LTD] = HotspotSystems.GetAllHotspotSystems(@"Data\ltdsystems.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Painite Mining Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.Painite] = MiningStations.GetAllMiningStations(@"Data\painitestations.json");

                splashScreen?.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading LTD Mining Stations...");
                MiningStations.FullMiningStationsList[MiningStations.MaterialTypes.LTD] = MiningStations.GetAllMiningStations(@"Data\ltdstations.json");

            }

            if (splashScreen == null)
            {
                Data.HandleJson();
            }

        }

        private static void RunProcess(string fileName)
        {
            Process process = new Process();
            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = fileName;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
        }

        private static void HandleJoystickButton(JoystickState state, JoystickButton button, int buttonId)
        {
            if (state.Buttons.Length >= buttonId)
            {
                var buttonState = state.Buttons[buttonId - 1];
                var oldButtonState = lastButtonState[buttonId - 1];

                if (buttonState && buttonState == oldButtonState)
                {
                    fipHandler.HandleJoystickButton(button, false);
                }

                if (buttonState || buttonState != oldButtonState)
                {
                    fipHandler.HandleJoystickButton(button, buttonState);
                }

                lastButtonState[buttonId - 1] = buttonState;
            }

        }

        protected override void OnStartup(StartupEventArgs evtArgs)
        {
            const string appName = "Fip-Elite";

            _mutex = new Mutex(true, appName, out var createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }

            base.OnStartup(evtArgs);

            log4net.Config.XmlConfigurator.Configure();

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            notifyIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Elite;component/Hourglass.ico"));
            notifyIcon.ToolTipText = "Elite Dangerous Flight Instrument Panel [WORKING]";

            var splashScreen = new SplashScreenWindow();
            splashScreen.Show();

            Task.Run(() =>
            {

                var config = new TemplateServiceConfiguration
                {
                    TemplateManager = new ResolvePathTemplateManager(new[] { "Templates" }),
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
                Engine.Razor.Compile("missions.cshtml", null);
                Engine.Razor.Compile("poi.cshtml", null);

                Engine.Razor.Compile("map.cshtml", null);
                Engine.Razor.Compile("powers.cshtml", null);
                Engine.Razor.Compile("materials.cshtml", null);
                Engine.Razor.Compile("cargo.cshtml", null);
                Engine.Razor.Compile("mining.cshtml", null);
                Engine.Razor.Compile("events.cshtml", null);

                cssData = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.ParseStyleSheet(
                    File.ReadAllText("Templates\\styles.css"), true);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading POI Items...");
                Poi.FullPoiList = Poi.GetAllPois(); //?.GroupBy(x => x.System.Trim().ToLower()).ToDictionary(x => x.Key, x => x.ToList());

                RefreshJson(splashScreen);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading History...");
                var path = History.GetEliteHistory();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Journal Status Watcher...");
                statusWatcher = new StatusWatcher(path);

                statusWatcher.StatusUpdated += Data.HandleStatusEvents;

                statusWatcher.StartWatching();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Journal Watcher...");
                watcher = new JournalWatcher(path);

                watcher.AllEventHandler += Data.HandleEliteEvents;

                watcher.StartWatching().Wait();
                
                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting FIP...");
                if (!fipHandler.Initialize())
                {
                    Application.Current.Shutdown();
                }

                log.Info("Fip-Elite started");

                if (File.Exists("joystickSettings.config") && ConfigurationManager.GetSection("joystickSettings") is NameValueCollection section)
                {
                    PID = section["PID"];
                    VID = section["VID"];
                    UpButton = Convert.ToInt32(section["UpButton"]);
                    DownButton = Convert.ToInt32(section["DownButton"]);
                    LeftButton = Convert.ToInt32(section["LeftButton"]);
                    RightButton = Convert.ToInt32(section["RightButton"]);
                    PushButton = Convert.ToInt32(section["PushButton"]);
                    FipSerialNumber = section["FipSerialNumber"];

                    if (!string.IsNullOrEmpty(PID) && !string.IsNullOrEmpty(VID) && UpButton > 0 && DownButton > 0 && LeftButton > 0 && RightButton > 0 && PushButton > 0)
                    {
                        splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Looking for Joystick...");

                        if (!string.IsNullOrEmpty(FipSerialNumber))
                        {
                            log.Info($"Sending joystick button presses to FIP panel with serial number {FipSerialNumber}");
                        }

                        log.Info($"Looking for directinput devices with PID={PID} and VID={VID}");

                        log.Info($"Button numbers : Up={UpButton} Down={DownButton} Left={LeftButton} Right={RightButton} Push={PushButton}");

                        foreach (var deviceInstance in directInput.GetDevices())
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
                                log.Info("P:" + deviceInstance.ProductGuid.ToString().Substring(0, 4) + " - V:" +
                                         deviceInstance.ProductGuid.ToString().Substring(4, 4) + " - " +
                                         deviceInstance.Type.ToString().PadRight(11) + " - " +
                                         deviceInstance.ProductGuid + " - " + deviceInstance.InstanceGuid + " - " +
                                         deviceInstance.InstanceName.Trim().Replace("\0", ""));

                                if (joystick == null &&
                                    deviceInstance.ProductGuid.ToString().ToUpper().StartsWith(PID + VID))
                                {
                                    log.Info(
                                        $"Using Joystick {deviceInstance.InstanceName.Trim().Replace("\0", "")} with Instance Guid {deviceInstance.InstanceGuid}");

                                    joystick = new Joystick(directInput, deviceInstance.InstanceGuid);

                                    //joystick.Properties.BufferSize = 128;

                                    /*
                                    joystick.SetCooperativeLevel(wih,
                                      CooperativeLevel.Background | CooperativeLevel.NonExclusive);*/

                                    joystick.Acquire();

                                    var joystickToken = joystickTokenSource.Token;

                                    joystickTask = Task.Run(async () =>
                                    {
                                        log.Info("joystick task started");

                                        while (true)
                                        {
                                            if (joystickToken.IsCancellationRequested)
                                            {
                                                joystickToken.ThrowIfCancellationRequested();
                                            }

                                            joystick.Poll();

                                            var state = joystick.GetCurrentState();

                                            HandleJoystickButton(state, JoystickButton.Up, UpButton);
                                            HandleJoystickButton(state, JoystickButton.Down, DownButton);
                                            HandleJoystickButton(state, JoystickButton.Left, LeftButton);
                                            HandleJoystickButton(state, JoystickButton.Right, RightButton);
                                            HandleJoystickButton(state, JoystickButton.Push, PushButton);

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

                                            await Task.Delay(100, jsonTokenSource.Token);
                                        }

                                    }, joystickToken);
                                }
                            }
                        }

                        if (joystick == null)
                        {
                            log.Info($"No joystick found with PID={PID} and VID={VID}");
                        }
                    }
                }

                this.Dispatcher.Invoke(() => { splashScreen.Close(); });

                var jsonToken = jsonTokenSource.Token;

                jsonTask = Task.Run(async () =>
                {
                    log.Info("json task started");

                    while (true)
                    {
                        if (jsonToken.IsCancellationRequested)
                        {
                            jsonToken.ThrowIfCancellationRequested();
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            notifyIcon.IconSource =
                                new BitmapImage(new Uri("pack://application:,,,/Elite;component/Hourglass.ico"));

                            notifyIcon.ToolTipText = "Elite Dangerous Flight Instrument Panel [WORKING]";
                        });

                        RunProcess("ImportData.exe");

                        RefreshJson();

                        this.Dispatcher.Invoke(() =>
                        {
                            notifyIcon.IconSource =
                                new BitmapImage(new Uri("pack://application:,,,/Elite;component/Elite.ico"));

                            notifyIcon.ToolTipText = "Elite Dangerous Flight Instrument Panel";
                        });

                        await Task.Delay(30 * 60 * 1000, jsonTokenSource.Token);
                    }

                }, jsonToken);

            });

        }

        protected override void OnExit(ExitEventArgs e)
        {
            statusWatcher.StatusUpdated -= Data.HandleStatusEvents;

            statusWatcher.StopWatching();

            watcher.AllEventHandler -= Data.HandleEliteEvents;

            watcher.StopWatching();

            fipHandler.Close();

            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner

            jsonTokenSource.Cancel();

            var jsonToken = jsonTokenSource.Token;

            try
            {
                jsonTask?.Wait(jsonToken);
            }
            catch (OperationCanceledException)
            {
                log.Info("json background task ended");
            }
            finally
            {
                jsonTokenSource.Dispose();
            }

            if (joystick != null)
            {
                joystickTokenSource.Cancel();

                var joystickToken = joystickTokenSource.Token;

                try
                {
                    joystickTask?.Wait(joystickToken);
                }
                catch (OperationCanceledException)
                {
                    log.Info("joystick background task ended");
                }
                finally
                {
                    joystickTokenSource.Dispose();

                    joystick?.Unacquire();
                    joystick?.Dispose();
                }
            }

            log.Info("exiting");

            base.OnExit(e);
        }
    }
}
