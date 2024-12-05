namespace Flow.Launcher.Plugin.Lively
{
	public static class Constants
	{
		public static class Files
		{
			public const string WallpaperLayout = "WallpaperLayout.json";
			public const string LivelyInfo = "LivelyInfo.json";
			public const string LivelySettings = "Settings.json";
		}

		public static class Folders
		{
			public const string LocalWallpapers = "wallpapers";
			public const string WebWallpapers = $"{SaveData}\\wptmp";
			public const string WpData = $"{SaveData}\\wpdata";
			public const string SaveData = "SaveData";
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
		}

		public static class Lively
		{
			public const string AppName = "12030rocksdanister.LivelyWallpaper";
			public const string AppID = AppName + "_97hta09mmv6hy";
		}
	}
}