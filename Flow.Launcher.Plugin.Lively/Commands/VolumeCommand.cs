using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class VolumeCommand : CommandBase
	{
		public override string Shortcut { get; } = "volume";
		protected override string Description { get; } = "Set wallpaper volume";
		protected override string IconPath { get; } = Constants.Icons.Volume;

		private static void Execute(int volume) => Execute($"--volume {volume}");

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null)
		{
			int? volume = null;
			string titleSuffix = null;
			if (int.TryParse(query, out var vol))
			{
				volume = Math.Clamp(vol, 0, 100);
				titleSuffix = $" to {volume}";
			}

			return new List<Result>
			{
				new()
				{
					Title = $"Set wallpaper volume{titleSuffix}",
					SubTitle = "Type a value between 0 and 100 to specify",
					Score = 3 * ResultsHelper.ScoreMultiplier,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						if (volume.HasValue)
							Execute(volume.Value);
						return volume.HasValue;
					}
				},
				new()
				{
					Title = "Set wallpaper volume to 0 (Mute)",
					Score = 2 * ResultsHelper.ScoreMultiplier,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						Execute(0);
						return true;
					}
				},
				new()
				{
					Title = "Set wallpaper volume to 50",
					Score = 1 * ResultsHelper.ScoreMultiplier,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						Execute(50);
						return true;
					}
				},
				new()
				{
					Title = "Set wallpaper volume to 100 (Max)",
					Score = 0,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						Execute(100);
						return true;
					}
				}
			};
		}
	}
}