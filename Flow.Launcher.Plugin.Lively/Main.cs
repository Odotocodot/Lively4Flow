using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Commands;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI;

namespace Flow.Launcher.Plugin.Lively
{
	public class Main : IAsyncPlugin, ISettingProvider, IDisposable, IContextMenu
	{
		private PluginInitContext context;
		private LivelyService livelyService;
		private Settings settings;
		private CommandContainer commands;
		private Query lastQuery;

		public Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			settings.Validate();
			QuickSetup.Run(settings, this.context);
			livelyService = new LivelyService(settings, context);
			commands = new CommandContainer();
			return Task.CompletedTask;
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
		{
			if (context.CurrentPluginMetadata.Disabled)
				return;

			if (args.IsVisible)
				livelyService.EnableLoading();
			// TODO: reimplement refreshing UI
			// if (lastQuery?.ActionKeyword == context.CurrentPluginMetadata.ActionKeyword
			//     && livelyService.Api.UIRefreshRequired())
			// 	context.API.ReQuery();
			else
				livelyService.DisableLoading();
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			if (settings.HasErrors)
				return ResultsHelper.SettingsErrors(settings);

			lastQuery = query;

			await livelyService.Load(token);

			var results = GetResults(query);
			if (!livelyService.IsLivelyRunning)
				results.Insert(0, ResultsHelper.LivelyNotRunningResult());
			return results;
		}

		private List<Result> GetResults(Query query)
		{
			if (string.IsNullOrWhiteSpace(query.Search))
			{
				var results = livelyService.Wallpapers.OrderBy(x => x.Title)
					.ToResultList(context, livelyService);
				if (settings.ShowViewCommandsResult)
					results.Insert(0, commands.ViewCommandResult(context));
				return results;
			}

			if (query.FirstSearch.StartsWith(Constants.Commands.Keyword))
			{
				if (query.FirstSearch.Length <= Constants.Commands.Keyword.Length)
					return commands.ToResultList(context, livelyService);

				var commandQuery = query.FirstSearch[1..].Trim();

				if (commands.TryGetCommand(commandQuery, out CommandBase command))
					return command.CommandResults(context, livelyService, query.SecondToEndSearch);

				return commands.ToResultList(context, livelyService, commandQuery);
			}

			return livelyService.Wallpapers.Cast<ISearchableResult>()
				.Concat(commands)
				.ToResultList(context, livelyService, query.Search);
		}

		public void Dispose()
		{
			context.API.VisibilityChanged -= OnVisibilityChanged;
			livelyService.DisableLoading();
		}

		public Control CreateSettingPanel() => settings.GetSettingsView(context);

		public List<Result> LoadContextMenus(Result result) =>
			result.ContextData is IHasContextMenu data ? data.ToContextMenu(livelyService) : null;
	}
}