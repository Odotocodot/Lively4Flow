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
		private Commands commands;
		private Query lastQuery;

		public Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			QuickSetup.Run(settings, this.context);
			livelyService = new LivelyService(settings, context);
			commands = new Commands(livelyService, context);
			return Task.CompletedTask;
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
		{
			if (context.CurrentPluginMetadata.Disabled)
				return;

			if (args.IsVisible)
			{
				livelyService.EnableLoading();
				if (lastQuery?.ActionKeyword == context.CurrentPluginMetadata.ActionKeyword
				    && livelyService.Api.UIRefreshRequired())
					context.API.ReQuery();
			}
			else
			{
				livelyService.DisableLoading();
			}
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			lastQuery = query;
			await livelyService.Load(token);

			var results = GetResults(query);
			if (!livelyService.IsLivelyRunning)
				results.Insert(0, Results.LivelyNotRunningResult());
			return results;
		}

		private List<Result> GetResults(Query query)
		{
			if (string.IsNullOrWhiteSpace(query.Search))
			{
				var results = livelyService.Wallpapers.ToResultList(livelyService, context);
				results.Insert(0, Results.ViewCommandResult(context));
				return results;
			}

			if (query.FirstSearch.StartsWith(Constants.Commands.Keyword))
			{
				if (query.FirstSearch.Length <= Constants.Commands.Keyword.Length)
					return commands.ToResultList(livelyService, context);

				var commandQuery = query.FirstSearch[1..].Trim();

				if (commands.TryGetCommand(commandQuery, out Command command))
					return command.ResultGetter(query.SecondToEndSearch);

				return commands.ToResultList(livelyService, context, commandQuery);
			}

			return livelyService.Wallpapers.Cast<ISearchable>()
				.Concat(commands)
				.ToResultList(livelyService, context, query.Search);
		}

		public void Dispose() => context.API.VisibilityChanged -= OnVisibilityChanged;

		public Control CreateSettingPanel() => settings.GetSettingsView(context);

		public List<Result> LoadContextMenus(Result result) => Results.ContextMenu(result, livelyService);
	}
}