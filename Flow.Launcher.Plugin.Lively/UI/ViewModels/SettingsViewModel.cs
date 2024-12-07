using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI.Validation;

namespace Flow.Launcher.Plugin.Lively.UI.ViewModels
{
	public partial class SettingsViewModel : ObservableValidator
	{
		private readonly Settings settings;
		private readonly PluginInitContext context;

		//For DesignTime
		public SettingsViewModel()
		{
			settings = new Settings
			{
				LivelySettingsJsonPath = @"C:\Folder\Settings.json",
				LivelyExePath = @"C:\Folder\Lively.exe",
				LivelyLibraryFolderPath = @"C:\Folder\Library",
				InstallType = LivelyInstallType.GitHub,
				HasRunQuickSetup = true,
				CommandKeyword = "!",
				UseMonitorName = false
			};
		}

		public SettingsViewModel(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			this.context = context;
		}

		[Required]
		[LivelyLibraryFolderValidation]
		public string LivelyLibraryFolder
		{
			get => settings.LivelyLibraryFolderPath;
			set
			{
				settings.LivelyLibraryFolderPath = value;
				OnPropertyChanged();
				ValidateProperty(value);
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

		[Required]
		[LivelySettingsFileValidation]
		public string LivelySettingsFile
		{
			get => settings.LivelySettingsJsonPath;
			set
			{
				settings.LivelySettingsJsonPath = value;
				OnPropertyChanged();
				ValidateProperty(value);
			}
		}

		[Required]
		[LivelyExePathValidation]
		public string LivelyExePath
		{
			get => settings.LivelyExePath;
			set
			{
				settings.LivelyExePath = value;
				OnPropertyChanged();
			}
		}

		[Required]
		[MinLength(1)]
		public string CommandKeyword
		{
			get => settings.CommandKeyword;
			set
			{
				settings.CommandKeyword = value;
				OnPropertyChanged();
				ValidateProperty(value);
			}
		}

		[EnumDataType(typeof(LivelyInstallType))]
		public LivelyInstallType LivelyInstallType
		{
			get => settings.InstallType;
			set
			{
				settings.InstallType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ExeVisibility));
				ValidateProperty(value);
			}
		}

		public bool ExeVisibility => LivelyInstallType == LivelyInstallType.GitHub;

		[RelayCommand]
		private void QuickSetup()
		{
			Lively.QuickSetup.ForceRun(settings, context);
			//TODO: Notify properties have changed
		}
	}
}