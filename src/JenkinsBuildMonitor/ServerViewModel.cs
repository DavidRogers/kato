using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using JenkinsApiClient;
using Newtonsoft.Json;

namespace Kato
{
	public enum BuildStatus
	{
		Unknown,
		Disabled,
		AbortedAndBuilding,
		Aborted,
		FailedAndBuilding,
		Failed,
		SuccessAndBuilding,
		Success,
	}

	public class ServerViewModel : PropertyChangedBase
	{
		public ServerViewModel(JenkinsClient client, Server server)
		{
			m_server = server;
			m_jobs = new ObservableCollection<JobViewModel>();
			m_client = client;

			Initialize();
		}

		public ObservableCollection<JobViewModel> Jobs { get { return m_jobs; } }
		public string DomainUrl { get { return m_client.BaseUri.OriginalString; } }
		public bool RequiresAuthentication { get { return m_server.RequiresAuthentication; } }
		public string Description { get; private set; }

		public bool IsValid
		{
			get { return m_isValid; }
			private set
			{
				if (value.Equals(m_isValid)) return;
				m_isValid = value;
				NotifyOfPropertyChange(() => IsValid);
			}
		}

		public void Update()
		{
			foreach (JobViewModel job in m_jobs.Where(x => x.IsSubscribed))
				UpdateJob(job);
		}

		private void UpdateJob(JobViewModel job)
		{
			try
			{
				Job source = JsonConvert.DeserializeObject<Job>(m_client.GetJsonAsync<Job>(job.Path).LogErrors().Result);

				if (source == null)
					return;
				job.Color = source.Color;
				job.InQueue = source.InQueue;
				job.HealthReports = source.HealthReport;
				job.Description = source.Description;

				UpdateLastBuild(job, source.LastBuild);
			}
			catch (Exception)
			{

			}
		}

		private void UpdateLastBuild(JobViewModel job, Build source)
		{
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

		private void Initialize()
		{
			Description = m_server.NodeDescription;
			IsValid = true;

			foreach (var job in m_server.Jobs)
				m_jobs.Add(new JobViewModel(job.Name, job.Url, m_client) { Color = job.Color });
		}

		static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		readonly ObservableCollection<JobViewModel> m_jobs;
		private readonly Server m_server;
		readonly JenkinsClient m_client;
		bool m_isValid;
	}
}
