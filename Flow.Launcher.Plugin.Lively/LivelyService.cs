using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly Settings settings;
		private readonly string localWallpapersPath;
		private readonly string webWallpapersPath;
		private readonly CommandCollection commands;
		private readonly Dictionary<Wallpaper, List<int>> wallpapers = new();
		private readonly Dictionary<int, Wallpaper> activeMonitorIndexes = new();

		private bool canLoadData;

		public IReadOnlyList<Command> Commands => commands;
		public IEnumerable<Wallpaper> Wallpapers => wallpapers.Keys;
		public IReadOnlyDictionary<int, Wallpaper> ActiveMonitorIndexes => activeMonitorIndexes;
		public PluginInitContext Context { get; }
		public LivelyCommandApi Api { get; }
		public WallpaperArrangement WallpaperArrangement { get; private set; }
		public int MonitorCount => Screen.AllScreens.Length;

		public LivelyService(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			Context = context;
			localWallpapersPath = Path.Combine(settings.LivelyLibraryFolderPath, Constants.Folders.LocalWallpapers);
			webWallpapersPath = Path.Combine(settings.LivelyLibraryFolderPath, Constants.Folders.WebWallpapers);
			Api = new LivelyCommandApi(settings, this);

			commands = new CommandCollection
			{
				new Command("setwp", "Search and set wallpapers", query => Wallpapers.ToResultList(this, query)),
				new Command("random", "Set a random Wallpaper", _ => Results.For.RandomiseCommand(this)),
				new Command("closewp", "Close a wallpaper", _ => Results.For.CloseCommand(this)),
				new Command("volume", "Set the volume of a wallpaper", query => Results.For.VolumeCommand(this, query)),
				new Command("layout", "Change the wallpaper layout", _ => Results.For.WallpaperArrangements(this)),
				new Command("playback", "Pause or play wallpaper playback", _ => Results.For.PlaybackCommand(this)),
				//new Command("seek", "Set wallpaper playback position", null),
				new Command("open", "Open Lively", null),
				new Command("quit", "Quit Lively", _ => Results.SingleResult("Quit Lively", null, Api.QuitLively, true))
			};
		}

		public async ValueTask Load(CancellationToken token)
		{
			if (!canLoadData)
				return;

			canLoadData = false;

			var currentWallpaperTask = LoadCurrentWallpaper(token);
			ValueTask settingsTask = LoadLivelySettings(token);

			var currentWallpapers = await currentWallpaperTask;
			
			await foreach (var wallpaperTask in LoadWallpapers(token).ToAsyncEnumerable().WithCancellation(token))
			{
				Wallpaper wallpaper = await wallpaperTask;
				List<int> activeIndexes = null;
				for (var i = 0; i < currentWallpapers.Length; i++)
				{
					var ((_, monitorIndex), folderPath) = currentWallpapers[i];
					if (wallpaper.FolderPath != folderPath)
						continue;
					activeIndexes ??= new List<int>();
					activeIndexes.Add(monitorIndex);
					activeMonitorIndexes.Add(monitorIndex, wallpaper);
				}

				activeIndexes?.Sort();

				wallpapers.Add(wallpaper, activeIndexes);
				//Context.API.LogInfo(nameof(LivelyService), "ADDED WALLPAPER TO DICT");
			}

			await settingsTask;
		}

		private async ValueTask LoadLivelySettings(CancellationToken token)
		{
			await using var file = new FileStream(settings.LivelySettingsJsonPath, FileMode.Open, FileAccess.Read);
			var livelySettings =
				await JsonSerializer.DeserializeAsync<LivelySettings>(file, JsonSerializerOptions.Default, token);
			WallpaperArrangement = livelySettings.WallpaperArrangement;
		}

		private async Task<WallpaperLayout[]> LoadCurrentWallpaper(CancellationToken token)
		{
			var path = Path.Combine(Path.GetDirectoryName(settings.LivelySettingsJsonPath),
				Constants.Files.WallpaperLayout);
			await using var file = new FileStream(path, FileMode.Open, FileAccess.Read);

			var wallpaperLayout = await JsonSerializer.DeserializeAsync<WallpaperLayout[]>(file,
				JsonSerializerOptions.Default,
				token);

			return wallpaperLayout;
		}

		private IEnumerable<Task<Wallpaper>> LoadWallpapers(CancellationToken token)
		{
			return Directory.EnumerateDirectories(localWallpapersPath)
				.Concat(Directory.EnumerateDirectories(webWallpapersPath))
				.AsParallel()
				.WithCancellation(token)
				.WithDegreeOfParallelism(8)
				.Select(async wallpaperFolder =>
				{
					//Context.API.LogInfo(nameof(LivelyService), "Creating Model:" + wallpaperFolder);
					var path = Path.Combine(wallpaperFolder, Constants.Files.LivelyInfo);
					await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					var wallpaper =
						await JsonSerializer.DeserializeAsync<Wallpaper>(file, JsonSerializerOptions.Default, token);
					wallpaper.Init(wallpaperFolder);
					//Context.API.LogInfo(nameof(LivelyService), "Finished Model:" + wallpaperFolder);
					return wallpaper;
				});
		}

		public bool IsActiveWallpaper(Wallpaper wallpaper, out IReadOnlyList<int> livelyMonitorIndexes) =>
			(livelyMonitorIndexes = wallpapers[wallpaper]) != null;

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