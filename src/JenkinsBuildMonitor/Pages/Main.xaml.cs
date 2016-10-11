using System.Windows;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;

namespace Kato.Pages
{
	/// <summary>
	/// Interaction logic for Main.xaml
	/// </summary>
	public partial class Main : IContent
	{
		public Main()
		{
			InitializeComponent();
		}

		public void OnNavigatedFrom(NavigationEventArgs e)
		{
			var window = (ModernWindow) Application.Current.MainWindow;
			window.IsNavigationControlVisible = true;
		}

		public void OnNavigatedTo(NavigationEventArgs e)
		{
			var window = (ModernWindow) Application.Current.MainWindow;
			window.IsNavigationControlVisible = false;
			e.Frame.ClearHistory();
		}

		public void OnFragmentNavigation(FragmentNavigationEventArgs e)
		{
		}

		public void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
		}

		private void Label_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SingleInstance<App>.Cleanup();
			// init restart here...
			Application.Current.Shutdown();
		}
	}
}
