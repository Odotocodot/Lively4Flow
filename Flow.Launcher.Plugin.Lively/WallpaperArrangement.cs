using System;

namespace Flow.Launcher.Plugin.Lively
{
	public enum WallpaperArrangement
	{
		Per,
		Span,
		Duplicate
	}

	public static class WallpaperArrangementExtensions
	{
		public static Result ToResult(this WallpaperArrangement wpArrangement, LivelyService livelyService)
		{
			var title = Enum.GetName(wpArrangement);
			if (wpArrangement == livelyService.WallpaperArrangement)
				title = $"\u2605 {title}"; //or ⭐\u2b50
			return new Result
			{
				Title = title,
				ContextData = wpArrangement,
				Action = _ =>
				{
					livelyService.Api.SetWallpaperLayout(wpArrangement);
					return true;
				}
			};
		}
	}
}