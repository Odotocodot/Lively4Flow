namespace Flow.Launcher.Plugin.Lively.Models
{
	public record Settings
	{
		public string LivelySettingsJsonPath { get; set; }
		public LivelyInstallType InstallType { get; set; }
		public bool HasRunQuickSetup { get; set; }
	}
}