using System.Collections.Generic;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class SetWallpaperCommand : CommandBase
	{
		public override string Shortcut { get; } = "setwp";
		protected override string Description { get; } = "Search and set wallpapers";
		protected override string IconPath { get; } = Constants.Icons.Set;

		public static void Execute(LivelyService livelyService, Wallpaper wallpaper = null,
			int? monitorIndex = null)
		{
			var args = $"setwp --file \"{(wallpaper == null ? "random" : wallpaper.LivelyFolderPath)}\"";

			if (monitorIndex.HasValue)
			{
				Execute($"{args} --monitor {monitorIndex.Value}");
				livelyService.UIUpdateSetWallpaper(wallpaper, monitorIndex.Value);
				return;
			}

			if (livelyService.IsSingleDisplay)
			{
				Execute(args);
				livelyService.UIUpdateSetWallpaper(wallpaper);
			}
			else
			{
				livelyService.IterateMonitors(index =>
				{
					Execute($"{args} --monitor {index}");
					livelyService.UIUpdateSetWallpaper(wallpaper, index);
				});
			}
		}

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) =>
			livelyService.Wallpapers.ToResultList(context, livelyService, query);
	}
}