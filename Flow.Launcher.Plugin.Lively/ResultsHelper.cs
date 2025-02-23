using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively
{
	using Icons = Constants.Icons;

	public static class ResultsHelper
	{
		public const string SelectedEmoji = "\u2605"; //or ⭐\u2b50
		public static int ScoreMultiplier { get; private set; } = 1000;

		public static void SetupScoreMultiplier(PluginInitContext context)
		{
			ScoreMultiplier = context.CurrentPluginMetadata.ActionKeyword != "*" ? 1000 : 1;
		}

		//Invert boolean maybe
		public static string AppendAllMonitors(bool singleMonitor) => singleMonitor ? "" : " on all monitors";

		public static string OffsetTitle(string title, string offset, List<int> highlightData)
		{
			title = title.Insert(0, offset);

			if (highlightData == null)
				return title;

			for (var i = 0; i < highlightData.Count; i++)
				highlightData[i] += offset.Length;
			return title;
		}

		public static Result GetMonitorIndexResult(string prefix, string iconPath, int index, int maxScore,
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


		public static List<Result> ToResultList<T>(this IEnumerable<T> source,
			PluginInitContext context,
			LivelyService livelyService,
			string query = null)
			where T : ISearchableResult
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

				results.Add(element.ToResult(context, livelyService, highlightData));
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

		public static Result LivelyNotRunningResult() => new()
		{
			Title = "Lively Wallpaper is not running",
			SubTitle = "Open Lively to use this plugin with full functionality",
			Score = 200 * ScoreMultiplier,
			Glyph = Icons.Warning
		};

		public static List<Result> SettingsErrors(Settings settings) =>
			settings.Errors.Select(e => new Result
				{
					Title = e,
					Glyph = Icons.Error
				})
				.ToList();

		public static string AutoCompleteText(PluginInitContext context, string suffix) =>
			context.CurrentPluginMetadata.ActionKeyword == "*"
				? suffix
				: context.CurrentPluginMetadata.ActionKeyword + " " + suffix;
	}
}