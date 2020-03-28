using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
        private static Mutex _mutex = null;

        private TaskbarIcon notifyIcon;

        public static FipHandler fipHandler = new FipHandler();

        public static JournalWatcher watcher;

        public static StatusWatcher statusWatcher;

        public static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CssData cssData;

        public static List<PoiItem> PoiItems = null;

        public static List<StationData> InterStellarFactors = null;
        public static List<StationData> RawMaterialTraders = null;
        public static List<StationData> ManufacturedMaterialTraders = null;
        public static List<StationData> EncodedDataTraders = null;
        public static List<StationData> HumanTechnologyBrokers = null;
        public static List<StationData> GuardianTechnologyBrokers = null;


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

            var splashScreen = new SplashScreenWindow();
            splashScreen.Show();

            Task.Factory.StartNew(() =>
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

                Engine.Razor.Compile("1.cshtml", null);
                Engine.Razor.Compile("2.cshtml", null);
                Engine.Razor.Compile("3.cshtml", null);
                Engine.Razor.Compile("4.cshtml", null);
                Engine.Razor.Compile("5.cshtml", null);
                Engine.Razor.Compile("6.cshtml", null);

                Engine.Razor.Compile("7.cshtml", null);
                Engine.Razor.Compile("12.cshtml", null);

                cssData = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.ParseStyleSheet(
                    File.ReadAllText("Templates\\styles.css"), true);

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading POI Items...");

                PoiItems = Poi.GetPoiItems(); //?.GroupBy(x => x.System.Trim().ToLower()).ToDictionary(x => x.Key, x => x.ToList());

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Inter Stellar Factors...");
                InterStellarFactors = Station.GetStations("interstellarfactors.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Raw Material Traders...");
                RawMaterialTraders = Station.GetStations("rawmaterialtraders.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Manufactured Material Traders...");
                ManufacturedMaterialTraders = Station.GetStations("manufacturedmaterialtraders.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Encoded Data Traders..");
                EncodedDataTraders = Station.GetStations("encodeddatatraders.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Human Technology Brokers...");
                HumanTechnologyBrokers = Station.GetStations("humantechnologybrokers.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading Guardian Technology Brokers...");
                GuardianTechnologyBrokers = Station.GetStations("guardiantechnologybrokers.json");

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Loading History...");
                var path = EliteHistory.GetEliteHistory();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Journal Status Watcher...");
                statusWatcher = new StatusWatcher(path);

                statusWatcher.StatusUpdated += EliteData.HandleStatusEvents;

                statusWatcher.StartWatching();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting Elite Journal Watcher...");
                watcher = new JournalWatcher(path);

                watcher.AllEventHandler += EliteData.HandleEliteEvents;

                watcher.StartWatching().Wait();

                splashScreen.Dispatcher.Invoke(() => splashScreen.ProgressText.Text = "Starting FIP...");
                if (!fipHandler.Initialize())
                {
                    Application.Current.Shutdown();
                }

                log.Info("Fip-Elite started");

                this.Dispatcher.Invoke(() =>
                {
                    splashScreen.Close();
                });
            });

        }

        protected override void OnExit(ExitEventArgs e)
        {
            statusWatcher.StatusUpdated -= EliteData.HandleStatusEvents;

            statusWatcher.StopWatching();

            watcher.AllEventHandler -= EliteData.HandleEliteEvents;

            watcher.StopWatching();

            fipHandler.Close();

            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
