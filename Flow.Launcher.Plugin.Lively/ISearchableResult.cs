using System.Collections.Generic;
using Flow.Launcher.Plugin.SharedModels;

namespace Flow.Launcher.Plugin.Lively
{
	public interface ISearchableResult
	{
		string SearchableString { get; }
		Result ToResult(PluginInitContext context, List<int> highlightData = null);
	}

	public static class SearchableResultExtensions
	{
		public static List<Result> FilterToResultsList<T>(this IReadOnlyList<T> source, PluginInitContext context,
			string query)
			where T : ISearchableResult
		{
			var results = new List<Result>();
			for (var i = 0; i < source.Count; i++)
			{
				T element = source[i];
				MatchResult matchResult = context.API.FuzzySearch(query, element.SearchableString);
				if (matchResult.Success)
					results.Add(element.ToResult(context));
			}

			return results;
		}

		public static List<Result> ToResultsList<T>(this IReadOnlyList<T> source, PluginInitContext context)
			where T : ISearchableResult
		{
			var results = new List<Result>();
			for (var i = 0; i < source.Count; i++)
				results.Add(source[i].ToResult(context));
			return results;
		}
	}
}