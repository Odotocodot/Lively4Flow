using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.UI.ViewModels;

namespace Flow.Launcher.Plugin.Lively.UI.Views
{
	public partial class SettingsView : UserControl
	{
		public SettingsView(SettingsViewModel settingsViewModel)
		{
			DataContext = settingsViewModel;
			InitializeComponent();
		}
	}
}