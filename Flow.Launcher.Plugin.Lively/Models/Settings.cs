using CommunityToolkit.Mvvm.ComponentModel;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public partial class Settings : ObservableValidator
	{
		[ObservableProperty] private string livelySettingsJsonPath;
		[ObservableProperty] private string livelyExePath;
		[ObservableProperty] private string livelyLibraryFolderPath;
		[ObservableProperty] private Setup.InstallType installType;
		[ObservableProperty] private bool runSetup;
	}
}