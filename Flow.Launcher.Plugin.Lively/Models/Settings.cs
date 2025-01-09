using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public record Settings
	{
		public string LivelySettingsJsonPath { get; set; }
		public LivelyInstallType InstallType { get; set; }
		public bool HasRunQuickSetup { get; set; }
		[JsonIgnore] public HashSet<string> Errors { get; } = new();
		[JsonIgnore] public bool HasErrors => Errors.Count > 0;

		public void Validate()
		{
			Errors.Clear();
			if (InstallType == LivelyInstallType.None)
				Errors.Add(ErrorStrings.NotInstalled);

			if (!(File.Exists(LivelySettingsJsonPath) &&
			      Path.GetFileName(LivelySettingsJsonPath) == Constants.Files.LivelySettings && File.Exists(
				      Path.Combine(Path.GetDirectoryName(LivelySettingsJsonPath) ?? string.Empty,
					      Constants.Files.WallpaperLayout))))
				Errors.Add(ErrorStrings.InvalidLivelySettingsFile);
		}


		public static class ErrorStrings
		{
			public const string NotInstalled = "Lively is not installed!";

			public const string InvalidLivelySettingsFile =
				"Could not find the Lively Settings.json file. Please update the plugin settings";

			public const string InvalidWallpaperDirectory =
				"Could not find the wallpaper directories. Please update the plugin settings";
		}
	}
}