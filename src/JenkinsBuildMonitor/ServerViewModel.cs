using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using JenkinsApiClient;

namespace Kato
{
	public enum BuildStatus
	{
		Unknown,
		AbortedAndBuilding,
		Aborted,
		FailedAndBuilding,
		Failed,
		SuccessAndBuilding,
		Success,
		Unstable,
		Disabled,
	}

	public class ServerViewModel : PropertyChangedBase
	{
		public ServerViewModel(JenkinsClient client)
		{
			m_jobs = new ObservableCollection<JobViewModel>();
			m_client = client;
		}

		public ObservableCollection<JobViewModel> Jobs => m_jobs;
		public string DomainUrl => m_client.BaseUri.OriginalString;
		public bool RequiresAuthentication => m_server?.RequiresAuthentication ?? false;
		public string Description { get; private set; }

		public bool Delete
		{
			get => m_delete;
			private set
			{
				if (value.Equals(m_delete)) return;
				m_delete = value;
				NotifyOfPropertyChange(() => Delete);
			}
		}

		public bool IsValid
		{
			get => m_isValid;
			private set
			{
				if (value.Equals(m_isValid)) return;
				m_isValid = value;
				NotifyOfPropertyChange(() => IsValid);
			}
		}

		public bool IsConnected
		{
			get => m_isConnected;
			private set
			{
				if (value.Equals(m_isConnected)) return;
				m_isConnected = value;
				NotifyOfPropertyChange(() => IsConnected);
			}
		}

		public async Task InitializeAsync()
		{
			m_server = await m_client.GetJsonAsync<Server>(m_client.BaseUri);
			if (m_server == null)
			{
				IsConnected = false;
				return;
			}

			Description = m_server.NodeDescription;
			IsConnected = true;
			IsValid = true;

			foreach (var job in m_server.Jobs)
				m_jobs.Add(new JobViewModel(job.Name, job.Url, m_client) { Color = job.Color });
		}

		public async Task<bool> VerifyConnectionAsync()
		{
			bool wasConnected = IsConnected;
			IsConnected = await m_client.VerifyConnectionAsync().ConfigureAwait(true);

			if (!wasConnected && IsConnected)
				await InitializeAsync();

			return IsConnected;
		}

		public async Task Update()
		{
			if (!IsValid || !IsConnected)
				return;

			await Task.WhenAll(m_jobs.Where(x => x.IsSubscribed).Select(UpdateJob)).ConfigureAwait(true);
		}

		public void Remove()
		{
			IsValid = false;
			m_jobs.Clear();
			Delete = true;
		}

		public void UpdateCredentials()
		{
			var userCredentials = CredentialsHelper.PromptForCredentials(DomainUrl, true);
			if (userCredentials != null)
			{
				CredentialCache.Invalidate(DomainUrl.TrimEnd('/') + "/");
				m_client.Credentials = userCredentials;
			}
			else
			{
				MessageBox.Show("Username and password are not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task UpdateJob(JobViewModel job)
		{
			if (!IsValid)
				return;

			try
			{
				Job source = await m_client.GetJsonAsync<Job>(job.Path);

				if (source == null)
					return;
				job.Color = source.Color;
				job.InQueue = source.InQueue;
				job.HealthReports = source.HealthReport;
				job.Description = source.Description;

				UpdateLastBuild(job, source.LastBuild);
			}
			catch (Exception e)
			{
				s_logger.Error(e);
			}
		}

		private void UpdateLastBuild(JobViewModel job, Build source)
		{
			if (!IsValid)
				return;

			if (job.LastBuild == null)
				job.LastBuild = new BuildViewModel();

			job.LastBuild.Number = source.Number;
			job.LastBuild.Path = source.Url;
			job.LastBuild.Building = source.Building;
			job.LastBuild.BuiltOn = source.BuiltOn;
			job.LastBuild.TimeStamp = ConvertTimestamp(source.Timestamp);
			job.LastBuild.Duration = source.Duration == 0 && source.Building ? (DateTime.UtcNow - job.LastBuild.TimeStamp) : TimeSpan.FromMilliseconds(source.Duration);
			job.LastBuild.EstimatedDuration = TimeSpan.FromMilliseconds(source.EstimatedDuration);
			job.LastBuild.Result = source.Result;
		}

		private DateTime ConvertTimestamp(long timeStamp)
		{
			return s_epoch + TimeSpan.FromMilliseconds(timeStamp);
		}

		static readonly log4net.ILog s_logger = log4net.LogManager.GetLogger("ServerViewModel");
		static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		readonly ObservableCollection<JobViewModel> m_jobs;
		private Server m_server;
		readonly JenkinsClient m_client;
		bool m_isValid;
		bool m_delete;
		bool m_isConnected;
	}
}
