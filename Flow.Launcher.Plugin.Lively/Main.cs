using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class Main : IAsyncPlugin, ISettingProvider, IDisposable, IResultUpdated
	{
		private PluginInitContext context;
		private LivelyService livelyService;
		private ResultCreator resultCreator;
		private IconProvider iconProvider;
		private Settings settings;

		private bool canLoadWallpapers;

		//private List<Result> results = new();
		private List<Task<Wallpaper>> wallpaperTasks;

		private List<Wallpaper> wallpapers = new();
		//private List<(Wallpaper wallpaper, List<int> highlightData)> wallpapers;

		public async Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			SettingsHelper.Setup(settings, this.context);
			livelyService = new LivelyService(settings, context);
			resultCreator = new ResultCreator();
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
		{
			if (context.CurrentPluginMetadata.Disabled)
				return;

			if (args.IsVisible)
			{
				canLoadWallpapers = true;
			}
			else
			{
				canLoadWallpapers = false;
				wallpapers.Clear();
			}
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			// if (string.IsNullOrWhiteSpace(query.Search))
			// 	return resultCreator.Empty();

			if (canLoadWallpapers)
			{
				canLoadWallpapers = false;
				var wallpaperTasks = await livelyService.GetWallpapers(token)
					.ToAsyncEnumerable()
					.SelectAwait(async task => await task).ToListAsync(token);
				
				// while (wallpaperTasks.Count > 0)
				// {
				// 	var task = await Task.WhenAny(wallpaperTasks);
				// 	wallpaperTasks.Remove(task);
				//
				// 	Wallpaper wallpaper = await task;
				// 	wallpapers.Add(wallpaper);
				// }
			}

			if (string.IsNullOrWhiteSpace(query.Search))
				return wallpapers.Select(wp => resultCreator.FromWallpaper(wp)).ToList();
			else
				return wallpapers.Where(wp => context.API.FuzzySearch(query.Search, wp.Title).Success)
					.Select(wp => resultCreator.FromWallpaper(wp))
					.ToList();
			// if (canLoadWallpapers)
			// {
			// 	canLoadWallpapers = false;
			//
			// 	var tasks = livelyService.GetWallpapers(token);
			// 	await foreach (var task in tasks.ToAsyncEnumerable().WithCancellation(token))
			// 	{
			// 		Wallpaper wallpaper = await task;
			// 		Result result = resultCreator.FromWallpaper(wallpaper);
			// 		results.Add(result);
			// 		ResultsUpdated?.Invoke(this, new ResultUpdatedEventArgs
			// 		{
			// 			Query = query,
			// 			Results = results,
			// 			Token = token
			// 		});
			// 	}
			// }
			//return results;
		}

		private void AddToResults(List<Result> results, Result result, Query query, CancellationToken token)
		{
			results.Add(result);
			ResultsUpdated?.Invoke(this, new ResultUpdatedEventArgs
			{
				Query = query,
				Results = results,
				Token = token
			});
		}

		public void Dispose()
		{
			context.API.VisibilityChanged -= OnVisibilityChanged;
		}

		public Control CreateSettingPanel() => throw new NotImplementedException();
		public event ResultUpdatedEventHandler ResultsUpdated;
	}

	public class IconProvider { }
}