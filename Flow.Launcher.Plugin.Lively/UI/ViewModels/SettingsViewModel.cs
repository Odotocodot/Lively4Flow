using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Lively.Models;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.Lively.UI.ViewModels
{
	public partial class SettingsViewModel : ObservableObject
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
			PropertyChanged += ValidateSettings;
		}

		private void ValidateSettings(object sender, PropertyChangedEventArgs e)
		{
			var settingsPath = settings.LivelySettingsJsonPath;

			settings.Errors.Clear();
			if (settings.InstallType == LivelyInstallType.None)
				settings.Errors.Add("Lively is not installed!");

			if (!(File.Exists(settingsPath) && Path.GetFileName(settingsPath) == Constants.Files.LivelySettings
			                                && File.Exists(Path.Combine(
				                                Path.GetDirectoryName(settingsPath) ?? string.Empty,
				                                Constants.Files.WallpaperLayout))))
				settings.Errors.Add("Could not find Lively settings file. Please update the plugin settings");
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
			new(LivelyInstallType.None, "Not Installed"),
			new(LivelyInstallType.GitHub, "GitHub"),
			new(LivelyInstallType.MicrosoftStore, "Microsoft Store")
		};


		public bool CanRevert => oldSettings != null && (settings.InstallType != oldSettings.InstallType
		                                                 || settings.LivelySettingsJsonPath !=
		                                                 oldSettings.LivelySettingsJsonPath);


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