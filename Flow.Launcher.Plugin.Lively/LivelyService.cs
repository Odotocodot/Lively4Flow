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
		private List<Wallpaper> wallpapers = new List<Wallpaper>();

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

			Commands = new[]
			{
				new Command("Set Wallpaper(s)", "set", "Search and set wallpapers", null)
			};

		}

		public async Task LoadWallpapers(CancellationToken ct)
		{
			var wallpaperTasks = Directory.EnumerateDirectories(localWallpapersPath)
				.Concat(Directory.EnumerateDirectories(webWallpapersPath))
				.AsParallel()
				.WithCancellation(ct)
				.WithDegreeOfParallelism(8)
				.Select(async wallpaperFolder =>
				{
					//context.API.LogInfo(nameof(LivelyService), "Creating Model:" + wallpaperFolder);
					var path = Path.Combine(wallpaperFolder, MagicStrings.LivelyInfo);
					await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					var wallpaper =
						await JsonSerializer.DeserializeAsync<Wallpaper>(file, JsonSerializerOptions.Default, ct);
					wallpaper.Init(wallpaperFolder);
					//context.API.LogInfo(nameof(LivelyService), "Finished Model:" + wallpaperFolder);
					return wallpaper;
				})
				.ToAsyncEnumerable();
				//.WithCancellation(token);
			//.SelectAwait(async task => await task).ToListAsync(token);
				
			await foreach(var task in wallpaperTasks)
			{
				wallpapers.Add(await task);
			}
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