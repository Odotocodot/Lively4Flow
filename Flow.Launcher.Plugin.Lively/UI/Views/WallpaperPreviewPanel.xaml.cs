using System;
using System.Windows;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.UI.ViewModels;

namespace Flow.Launcher.Plugin.Lively.UI.Views
{
	public partial class WallpaperPreviewPanel : UserControl
	{
		public WallpaperPreviewPanel(WallpaperViewModel viewModel)
		{
			DataContext = viewModel;
			InitializeComponent();
		}


		private void LoopGif(object sender, RoutedEventArgs e)
		{
			if (sender is not MediaElement element)
				return;
			element.Position = TimeSpan.FromMilliseconds(1);
			element.Play();
		}
	}
}