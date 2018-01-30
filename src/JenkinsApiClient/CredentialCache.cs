using System.Collections.Generic;

namespace JenkinsApiClient
{
	public static class CredentialCache
	{
		public static void Invalidate(string baseAddress)
		{
			s_cachedCredentials.Remove(baseAddress);
		}

		internal static UserCredentials TryGetCachedCredentials(string baseAddress)
		{
			if (string.IsNullOrWhiteSpace(baseAddress))
				return null;

			var lowerAddress = baseAddress.ToLower();
			return s_cachedCredentials.ContainsKey(lowerAddress) ? s_cachedCredentials[lowerAddress] : null;
		}

		internal static void SetCachedCredentials(string baseAddress, UserCredentials creds)
		{
			if (string.IsNullOrWhiteSpace(baseAddress) || creds == null || !creds.IsValid)
				return;

			var lowerAddress = baseAddress.ToLower();
			if (s_cachedCredentials.ContainsKey(lowerAddress))
				return;

			s_cachedCredentials.Add(lowerAddress, creds);
		}

		static readonly Dictionary<string, UserCredentials> s_cachedCredentials = new Dictionary<string, UserCredentials>();
	}
}
