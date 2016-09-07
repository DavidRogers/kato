using System.Threading.Tasks;

namespace Kato
{
	internal static class AsyncHelper
	{
		public static Task<T> LogErrors<T>(this Task<T> task)
		{
			return task.ContinueWith(x =>
			{
				if (x.IsFaulted || x.IsCanceled)
				{
					s_logger.Error("Asyc call failed", x.Exception);
					return default(T);
				}

				return x.Result;
			});

		}

		static readonly log4net.ILog s_logger = log4net.LogManager.GetLogger("AsyncHelper");
	}
}
