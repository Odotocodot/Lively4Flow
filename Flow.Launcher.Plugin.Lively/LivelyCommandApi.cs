using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyCommandApi
	{
		private readonly LivelyService livelyService;

		public LivelyCommandApi(LivelyService livelyService)
		{
			this.livelyService = livelyService;
		}

		public void SetWallpaper(Wallpaper wallpaper)
		{
			if (livelyService.WallpaperArrangement == WallpaperArrangement.Per)
				livelyService.IterateMonitors(index => SetWallpaper(wallpaper, index));
			else
				InternalSetWallpaper(wallpaper.FolderPath, null);
		}

		public void SetWallpaper(Wallpaper wallpaper, int monitorIndex) =>
			InternalSetWallpaper(wallpaper.FolderPath, monitorIndex);

		public void RandomiseWallpaper() => InternalSetWallpaper("random", null);
		public void RandomiseWallpaper(int monitorIndex) => InternalSetWallpaper("random", monitorIndex);

		public void SetVolume(int value) => RunCommand($"--volume {value}");

		public void SetWallpaperLayout(WallpaperArrangement arrangement) =>
			RunCommand($"--layout {Enum.GetName(arrangement)!.ToLower()}");

		public void WallpaperPlayback(bool playbackState) => RunCommand($"--play {playbackState}");
		public void CloseWallpaper(int monitorIndex) => InternalCloseWallpaper(monitorIndex);
		public void CloseWallpaper() => InternalCloseWallpaper(-1);

		public void OpenLively() => RunCommand("--showApp true");
		public void QuitLively() => RunCommand("--shutdown true");

		private void InternalSetWallpaper(string wallpaperPath, int? monitorIndex)
		{
			var args = $"setwp --file \"{wallpaperPath}\"";
			if (monitorIndex.HasValue)
				args += $" --monitor {monitorIndex.Value}";
			RunCommand(args);
		}

		private void InternalCloseWallpaper(int monitorIndex) => RunCommand($"closewp --monitor {monitorIndex}");

		public async Task<bool> IsLivelyRunning(CancellationToken token)
		{
			Process process = Process.Start(new ProcessStartInfo
			{
				FileName = Constants.CommandUtility,
				CreateNoWindow = true,
				RedirectStandardOutput = true
			});
			return (await process!.StandardOutput.ReadToEndAsync(token)).Length <= 10;
		}

		private void RunCommand(string args)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = Constants.CommandUtility,
				Arguments = args,
				CreateNoWindow = true
			});
		}
	}
}