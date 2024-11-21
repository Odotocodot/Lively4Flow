using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class LivelyService
	{
		private Settings settings;
		private readonly string localWallpapersPath;
		private readonly string webWallpapersPath;
		private readonly PluginInitContext context;
		private List<Wallpaper> wallpapers = new();

		public IReadOnlyList<Wallpaper> Wallpapers => wallpapers;
		public Command[] Commands { get; init; }

		private static class MagicStrings
		{
			public const string LivelyInfo = "LivelyInfo.json";
		}

		public LivelyService(Settings settings, PluginInitContext context)
		{
			this.settings = settings;
			this.context = context;
			localWallpapersPath = Path.Combine(this.settings.LivelyLibraryFolderPath, "wallpapers");
			webWallpapersPath = Path.Combine(this.settings.LivelyLibraryFolderPath, "SaveData", "wptmp");

			//plural of wallpapers depends on the current wallpaper arrangement
			Commands = new[]
			{
				new Command("Set Wallpaper(s)", "setwp", "Search and set wallpapers", null), //
				new Command("Set Random Wallpaper(s)", "random", null, null),
				new Command("Set Wallpaper(s) Volume", "volume", null, null),
				new Command("Set Wallpaper Layout", "layout", null, null),
				new Command("Set Wallpaper(s) Playback", "playback", "Play or pause wallpaper(s)", null),
				new Command("Close Wallpaper(s)", "closewp", null, null),
				new Command("Open Lively", "open", null, null),
				new Command("Quit Lively", "quit", null, null)
			};
		}

		public async Task LoadWallpapers(CancellationToken token)
		{
			var wallpaperTasks = Directory.EnumerateDirectories(localWallpapersPath)
				.Concat(Directory.EnumerateDirectories(webWallpapersPath))
				.AsParallel()
				.WithCancellation(token)
				.WithDegreeOfParallelism(8)
				.Select(async wallpaperFolder =>
				{
					//context.API.LogInfo(nameof(LivelyService), "Creating Model:" + wallpaperFolder);
					var path = Path.Combine(wallpaperFolder, MagicStrings.LivelyInfo);
					await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					var wallpaper =
						await JsonSerializer.DeserializeAsync<Wallpaper>(file, JsonSerializerOptions.Default, token);
					wallpaper.Init(wallpaperFolder);
					//context.API.LogInfo(nameof(LivelyService), "Finished Model:" + wallpaperFolder);
					return wallpaper;
				})
				.ToAsyncEnumerable();
			//.WithCancellation(token);
			//.SelectAwait(async task => await task).ToListAsync(token);

			await foreach (var task in wallpaperTasks)
				wallpapers.Add(await task);
			// var wallpapers = await Task.WhenAll(wallpapersQuery);
			// return wallpapers;

			// wallpapers = new List<(Wallpaper wallpaper, List<int> highlightData)>(wallpaperTasks.Count);
			// foreach (var wallpaperTask in wallpaperTasks)
			// {
			// 	Wallpaper wallpaper = await wallpaperTask;
			// 	wallpapers.Add((wallpaper, context.API.FuzzySearch(query.Search, wallpaper.Title).MatchData));
			// }
			// while (wallpaperTasks.Count > 0)
			// {
			// 	var task = await Task.WhenAny(wallpaperTasks);
			// 	wallpaperTasks.Remove(task);
			//
			// 	Wallpaper wallpaper = await task;
			// 	wallpapers.Add(wallpaper);
			// }
		}

		public void ClearLoadedWallpapers()
		{
			wallpapers.Clear();
		}
	}
}