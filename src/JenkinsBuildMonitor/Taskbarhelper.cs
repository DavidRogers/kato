using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kato
{
	public static class TaskbarHelper
	{
		public static ImageSource GetNewMessagesNotificationOverlay(Window window, DataTemplate template, int count = 0)
		{
			if (window == null)
				return null;

			var presentation = PresentationSource.FromVisual(window);
			if (presentation == null)
				return null;
			Matrix m = presentation.CompositionTarget.TransformToDevice;
			double dx = m.M11;
			double dy = m.M22;

			double iconWidth = 16.0 * dx;
			double iconHeight = 16.0 * dy;

			string countText = count.ToString();

			RenderTargetBitmap bmp = new RenderTargetBitmap((int) iconWidth, (int) iconHeight, 96, 96, PixelFormats.Default);

			ContentControl root = new ContentControl
			{
				ContentTemplate = template,
				Content = count > 99 ? "…" : countText
			};

			root.Arrange(new Rect(0, 0, iconWidth, iconHeight));

			bmp.Render(root);

			return bmp;
		}
	}
}
