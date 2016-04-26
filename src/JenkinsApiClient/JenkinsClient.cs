using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JenkinsApiClient
{
	public sealed class JenkinsClient
	{
		public UserCredentials Credentials { get; set; }

		public JenkinsClient(Uri baseUri, UserCredentials creds)
		{
			Credentials = creds;
			m_baseUri = baseUri;

		}

		public Uri BaseUri
		{
			get { return m_baseUri; }
		}

		public T GetData<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		private string GetMembers<T>()
		{
			var members = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

			return string.Join(",", members.Select(x =>
			{
				string subMembers = null;
				string name = x.Name;
				if (x.PropertyType == typeof(Build))
					subMembers = GetMembers<Build>();
				if (x.PropertyType == typeof(HealthReport[]))
					subMembers = GetMembers<HealthReport>();
				if (x.PropertyType == typeof(ServerJob[]))
					subMembers = GetMembers<ServerJob>();
				return LowerCaseFirstLetter(name) + (subMembers == null ? "" : "[" + subMembers + "]");
			}));
		}

		private string LowerCaseFirstLetter(string source)
		{
			return source.Substring(0, 1).ToLower() + source.Substring(1, source.Length - 1);
		}

		public Task<string> GetJsonAsync<T>(Uri uri)
		{
			Uri apiRoute = JenkinsApiHelper.GetApiRoute(uri);
			string members = GetMembers<T>();
			return HttpHelper.GetJsonAsync(new Uri(apiRoute.OriginalString + "?tree=" + members, UriKind.Absolute),Credentials);
		}

		public Task<string> ForceBuild(Uri jobUri)
		{
			return HttpHelper.PostData(new Uri(jobUri, "build"),"",Credentials);
		}

		public Task<string> DisableJob(Uri jobUri)
		{
			return HttpHelper.PostData(new Uri(jobUri, "disable"),"",Credentials);
		}
		public Task<string> EnableJob(Uri jobUri)
		{
			return HttpHelper.PostData(new Uri(jobUri, "enable"),"",Credentials);
		}

		readonly Uri m_baseUri;

		public T GetJson<T>(Uri uri) where T : class
		{
			Uri apiRoute = JenkinsApiHelper.GetApiRoute(uri);
			string members = GetMembers<T>();
			return HttpHelper.GetObject<T>(HttpHelper.GetJson(new Uri(apiRoute.OriginalString + "?tree=" + members, UriKind.Absolute),Credentials));
		}
	}
}
