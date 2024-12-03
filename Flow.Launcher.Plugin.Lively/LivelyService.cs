using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly string localWallpapersPath;
		private readonly string webWallpapersPath;
		private readonly CommandCollection commands;
		private readonly ConcurrentDictionary<Wallpaper, List<int>> wallpapers = new();
		private readonly ConcurrentDictionary<int, Wallpaper> activeMonitorIndexes = new();

		private bool canLoadData;
		public bool IsLivelyRunning { get; private set; }

		public IReadOnlyList<Command> Commands => commands;
		public IEnumerable<Wallpaper> Wallpapers => wallpapers.Keys;
		public IReadOnlyDictionary<int, Wallpaper> ActiveMonitorIndexes => activeMonitorIndexes;
		public LivelyCommandApi Api { get; }
		public WallpaperArrangement WallpaperArrangement { get; private set; }
		public int MonitorCount { get; private set; }

		public LivelyService(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			this.context = context;
			localWallpapersPath = Path.Combine(settings.LivelyLibraryFolderPath, Constants.Folders.LocalWallpapers);
			webWallpapersPath = Path.Combine(settings.LivelyLibraryFolderPath, Constants.Folders.WebWallpapers);
			Api = new LivelyCommandApi(this, settings);

			commands = new CommandCollection
			{
				new Command("setwp", "Search and set wallpapers", Constants.Icons.Set,
					q => Wallpapers.ToResultList(this, context, q)),
				new Command("random", "Set a random Wallpaper", Constants.Icons.Random,
					_ => Results.For.RandomiseCommand(this)),
				new Command("closewp", "Close a wallpaper", Constants.Icons.Close,
					_ => Results.For.CloseCommand(this)),
				new Command("volume", "Set the volume of a wallpaper", Constants.Icons.Volume,
					q => Results.For.VolumeCommand(this, q)),
				new Command("layout", "Change the wallpaper layout", Constants.Icons.Layout,
					_ => Results.For.WallpaperArrangements(this)),
				new Command("playback", "Pause or play wallpaper playback", Constants.Icons.Playback,
					_ => Results.For.PlaybackCommand(this)),
				//new Command("seek", "Set wallpaper playback position", null),
				new Command("open", "Open or Show Lively", Constants.Icons.Open, Api.OpenLively),
				new Command("quit", "Quit Lively", Constants.Icons.Quit, Api.QuitLively)
			};
		}

		public async ValueTask Load(CancellationToken token)
		{
			IsLivelyRunning = Process.GetProcessesByName("Lively")
				.FirstOrDefault(p => p.MainModule?.FileName.StartsWith(settings.LivelyExePath) == true) != null;

			MonitorCount = Screen.AllScreens.Length;

			if (!canLoadData || token.IsCancellationRequested)
				return;

			canLoadData = false;

			var currentWallpaperTask = LoadCurrentWallpaper(token);
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
				context.API.LogException("LivelyWallpaperController." + nameof(LivelyService),
					$"Failed loading wallpaper at: \"{wallpaperFolder}\"",
					e);
				return;
			}

			List<int> activeIndexes = null;
			for (var i = 0; i < currentWallpapers.Length; i++)
			{
				var ((_, monitorIndex), folderPath) = currentWallpapers[i];
				if (wallpaper.FolderPath != folderPath)
					continue;
				activeIndexes ??= new List<int>();
				activeIndexes.Add(monitorIndex);
				activeMonitorIndexes.TryAdd(monitorIndex, wallpaper);
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
		}

		private async ValueTask<WallpaperLayout[]> LoadCurrentWallpaper(CancellationToken token)
		{
			var path = Path.Combine(Path.GetDirectoryName(settings.LivelySettingsJsonPath),
				Constants.Files.WallpaperLayout);
			await using var file = new FileStream(path, FileMode.Open, FileAccess.Read);

			var wallpaperLayout = await JsonSerializer.DeserializeAsync<WallpaperLayout[]>(file,
				JsonSerializerOptions.Default,
				token);

			return wallpaperLayout;
		}

		private ParallelQuery<string> LoadWallpaperFolders(int degreeOfParallelism, CancellationToken token) =>
			Directory.EnumerateDirectories(localWallpapersPath)
				.Concat(Directory.EnumerateDirectories(webWallpapersPath))
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
			wallpaper.Init(wallpaperFolder);
			//Context.API.LogInfo(nameof(LivelyService), "Finished Model:" + wallpaperFolder);
			return wallpaper;
		}

		public bool IsActiveWallpaper(Wallpaper wallpaper, out List<int> livelyMonitorIndexes) =>
			!wallpapers.TryGetValue(wallpaper, out livelyMonitorIndexes);

		public bool TryGetCommand(string query, out Command command) => commands.TryGetValue(query, out command);

		//Lively monitor indexes are not zero indexed, hence this method for ease
		public void IterateMonitors(Action<int> action)
		{
			for (var i = 1; i < MonitorCount + 1; i++)
				action(i);
		}

		public void EnableLoading()
		{
			canLoadData = true;
		}

		public void DisableLoading()
		{
			canLoadData = false;
			wallpapers.Clear();
			activeMonitorIndexes.Clear();
		}

		private class CommandCollection : KeyedCollection<string, Command>
		{
			protected override string GetKeyForItem(Command item) => item.Shortcut;
		}
	}
}