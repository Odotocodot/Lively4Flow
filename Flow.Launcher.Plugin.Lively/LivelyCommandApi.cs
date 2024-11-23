using System.Diagnostics;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyCommandApi
	{
		private readonly Settings settings;

		public LivelyCommandApi(Settings settings)
		{
			this.settings = settings;
		}

		public void SetWallpaper(Wallpaper wp, int monitorIndex) =>
			ShellRun($"setwp --file \"{wp.FolderPath}\" --monitor {monitorIndex}");

		public void RandomiseWallpaper() => InternalRandomiseWallpaper(null);
		public void RandomiseWallpaper(int monitorIndex) => InternalRandomiseWallpaper(monitorIndex);

		public void SetVolume(int value) => ShellRun($"--volume {value}");

		public void SetWallpaperLayout(WallpaperLayout layout) =>
			ShellRun($"--layout {layout.ToString().ToLower()}");

		public void WallpaperPlayback(bool playbackState) => ShellRun($"--play {playbackState}");
		public void CloseWallpaper(int monitorIndex) => ShellRun($"closewp --monitor {monitorIndex}");

		public void OpenLively() => ShellRun("--showApp true");
		public void QuitLively() => ShellRun("--shutdown true");

		private void InternalRandomiseWallpaper(int? monitorIndex)
		{
			var args = "setwp --file random";
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

	public enum WallpaperLayout
	{
		Per,
		Span,
		Duplicate
	}
}