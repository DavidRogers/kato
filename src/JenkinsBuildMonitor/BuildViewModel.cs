using System;
using Caliburn.Micro;

namespace Kato
{
	public class BuildViewModel : PropertyChangedBase
	{
		public bool Building
		{
			get { return m_building; }
			set
			{
				if (value.Equals(m_building)) return;
				m_building = value;
				NotifyOfPropertyChange(() => Building);
			}
		}

		public string Result
		{
			get { return m_result; }
			set
			{
				if (value == m_result) return;
				m_result = value;
				NotifyOfPropertyChange(() => Result);
			}
		}

		public string BuiltOn
		{
			get { return m_builtOn; }
			set
			{
				if (value == m_builtOn) return;
				m_builtOn = value;
				NotifyOfPropertyChange(() => BuiltOn);
			}
		}

		public int Number
		{
			get { return m_number; }
			set
			{
				if (value == m_number) return;
				m_number = value;
				NotifyOfPropertyChange(() => Number);
			}
		}

		public double BuildPercentage
		{
			get { return m_building ? (Duration.TotalSeconds / EstimatedDuration.TotalSeconds) * 100.00 : 0; }
		}

		public TimeSpan Duration
		{
			get { return m_duration; }
			set
			{
				if (value.Equals(m_duration)) return;
				m_duration = value;
				NotifyOfPropertyChange(() => Duration);
				NotifyOfPropertyChange(() => BuildPercentage);
			}
		}

		public TimeSpan EstimatedDuration
		{
			get { return m_estimatedDuration; }
			set
			{
				if (value.Equals(m_estimatedDuration)) return;
				m_estimatedDuration = value;
				NotifyOfPropertyChange(() => EstimatedDuration);
			}
		}

		public DateTime TimeStamp
		{
			get { return m_timeStamp; }
			set
			{
				m_timeStamp = value;
				NotifyOfPropertyChange(() => TimeStamp);
			}
		}

		public Uri Path { get; set; }

		bool m_building;
		string m_result;
		string m_builtOn;
		int m_number;
		TimeSpan m_duration;
		TimeSpan m_estimatedDuration;
		DateTime m_timeStamp;
	}
}