using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class VolumeCommand : CommandBase
	{
		public override string Shortcut { get; } = "volume";
		protected override string Description { get; } = "Set wallpaper volume";
		protected override string IconPath { get; } = Constants.Icons.Volume;

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
					Score = 3 * Results.ScoreMultiplier,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						if (volume.HasValue)
							livelyService.Api.SetVolume(volume.Value);
						return volume.HasValue;
					}
				},
				new()
				{
					Title = "Set wallpaper volume to 0 (Mute)",
					Score = 2 * Results.ScoreMultiplier,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						livelyService.Api.SetVolume(0);
						return true;
					}
				},
				new()
				{
					Title = "Set wallpaper volume to 50",
					Score = 1 * Results.ScoreMultiplier,
					IcoPath = Constants.Icons.Volume,
					Action = _ =>
					{
						livelyService.Api.SetVolume(50);
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
						livelyService.Api.SetVolume(100);
						return true;
					}
				}
			};
		}
	}
}