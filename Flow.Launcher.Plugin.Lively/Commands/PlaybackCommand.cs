using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class PlaybackCommand : CommandBase
	{
		public override string Shortcut { get; } = "playback";
		protected override string Description { get; } = "Pause or resume wallpaper playback";
		protected override string IconPath { get; } = Constants.Icons.Playback;
		private static void Execute(bool playbackState) => Execute($"--play {playbackState}");

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
					Execute(true);
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
					Execute(false);
					return true;
				}
			}
		};
	}
}