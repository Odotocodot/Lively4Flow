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
		}

		public IEnumerable<Task<Wallpaper>> GetWallpapers(CancellationToken ct)
		{
			var wallpapersQuery = Directory.EnumerateDirectories(localWallpapersPath)
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
				});

			return wallpapersQuery;
			// var wallpapers = await Task.WhenAll(wallpapersQuery);
			// return wallpapers;
		}
	}
}