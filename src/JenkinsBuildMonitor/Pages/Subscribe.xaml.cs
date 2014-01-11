using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kato.Pages
{
	/// <summary>
	/// Interaction logic for Subscribe.xaml
	/// </summary>
	public partial class Subscribe : Page
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
	}
}
