using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively
{
	public static class ResultFrom
	{
		public static List<Result> WallpaperArrangements(LivelyService livelyService) =>
			Enum.GetValues<WallpaperArrangement>()
				.Select(arrangement =>
				{
					var title = Enum.GetName(arrangement);
					if (arrangement == livelyService.WallpaperArrangement)
						title = $"\u2605 {title}"; //or ⭐\u2b50
					return new Result
					{
						Title = title,
						ContextData = arrangement,
						Action = _ =>
						{
							//TODO reapply the current wallpapers, reset the query, update the in memory value of the arrangement
							livelyService.Api.SetWallpaperLayout(arrangement);
							livelyService.Context.API.ChangeQuery(livelyService.Context.CurrentPluginMetadata
								.ActionKeyword);
							return true;
						}
					};
				})
				.ToList();

		public static List<Result> RandomiseCommand(LivelyService livelyService)
		{
			var singleMonitor = livelyService.MonitorCount == 1;
			var results = new List<Result>();
			results.Add(new Result
			{
				Title = $"Set a random wallpaper {(singleMonitor ? "" : "on all monitors")}",
				Action = _ =>
				{
					livelyService.Api.RandomiseWallpaper();
					return true;
				}
			});
			if (!singleMonitor)
				livelyService.IterateMonitors(index =>
				{
					results.Add(new Result
					{
						Title = $"Set a random wallpaper on monitor {index}",
						Action = _ =>
						{
							livelyService.Api.RandomiseWallpaper(index);
							return true;
						}
					});
				});
			return results;
		}
	}


	public static class ResultCreator
	{
		public static List<Result> SingleResult(string title, string iconPath, Action action, bool closeFlow) => new()
		{
			new Result
			{
				Title = title,
				IcoPath = iconPath,
				Action = _ =>
				{
					action();
					return closeFlow;
				}
			}
		};

		public static List<Result> FilterToResultsList<T>(this IReadOnlyList<T> source, LivelyService livelyService,
			string query)
			where T : ISearchableResult
		{
			var results = new List<Result>();
			for (var i = 0; i < source.Count; i++)
			{
				T element = source[i];
				MatchResult matchResult = livelyService.Context.API.FuzzySearch(query, element.SearchableString);
				if (matchResult.Success)
					results.Add(element.ToResult(livelyService, matchResult.MatchData));
			}

			return results;
		}

		public static List<Result> ToResultsList<T>(this IReadOnlyList<T> source, LivelyService livelyService)
			where T : ISearchableResult
		{
			var results = new List<Result>();
			for (var i = 0; i < source.Count; i++)
				results.Add(source[i].ToResult(livelyService));
			return results;
		}
	}
}