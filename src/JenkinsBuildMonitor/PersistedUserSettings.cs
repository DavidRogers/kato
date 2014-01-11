using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Kato
{
	public sealed class PersistedUserSettings
	{
		public static T Open<T>()
		{
			string filePath = Path.Combine(s_savedFilePathBase, typeof(T).Name + ".dat");

			if (!File.Exists(filePath))
				return default(T);

			s_logger.Info("Retrieving user settings");
			string savedData = File.ReadAllText(filePath, Encoding.UTF8);
			return JsonConvert.DeserializeObject<T>(savedData);
		}

		public static void Save<T>(T data)
		{
			string filePath = Path.Combine(s_savedFilePathBase, typeof(T).Name + ".dat");

			string serializedData = JsonConvert.SerializeObject(data);
			try
			{
				s_logger.Info("Saving user settings");
				File.WriteAllText(filePath, serializedData, Encoding.UTF8);
			}
			catch (Exception e)
			{
				s_logger.Info("Failed to save user settings", e);
			}
		}

		static readonly log4net.ILog s_logger = log4net.LogManager.GetLogger("PersistedUserSettings");
		static readonly string s_savedFilePathBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.AppName);
	}
}
