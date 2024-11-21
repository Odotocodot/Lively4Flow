using System.Collections.Generic;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class ResultCreator
	{
		public Result FromWallpaper(Wallpaper wallpaper) => FromWallpaper(wallpaper, null);

		public Result FromWallpaper(Wallpaper wallpaper, List<int> highlightData) => new()
		{
			Title = wallpaper.Title,
			SubTitle = wallpaper.Desc,
			IcoPath = wallpaper.IconPath,
			ContextData = wallpaper,
			TitleHighlightData = highlightData
		};

		public List<Result> Empty() => new()
		{
			new Result
			{
				Title = "Search"
			}
		};
	}
}