using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class SetWallpaperCommand : CommandBase
	{
		public override string Shortcut { get; } = "setwp";
		protected override string Description { get; } = "Search and set wallpapers";
		protected override string IconPath { get; } = Constants.Icons.Set;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) =>
			livelyService.Wallpapers.ToResultList(context, livelyService, query);
	}
}