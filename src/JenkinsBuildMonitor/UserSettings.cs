using System.Collections.Generic;
using JenkinsApiClient;

namespace Kato
{
	public sealed class UserSettings
	{
		public List<SavedJenkinsServers> Servers { get; set; }
	}

    public class SavedJenkinsServers
	{
		public string DomainUrl { get; set; }

        public bool RequiresAuthentication { get; set; }

        public List<SavedJob> Jobs { get; set; }
	}

	public class SavedJob
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}
}
