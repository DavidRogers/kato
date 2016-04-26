namespace JenkinsApiClient
{
	public sealed class Server
	{
		public ServerJob[] Jobs { get; set; }
		public string Mode { get; set; }
		public string NodeName { get; set; }
		public string NodeDescription { get; set; }
        public string DomainUrl { get; set; }

        public bool RequiresAuthentication { get; set; }

    }
}