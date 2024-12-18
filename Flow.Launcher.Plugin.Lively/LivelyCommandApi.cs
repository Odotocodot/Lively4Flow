using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyCommandApi
	{
		private readonly LivelyService livelyService;
		private readonly Settings settings;
		private bool refreshUI;

		public LivelyCommandApi(LivelyService livelyService, Settings settings)
		{
			this.livelyService = livelyService;
			this.settings = settings;
		}

		public bool UIRefreshRequired()
		{
			if (!refreshUI)
				return false;
			refreshUI = false;
			return true;
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
		public void QuitLively() => RunCommand("--shutdown true", false, false);

		private void InternalSetWallpaper(string wallpaperPath, int? monitorIndex)
		{
			var args = $"setwp --file \"{wallpaperPath}\"";
			if (monitorIndex.HasValue)
				args += $" --monitor {monitorIndex.Value}";
			RunCommand(args, true);
		}

		private void InternalCloseWallpaper(int monitorIndex) =>
			RunCommand($"closewp --monitor {monitorIndex}", true, false);


		/// <param name="uiRefreshRequired">true if the command causes a changed that requires an UI update to be displayed</param>
		private void RunCommand(string args, bool uiRefreshRequired, bool canOpenLively = true)
		{
			if (!livelyService.IsLivelyRunning)
			{
				if (!canOpenLively)
					return;
				//TODO: Is a command queue needed or a lock?
				//Maybe have a static bool field that will force a result with a progress bar saying Lively is loading?
				StartLively();
			}

			// ProcessStartInfo psi = settings.InstallType switch
			// {
			// 	LivelyInstallType.GitHub => new ProcessStartInfo
			// 	{
			// 		FileName = settings.LivelyExePath,
			// 		Arguments = args,
			// 		CreateNoWindow = true
			// 	},
			// 	LivelyInstallType.MicrosoftStore => new ProcessStartInfo
			// 	{
			// 		UseShellExecute = true,
			// 		FileName = $"shell:AppsFolder\\{Constants.Lively.AppId}!App",
			// 		Arguments = args
			// 	},
			// 	LivelyInstallType.None => throw new InvalidOperationException("Lively Wallpaper is not installed!"),
			// 	_ => throw new InvalidCastException("Invalid InstallType")
			// };

			var psi = new ProcessStartInfo(Constants.CommandUtility, args);
			using Process process = Process.Start(psi);

			if (uiRefreshRequired)
				refreshUI = true;
		}

		private void StartLively()
		{
			ProcessStartInfo livelyProcessStartInfo = settings.InstallType switch
			{
				LivelyInstallType.GitHub => new ProcessStartInfo(settings.LivelyExePath),
				LivelyInstallType.MicrosoftStore => new ProcessStartInfo("explorer.exe",
					$"shell:AppsFolder\\{Constants.Lively.AppId}!App"),
				LivelyInstallType.None => throw new InvalidOperationException("Lively Wallpaper is not installed!"),
				_ => throw new InvalidCastException("Invalid InstallType")
			};
			livelyProcessStartInfo.CreateNoWindow = true;
			using Process process = Process.Start(livelyProcessStartInfo);
			process?.WaitForInputIdle(5000);
			Task.Delay(1000);
		}
	}
}