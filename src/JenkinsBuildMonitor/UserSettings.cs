using System.Collections.Generic;

namespace Kato
{
	public sealed class UserSettings
	{
		public List<SavedJenkinsServers> Servers { get; set; }
	}
	public class SavedJenkinsServers
	{
		public string DomainUrl { get; set; }
		public List<SavedJob> Jobs { get; set; }
	}

	public class SavedJob
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}
}
