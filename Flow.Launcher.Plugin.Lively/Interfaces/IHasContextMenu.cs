using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public interface IHasContextMenu
	{
		List<Result> ToContextMenu(LivelyService livelyService);
	}
}