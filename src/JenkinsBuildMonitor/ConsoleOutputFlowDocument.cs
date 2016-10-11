using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using JenkinsApiClient;

namespace Kato
{
	public class ConsoleOutputControl
	{
		public ConsoleOutputControl(BuildViewModel model)
		{
			m_model = model;
			m_timer = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Normal, FetchOutputHandler, Dispatcher.CurrentDispatcher) { IsEnabled = false };
			DoFetchOutput();
		}

		private void FetchOutputHandler(object sender, EventArgs e)
		{
			DoFetchOutput();
		}

		private void DoFetchOutput()
		{
			m_timer.IsEnabled = false;
			Task.Run(() => HttpHelper.GetConsoleOutput(m_model.Path, m_lastOffset))
			.ContinueWith(x =>
			{
				var result = x.Result;
				m_lastOffset = result.Offset;
				m_isBuilding = result.IsBuilding;
				RaiseTextAddedEvent(result.Text);
				m_timer.IsEnabled = m_isBuilding;
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public event EventHandler<TextAddedEventArgs> TextAddedEvent;

		private void RaiseTextAddedEvent(string text)
		{
			if (TextAddedEvent != null)
				TextAddedEvent(this, new TextAddedEventArgs(text));
		}

		readonly BuildViewModel m_model;
		readonly DispatcherTimer m_timer;
		long m_lastOffset;
		bool m_isBuilding;
	}

	public class TextAddedEventArgs : EventArgs
	{
		public TextAddedEventArgs(string text)
		{
			Text = text;
		}

		public string Text { get; private set; }
	}
}
