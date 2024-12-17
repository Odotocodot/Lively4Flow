using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively
{
	using Icons = Constants.Icons;

	public static class Results
	{
		public static class For
		{
			public static List<Result> RandomiseCommand(LivelyService livelyService)
			{
				var results = new List<Result>();
				const string prefix = "Set a random wallpaper";
				results.Add(new Result
				{
					Title = $"{prefix}{AppendAllMonitors(livelyService.IsSingleDisplay)}",
					Score = (livelyService.MonitorCount + 2) * ScoreMultiplier,
					IcoPath = Icons.Random,
					Action = _ =>
					{
						livelyService.Api.RandomiseWallpaper();
						return true;
					}
				});
				if (livelyService.IsSingleDisplay)
					return results;

				livelyService.IterateMonitors(index =>
					results.Add(GetMonitorIndexResult(prefix,
						Icons.Random,
						index,
						(livelyService.MonitorCount + 1) * ScoreMultiplier,
						i => livelyService.Api.RandomiseWallpaper(i))));

				return results;
			}

			public static List<Result> CloseCommand(LivelyService livelyService)
			{
				var activeMonitors = livelyService.ActiveMonitorIndexes;
				if (!activeMonitors.Any())
					return SingleResult("No active Lively wallpapers", Icons.Close, null, false);

				var results = new List<Result>();
				const string prefix = "Close active wallpaper";

				if (livelyService.IsSingleDisplay || activeMonitors.Count > 1)
					results.Add(new Result
					{
						Title = $"{prefix}{AppendAllMonitors(activeMonitors.Count <= 1)}",
						SubTitle = activeMonitors.Count <= 1
							? $"Current Wallpaper: {activeMonitors.First().Value.Title}"
							: null,
						Score = (livelyService.MonitorCount + 2) * ScoreMultiplier,
						IcoPath = Icons.Close,
						Action = _ =>
						{
							livelyService.Api.CloseWallpaper();
							return true;
						}
					});

				if (livelyService.IsSingleDisplay)
					return results;

				results.AddRange(activeMonitors.Select(activeMonitor =>
					GetMonitorIndexResult(prefix,
						Icons.Close,
						activeMonitor.Key,
						(livelyService.MonitorCount + 1) * ScoreMultiplier,
						index => livelyService.Api.CloseWallpaper(index),
						$"Current Wallpaper: {activeMonitor.Value.Title}")));

				return results;
			}

			public static List<Result> VolumeCommand(LivelyService livelyService, string query)
			{
				int? volume = null;
				string titleSuffix = null;
				if (int.TryParse(query, out var vol))
				{
					volume = Math.Clamp(vol, 0, 100);
					titleSuffix = $" to {volume}";
				}

				return new List<Result>
				{
					new()
					{
						Title = $"Set wallpaper volume{titleSuffix}",
						SubTitle = "Type a value between 0 and 100 to specify",
						Score = 3 * ScoreMultiplier,
						IcoPath = Icons.Volume,
						Action = _ =>
						{
							if (volume.HasValue)
								livelyService.Api.SetVolume(volume.Value);
							return volume.HasValue;
						}
					},
					new()
					{
						Title = "Set wallpaper volume to 0 (Mute)",
						Score = 2 * ScoreMultiplier,
						IcoPath = Icons.Volume,
						Action = _ =>
						{
							livelyService.Api.SetVolume(0);
							return true;
						}
					},
					new()
					{
						Title = "Set wallpaper volume to 50",
						Score = 1 * ScoreMultiplier,
						IcoPath = Icons.Volume,
						Action = _ =>
						{
							livelyService.Api.SetVolume(50);
							return true;
						}
					},
					new()
					{
						Title = "Set wallpaper volume to 100 (Max)",
						Score = 0,
						IcoPath = Icons.Volume,
						Action = _ =>
						{
							livelyService.Api.SetVolume(100);
							return true;
						}
					}
				};
			}

			public static List<Result> PlaybackCommand(LivelyService livelyService) => new()
			{
				new Result
				{
					Title = "Resume wallpaper playback",
					Score = 2000,
					IcoPath = Icons.Playback,
					Action = _ =>
					{
						livelyService.Api.WallpaperPlayback(true);
						return true;
					}
				},
				new Result
				{
					Title = "Pause wallpaper playback",
					Score = 0,
					IcoPath = Icons.Playback,
					Action = _ =>
					{
						livelyService.Api.WallpaperPlayback(false);
						return true;
					}
				}
			};

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
							Score = ScoreMultiplier * (3 - (int)arrangement),
							IcoPath = Icons.Layout,
							Action = _ =>
							{
								if (arrangement == livelyService.WallpaperArrangement)
									return false;

								Wallpaper activeWallpaper = livelyService.ActiveMonitorIndexes
									.DefaultIfEmpty()
									.MinBy(x => x.Key)
									.Value;
								livelyService.Api.SetWallpaperLayout(arrangement);
								//Task.Delay(500).Wait();
								if (activeWallpaper != null)
									livelyService.Api.SetWallpaper(activeWallpaper);
								return true;
							}
						};
					})
					.ToList();

			public static Result Command(Command command, PluginInitContext context, List<int> highlightData = null)
			{
				var autoCompleteText =
					$"{context.CurrentPluginMetadata.ActionKeyword} {Constants.Commands.Keyword}{command.Shortcut} ";
				return new Result
				{
					Title = command.Shortcut,
					SubTitle = command.Description,
					ContextData = command,
					TitleHighlightData = highlightData,
					IcoPath = command.IconPath,
					AutoCompleteText = autoCompleteText,
					Action = _ =>
					{
						context.API.ChangeQuery(autoCompleteText);
						return false;
					}
				};
			}

			public static Result Wallpaper(Wallpaper wallpaper, LivelyService livelyService,
				List<int> highlightData = null)
			{
				var title = wallpaper.Title;
				var score = 0;
				if (livelyService.IsActiveWallpaper(wallpaper, out var monitorIndexes))
				{
					var offset = livelyService.IsSingleDisplay
						? $"{SelectedEmoji} | "
						: $"{SelectedEmoji} {string.Join(", ", monitorIndexes)} | ";
					title = OffsetTitle(title, offset, highlightData);
					score = 2 * ScoreMultiplier;
				}

				return new Result
				{
					Title = title,
					SubTitle = wallpaper.Desc,
					IcoPath = wallpaper.IconPath,
					Score = score,
					ContextData = wallpaper,
					PreviewPanel = wallpaper.GetUserControl(),
					TitleHighlightData = highlightData,
					Action = _ =>
					{
						livelyService.Api.SetWallpaper(wallpaper);
						return true;
					}
				};
			}
		}

		private const string SelectedEmoji = "\u2605"; //or ⭐\u2b50
		private const int ScoreMultiplier = 1000;

		//Invert boolean maybe
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

		private static Result GetMonitorIndexResult(string prefix, string iconPath, int index, int maxScore,
			Action<int> action,
			string subTitle = null) => new()
		{
			Title = $"{prefix} on monitor {index}",
			SubTitle = subTitle,
			IcoPath = iconPath,
			Score = maxScore - ScoreMultiplier * index,
			Action = _ =>
			{
				action(index);
				return true;
			}
		};

		public static List<Result> ToResultList<T>(this IEnumerable<T> source, LivelyService livelyService,
			PluginInitContext context,
			string query = null)
			where T : ISearchable
		{
			var validQuery = !string.IsNullOrWhiteSpace(query);
			var results = new List<Result>();
			foreach (T element in source)
			{
				List<int> highlightData = null;

				if (validQuery)
				{
					MatchResult matchResult = context.API.FuzzySearch(query, element.SearchableString);
					highlightData = matchResult.MatchData;
					if (!matchResult.Success)
						continue;
				}

				Result result = element switch
				{
					Command command => For.Command(command, context, highlightData),
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


		public static Result ViewCommandResult(PluginInitContext context)
		{
			var autoCompleteText = $"{context.CurrentPluginMetadata.ActionKeyword} {Constants.Commands.Keyword}";

			return new Result
			{
				Title = "View Lively commands",
				SubTitle = $"Type '{Constants.Commands.Keyword}' or select this result to view commands",
				Score = 100 * ScoreMultiplier,
				IcoPath = Icons.Lively,
				AutoCompleteText = autoCompleteText,
				Action = _ =>
				{
					context.API.ChangeQuery(autoCompleteText);
					return false;
				}
			};
		}

		public static Result LivelyNotRunningResult(PluginInitContext context)
		{
			var autoCompleteText =
				$"{context.CurrentPluginMetadata.ActionKeyword} {Constants.Commands.Keyword}{Constants.Commands.Open}";
			return new Result
			{
				Title = "Lively Wallpaper is not running",
				SubTitle =
					$"The plugin may not function properly. Please '!{Constants.Commands.Open}' Lively Wallpaper",
				Score = 200 * ScoreMultiplier,
				IcoPath = Icons.Warning,
				AutoCompleteText = autoCompleteText,
				Action = _ =>
				{
					context.API.ChangeQuery(autoCompleteText);
					return false;
				}
			};
		}

		public static List<Result> ContextMenu(Result selectedResult, LivelyService livelyService)
		{
			if (selectedResult.ContextData is not Wallpaper wallpaper)
				return null;

			//The readability here is pretty bad
			var results = new List<Result>();
			//Setting Wallpapers
			const string setPrefix = "Set as wallpaper";

			results.Add(new Result
			{
				Title = $"{setPrefix}{AppendAllMonitors(livelyService.IsSingleDisplay)}",
				Score = (livelyService.MonitorCount + 2) * ScoreMultiplier,
				IcoPath = Icons.Set,
				Action = _ =>
				{
					livelyService.Api.SetWallpaper(wallpaper);
					return true;
				}
			});

			if (!livelyService.IsSingleDisplay)
				livelyService.IterateMonitors(index =>
					results.Add(GetMonitorIndexResult(setPrefix,
						Icons.Set,
						index,
						(livelyService.MonitorCount + 1) * ScoreMultiplier,
						i => livelyService.Api.SetWallpaper(wallpaper, i))));

			//Closing wallpapers
			const string closePrefix = "Close wallpaper";
			if (!livelyService.IsActiveWallpaper(wallpaper, out var activeIndexes))
				return results;

			if (livelyService.IsSingleDisplay || activeIndexes.Count > 1)
				results.Add(new Result
				{
					Title = $"{closePrefix}{AppendAllMonitors(activeIndexes.Count <= 1)}",
					Score = (livelyService.MonitorCount + 2) * ScoreMultiplier,
					IcoPath = Icons.Close,
					Action = _ =>
					{
						livelyService.Api.CloseWallpaper();
						return true;
					}
				});

			if (livelyService.IsSingleDisplay)
				return results;

			results.AddRange(activeIndexes.Select(i =>
				GetMonitorIndexResult(
					closePrefix,
					Icons.Close,
					i,
					(livelyService.MonitorCount + 1) * ScoreMultiplier,
					index => livelyService.Api.CloseWallpaper(index))));
			return results;
		}
	}
}