using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JenkinsApiClient
{
	public static class HttpHelper
	{

        #region Credentials local in memory cache

        private static Dictionary<string, UserCredentials> _cachedCredentials = new Dictionary<string, UserCredentials>();

        private static UserCredentials CachedCreds_TryGet(string baseAddress)
        {
            if (String.IsNullOrWhiteSpace(baseAddress))
                return null;

            var lowerAddress = baseAddress.ToLower();
            return _cachedCredentials.ContainsKey(lowerAddress) ? _cachedCredentials[lowerAddress] : null;
        }

        private static void CachedCreds_Set(string baseAddress, UserCredentials creds)
        {
            if (String.IsNullOrWhiteSpace(baseAddress) || creds == null || !creds.IsValid)
                return;

            var lowerAddress = baseAddress.ToLower();
            if (_cachedCredentials.ContainsKey(lowerAddress))
                return;

            _cachedCredentials.Add(lowerAddress, creds);

        }

        private static bool CachedCreds_Contains(string baseAddress)
        {
            if (String.IsNullOrWhiteSpace(baseAddress))
                return false;

            var lowerAddress = baseAddress.ToLower();
            return _cachedCredentials.ContainsKey(lowerAddress);
        }

	    private static UserCredentials EnsureCredentials(string baseAddress, UserCredentials credentials)
	    {
	        if (credentials != null)
	            CachedCreds_Set(baseAddress, credentials);

	        return credentials != null ? credentials : CachedCreds_TryGet(baseAddress);
	    }
        #endregion


        public static Task<string> GetJsonAsync(Uri path, UserCredentials credential = null)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{

			    var cred = EnsureCredentials(client.BaseAddress.ToString(), credential);
                client.ApplyCredentials(cred);

                HttpResponseMessage result = client.GetAsync(path.PathAndQuery).Result;
				result.EnsureSuccessStatusCode();
				return result.Content.ReadAsStringAsync();
			}
		}

		public static string GetJson(Uri path, UserCredentials credential = null)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{

                var cred = EnsureCredentials(client.BaseAddress.ToString(), credential);
                client.ApplyCredentials(cred);
                HttpResponseMessage result = client.GetAsync(path.PathAndQuery).Result;
				result.EnsureSuccessStatusCode();
				return result.Content.ReadAsStringAsync().Result;
			}
		}

		public static ConsoleOutput GetConsoleOutput(Uri path, long offset, UserCredentials credential = null)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
                var cred = EnsureCredentials(client.BaseAddress.ToString(), credential);
                client.ApplyCredentials(cred);
                HttpResponseMessage result = client.GetAsync(path.PathAndQuery + "logText/progressiveText?start=" + offset).Result;
				result.EnsureSuccessStatusCode();

				long newOffset = int.Parse(result.Headers.GetValues("X-Text-Size").Single());

				IEnumerable<string> values;
				bool isBuilding = result.Headers.TryGetValues("X-More-Data", out values);

				return new ConsoleOutput { Text = result.Content.ReadAsStringAsync().Result, Offset = newOffset, IsBuilding = isBuilding };
			}
		}

		public static Task<string> PostData(Uri path, string data = "", UserCredentials credential = null)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
                var cred = EnsureCredentials(client.BaseAddress.ToString(), credential);
                client.ApplyCredentials(cred);
                HttpResponseMessage result = client.PostAsync(path.PathAndQuery, new StringContent(data)).Result;
				result.EnsureSuccessStatusCode();
				return result.Content.ReadAsStringAsync();
			}
		}

		public static T GetObject<T>(string json) where T : class
		{
			if (string.IsNullOrWhiteSpace(json))
				return null;

			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
