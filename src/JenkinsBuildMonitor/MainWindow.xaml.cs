using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI.Presentation;
using Hardcodet.Wpf.TaskbarNotification;

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

		private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			Loaded -= OnLoaded;

			await Model.Initialize().ConfigureAwait(true);

			// load settings page to subscribe to jobs if there are none selected
			await Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
			{
				if (Model.Servers != null && !Model.Servers.SelectMany(x => x.Jobs).Any(x => x.IsSubscribed))
					ContentSource = new Uri("Pages/Subscribe.xaml", UriKind.Relative);
			}));
		}

		protected override void OnStateChanged(EventArgs e)
		{
			ShowInTaskbar = WindowState != WindowState.Minimized;

			base.OnStateChanged(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!Model.AllowExit)
			{
				e.Cancel = true;
				WindowState = WindowState.Minimized;
			}

			Model.SaveSettings();
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
