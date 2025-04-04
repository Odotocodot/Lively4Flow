using System;
using System.IO;
using System.Reflection;

namespace Flow.Launcher.Plugin.Lively
{
	public static class Constants
	{
		public const string PluginName = "LivelyWallpaperController";

		public static string CommandUtility { get; } = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
			"LivelyCommandUtility", "Livelycu.exe");

		public static class Files
		{
			public const string WallpaperLayout = "WallpaperLayout.json";
			public const string LivelyInfo = "LivelyInfo.json";
			public const string LivelySettings = "Settings.json";
		}

		public static class Folders
		{
			public static string[] Wallpapers { get; } = { "wallpapers", Path.Combine("SaveData", "wptmp") };

			public static string DefaultWallpaperFolder { get; } = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Lively Wallpaper", "Library");

			public static string DefaultWallpaperFolderMSStore { get; } = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Packages", Lively.AppId, "LocalCache", "Local", "Lively Wallpaper", "Library");
		}

		public static class Commands
		{
			public const string Keyword = "!";
		}

		public static class Icons
		{
			private const string Base = "Images\\";
			public const string Set = Lively;
			public const string Random = Lively;
			public const string Close = Lively;
			public const string Volume = Lively;
			public const string Layout = Lively;
			public const string Playback = Lively;
			public const string Open = Lively;
			public const string Quit = Lively;
			public const string Lively = Base + "icon.png";
			public static GlyphInfo Warning { get; } = new("Segoe Fluent Icons,Segoe MDL2 Assets", "\xE7BA");
			public static GlyphInfo Error { get; } = new("Segoe Fluent Icons,Segoe MDL2 Assets", "\xEA39");
		}

		public static class Lively
		{
			public const string AppName = "12030rocksdanister.LivelyWallpaper";
			public const string AppId = AppName + "_97hta09mmv6hy";
		}
	}
}