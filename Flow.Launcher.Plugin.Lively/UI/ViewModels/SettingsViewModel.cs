using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively.UI.ViewModels
{
	public partial class SettingsViewModel : ObservableValidator
	{
		private readonly Settings settings;
		private readonly PluginInitContext context;

		public SettingsViewModel(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			this.context = context;
		}

		public string LivelyLibraryFolder
		{
			get => settings.LivelyLibraryFolderPath;
			set
			{
				settings.LivelyLibraryFolderPath = value;
				OnPropertyChanged();
			}
		}

		public bool UseMonitorName
		{
			get => settings.UseMonitorName;
			set
			{
				settings.UseMonitorName = value;
				OnPropertyChanged();
			}
		}

		public string LivelySettingsFile
		{
			get => settings.LivelySettingsJsonPath;
			set
			{
				settings.LivelySettingsJsonPath = value;
				OnPropertyChanged();
			}
		}

		public string LivelyExePath
		{
			get => settings.LivelyExePath;
			set
			{
				settings.LivelyExePath = value;
				OnPropertyChanged();
			}
		}

		public string CommandKeyword
		{
			get => settings.CommandKeyword;
			set
			{
				settings.CommandKeyword = value;
				OnPropertyChanged();
			}
		}

		public Setup.InstallType LivelyInstallType
		{
			get => settings.InstallType;
			set
			{
				settings.InstallType = value;
				OnPropertyChanged();
			}
		}

		[RelayCommand]
		private void QuickSetup()
		{
			Setup.ForceRun(settings, context);
			//TODO: Notify properties have changed
		}
	}
}