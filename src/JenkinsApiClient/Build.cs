using System;

namespace JenkinsApiClient
{
	public sealed class Build
	{
		public bool Building { get; set; }
		public string Result { get; set; }
		public string BuiltOn { get; set; }
		public int Number { get; set; }
		public int Duration { get; set; }
		public int EstimatedDuration { get; set; }
		public long Timestamp { get; set; }
		public Uri Url { get; set; }
	}
}