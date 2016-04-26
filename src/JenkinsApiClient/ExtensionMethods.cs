using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace JenkinsApiClient
{
	public static class ExtensionMethods
	{
		public static void ApplyCredentials(this HttpClient client, UserCredentials credentials)
		{
			if (credentials == null || String.IsNullOrWhiteSpace(credentials.UserName))
				return;

			var byteArray = Encoding.UTF8.GetBytes(String.Format("{0}:{1}", credentials.UserName, credentials.Password));
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

		}
	}
}
