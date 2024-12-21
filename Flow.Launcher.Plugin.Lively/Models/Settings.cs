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
		[JsonIgnore] public List<string> Errors { get; } = new();
		[JsonIgnore] public bool HasErrors => Errors.Count > 0;

		public void Validate()
		{
			Errors.Clear();
			if (InstallType == LivelyInstallType.None)
				Errors.Add("Lively is not installed!");

			if (!(File.Exists(LivelySettingsJsonPath) &&
			      Path.GetFileName(LivelySettingsJsonPath) == Constants.Files.LivelySettings && File.Exists(
				      Path.Combine(Path.GetDirectoryName(LivelySettingsJsonPath) ?? string.Empty,
					      Constants.Files.WallpaperLayout))))
				Errors.Add("Could not find Lively settings file. Please update the plugin settings");
		}
	}
}