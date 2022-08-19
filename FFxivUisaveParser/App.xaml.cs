using FFxivUisaveParser.Views;
using NLog;
using Prism.Ioc;
using System.Windows;

namespace FFxivUisaveParser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<XmlTreeView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureLogger();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NLog.LogManager.Shutdown();
            base.OnExit(e);
        }

        private void ConfigureLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logFile = new NLog.Targets.FileTarget("logger")
            {
                FileName = "Log.log",
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${threadid}|${message}|${exception:format=tostring}"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile);
            NLog.LogManager.Configuration = config;
        }
    }
}
