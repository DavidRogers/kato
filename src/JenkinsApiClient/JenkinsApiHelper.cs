using System;

namespace JenkinsApiClient
{
	public static class JenkinsApiHelper
	{
		public static Uri GetApiRoute(Uri baseUri)
		{
			return new Uri(baseUri, c_jsonApiRoute);
		}

		const string c_jsonApiRoute = "api/json";
	}
}
