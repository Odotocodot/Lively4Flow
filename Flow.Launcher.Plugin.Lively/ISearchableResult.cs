using System.Collections.Generic;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively
{
	public interface ISearchableResult
	{
		string SearchableString { get; }
		Result ToResult(LivelyService livelyService, List<int> highlightData = null);
	}

	public static class SearchableResultExtensions //TODO maybe move to ResultCreator.cs and make it static?
	{
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