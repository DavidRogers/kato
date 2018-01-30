using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JenkinsApiClient
{
	public static class HttpHelper
	{
		private static UserCredentials EnsureCredentials(string baseAddress, UserCredentials credentials)
		{
			if (credentials != null)
				CredentialCache.SetCachedCredentials(baseAddress, credentials);

			return credentials ?? CredentialCache.TryGetCachedCredentials(baseAddress);
		}


		public static async Task<string> GetJsonAsync(Uri path, UserCredentials credential = null)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{

				var cred = EnsureCredentials(client.BaseAddress.ToString(), credential);
				client.ApplyCredentials(cred);

				HttpResponseMessage result = await client.GetAsync(path.PathAndQuery);
				result.EnsureSuccessStatusCode();
				return await result.Content.ReadAsStringAsync();
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

		public static async Task<string> PostDataAsync(Uri path, string data = "", UserCredentials credential = null)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
				var cred = EnsureCredentials(client.BaseAddress.ToString(), credential);
				client.ApplyCredentials(cred);

				var content = new StringContent(data);

				var crumbResponse = await client.GetAsync("/crumbIssuer/api/json");
				if (crumbResponse.IsSuccessStatusCode)
				{
					CrumbToken token = GetObject<CrumbToken>(await crumbResponse.Content.ReadAsStringAsync());
					content.Headers.Add(token.CrumbRequestField, token.Crumb);
				}

				HttpResponseMessage result = await client.PostAsync(path.PathAndQuery, content);
				return result.IsSuccessStatusCode ? await result.Content.ReadAsStringAsync() : null;
			}
		}

		public static T GetObject<T>(string json) where T : class
		{
			if (string.IsNullOrWhiteSpace(json))
				return null;

			return JsonConvert.DeserializeObject<T>(json);
		}

		private class CrumbToken
		{
			public string Crumb { get; set; }
			public string CrumbRequestField { get; set; }
		}
	}
}
