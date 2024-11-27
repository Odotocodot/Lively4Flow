using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively
{
	public static class Results
	{
		public static class For
		{
			public static List<Result> RandomiseCommand(LivelyService livelyService)
			{
				var singleMonitor = livelyService.MonitorCount == 1;
				var results = new List<Result>();
				const string prefix = "Set a random wallpaper";
				results.Add(new Result
				{
					Title = $"{prefix}{AppendAllMonitors(singleMonitor)}",
					Action = _ =>
					{
						livelyService.Api.RandomiseWallpaper();
						return true;
					}
				});
				if (singleMonitor)
					return results;

				livelyService.IterateMonitors(index =>
					results.Add(GetMonitorIndexResult(prefix, index, i => livelyService.Api.RandomiseWallpaper(i))));

				return results;
			}

			public static List<Result> CloseCommand(LivelyService livelyService)
			{
				if (!livelyService.GetActiveMonitorIndexes(out var activeIndexes))
					return SingleResult("No active Lively wallpapers", null, null, false);

				var indexes = activeIndexes.ToList();
				var results = new List<Result>();

				const string prefix = "Close active wallpaper";

				if (livelyService.MonitorCount == 1
				    || livelyService.WallpaperArrangement != WallpaperArrangement.Per
				    || indexes.Count > 1)
					results.Add(new Result
					{
						Title = $"{prefix}{AppendAllMonitors(indexes.Count <= 1)}",
						//TODO SubTitle = ""The name of the monitor
						Action = _ =>
						{
							livelyService.Api.CloseWallpaper();
							return true;
						}
					});

				if (livelyService.WallpaperArrangement != WallpaperArrangement.Per)
					return results;

				for (var i = 0; i < indexes.Count; i++)
					results.Add(GetMonitorIndexResult(prefix, indexes[i],
						index => livelyService.Api.CloseWallpaper(index)));

				return results;
			}

			public static List<Result> WallpaperArrangements(LivelyService livelyService) =>
				Enum.GetValues<WallpaperArrangement>()
					.Select(arrangement =>
					{
						var name = Enum.GetName(arrangement);
						var title = arrangement == livelyService.WallpaperArrangement
							? $"{SelectedEmoji} {name}"
							: name;

						return new Result
						{
							Title = title,
							ContextData = arrangement,
							Action = _ =>
							{
								//TODO reapply the current wallpapers, reset the query, update the in memory value of the arrangement
								livelyService.Api.SetWallpaperLayout(arrangement);
								// livelyService.Context.API.ChangeQuery(livelyService.Context.CurrentPluginMetadata
								// 	.ActionKeyword);
								return true;
							}
						};
					})
					.ToList();

			public static Result Command(Command command, LivelyService livelyService, List<int> highlightData = null)
			{
				var autoCompleteText =
					$"{livelyService.Context.CurrentPluginMetadata.ActionKeyword} {Models.Command.Keyword}{command.Shortcut} ";
				return new Result
				{
					Title = command.Shortcut,
					SubTitle = command.Description,
					ContextData = command,
					TitleHighlightData = highlightData,
					AutoCompleteText = autoCompleteText,
					Action = _ =>
					{
						livelyService.Context.API.ChangeQuery(autoCompleteText);
						return false;
					}
				};
			}

			public static Result Wallpaper(Wallpaper wallpaper, LivelyService livelyService,
				List<int> highlightData = null)
			{
				var title = wallpaper.Title;
				if (livelyService.IsActiveWallpaper(wallpaper, out var monitorIndexes))
					title = OffsetTitle(title,
						$"[{SelectedEmoji} {string.Join(", ", monitorIndexes.Order())}] ",
						highlightData);

				return new Result
				{
					Title = title,
					SubTitle = wallpaper.Desc,
					IcoPath = wallpaper.IconPath,
					ContextData = wallpaper,
					TitleHighlightData = highlightData,
					Action = _ =>
					{
						livelyService.Api.SetWallpaper(wallpaper);
						livelyService.Context.API.ReQuery();
						return true;
					}
				};
			}
		}

		private const string SelectedEmoji = "\u2605"; //or ⭐\u2b50
		private static string AppendAllMonitors(bool singleMonitor) => singleMonitor ? "" : " on all monitors";

		private static string OffsetTitle(string title, string offset, List<int> highlightData)
		{
			title = title.Insert(0, offset);

			if (highlightData == null)
				return title;

			for (var i = 0; i < highlightData.Count; i++)
				highlightData[i] += offset.Length;
			return title;
		}

		private static Result GetMonitorIndexResult(string prefix, int index, Action<int> action)
		{
			return new Result
			{
				Title = $"{prefix} on monitor {index}",
				Action = _ =>
				{
					action(index);
					return true;
				}
			};
		}

		public static List<Result> ToResultList<T>(this IEnumerable<T> source,
			LivelyService livelyService, string query = null)
			where T : ISearchable
		{
			var validQuery = !string.IsNullOrWhiteSpace(query);
			var results = new List<Result>();
			foreach (T element in source)
			{
				List<int> highlightData = null;

				if (validQuery)
				{
					MatchResult matchResult = livelyService.Context.API.FuzzySearch(query, element.SearchableString);
					highlightData = matchResult.MatchData;
					if (!matchResult.Success)
						continue;
				}

				Result result = element switch
				{
					Command command => For.Command(command, livelyService, highlightData),
					Wallpaper wallpaper => For.Wallpaper(wallpaper, livelyService, highlightData),
					_ => throw new InvalidCastException()
				};
				results.Add(result);
			}

			return results;
		}

		public static List<Result> SingleResult(string title, string iconPath, Action action, bool closeFlow) => new()
		{
			new Result
			{
				Title = title,
				IcoPath = iconPath,
				Action = _ =>
				{
					action?.Invoke();
					return closeFlow;
				}
			}
		};
	}
}