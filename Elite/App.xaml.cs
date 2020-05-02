using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();

        private static Mutex _mutex = null;

        private TaskbarIcon notifyIcon;

        public static FipHandler fipHandler = new FipHandler();

        public static JournalWatcher watcher;

        public static StatusWatcher statusWatcher;

        public static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CssData cssData;

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

                this.Dispatcher.Invoke(() => { splashScreen.Close(); });

                var token = tokenSource.Token;

                jsonTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
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

                        await Task.Delay(30 * 60 * 1000, tokenSource.Token);
                    }

                }, token);

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

            tokenSource.Cancel();

            var token = tokenSource.Token;

            try
            {
                jsonTask?.Wait(token);
            }
            catch (OperationCanceledException)
            {
                log.Info("background task ended");
            }
            finally
            {
                tokenSource.Dispose();
            }

            log.Info("exiting");

            base.OnExit(e);
        }
    }
}
