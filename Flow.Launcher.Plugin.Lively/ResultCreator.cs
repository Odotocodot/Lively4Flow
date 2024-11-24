using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.Lively
{
	public class ResultCreator
	{
		public static List<Result> SingleResult(string title, string iconPath, Action action, bool closeOnAction) =>
			new()
			{
				new Result
				{
					Title = title,
					IcoPath = iconPath,
					Action = _ =>
					{
						action();
						return closeOnAction;
					}
				}
			};


		public static List<Result> WallpaperArrangementResults(LivelyService livelyService)
		{
			return Enum.GetValues<WallpaperArrangement>().Select(value => value.ToResult(livelyService)).ToList();
		}
	}
}