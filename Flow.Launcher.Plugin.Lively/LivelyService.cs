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
		public CommandResult[] Commands { get; init; }

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
				new CommandResult(context, "setwp", "Search and set wallpapers", null), //
				new CommandResult(context, "random", "Set a random Wallpaper", null),
				new CommandResult(context, "volume", "Set the volume of a wallpaper", null),
				new CommandResult(context, "layout", "Change the wallpaper layout", null),
				new CommandResult(context, "playback", "Pause or play wallpaper playback", null),
				new CommandResult(context, "closewp", "Close a wallpaper", null),
				new CommandResult(context, "open", "Open Lively", null),
				new CommandResult(context, "quit", "Quit Lively",
					(command, _) => new List<Result> { new() { Title = command.Description, Action = _ => true } })
			};
		}

		public void RunCommand(string args) { }

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