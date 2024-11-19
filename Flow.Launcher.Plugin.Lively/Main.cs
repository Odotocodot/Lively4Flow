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
			if (args.IsVisible && !context.CurrentPluginMetadata.Disabled) { }
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			List<Result> results = new();
			if (string.IsNullOrWhiteSpace(query.Search))
				return results;

			var tasks = livelyService.GetWallpapers(token);
			await foreach (var wallpaper in tasks.ToAsyncEnumerable().WithCancellation(token))
			{
				Result result = resultCreator.FromWallpaper(await wallpaper);
				//context.API.LogInfo("LivelyPlugin", "Created Wallpaper");
				results.Add(result);
				ResultsUpdated?.Invoke(this, new ResultUpdatedEventArgs
				{
					Query = query,
					Results = results,
					Token = token
				});
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