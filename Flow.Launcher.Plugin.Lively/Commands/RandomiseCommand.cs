using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class RandomiseCommand : CommandBase
	{
		public override string Shortcut { get; } = "random";
		protected override string Description { get; } = "Set a random wallpaper";
		protected override string IconPath { get; } = Constants.Icons.Random;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null)
		{
			var results = new List<Result>();
			const string prefix = "Set a random wallpaper";
			results.Add(new Result
			{
				Title = $"{prefix}{ResultsHelper.AppendAllMonitors(livelyService.IsSingleDisplay)}",
				Score = (livelyService.MonitorCount + 2) * ResultsHelper.ScoreMultiplier,
				IcoPath = Constants.Icons.Random,
				Action = _ =>
				{
					SetWallpaperCommand.Execute(livelyService, "random");
					return true;
				}
			});
			if (livelyService.IsSingleDisplay)
				return results;

			livelyService.IterateMonitors(index =>
				results.Add(ResultsHelper.GetMonitorIndexResult(prefix,
					Constants.Icons.Random,
					index,
					(livelyService.MonitorCount + 1) * ResultsHelper.ScoreMultiplier,
					i => SetWallpaperCommand.Execute(livelyService, "random", i))));

			return results;
		}
	}
}