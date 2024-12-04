namespace Flow.Launcher.Plugin.Lively.Models
{
	public class Settings
	{
		public string LivelySettingsJsonPath { get; set; }
		public string LivelyExePath { get; set; }
		public string LivelyLibraryFolderPath { get; set; }
		public Setup.InstallType InstallType { get; set; }
		public bool RunSetup { get; set; }
		public string CommandKeyword { get; set; } = "!";
		public bool UseMonitorName { get; set; }
	}
}