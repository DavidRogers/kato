using System;
using CredentialManagement;
using JenkinsApiClient;
using log4net;
using SecureCredentialsLibrary;

namespace Kato
{
	public static class CredentialsHelper
	{
		public static UserCredentials PromptForCredentials(string url, bool forceDisplay = false)
		{
			UserCredentials result = null;
			CredentialsDialog dialog = new CredentialsDialog("Kato - Jenkins Authentication : \r\n" + url) { AlwaysDisplay = forceDisplay, SaveDisplayed = false };

			if (dialog.Show(saveChecked: true) == DialogResult.OK && (!string.IsNullOrWhiteSpace(dialog.Password) && !string.IsNullOrWhiteSpace(dialog.Name)))
			{
				var creds = new UserCredentials { UserName = dialog.Name, Password = dialog.Password };

				if (!TryToAuthenticate(url, creds))
					return null;

				if (dialog.SaveChecked)
					dialog.Confirm(true);

				result = creds;
			}

			return result;
		}

		public static bool TryToAuthenticate(string serverUrl, UserCredentials creds)
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

		static readonly ILog s_logger = LogManager.GetLogger("CredentialsHelper");
	}
}
