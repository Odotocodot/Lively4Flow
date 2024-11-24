using System;
using System.Collections.Generic;
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
		private IconProvider iconProvider;
		private Settings settings;

		private bool canLoadWallpapers;

		public async Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			SettingsHelper.Setup(settings, this.context);
			livelyService = new LivelyService(settings, context);
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

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			if (canLoadWallpapers)
			{
				canLoadWallpapers = false;
				await Task.WhenAll(livelyService.LoadWallpapers(token), livelyService.LoadLivelySettings(token));
			}

			if (string.IsNullOrWhiteSpace(query.Search))
			{
				var results = livelyService.Wallpapers.ToResultsList(livelyService);
				results.Insert(0, new Result
				{
					Title = "Type '!' to view commands",
					Score = 10000
				});
				return results;
			}

			if (query.FirstSearch.StartsWith(Command.Keyword))
			{
				if (query.FirstSearch.Length <= Command.Keyword.Length)
					return livelyService.Commands.ToResultsList(livelyService);

				var commandQuery = query.FirstSearch[1..].Trim();

				if (livelyService.Commands.TryGetValue(commandQuery, out Command command))
					return command.ResultGetter(query.SecondToEndSearch);

				return livelyService.Commands.FilterToResultsList(livelyService, commandQuery);
			}

			return livelyService.Wallpapers.FilterToResultsList(livelyService, query.Search);

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

		public void Dispose() => context.API.VisibilityChanged -= OnVisibilityChanged;

		public Control CreateSettingPanel() => throw new NotImplementedException();
		public event ResultUpdatedEventHandler ResultsUpdated;
	}

	public class IconProvider { }
}