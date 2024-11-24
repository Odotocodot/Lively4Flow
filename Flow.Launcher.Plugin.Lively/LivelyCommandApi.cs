using System;
using System.Diagnostics;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyCommandApi
	{
		private readonly Settings settings;
		private readonly LivelyService livelyService;

		public LivelyCommandApi(Settings settings, LivelyService livelyService)
		{
			this.settings = settings;
			this.livelyService = livelyService;
		}

		public void SetWallpaper(Wallpaper wallpaper)
		{
			switch (livelyService.WallpaperArrangement)
			{
				case WallpaperArrangement.Per:
					for (var i = 0; i < livelyService.MonitorCount; i++)
						SetWallpaper(wallpaper, i);
					break;
				case WallpaperArrangement.Span:
				case WallpaperArrangement.Duplicate:
					InternalSetWallpaper(wallpaper.FolderPath, null);
					break;
			}
		}

		public void SetWallpaper(Wallpaper wallpaper, int monitorIndex) =>
			InternalSetWallpaper(wallpaper.FolderPath, monitorIndex);

		public void RandomiseWallpaper() => InternalSetWallpaper("random", null);
		public void RandomiseWallpaper(int monitorIndex) => InternalSetWallpaper("random", monitorIndex);

		public void SetVolume(int value) => ShellRun($"--volume {value}");

		public void SetWallpaperLayout(WallpaperArrangement arrangement) =>
			ShellRun($"--layout {Enum.GetName(arrangement)!.ToLower()}");

		public void WallpaperPlayback(bool playbackState) => ShellRun($"--play {playbackState}");
		public void CloseWallpaper(int monitorIndex) => ShellRun($"closewp --monitor {monitorIndex}");

		public void OpenLively() => ShellRun("--showApp true");
		public void QuitLively() => ShellRun("--shutdown true");

		private void InternalSetWallpaper(string wallpaperPath, int? monitorIndex)
		{
			var args = $"setwp --file \"{wallpaperPath}\"";
			if (monitorIndex.HasValue)
				args += $"--monitor {monitorIndex.Value}";
			ShellRun(args);
		}

		private void ShellRun(string args)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = settings.LivelyExePath,
				Arguments = args,
				CreateNoWindow = true
			});
		}
	}
}