using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public interface ISearchableResult
	{
		string SearchableString { get; }
		Result ToResult(PluginInitContext context, LivelyService livelyService, List<int> highlightData = null);
	}
}