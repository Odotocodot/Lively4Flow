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

		public LivelyCommandApi(LivelyService livelyService, Settings settings)
		{
			this.livelyService = livelyService;
			this.settings = settings;
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
		public void QuitLively() => RunCommand("--shutdown true", false);

		private void InternalSetWallpaper(string wallpaperPath, int? monitorIndex)
		{
			var args = $"setwp --file \"{wallpaperPath}\"";
			if (monitorIndex.HasValue)
				args += $" --monitor {monitorIndex.Value}";
			RunCommand(args);
		}

		private void InternalCloseWallpaper(int monitorIndex) => RunCommand($"closewp --monitor {monitorIndex}", false);

		private void RunCommand(string args, bool autoOpenLively = true)
		{
			if (!livelyService.IsLivelyRunning && autoOpenLively) 
			{
				//Is a command queue needed or a lock?
				//Maybe have a static bool field that will force a result with a progress bar saying Lively is loading?
				ProcessStartInfo livelyProcessStartInfo = settings.InstallType switch
				{
					Setup.InstallType.GitHub => new ProcessStartInfo(settings.LivelyExePath),
					Setup.InstallType
						.MicrosoftStore => throw new NotImplementedException(), // new ProcessStartInfo("explorer.exe",)
					Setup.InstallType.None => throw new InvalidOperationException("Lively Wallpaper is not installed!")
				};
				livelyProcessStartInfo.CreateNoWindow = true;

				using Process process = Process.Start(livelyProcessStartInfo);
				process?.WaitForInputIdle();
				Task.Delay(1000);
			}

			Process.Start(new ProcessStartInfo
			{
				FileName = Constants.CommandUtility,
				Arguments = args,
				CreateNoWindow = true
			});
		}
	}
}