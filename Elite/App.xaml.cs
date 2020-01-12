using System;
using System.IO;
using System.Threading;
using System.Windows;
using EliteAPI;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Somfic.Logging;
using Somfic.Logging.Handlers;

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

        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");


            EliteApi.Start(false);

            if (!fipHandler.Initialize())
            {
                Application.Current.Shutdown();
            }

            var config = new TemplateServiceConfiguration
            {
                TemplateManager = new ResolvePathTemplateManager(new[] { "Templates" }),
                DisableTempFileLocking = true
            };

            Engine.Razor = RazorEngineService.Create(config);

            Engine.Razor.Compile("1.cshtml", null);
            Engine.Razor.Compile("2.cshtml", null);
            Engine.Razor.Compile("3.cshtml", null);
            Engine.Razor.Compile("4.cshtml", null);
            Engine.Razor.Compile("5.cshtml", null);
            Engine.Razor.Compile("6.cshtml", null);

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
