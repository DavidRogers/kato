using Caliburn.Micro;

namespace Kato
{
	public sealed class StatusViewModel : PropertyChangedBase
	{
		public string Message
		{
			get { return m_message; }
			set
			{
				if (value == m_message) return;
				m_message = value;
				NotifyOfPropertyChange(() => Message);
			}
		}

		public int TotalFail
		{
			get { return m_totalFail; }
			set
			{
				if (value == m_totalFail) return;
				m_totalFail = value;
				NotifyOfPropertyChange(() => TotalFail);
			}
		}

		public int TotalSuccess
		{
			get { return m_totalSuccess; }
			set
			{
				if (value == m_totalSuccess) return;
				m_totalSuccess = value;
				NotifyOfPropertyChange(() => TotalSuccess);
			}
		}

		string m_message;
		int m_totalSuccess;
		int m_totalFail;
	}
}