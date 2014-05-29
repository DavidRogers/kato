using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;

namespace Kato.Pages
{
	/// <summary>
	/// Interaction logic for Subscribe.xaml
	/// </summary>
	public partial class Subscribe : IContent
	{
		public Subscribe()
		{
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			AppModel model = (AppModel) ((Page) sender).DataContext;
			if (model.Servers != null && model.Servers.Any())
			{
				FilterBox.Focus();
			}
			else
			{
				AddServerBorder.Width = 200;
				AddServerBox.Focus();
			}
		}

		private void AddServerButton_Click(object sender, RoutedEventArgs e)
		{
			AddServerBox.Focus();
		}

		public void OnFragmentNavigation(FragmentNavigationEventArgs e)
		{
		}

		public void OnNavigatedFrom(NavigationEventArgs e)
		{
			try
			{
				((MainWindow) Application.Current.MainWindow).Model.SaveSettings();
			}
			catch
			{
			}
		}

		public void OnNavigatedTo(NavigationEventArgs e)
		{
		}

		public void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
		}
	}
}
