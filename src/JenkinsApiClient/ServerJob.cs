using System;

namespace JenkinsApiClient
{
	public sealed class ServerJob
	{
		public string Name { get; set; }
		public string Color { get; set; }
		public Uri Url { get; set; }
	}
}