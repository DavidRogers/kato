using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Kato.Resources
{
	public static class Converters
	{
		public static IValueConverter BoolToVisibilityConverter = new BooleanToVisibilityConverter();
		public static IValueConverter ReverseBoolToVisibilityConverter = new ReverseBooleanToVisibilityConverter();
		public static IValueConverter TextNullOrEmptyToVisibility = new TextNullOrEmptyToVisibilityConverter();
		public static IValueConverter IsNotNull = new IsNotNullConverter();
		public static IValueConverter IsSelected = new IsSelectedConverter();
		public static IValueConverter NullIfEmpty = new NullIfEmpty();
		public static IValueConverter TimeAgo = new FriendlyTimeDescription();
		public static IValueConverter IsLessThan = new IsLessThanConverter();
		public static IValueConverter StatusToColorConverter = new StatusToColorConverter();
		public static IValueConverter StatusToHumanReadableConverter = new StatusToHumanReadableConverter();
		public static IValueConverter StatusToBuildingColorConverter = new StatusToBuildingColorConverter();
		public static IValueConverter IsValidToBrush = new IsValidToBrushConverter();
		public static IValueConverter PrettyTimeSpanConverter = new PrettyTimeSpanConverter();
	}

	public class StatusToHumanReadableConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			BuildStatus status = (BuildStatus) value;

			string text;
			switch (status)
			{
			case BuildStatus.Success:
				text = "Successful";
				break;
			case BuildStatus.SuccessAndBuilding:
				text = "Successful and building";
				break;
			case BuildStatus.Failed:
				text = "Failed";
				break;
			case BuildStatus.FailedAndBuilding:
				text = "Failed and building";
				break;
			case BuildStatus.Aborted:
				text = "Aborted";
				break;
			case BuildStatus.Disabled:
				text = "Disabled";
				break;
			default:
				text = status.ToString();
				break;
			}

			return text;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class PrettyTimeSpanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return "";

			TimeSpan time = (TimeSpan) value;
			string format = time.Hours > 0 ? "{0}h {1}m {2}s" : "{0}m {1}s";

			return time.Hours > 0 ? string.Format(format, time.Hours, time.Minutes, time.Seconds) : string.Format(format, time.Minutes, time.Seconds);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IsValidToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool) value ? null : Brushes.Red;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class StatusToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string status = value as string;
			bool alterBuilding;
			bool.TryParse((string) parameter, out alterBuilding);

			Brush color = Brushes.Gray;
			switch (status)
			{
			case "blue":
				color = Brushes.DarkGreen;
				break;
			case "blue_anime":
				color = alterBuilding ? Brushes.DarkGreen : Brushes.Yellow;
				break;
			case "red":
				color = Brushes.DarkRed;
				break;
			case "red_anime":
				color = alterBuilding ? Brushes.DarkRed : Brushes.Orange;
				break;
			case "aborted":
			case "disabled":
			default:
				color = Brushes.Gray;
				break;
			}

			return color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class StatusToBuildingColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string status = value as string;
			Color color = Colors.Gray;
			switch (status)
			{
			case "blue":
				color = Colors.DarkGreen;
				break;
			case "blue_anime":
				color = Colors.ForestGreen;
				break;
			case "red":
				color = Colors.DarkRed;
				break;
			case "red_anime":
				color = Colors.Red;
				break;
			case "aborted":
			case "disabled":
			default:
				color = Colors.Gray;
				break;
			}

			return color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IsLessThanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int) value < int.Parse((string) parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ReverseBooleanToVisibilityConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool) value)
				return Visibility.Collapsed;

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class TextNullOrEmptyToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (string.IsNullOrEmpty((string) value))
				return Visibility.Collapsed;

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class IsNotNullConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class IsSelectedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Compare((string) value, (string) parameter, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class NullIfEmpty : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			string text = (string) value;
			return text.Trim().Length == 0 ? null : text;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	[ValueConversion(typeof(DateTime), typeof(string))]
	public class FriendlyTimeDescription : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			DateTime time = (DateTime) value;
			return Describe(DateTime.UtcNow - time);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}

		private static string Describe(TimeSpan t)
		{
			int[] ints = { t.Days, t.Hours, t.Minutes, t.Seconds };

			double[] doubles = { t.TotalDays, t.TotalHours, t.TotalMinutes, t.TotalSeconds };

			var firstNonZero = ints
				.Select((value, index) => new { value, index })
				.FirstOrDefault(x => x.value != 0);

			if (firstNonZero == null)
				return "now";

			int i = firstNonZero.index;
			string prefix = (i >= 3) ? "" : "about ";
			int quantity = (int) Math.Round(doubles[i]);
			return prefix + Tense(quantity, s_names[i]) + " ago";
		}

		private static string Tense(int quantity, string noun)
		{
			return quantity == 1
				? "1 " + noun
				: string.Format("{0} {1}s", quantity, noun);
		}

		static readonly string[] s_names = { "day", "hour", "minute", "second" };
	}
}
