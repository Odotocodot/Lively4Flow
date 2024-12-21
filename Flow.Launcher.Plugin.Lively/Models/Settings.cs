using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public record Settings : IErrorInfo
	{
		public string LivelySettingsJsonPath { get; set; }
		public LivelyInstallType InstallType { get; set; }
		public bool HasRunQuickSetup { get; set; }
		[JsonIgnore] public List<string> Errors { get; } = new();
	}
}