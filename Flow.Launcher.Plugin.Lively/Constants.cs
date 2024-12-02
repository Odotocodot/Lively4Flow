using System.IO;
using System.Reflection;

namespace Flow.Launcher.Plugin.Lively
{
	public static class Constants
	{
		public static class Files
		{
			public const string WallpaperLayout = "WallpaperLayout.json";
			public const string LivelyInfo = "LivelyInfo.json";
		}

		public static class Folders
		{
			public const string LocalWallpapers = "wallpapers";
			public const string WebWallpapers = "SaveData\\wptmp";
		}

		public static class Commands
		{
			public const string Keyword = "!";
			public const string Open = "open";
		}

		public static readonly string CommandUtility = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			"Lively Command Utility\\Livelycu.exe");
	}
}