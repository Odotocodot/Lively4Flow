using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class CloseWallpaperCommand : CommandBase
	{
		public override string Shortcut { get; } = "closewp";
		protected override string Description { get; } = "Close a wallpaper";
		protected override string IconPath { get; } = Constants.Icons.Volume;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null)
		{
			var activeMonitors = livelyService.ActiveMonitorIndexes;
			if (!activeMonitors.Any())
				return Results.SingleResult("No active Lively wallpapers", Constants.Icons.Close, null, false);

			var results = new List<Result>();
			const string prefix = "Close active wallpaper";

			if (livelyService.IsSingleDisplay || activeMonitors.Count > 1)
				results.Add(new Result
				{
					Title = $"{prefix}{Results.AppendAllMonitors(activeMonitors.Count <= 1)}",
					SubTitle = activeMonitors.Count <= 1
						? $"Current Wallpaper: {activeMonitors.First().Value.Title}"
						: null,
					Score = (livelyService.MonitorCount + 2) * Results.ScoreMultiplier,
					IcoPath = Constants.Icons.Close,
					Action = _ =>
					{
						livelyService.Api.CloseWallpaper();
						return true;
					}
				});

			if (livelyService.IsSingleDisplay)
				return results;

			results.AddRange(activeMonitors.Select(activeMonitor =>
				Results.GetMonitorIndexResult(prefix,
					Constants.Icons.Close,
					activeMonitor.Key,
					(livelyService.MonitorCount + 1) * Results.ScoreMultiplier,
					index => livelyService.Api.CloseWallpaper(index),
					$"Current Wallpaper: {activeMonitor.Value.Title}")));

			return results;
		}
	}
}