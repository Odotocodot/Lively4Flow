using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class LayoutCommand : CommandBase
	{
		public override string Shortcut { get; } = "layout";
		protected override string Description { get; } = "Change the wallpaper layout";
		protected override string IconPath { get; } = Constants.Icons.Layout;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) =>
			Enum.GetValues<WallpaperArrangement>()
				.Select(arrangement =>
				{
					var name = Enum.GetName(arrangement);
					var title = arrangement == livelyService.WallpaperArrangement
						? $"{Results.SelectedEmoji} {name}"
						: name;

					return new Result
					{
						Title = title,
						ContextData = arrangement,
						Score = Results.ScoreMultiplier * (3 - (int)arrangement),
						IcoPath = Constants.Icons.Layout,
						Action = _ =>
						{
							if (arrangement == livelyService.WallpaperArrangement)
								return false;

							Wallpaper activeWallpaper = livelyService.ActiveMonitorIndexes
								.DefaultIfEmpty()
								.MinBy(x => x.Key)
								.Value;
							Execute($"--layout {Enum.GetName(arrangement)!.ToLower()}");
							//Task.Delay(500).Wait();
							if (activeWallpaper != null)
								SetWallpaperCommand.Execute(livelyService, activeWallpaper);
							return true;
						}
					};
				})
				.ToList();
	}
}