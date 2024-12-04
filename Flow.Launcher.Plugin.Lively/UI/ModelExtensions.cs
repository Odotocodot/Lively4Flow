using System;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI.ViewModels;
using Flow.Launcher.Plugin.Lively.UI.Views;

namespace Flow.Launcher.Plugin.Lively.UI
{
	public static class ModelExtensions
	{
		public static Lazy<UserControl> GetUserControl(this Wallpaper wallpaper) =>
			new(() => new WallpaperPreviewPanel(new WallpaperViewModel(wallpaper)));

		public static Control GetSettingsView(this Settings settings, PluginInitContext context) =>
			new SettingsView(new SettingsViewModel(settings, context));
	}
}