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
			PropertyChanged += (_, _) => this.settings.Validate();
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


		public bool CanRevert => oldSettings != null && settings != oldSettings;


		[RelayCommand]
		private void RunQuickSetup()
		{
			oldSettings = settings with { };
			QuickSetup.Run(settings, context, true);
			OnPropertyChanged(nameof(LivelySettingsFile));
			OnPropertyChanged(nameof(LivelyInstallType));
			revertCommand?.NotifyCanExecuteChanged();
		}

		[RelayCommand(CanExecute = nameof(CanRevert))]
		private void Revert()
		{
			LivelySettingsFile = oldSettings.LivelySettingsJsonPath;
			LivelyInstallType = oldSettings.InstallType;
			oldSettings = null;
			revertCommand?.NotifyCanExecuteChanged();
		}
	}
}