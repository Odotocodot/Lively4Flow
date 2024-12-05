using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI;

namespace Flow.Launcher.Plugin.Lively
{
	public class Main : IAsyncPlugin, ISettingProvider, IDisposable, IContextMenu
	{
		private PluginInitContext context;
		private LivelyService livelyService;
		private Settings settings;

		public Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			QuickSetup.Run(settings, this.context);
			livelyService = new LivelyService(settings, context);
			return Task.CompletedTask;
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
		{
			if (context.CurrentPluginMetadata.Disabled)
				return;

			if (args.IsVisible)
			{
				livelyService.EnableLoading();
			}
			else
			{
				livelyService.DisableLoading();
				context.API.ReQuery();
			}
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			await livelyService.Load(token);

			if (string.IsNullOrWhiteSpace(query.Search))
			{
				var results = livelyService.Wallpapers.ToResultList(livelyService, context);
				results.Insert(0, Results.ViewCommandResult(context));
				return results;
			}

			if (query.FirstSearch.StartsWith(Constants.Commands.Keyword))
			{
				if (query.FirstSearch.Length <= Constants.Commands.Keyword.Length)
					return livelyService.Commands.ToResultList(livelyService, context);

				var commandQuery = query.FirstSearch[1..].Trim();

				if (livelyService.TryGetCommand(commandQuery, out Command command))
					return command.ResultGetter(query.SecondToEndSearch);

				return livelyService.Commands.ToResultList(livelyService, context, commandQuery);
			}

			return livelyService.Wallpapers.Cast<ISearchable>()
				.Concat(livelyService.Commands)
				.ToResultList(livelyService, context, query.Search);
		}

		public void Dispose() => context.API.VisibilityChanged -= OnVisibilityChanged;

		public Control CreateSettingPanel() => settings.GetSettingsView(context);

		public List<Result> LoadContextMenus(Result result) => Results.ContextMenu(result, livelyService);
	}
}