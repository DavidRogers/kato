using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using log4net;

namespace Kato
{
	public class FlowDocumentExt
	{
		public static readonly DependencyProperty MessageCollectionProperty =
			DependencyProperty.RegisterAttached("MessageCollection", typeof(ConsoleOutputControl), typeof(FlowDocumentExt), new PropertyMetadata(null, MessageCollectionChanged));

		public static ConsoleOutputControl GetMessageCollection(DependencyObject d)
		{
			return (ConsoleOutputControl) d.GetValue(MessageCollectionProperty);
		}

		public static void SetMessageCollection(DependencyObject d, ConsoleOutputControl value)
		{
			d.SetValue(MessageCollectionProperty, value);
		}

		private static void MessageCollectionChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			FlowDocumentScrollViewer viewer = (FlowDocumentScrollViewer) sender;
			if (e.NewValue != null)
				HookupNewCollection(viewer, (ConsoleOutputControl) e.NewValue);
			else
				UnhookOldCollection(viewer);
		}

		private static void UnhookOldCollection(FlowDocumentScrollViewer viewer)
		{
			if (viewer.Tag != null)
			{
				DocumentManager manager = (DocumentManager) viewer.Tag;
				manager.Unhook();
				viewer.Tag = null;
			}
		}

		private static void HookupNewCollection(FlowDocumentScrollViewer viewer, ConsoleOutputControl messages)
		{
			UnhookOldCollection(viewer);
			viewer.Tag = new DocumentManager(messages, viewer);
		}

		private static ScrollViewer FindScrollViewer(FlowDocumentScrollViewer flowDocumentScrollViewer)
		{
			if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
			{
				return null;
			}

			// Border is the first child of first child of a ScrolldocumentViewer
			DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
			if (firstChild == null)
			{
				return null;
			}

			Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

			if (border == null)
			{
				return null;
			}

			return border.Child as ScrollViewer;
		}

		private class DocumentManager
		{
			public DocumentManager(ConsoleOutputControl consoleOutput, FlowDocumentScrollViewer viewer)
			{
				m_viewer = viewer;
				m_consoleOutput = consoleOutput;
				consoleOutput.TextAddedEvent += consoleOutput_TextAddedEvent;
				m_viewer.Document = new FlowDocument(new Paragraph());
			}

			private void consoleOutput_TextAddedEvent(object sender, TextAddedEventArgs e)
			{
				AddText(e.Text);
			}

			private void AddText(string text)
			{
				((Paragraph) m_viewer.Document.Blocks.FirstBlock).Inlines.Add(text);

				ScrollToLatestContent();
			}

			private void ScrollToLatestContent()
			{
				ScrollViewer scrollViewer = FindScrollViewer(m_viewer);
				scrollViewer.ScrollToEnd();
			}

			public void Unhook()
			{
				m_consoleOutput.TextAddedEvent -= consoleOutput_TextAddedEvent;
				if (m_viewer.Document != null)
					m_viewer.Document.Blocks.Clear();
			}

			readonly ConsoleOutputControl m_consoleOutput;
			readonly FlowDocumentScrollViewer m_viewer;
		}
	}
}
