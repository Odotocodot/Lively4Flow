using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

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
		private List<Result> results = new();

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
				results.Clear();
			}
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			if (string.IsNullOrWhiteSpace(query.Search))
				return resultCreator.Empty();

			if (canLoadWallpapers)
			{
				canLoadWallpapers = false;

				var tasks = livelyService.GetWallpapers(token);
				await foreach (var wallpaper in tasks.ToAsyncEnumerable().WithCancellation(token))
				{
					Result result = resultCreator.FromWallpaper(await wallpaper);
					//context.API.LogInfo("LivelyWallpaperController", "Created Wallpaper");
					results.Add(result);
					ResultsUpdated?.Invoke(this, new ResultUpdatedEventArgs
					{
						Query = query,
						Results = results,
						Token = token
					});
				}
			}
			return results;
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