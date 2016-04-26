using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JenkinsApiClient
{
	public class UserCredentials
	{
		public string UserName { get; set; }

		public string Password { get; set; }

		public bool IsValid
		{
			get { return !String.IsNullOrWhiteSpace(UserName) && !String.IsNullOrWhiteSpace(Password); }
		}
	}
}
