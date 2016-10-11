using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Windows;
using log4net;
using log4net.Config;

namespace Kato
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, ISingleInstanceApp
	{
		[STAThread]
		public static void Main()
		{
			if (SingleInstance<App>.InitializeAsFirstInstance(AppName))
			{
				XmlConfigurator.Configure();
				s_log = LogManager.GetLogger(typeof(App));
				AppDomain.CurrentDomain.UnhandledException += Application_DispatcherUnhandledException;
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					AppVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4);
					s_log.Info("Configure crash reports");
				}

				s_log.Info("Displaying splash screen");
				SplashScreen splashScreen = new SplashScreen("Images/logo.png");
				splashScreen.Show(true);

				s_log.Info("Starting Jenkins Build Monitor...");
				App application = new App();
				application.InitializeComponent();
				application.Run();

				// Allow single instance code to perform cleanup operations
				s_log.Info("Cleaning up single instance app...");
				SingleInstance<App>.Cleanup();
			}
		}

		private static void Application_DispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			s_log.Error("Unhandled App Exception", e.ExceptionObject as Exception);
		}

		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			if (MainWindow != null)
				MainWindow.Activate();

			return true;
		}

		public static string AppVersion = "1.0";
		public const string AppName = "Kato";
		static ILog s_log;
	}
}
