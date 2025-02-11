using System.Collections.Generic;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class SetWallpaperCommand : CommandBase
	{
		public override string Shortcut { get; } = "setwp";
		protected override string Description { get; } = "Search and set wallpapers";
		protected override string IconPath { get; } = Constants.Icons.Set;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null)
		{
			var validQuery = !string.IsNullOrWhiteSpace(query);
			var results = new List<Result>();
			foreach (ISearchableResult element in livelyService.Wallpapers)
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
	}
}