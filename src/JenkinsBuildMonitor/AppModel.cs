using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Xml.Linq;
using Caliburn.Micro;
using CredentialManagement;
using Hardcodet.Wpf.TaskbarNotification;
using JenkinsApiClient;
using Kato.Properties;
using SecureCredentialsLibrary;
using Action = System.Action;

namespace Kato
{
	public sealed class AppModel : PropertyChangedBase
	{
		public AppModel(TaskbarIcon notifyIcon, TaskbarItemInfo taskbarItemInfo)
		{
			m_isAddServerUrlValid = true;
			m_updateManager = new AutoUpdater();
			m_notifyIcon = notifyIcon;
			Enum.TryParse(Settings.Default.ViewMode, true, out m_viewMode);
			m_taskbarItemInfo = taskbarItemInfo;
			m_servers = new ObservableCollection<ServerViewModel>();
			m_settings = PersistedUserSettings.Open<UserSettings>() ?? new UserSettings { Servers = new List<SavedJenkinsServers>() };
			m_updateTimerInterval = Settings.Default.JobUpdateInterval < c_minJobUpdateInterval ? c_projectUpdateInterval : Settings.Default.JobUpdateInterval;
			m_timer = new DispatcherTimer(TimeSpan.FromSeconds(m_updateTimerInterval), DispatcherPriority.Background, (sender, args) => Update(), Dispatcher.CurrentDispatcher);
			Status = new StatusViewModel();
			m_subscribedJobs = new ObservableCollection<JobViewModel>();

			if (ApplicationDeployment.IsNetworkDeployed)
			{
				m_updateManager.CheckForUpdate();
				m_updateManager.PropertyChanged += UpdateManager_PropertyChanged;
				m_updateTimer = new DispatcherTimer(TimeSpan.FromHours(4), DispatcherPriority.Background, CheckForUpdate, Dispatcher.CurrentDispatcher);
			}

			if (CheckForInternetConnection())
				Initialize();
		}

		public ObservableCollection<ServerViewModel> Servers { get { return m_servers; } }

		public ObservableCollection<JobViewModel> SubscribedJobs
		{
			get { return m_subscribedJobs; }
			set
			{
				if (Equals(value, m_subscribedJobs)) return;
				m_subscribedJobs = value;
				NotifyOfPropertyChange(() => SubscribedJobs);
			}
		}

		public StatusViewModel Status { get; private set; }

		public bool HasInternetConnection
		{
			get { return m_hasInternetConnection; }
			set
			{
				if (Equals(value, m_hasInternetConnection)) return;
				m_hasInternetConnection = value;
				if (value)
				{
					m_servers.Clear();
					Initialize();
				}
				NotifyOfPropertyChange(() => HasInternetConnection);
			}
		}

		public string NewServerUrl
		{
			get { return m_newServerUrl; }
			set
			{
				if (value == m_newServerUrl) return;

				if (value == "")
					IsAddServerUrlValid = true;

				m_newServerUrl = value;
				NotifyOfPropertyChange(() => NewServerUrl);
			}
		}

		public bool NewServerAuthRequired
		{
			get { return m_newServerAuthRequired; }
			set
			{
				if (value == m_newServerAuthRequired) return;

				m_newServerAuthRequired = value;
				NotifyOfPropertyChange(() => NewServerAuthRequired);
			}
		}

		public ViewModeKind ViewMode
		{
			get { return m_viewMode; }
			set
			{
				if (value == m_viewMode) return;
				m_viewMode = value;
				NotifyOfPropertyChange(() => ViewMode);
			}
		}

		public string SubscribeFilter
		{
			get { return m_subscribeFilter; }
			set
			{
				if (value == m_subscribeFilter) return;
				m_subscribeFilter = value;
				DoFilter();
				NotifyOfPropertyChange(() => SubscribeFilter);
			}
		}

		public bool IsUpdateAvailable
		{
			get { return m_isUpdateAvailable; }
			set
			{
				if (value.Equals(m_isUpdateAvailable)) return;
				m_isUpdateAvailable = value;
				NotifyOfPropertyChange(() => IsUpdateAvailable);
			}
		}

		public double UpdateTimerInterval
		{
			get { return m_updateTimerInterval; }
			set
			{
				if (value.Equals(m_updateTimerInterval)) return;
				m_updateTimerInterval = value;
				NotifyOfPropertyChange(() => UpdateTimerInterval);

				UpdateTimer(TimeSpan.FromSeconds(m_updateTimerInterval));
			}
		}

		public JobViewModel SelectedProject
		{
			get { return m_selectedProject; }
			set
			{
				if (Equals(value, m_selectedProject)) return;
				m_selectedProject = value;
				NotifyOfPropertyChange(() => SelectedProject);
			}
		}

		public bool IsAddServerUrlValid
		{
			get { return m_isAddServerUrlValid; }
			set
			{
				if (value.Equals(m_isAddServerUrlValid)) return;
				m_isAddServerUrlValid = value;
				NotifyOfPropertyChange(() => IsAddServerUrlValid);
			}
		}

		public void ClearFilter(ActionExecutionContext context)
		{
			var eventArgs = (KeyEventArgs) context.EventArgs;
			if (eventArgs.Key == Key.Escape)
				SubscribeFilter = "";
		}

		public void AddServerEnter(ActionExecutionContext context)
		{
			var eventArgs = (KeyEventArgs) context.EventArgs;
			if (eventArgs.Key == Key.Enter)
				AddServer();
		}

		public void ToggleViewMode(string mode)
		{
			ViewModeKind kind;
			if (Enum.TryParse(mode, true, out kind))
				ViewMode = kind;
		}

		public void SaveSettings()
		{
			if (m_servers.Any(x => !x.IsValid) || !HasInternetConnection)
				return;

			UserSettings settings = new UserSettings { Servers = new List<SavedJenkinsServers>() };
			foreach (ServerViewModel server in Servers)
			{
				List<SavedJob> jobs = server.Jobs.Where(x => x.IsSubscribed).Select(x => new SavedJob { Name = x.Name }).ToList();
				settings.Servers.Add(new SavedJenkinsServers { DomainUrl = server.DomainUrl, Jobs = jobs, RequiresAuthentication = server.RequiresAuthentication });
			}

			PersistedUserSettings.Save(settings);
			Settings.Default.ViewMode = ViewMode.ToString();
			Settings.Default.JobUpdateInterval = UpdateTimerInterval;
			Settings.Default.Servers = new StringCollection();
			Settings.Default.Servers.AddRange(Servers.Select(x => x.DomainUrl).ToArray());
			Settings.Default.Save();
		}

		public void AddServer()
		{
			if (string.IsNullOrWhiteSpace(NewServerUrl))
				return;

			if (!Uri.IsWellFormedUriString(NewServerUrl, UriKind.Absolute))
			{
				IsAddServerUrlValid = false;
				return;
			}

			if (AddServer(NewServerUrl, NewServerAuthRequired))
			{
				IsAddServerUrlValid = true;
				NewServerUrl = "";
				NewServerAuthRequired = false;
			}
		}

		public void ClearSelection()
		{
			SetJobSubscriptions("c");
		}
		public void SelectAll()
		{
			SetJobSubscriptions("a");
		}

		private void UpdateTimer(TimeSpan interval)
		{
			m_timer.Interval = interval;
		}

		private void SetJobSubscriptions(string mode)
		{
			if (m_servers == null)
				return;

			var jobs = m_servers.Where(s => s.Jobs != null).SelectMany(s => s.Jobs);

			foreach (var j in jobs)
			{
				switch (mode)
				{
				case "c":
					j.IsSubscribed = false;
					break;
				case "a":
					j.IsSubscribed = true;
					break;
				default:
					break;
				}

			}

			SaveSettings();
		}

		public void OpenDashboard()
		{
			if (Application.Current.MainWindow != null)
				((MainWindow) Application.Current.MainWindow).OpenDashboard();
		}

		public bool AllowExit { get; set; }

		public void ExitApplication()
		{
			SaveSettings();
			AllowExit = true;
			Application.Current.Shutdown();
		}

		private void DoFilter()
		{
			foreach (JobViewModel job in m_servers.SelectMany(x => x.Jobs))
				job.IsHidden = !string.IsNullOrWhiteSpace(m_subscribeFilter) && !job.Name.ToLower().Contains(m_subscribeFilter.ToLower());
		}

		private async void Update()
		{
			if (!CheckForInternetConnection())
				return;

			if (!m_servers.Any())
				return;

			await Task.WhenAll(m_servers.Select(x => Task.Run(new Action(x.Update))).ToArray()).ConfigureAwait(true);

			var subscribedJobs = m_servers.SelectMany(x => x.Jobs.Where(j => j.IsSubscribed)).ToList();
			BuildStatus status = subscribedJobs.Any() ? subscribedJobs.Where(x => x.Status > BuildStatus.Aborted).Min(x => x.Status) : BuildStatus.Unknown;

			if (m_overallStatus != status)
			{
				StreamResourceInfo streamInfo = Application.GetResourceStream(new Uri("/Images/" + GetIconName(status) + ".ico", UriKind.Relative));
				m_notifyIcon.Icon = new Icon(streamInfo.Stream);
				m_overallStatus = status;
			}

			subscribedJobs.Except(SubscribedJobs).ToList().ForEach(SubscribedJobs.Add);

			await Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(SetTaskBarStatus));
		}

		private string GetIconName(BuildStatus status)
		{
			string name;
			switch (status)
			{
			case BuildStatus.Success:
				name = "green";
				break;
			case BuildStatus.SuccessAndBuilding:
				name = "yellow";
				break;
			case BuildStatus.Failed:
				name = "red";
				break;
			case BuildStatus.FailedAndBuilding:
				name = "orange";
				break;
			default:
				name = "gray";
				break;
			}

			return name;
		}

		private void Initialize()
		{
			AutoDetectServers();

			if (m_settings == null || m_settings.Servers == null || m_settings.Servers.Count == 0)
			{
				if (Settings.Default.Servers != null && Settings.Default.Servers.Count > 0)
					AddServers(Settings.Default.Servers);
				else
					AddServers(new[] { "https://jenkins" });
			}
			else
			{
				foreach (var server in m_settings.Servers)
					AddServer(server.DomainUrl, server.RequiresAuthentication);
			}

			Update();
		}

		private void AutoDetectServers()
		{
			// 33848
			try
			{
				if (m_udpWorker == null)
				{
					m_udpWorker = new BackgroundWorker();
					m_udpWorker.DoWork += Worker_DoWork;
					m_udpWorker.ProgressChanged += Worker_ProgressChanged;
					m_udpWorker.RunWorkerAsync();
				}
				SendUDPMessage();
			}
			catch (Exception e)
			{
				s_logger.Error(e.Message, e);
			}
		}

		private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			var xml = XElement.Parse((string) e.UserState);
			var urlElement = xml.Elements("url").FirstOrDefault();
			AddServers(new[] { (string) urlElement });
		}

		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			ListenForAutoDiscovery();
		}

		private void ListenForAutoDiscovery()
		{
			var address = IPAddress.Parse("239.77.124.213");
			using (UdpClient client = new UdpClient(AddressFamily.InterNetwork))
			{
				client.AllowNatTraversal(true);
				client.JoinMulticastGroup(address, 50);

				while (true)
				{
					var endPoint = new IPEndPoint(IPAddress.Any, 0);
					var response = client.Receive(ref endPoint);
					string data = Encoding.UTF8.GetString(response);
					s_logger.Info("UDP response received: " + data);
					m_udpWorker.ReportProgress(0, data);
				}
			}
		}

		private async void SendUDPMessage()
		{
			byte[] message = Encoding.ASCII.GetBytes("Hello");
			var address = IPAddress.Parse("239.77.124.213");
			using (UdpClient client = new UdpClient())
			{
				client.AllowNatTraversal(true);
				client.JoinMulticastGroup(address);
				await client.SendAsync(message, message.Length, new IPEndPoint(address, 33848)).ConfigureAwait(false);
				client.Close();
			}
		}

		private void AddServers(IEnumerable servers)
		{
			foreach (string serverUri in servers.Cast<string>().Where(x => !string.IsNullOrWhiteSpace(x) && Uri.IsWellFormedUriString(x, UriKind.Absolute) && !m_servers.Any(y => y.DomainUrl == x)))
				AddServer(serverUri);
		}

		private bool AddServer(string serverUri, bool authRequired = false)
		{
			if (m_servers.Any(x => x.DomainUrl == serverUri))
				return false;

			UserCredentials userCreds = null;
			if (authRequired)
			{
				userCreds = PromptForCredentials(serverUri);
				if (userCreds == null)
				{
					MessageBox.Show("Please provide your username and password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}
			}

			ServerViewModel serverViewModel;
			JenkinsClient client = new JenkinsClient(new Uri(serverUri, UriKind.Absolute), userCreds);
			try
			{
				Server server = client.GetJson<Server>(new Uri(serverUri));
				server.RequiresAuthentication = authRequired;

				serverViewModel = new ServerViewModel(client, server);
			}
			catch (Exception)
			{
				serverViewModel = new ServerViewModel(client, null);
			}

			SavedJenkinsServers savedServer = m_settings.Servers.FirstOrDefault(x => x.DomainUrl == serverViewModel.DomainUrl);

			foreach (JobViewModel job in serverViewModel.Jobs)
			{
				if (savedServer != null)
					job.IsSubscribed = savedServer.Jobs.Any(x => x.Name == job.Name);
				job.StatusChanged += JobOnStatusChanged;
			}

			m_servers.Add(serverViewModel);
			return true;
		}

		private void JobOnStatusChanged(object sender, StatusChangedArgs args)
		{
			JobViewModel job = (JobViewModel) sender;

			if (args.NewValue == BuildStatus.Failed)
				m_notifyIcon.ShowBalloonTip(job.Name, "Build " + args.NewValue, BalloonIcon.Error);
			else if (args.NewValue == BuildStatus.Success && args.OldValue < BuildStatus.Success)
				m_notifyIcon.ShowBalloonTip(job.Name, "Build " + args.NewValue, BalloonIcon.Info);

			SetTaskBarStatus();
		}

		private void UpdateManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			IsUpdateAvailable = m_updateManager.UpdateInstalled;
		}

		private void SetTaskBarStatus()
		{
			List<JobViewModel> jobs = m_servers.SelectMany(x => x.Jobs).Where(x => x.IsSubscribed).ToList();
			int failedbuilds = jobs.Count(x => x.Status == BuildStatus.Failed);
			if (failedbuilds > 0)
			{
				m_taskbarItemInfo.Description = failedbuilds + " build" + (failedbuilds == 1 ? " is" : "s are") + " failing.";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.Failed), failedbuilds);
			}
			else if (jobs.Any(x => x.Status == BuildStatus.FailedAndBuilding))
			{
				m_taskbarItemInfo.Description = "";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.FailedAndBuilding));
			}
			else if (jobs.Any(x => x.Status == BuildStatus.SuccessAndBuilding))
			{
				m_taskbarItemInfo.Description = "";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.SuccessAndBuilding));
			}
			else if (jobs.All(x => x.Status == BuildStatus.Disabled))
			{
				m_taskbarItemInfo.Description = "One or more builds are disabled";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.Disabled));
			}
			else if (jobs.All(x => x.Status == BuildStatus.Unknown))
			{
				m_taskbarItemInfo.Description = "";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.Unknown));
			}
			else if (jobs.Any(x => x.Status == BuildStatus.Success))
			{
				m_taskbarItemInfo.Description = "";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.Success));
			}
			else
			{
				m_taskbarItemInfo.Description = "";
				m_taskbarItemInfo.Overlay = TaskbarHelper.GetNewMessagesNotificationOverlay(Application.Current.MainWindow,
					GetOverlayIcon(BuildStatus.Success));
			}
		}

		private DataTemplate GetOverlayIcon(BuildStatus newValue)
		{
			var icon = ((DataTemplate) Application.Current.Resources[newValue + "OverlayIcon"]);
			return icon;
		}

		private void CheckForUpdate(object sender, EventArgs e)
		{
			m_updateManager.CheckForUpdate();
		}

		private UserCredentials PromptForCredentials(string url)
		{
			UserCredentials result = null;
			CredentialsDialog dialog = new CredentialsDialog("Kato - Jenkins Authentication : \r\n" + url);

			if (dialog.Show() == DialogResult.OK && (!String.IsNullOrWhiteSpace(dialog.Password) && !String.IsNullOrWhiteSpace(dialog.Name)))
			{
				var creds = new UserCredentials { UserName = dialog.Name, Password = dialog.Password };

				if (!TryToAuthenticate(url, creds))
					return null;

				if (dialog.SaveChecked)
				{
					dialog.Confirm(true);
				}
				result = creds;
			}

			return result;
		}

		private bool TryToAuthenticate(string serverUrl, UserCredentials creds)
		{
			if (!creds.IsValid)
				return false;

			try
			{
				var apiUri = JenkinsApiHelper.GetApiRoute(new Uri(serverUrl, UriKind.Absolute));
				var response = HttpHelper.GetJson(apiUri, creds);
				return true;
			}
			catch (Exception e)
			{
				s_logger.Error(e.Message, e);
				return false;
			}
		}

		public bool CheckForInternetConnection()
		{
			try
			{
				using (var client = new WebClient())
				{
					using (client.OpenRead("https://google.com"))
					{
						HasInternetConnection = true;
					}
				}
			}
			catch
			{
				HasInternetConnection = false;
			}

			return HasInternetConnection;
		}

		const double c_projectUpdateInterval = 10;
		const int c_minJobUpdateInterval = 3;
		DispatcherTimer m_updateTimer;
		bool m_hasInternetConnection;
		static readonly log4net.ILog s_logger = log4net.LogManager.GetLogger("AppModel");
		readonly ObservableCollection<ServerViewModel> m_servers;
		readonly UserSettings m_settings;
		readonly DispatcherTimer m_timer;
		readonly TaskbarIcon m_notifyIcon;
		readonly AutoUpdater m_updateManager;
		readonly TaskbarItemInfo m_taskbarItemInfo;
		string m_subscribeFilter;
		BuildStatus m_overallStatus;
		string m_newServerUrl;
		bool m_newServerAuthRequired;
		bool m_isAddServerUrlValid;
		ViewModeKind m_viewMode;
		bool m_isUpdateAvailable;
		JobViewModel m_selectedProject;
		ObservableCollection<JobViewModel> m_subscribedJobs;
		BackgroundWorker m_udpWorker;
		double m_updateTimerInterval;
	}
}
