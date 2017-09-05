using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using JenkinsApiClient;

namespace Kato
{
	public sealed class JobViewModel : PropertyChangedBase, IEqualityComparer<JobViewModel>
	{
		public JobViewModel(string name, Uri path, JenkinsClient client)
		{
			m_name = name;
			m_path = path;
			m_client = client;
		}

		public string Name
		{
			get { return m_name; }
		}

		public Uri Path
		{
			get { return m_path; }
		}

		public BuildStatus Status
		{
			get
			{
				BuildStatus status;
				switch (m_color)
				{
				case "blue":
					status = BuildStatus.Success;
					break;
				case "blue_anime":
					status = BuildStatus.SuccessAndBuilding;
					break;
				case "red":
					status = BuildStatus.Failed;
					break;
				case "red_anime":
					status = BuildStatus.FailedAndBuilding;
					break;
				case "aborted":
					status = BuildStatus.Aborted;
					break;
				case "aborted_anime":
					status = BuildStatus.AbortedAndBuilding;
					break;
				case "disabled":
					status = BuildStatus.Disabled;
					break;
				default:
					status = BuildStatus.Unknown;
					break;
				}

				return status;
			}
		}

		public string Description
		{
			get { return m_description; }
			set
			{
				if (value == m_description) return;
				m_description = value;
				NotifyOfPropertyChange(() => Description);
			}
		}

		public bool InQueue
		{
			get { return m_inQueue; }
			set
			{
				if (value.Equals(m_inQueue)) return;
				m_inQueue = value;
				NotifyOfPropertyChange(() => InQueue);
			}
		}

		public Uri WeatherUrl
		{
			get { return null; }
		}

		public HealthReport[] HealthReports
		{
			get { return m_healthReports; }
			set
			{
				if (Equals(value, m_healthReports)) return;
				m_healthReports = value;
				NotifyOfPropertyChange(() => HealthReports);
			}
		}

		public BuildViewModel LastBuild
		{
			get { return m_lastBuild; }
			set
			{
				if (Equals(value, m_lastBuild)) return;
				m_lastBuild = value;
				NotifyOfPropertyChange(() => LastBuild);
			}
		}

		public bool IsSubscribed
		{
			get { return m_isSubscribed; }
			set
			{
				if (m_isSubscribed != value)
				{
					m_isSubscribed = value;
					NotifyOfPropertyChange(() => IsSubscribed);
				}
			}
		}

		public bool IsHidden
		{
			get { return m_isHidden; }
			set
			{
				if (m_isHidden != value)
				{
					m_isHidden = value;
					NotifyOfPropertyChange(() => IsHidden);
				}
			}
		}

		public ConsoleOutputControl ConsoleOuputDocument
		{
			get { return m_consoleOuputDocument; }
			set
			{
				if (Equals(value, m_consoleOuputDocument)) return;
				m_consoleOuputDocument = value;
				NotifyOfPropertyChange(() => ConsoleOuputDocument);
			}
		}

		public bool CanSubscribe()
		{
			return true;
		}

		public void Subscribe()
		{
			IsSubscribed = !IsSubscribed;
		}

		public bool CanForceBuild()
		{
			return Status != BuildStatus.FailedAndBuilding && Status != BuildStatus.SuccessAndBuilding;
		}

		public void ForceBuild()
		{
			m_client.ForceBuild(Path).LogErrors();
		}

		public bool CanViewConsoleOutput()
		{
			return LastBuild != null;
		}

		public void ViewConsoleOutput()
		{
			ConsoleOuputDocument = new ConsoleOutputControl(LastBuild);
			((MainWindow) Application.Current.MainWindow).OpenProjectDetails(this);
		}

		public bool CanDisableBuild()
		{
			return Status != BuildStatus.Disabled;
		}

		public void DisableBuild()
		{
			m_client.DisableJob(Path).LogErrors();
		}

		public bool CanEnableBuild()
		{
			return Status == BuildStatus.Disabled;
		}

		public void EnableBuild()
		{
			m_client.EnableJob(Path).LogErrors();
		}

		public void OpenInBrowser(ActionExecutionContext context)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs) context.EventArgs;
			if (args.ClickCount > 1)
			{
				Uri path = (LastBuild != null && Status < BuildStatus.Success) ? (LastBuild.Path ?? Path) : Path;
				Process.Start(path.OriginalString);
			}
		}
		public string Color
		{
			get { return m_color; }
			set
			{
				if (m_color != value && value != null)
				{
					BuildStatus oldStatus = Status;
					m_color = value;
					NotifyOfPropertyChange(() => Color);
					NotifyOfPropertyChange(() => Status);
					RaiseStatusChangedEvent(oldStatus, Status);
				}
			}
		}

		private void RaiseStatusChangedEvent(BuildStatus oldValue, BuildStatus newValue)
		{
			StatusChanged?.Invoke(this, new StatusChangedArgs(oldValue, newValue));
		}

		public bool Equals(JobViewModel x, JobViewModel y)
		{
			return x?.Name + (x?.Path?.OriginalString ?? "") == y?.Name + (y?.Path?.OriginalString ?? "");
		}

		public int GetHashCode(JobViewModel obj)
		{
			return obj.Name.GetHashCode() + obj.Path.GetHashCode();
		}

		public event StatusChangedEvent StatusChanged;

		bool m_isHidden;
		bool m_isSubscribed;
		readonly string m_name;
		readonly Uri m_path;
		private readonly JenkinsClient m_client;
		string m_color;
		string m_description;
		bool m_inQueue;
		BuildViewModel m_lastBuild;
		HealthReport[] m_healthReports;
		ConsoleOutputControl m_consoleOuputDocument;
	}

	public delegate void StatusChangedEvent(object sender, StatusChangedArgs args);

	public class StatusChangedArgs
	{
		public StatusChangedArgs(BuildStatus oldValue, BuildStatus newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public BuildStatus OldValue { get; }
		public BuildStatus NewValue { get; }
	}
}