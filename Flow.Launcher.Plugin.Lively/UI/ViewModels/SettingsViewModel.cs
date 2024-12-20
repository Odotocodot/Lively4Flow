using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI.Validation;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.Lively.UI.ViewModels
{
	public partial class SettingsViewModel : ObservableValidator
	{
		private readonly Settings settings;
		private readonly PluginInitContext context;

		private Settings oldSettings;

		[UsedImplicitly(Reason = "For design time WPF viewing")]
		public SettingsViewModel()
		{
			settings = new Settings
			{
				LivelySettingsJsonPath = @"C:\Folder\Settings.json",
				InstallType = LivelyInstallType.GitHub,
				HasRunQuickSetup = true
			};
		}

		public SettingsViewModel(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			this.context = context;
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


		public LivelyInstallType LivelyInstallType
		{
			get => settings.InstallType;
			set
			{
				settings.InstallType = value;
				OnPropertyChanged();
			}
		}

		public LivelyInstallTypeViewModel[] InstallTypes { get; } =
		{
			new(LivelyInstallType.None, "Not Installed", false),
			new(LivelyInstallType.GitHub, "GitHub", true),
			new(LivelyInstallType.MicrosoftStore, "Microsoft Store", true)
		};


		public bool CanRevert => oldSettings != null
		                         && (settings.InstallType != oldSettings.InstallType
		                             || settings.LivelySettingsJsonPath != oldSettings.LivelySettingsJsonPath);


		[RelayCommand]
		private void RunQuickSetup()
		{
			oldSettings = settings with { HasRunQuickSetup = false };
			QuickSetup.Run(settings, context, true);
			OnPropertyChanged(nameof(LivelySettingsFile));
			OnPropertyChanged(nameof(LivelyInstallType));
			OnPropertyChanged(nameof(CanRevert));
		}

		[RelayCommand(CanExecute = nameof(CanRevert))]
		private void Revert()
		{
			settings.LivelySettingsJsonPath = oldSettings.LivelySettingsJsonPath;
			settings.InstallType = oldSettings.InstallType;
			oldSettings = null;
			OnPropertyChanged(nameof(LivelySettingsFile));
			OnPropertyChanged(nameof(LivelyInstallType));
			OnPropertyChanged(nameof(CanRevert));
		}
	}
}