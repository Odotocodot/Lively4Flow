using System;
using System.Diagnostics;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyCommandApi
	{
		private readonly LivelyService livelyService;
		private bool refreshUI;

		public LivelyCommandApi(LivelyService livelyService)
		{
			this.livelyService = livelyService;
		}

		public bool UIRefreshRequired()
		{
			var result = refreshUI;
			refreshUI = false;
			return result;
		}

		public void SetWallpaper(Wallpaper wallpaper)
		{
			if (livelyService.IsSingleDisplay)
				InternalSetWallpaper(wallpaper.LivelyFolderPath, null);
			else
				livelyService.IterateMonitors(index => SetWallpaper(wallpaper, index));
		}

		public void SetWallpaper(Wallpaper wallpaper, int monitorIndex) =>
			InternalSetWallpaper(wallpaper.LivelyFolderPath, monitorIndex);

		public void RandomiseWallpaper() => InternalSetWallpaper("random", null);
		public void RandomiseWallpaper(int monitorIndex) => InternalSetWallpaper("random", monitorIndex);

		public void SetVolume(int value) => RunCommand($"--volume {value}", false);

		public void SetWallpaperLayout(WallpaperArrangement arrangement) =>
			RunCommand($"--layout {Enum.GetName(arrangement)!.ToLower()}", true);

		public void WallpaperPlayback(bool playbackState) => RunCommand($"--play {playbackState}", false);
		public void CloseWallpaper(int monitorIndex) => InternalCloseWallpaper(monitorIndex);
		public void CloseWallpaper() => InternalCloseWallpaper(-1);

		public void OpenLively() => RunCommand("--showApp true", false);
		public void QuitLively() => RunCommand("--shutdown true", false);

		private void InternalSetWallpaper(string wallpaperPath, int? monitorIndex)
		{
			var args = $"setwp --file \"{wallpaperPath}\"";
			if (monitorIndex.HasValue)
				args += $" --monitor {monitorIndex.Value}";
			RunCommand(args, true);
		}

		private void InternalCloseWallpaper(int monitorIndex) =>
			RunCommand($"closewp --monitor {monitorIndex}", true);

		/// <param name="uiRefreshRequired">true if the command causes a changed that requires an UI update to be displayed</param>
		private void RunCommand(string args, bool uiRefreshRequired)
		{
			// if (!livelyService.IsLivelyRunning)
			// 	return;
			var psi = new ProcessStartInfo(Constants.CommandUtility, args);
			using Process process = Process.Start(psi);

			if (uiRefreshRequired)
				refreshUI = true;
		}
	}
}