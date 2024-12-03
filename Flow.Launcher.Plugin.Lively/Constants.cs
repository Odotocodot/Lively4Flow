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

		public static readonly string CommandUtility = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			"Lively Command Utility\\Livelycu.exe");
	}
}