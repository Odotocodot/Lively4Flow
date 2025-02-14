using System.Collections.Generic;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class SetWallpaperCommand : CommandBase
	{
		public override string Shortcut { get; } = "setwp";
		protected override string Description { get; } = "Search and set wallpapers";
		protected override string IconPath { get; } = Constants.Icons.Set;

		public static void Execute(LivelyService livelyService, Wallpaper wallpaper, int? monitorIndex = null)
		{
			Execute(livelyService, wallpaper.LivelyFolderPath, monitorIndex);
		}

		public static void Execute(LivelyService livelyService, string wallpaperPath, int? monitorIndex = null)
		{
			var args = $"setwp --file \"{wallpaperPath}\"";
			if (!livelyService.IsSingleDisplay && monitorIndex.HasValue)
				args += $" --monitor {monitorIndex.Value}";

			Execute(args);
		}

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) =>
			livelyService.Wallpapers.ToResultList(context, livelyService, query);
	}
}