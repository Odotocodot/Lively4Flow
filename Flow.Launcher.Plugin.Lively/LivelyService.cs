using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.Lively.Models;
using Screen = System.Windows.Forms.Screen;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyService
	{
		private readonly PluginInitContext context;
		private readonly Settings settings;

		/// <summary>
		/// Retrieved from Lively settings. This can be "incorrect" when using the MS store version. See <see cref="Wallpaper.LivelyFolderPath"/>
		/// </summary>
		private string livelyWallpaperFolder;

		private string localWallpapersFolder;
		private string webWallpapersFolder;
		private readonly ConcurrentDictionary<Wallpaper, List<int>> wallpapers = new();
		private readonly ConcurrentDictionary<int, Wallpaper> activeMonitorIndexes = new();

		private bool canLoadData;
		public bool IsLivelyRunning { get; private set; }
		public IEnumerable<Wallpaper> Wallpapers => wallpapers.Keys;
		public IReadOnlyDictionary<int, Wallpaper> ActiveMonitorIndexes => activeMonitorIndexes;
		public LivelyCommandApi Api { get; }
		public WallpaperArrangement WallpaperArrangement { get; private set; }
		public int MonitorCount { get; private set; }

		/// <returns><see langword="true"/> if only there is only one place to display a wallpaper</returns>
		public bool IsSingleDisplay => MonitorCount == 1 || WallpaperArrangement != WallpaperArrangement.Per;

		public LivelyService(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			this.context = context;
			Api = new LivelyCommandApi(this);
		}

		public async ValueTask Load(CancellationToken token)
		{
			IsLivelyRunning = Process.GetProcessesByName("Lively")
				.Any(p => p.MainModule?.FileName.EndsWith("Lively.exe") == true);

			MonitorCount = Screen.AllScreens.Length;

			if (!canLoadData || token.IsCancellationRequested)
				return;

			canLoadData = false;

			var currentWallpaperTask = LoadActiveWallpapers(token);
			ValueTask settingsTask = LoadLivelySettings(token);

			var parallelOptions = new ParallelOptions
			{
				MaxDegreeOfParallelism = 8,
				CancellationToken = token
			};
			await settingsTask;
			await Parallel.ForEachAsync(
				LoadWallpaperFolders(parallelOptions.MaxDegreeOfParallelism, token),
				parallelOptions,
				async (wallpaperFolder, t) => await LoadAllWallpapers(wallpaperFolder, await currentWallpaperTask, t));
		}

		private async ValueTask LoadAllWallpapers(string wallpaperFolder, WallpaperLayout[] currentWallpapers,
			CancellationToken token)
		{
			if (token.IsCancellationRequested)
				return;

			Wallpaper wallpaper;
			try
			{
				wallpaper = await LoadWallpaper(wallpaperFolder, token);
			}
			catch (Exception e) when (e is FileNotFoundException or JsonException)
			{
				context.API.LogException($"{Constants.PluginName}.{nameof(LivelyService)}",
					$"Failed loading wallpaper at: \"{wallpaperFolder}\"",
					e);
				return;
			}

			List<int> activeIndexes = null;
			for (var i = 0; i < currentWallpapers.Length; i++)
			{
				WallpaperLayout currentWallpaper = currentWallpapers[i];
				if (wallpaper.LivelyFolderPath != currentWallpaper.LivelyInfoPath)
					continue;
				activeIndexes ??= new List<int>();
				activeIndexes.Add(currentWallpaper.LivelyScreen.Index);
				activeMonitorIndexes.TryAdd(currentWallpaper.LivelyScreen.Index, wallpaper);
			}

			activeIndexes?.Sort();
			wallpapers.TryAdd(wallpaper, activeIndexes);
			//Context.API.LogInfo(nameof(LivelyService), "ADDED WALLPAPER TO DICT - " + wallpaperFolder);
		}

		private async ValueTask LoadLivelySettings(CancellationToken token)
		{
			await using var file = new FileStream(settings.LivelySettingsJsonPath, FileMode.Open, FileAccess.Read);
			var livelySettings =
				await JsonSerializer.DeserializeAsync<LivelySettings>(file, JsonSerializerOptions.Default, token);
			WallpaperArrangement = livelySettings.WallpaperArrangement;
			livelyWallpaperFolder = livelySettings.WallpaperDir;

			if (settings.InstallType == LivelyInstallType.MicrosoftStore &&
			    livelyWallpaperFolder == Constants.Folders.DefaultWallpaperFolder)
			{
				localWallpapersFolder = Path.Combine(Constants.Folders.DefaultWallpaperFolderMSStore,
					Constants.Folders.LocalWallpapers);
				webWallpapersFolder = Path.Combine(Constants.Folders.DefaultWallpaperFolderMSStore,
					Constants.Folders.WebWallpapers);
			}
			else
			{
				localWallpapersFolder = Path.Combine(livelyWallpaperFolder, Constants.Folders.LocalWallpapers);
				webWallpapersFolder = Path.Combine(livelyWallpaperFolder, Constants.Folders.WebWallpapers);
			}
		}

		private async ValueTask<WallpaperLayout[]> LoadActiveWallpapers(CancellationToken token)
		{
			var path = Path.Combine(Path.GetDirectoryName(settings.LivelySettingsJsonPath) ?? string.Empty,
				Constants.Files.WallpaperLayout);

			if (!File.Exists(path))
			{
				context.API.LogWarn($"{Constants.PluginName}.{nameof(LivelyService)}",
					$"Could not find {Constants.Files.WallpaperLayout} file.");
				return Array.Empty<WallpaperLayout>();
			}

			await using var file = new FileStream(path, FileMode.Open, FileAccess.Read);

			var wallpaperLayout = await JsonSerializer.DeserializeAsync<WallpaperLayout[]>(file,
				JsonSerializerOptions.Default,
				token);

			return wallpaperLayout;
		}

		private ParallelQuery<string> LoadWallpaperFolders(int degreeOfParallelism, CancellationToken token) =>
			Directory.EnumerateDirectories(localWallpapersFolder)
				.Concat(Directory.EnumerateDirectories(webWallpapersFolder))
				.AsParallel()
				.WithCancellation(token)
				.WithDegreeOfParallelism(degreeOfParallelism);

		private async ValueTask<Wallpaper> LoadWallpaper(string wallpaperFolder, CancellationToken token)
		{
			//Context.API.LogInfo(nameof(LivelyService), "Creating Model:" + wallpaperFolder);
			var path = Path.Combine(wallpaperFolder, Constants.Files.LivelyInfo);
			await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			var wallpaper =
				await JsonSerializer.DeserializeAsync<Wallpaper>(file, JsonSerializerOptions.Default, token);
			wallpaper.Init(wallpaperFolder, livelyWallpaperFolder);
			//Context.API.LogInfo(nameof(LivelyService), "Finished Model:" + wallpaperFolder);
			return wallpaper;
		}

		public bool IsActiveWallpaper(Wallpaper wallpaper, out List<int> livelyMonitorIndexes)
		{
			livelyMonitorIndexes = wallpapers[wallpaper];
			return livelyMonitorIndexes?.Any() == true;
		}

		//Lively monitor indexes are not zero indexed, hence this method for ease
		public void IterateMonitors(Action<int> action)
		{
			for (var i = 1; i < MonitorCount + 1; i++)
				action(i);
		}

		public void EnableLoading() => canLoadData = true;

		public void DisableLoading()
		{
			canLoadData = false;
			wallpapers.Clear();
			activeMonitorIndexes.Clear();
		}
	}
}