using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class ResultCreator
	{
		public Result FromWallpaper(Wallpaper wallpaper) =>
			new()
			{
				Title = wallpaper.Title,
				SubTitle = wallpaper.Desc,
				IcoPath = wallpaper.IconPath
			};
	}
}