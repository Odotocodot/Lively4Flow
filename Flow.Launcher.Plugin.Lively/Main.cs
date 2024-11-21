using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.SharedModels;

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
		//private List<Task<Wallpaper>> wallpaperTasks;

		//private List<Wallpaper> wallpapers = new();
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
				livelyService.ClearLoadedWallpapers();
			}
		}


		private record WallpaperResult(Wallpaper wallpaper)
		{
			public List<int> HighlightData { get; set; }
		}
		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			// if (string.IsNullOrWhiteSpace(query.Search))
			// 	return resultCreator.Empty();

			if (canLoadWallpapers)
			{
				canLoadWallpapers = false;
				await livelyService.LoadWallpapers(token);
			}

			if (string.IsNullOrWhiteSpace(query.Search))
				return livelyService.Wallpapers.Select(resultCreator.FromWallpaper).ToList();

			if (query.FirstSearch == "!")
			{
				return livelyService.Commands.Select(command => command.ToResult()).ToList();
			}
			
			return livelyService.Wallpapers.Select(wallpaper => new WallpaperResult(wallpaper))
				.Where(value =>
				{
					MatchResult matchResult = context.API.FuzzySearch(query.Search, value.wallpaper.Title);
					value.HighlightData = matchResult.MatchData;
					return matchResult.Success;
				})
				.Select(value => resultCreator.FromWallpaper(value.wallpaper, value.HighlightData))
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