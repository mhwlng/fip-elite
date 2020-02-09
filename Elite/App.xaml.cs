using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using EliteAPI;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Somfic.Logging;
using Somfic.Logging.Handlers;
using TheArtOfDev.HtmlRenderer.Core;

namespace Elite
{

    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        private TaskbarIcon notifyIcon;

        public static EliteDangerousAPI EliteApi = new EliteDangerousAPI();

        private static FipHandler fipHandler = new FipHandler();

        public static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CssData cssData;

        public static List<PoiItem> PoiItems = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "Fip-Elite";

            _mutex = new Mutex(true, appName, out var createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }

            base.OnStartup(e);

            log4net.Config.XmlConfigurator.Configure();

            var config = new TemplateServiceConfiguration
            {
                TemplateManager = new ResolvePathTemplateManager(new[] {"Templates"}),
                DisableTempFileLocking = true,
                BaseTemplateType = typeof(HtmlSupportTemplateBase<>) /*,
                    Namespaces = new HashSet<string>(){
                        "System",
                        "System.Linq",
                        "System.Collections",
                        "System.Collections.Generic"
                        }*/


            };

            Engine.Razor = RazorEngineService.Create(config);

            Engine.Razor.Compile("menu.cshtml", null);
            Engine.Razor.Compile("layout.cshtml", null);

            Engine.Razor.Compile("1.cshtml", null);
            Engine.Razor.Compile("2.cshtml", null);
            Engine.Razor.Compile("3.cshtml", null);
            Engine.Razor.Compile("4.cshtml", null);
            Engine.Razor.Compile("5.cshtml", null);
            Engine.Razor.Compile("6.cshtml", null);

            cssData = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.ParseStyleSheet(
                File.ReadAllText("Templates\\styles.css"), true);

            PoiItems = Poi.GetPoiItems(); //?.GroupBy(x => x.System.Trim().ToLower()).ToDictionary(x => x.Key, x => x.ToList());

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            
            EliteApi.Start(false);

            if (!fipHandler.Initialize())
            {
                Application.Current.Shutdown();
            }


            log.Info("Fip-Elite started");

        }

        protected override void OnExit(ExitEventArgs e)
        {
            EliteApi.Stop();

            fipHandler.Close();

            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
