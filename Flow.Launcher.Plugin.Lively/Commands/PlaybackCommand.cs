using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class PlaybackCommand : CommandBase
	{
		public override string Shortcut { get; } = "playback";
		protected override string Description { get; } = "Pause or resume wallpaper playback";
		protected override string IconPath { get; } = Constants.Icons.Playback;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) => new()
		{
			new Result
			{
				Title = "Resume wallpaper playback",
				Score = 2000,
				IcoPath = Constants.Icons.Playback,
				Action = _ =>
				{
					livelyService.Api.WallpaperPlayback(true);
					return true;
				}
			},
			new Result
			{
				Title = "Pause wallpaper playback",
				Score = 0,
				IcoPath = Constants.Icons.Playback,
				Action = _ =>
				{
					livelyService.Api.WallpaperPlayback(false);
					return true;
				}
			}
		};
	}
}