using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively.UI.ViewModels
{
	public partial class WallpaperViewModel
	{
		private readonly Wallpaper wallpaper;

		public WallpaperViewModel(Wallpaper wallpaper)
		{
			this.wallpaper = wallpaper;
		}

		public string Title => wallpaper.Title;
		public string Description => wallpaper.Desc;
		public string Gif => wallpaper.PreviewPath;
		public string Author => wallpaper.Author;
		public string Contact => wallpaper.Contact;
		public string Folder => wallpaper.FolderPath;
		public bool DescriptionVisibility => !string.IsNullOrWhiteSpace(Description);
		public bool AuthorVisibility => !string.IsNullOrWhiteSpace(Author);
		public bool ContactVisibility => !string.IsNullOrWhiteSpace(Contact);

		[RelayCommand]
		private async Task OpenWallpaperFolder()
		{
			using Process process = Process.Start("explorer.exe", Folder);
			await process?.WaitForExitAsync()!;
		}
	}
}