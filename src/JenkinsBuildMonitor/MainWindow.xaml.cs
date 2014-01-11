using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using FirstFloor.ModernUI.Presentation;
using Hardcodet.Wpf.TaskbarNotification;
using Kato.Properties;

namespace Kato
{
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			m_notifyIcon = MyNotifyIcon;
			m_openCommand = new RelayCommand(x =>
			{
				if (WindowState == WindowState.Minimized)
				{
					WindowState = WindowState.Normal;
					ShowInTaskbar = true;
				}
				else if (WindowState == WindowState.Normal)
				{
					Activate();
				}
			}, x => WindowState == WindowState.Minimized || WindowState == WindowState.Normal);
			m_notifyIcon.DoubleClickCommand = m_openCommand;
			m_notifyIcon.TrayBalloonTipClicked += TrayBalloonTipClicked;

			DataContext = Model = new AppModel(m_notifyIcon, TaskbarItemInfo);
			Loaded += OnLoaded;
		}

		public void OpenDashboard()
		{
			if (m_openCommand.CanExecute(null))
				m_openCommand.Execute(null);

			LoadContent("Main");
		}

		public void OpenProjectDetails(JobViewModel model)
		{
			Model.SelectedProject = model;
			LoadContent("ProjectDetails");
		}

		private void LoadContent(string page)
		{
			ContentSource = new Uri("Pages/" + page + ".xaml", UriKind.Relative);
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			Loaded -= OnLoaded;

			// load settings page to subscribe to jobs if there are none selected
			if (Model.Servers != null && !Model.Servers.SelectMany(x => x.Jobs).Any(x => x.IsSubscribed))
				ContentSource = new Uri("Pages/Subscribe.xaml", UriKind.Relative);
		}

		protected override void OnStateChanged(EventArgs e)
		{
			ShowInTaskbar = WindowState != WindowState.Minimized;

			base.OnStateChanged(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (Model.AllowExit)
			{
				UserSettings settings = new UserSettings { Servers = new List<SavedJenkinsServers>() };
				foreach (ServerViewModel server in Model.Servers)
				{
					List<SavedJob> jobs = server.Jobs.Where(x => x.IsSubscribed).Select(x => new SavedJob { Name = x.Name }).ToList();
					settings.Servers.Add(new SavedJenkinsServers { DomainUrl = server.DomainUrl, Jobs = jobs });
				}

				PersistedUserSettings.Save(settings);
				Settings.Default.ViewMode = Model.ViewMode.ToString();
				Settings.Default.Servers = new StringCollection();
				Settings.Default.Servers.AddRange(Model.Servers.Select(x => x.DomainUrl).ToArray());
				Settings.Default.Save();
			}
			else
			{
				e.Cancel = true;
				WindowState = WindowState.Minimized;
			}
			base.OnClosing(e);
		}

		private void TrayBalloonTipClicked(object sender, RoutedEventArgs e)
		{
			if (m_openCommand.CanExecute(null))
				m_openCommand.Execute(null);
		}

		public AppModel Model { get; private set; }

		public static Guid AppGuid = new Guid("2B62118F-3FF0-461D-ACDE-58A5144E705D");
		TaskbarIcon m_notifyIcon;
		RelayCommand m_openCommand;
	}
}
