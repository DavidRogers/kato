using System.ComponentModel;
using System.Deployment.Application;
using Caliburn.Micro;
using Kato.Properties;
using ILog = log4net.ILog;

namespace Kato
{
	public class AutoUpdater : PropertyChangedBase
	{
		public AutoUpdater()
		{
			if (!ApplicationDeployment.IsNetworkDeployed)
				return;

			ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += CurrentDeploymentCheckForUpdateCompleted;
		}

		public bool UpdateInstalled
		{
			get { return m_updateIsInstalled; }
			set
			{
				if (m_updateIsInstalled != value)
				{
					m_updateIsInstalled = value;

					NotifyOfPropertyChange(() => UpdateInstalled);
				}
			}
		}

		public bool UpdateAvailable
		{
			get { return m_updateAvailable; }
			set
			{
				if (m_updateAvailable != value)
				{
					m_updateAvailable = value;
					NotifyOfPropertyChange(() => UpdateAvailable);
				}
			}
		}

		public int UpdateProgress
		{
			get { return m_progressPercentage; }
			set
			{
				if (m_progressPercentage != value)
				{
					m_progressPercentage = value;
					NotifyOfPropertyChange(() => UpdateProgress);
				}
			}
		}

		public bool IsUpdating
		{
			get { return m_isUpdating; }
		}

		public void CheckForUpdate()
		{
			if (ApplicationDeployment.IsNetworkDeployed)
			{
				if (!UpdateAvailable && !m_isUpdating && !m_isCheckingForUpdate)
				{
					ApplicationDeployment.CurrentDeployment.CheckForUpdateAsync();
					m_isCheckingForUpdate = true;
					s_log.Info("Checking for update");
				}
			}
		}

		private void CurrentDeploymentCheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
		{
			m_isCheckingForUpdate = false;
			if (e.Error != null)
			{
				s_log.Error("Auto update check failed", e.Error);
			}
			else
			{
				if (e.UpdateAvailable && Settings.Default.AutoInstallUpdates)
					Update();
				else
					UpdateAvailable = e.UpdateAvailable;
				s_log.Info("Auto update check succeeded, update available: " + e.UpdateAvailable.ToString());
			}
		}

		public void Update()
		{
			if (!ApplicationDeployment.IsNetworkDeployed)
				return;

			ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeploymentUpdateCompleted;
			ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += CurrentDeploymentUpdateProgressChanged;
			ApplicationDeployment.CurrentDeployment.UpdateAsync();
			m_isUpdating = true;
		}

		private void CurrentDeploymentUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
		{
			int nProgress = e.ProgressPercentage;
			UpdateProgress = nProgress;
		}

		private void CurrentDeploymentUpdateCompleted(object sender, AsyncCompletedEventArgs e)
		{
			m_isUpdating = false;
			ApplicationDeployment.CurrentDeployment.UpdateCompleted -= CurrentDeploymentUpdateCompleted;
			ApplicationDeployment.CurrentDeployment.UpdateProgressChanged -= CurrentDeploymentUpdateProgressChanged;

			if (e.Error != null)
			{
				s_log.Error("Auto update failed", e.Error);
			}
			else
			{
				UpdateInstalled = true;
				s_log.Info("Auto update succeeded");
			}
		}

		static readonly ILog s_log = log4net.LogManager.GetLogger("AutoUpdater");
		int m_progressPercentage;
		bool m_updateAvailable;
		bool m_updateIsInstalled;
		bool m_isCheckingForUpdate;
		bool m_isUpdating;
	}
}
