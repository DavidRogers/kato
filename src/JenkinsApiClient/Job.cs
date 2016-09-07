namespace JenkinsApiClient
{
	public sealed class Job
	{
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string Color { get; set; }
		public bool InQueue { get; set; }
		public HealthReport[] HealthReport { get; set; }
		public Build LastBuild { get; set; }
		////public Build LastCompletedBuild { get; set; }
		////public Build LastFailedBuild { get; set; }
		////public Build LastSuccessfulBuild { get; set; }
	}
}