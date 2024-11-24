using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public interface ISearchableResult
	{
		string SearchableString { get; }
		Result ToResult(LivelyService livelyService, List<int> highlightData = null);
	}
}