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
		public static Task<string> GetJsonAsync(Uri path)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
				HttpResponseMessage result = client.GetAsync(path.PathAndQuery).Result;
				result.EnsureSuccessStatusCode();
				return result.Content.ReadAsStringAsync();
			}
		}

		public static string GetJson(Uri path)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
				HttpResponseMessage result = client.GetAsync(path.PathAndQuery).Result;
				result.EnsureSuccessStatusCode();
				return result.Content.ReadAsStringAsync().Result;
			}
		}

		public static ConsoleOutput GetConsoleOutput(Uri path, long offset)
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
				HttpResponseMessage result = client.GetAsync(path.PathAndQuery + "logText/progressiveText?start=" + offset).Result;
				result.EnsureSuccessStatusCode();

				long newOffset = int.Parse(result.Headers.GetValues("X-Text-Size").Single());

				IEnumerable<string> values;
				bool isBuilding = result.Headers.TryGetValues("X-More-Data", out values);

				return new ConsoleOutput { Text = result.Content.ReadAsStringAsync().Result, Offset = newOffset, IsBuilding = isBuilding };
			}
		}

		public static Task<string> PostData(Uri path, string data = "")
		{
			using (HttpClient client = new HttpClient { BaseAddress = new Uri(path.Scheme + "://" + path.Host + ":" + path.Port) })
			{
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
