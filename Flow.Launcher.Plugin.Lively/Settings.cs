using CommunityToolkit.Mvvm.ComponentModel;

namespace Flow.Launcher.Plugin.Lively
{
	public partial class Settings : ObservableValidator
	{
		[ObservableProperty] private string livelySettingsJsonPath;
		[ObservableProperty] private string livelyExePath;
		[ObservableProperty] private string livelyLibraryFolderPath;
		[ObservableProperty] private bool runSetup;
	}
}