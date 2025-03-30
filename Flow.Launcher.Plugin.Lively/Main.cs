using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Commands;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI;

namespace Flow.Launcher.Plugin.Lively
{
	public class Main : IAsyncPlugin, ISettingProvider, IDisposable, IContextMenu, IAsyncReloadable
	{
		private PluginInitContext context;
		private LivelyService livelyService;
		private Settings settings;
		private CommandContainer commands;
		private Query lastQuery;

		private bool initialised;
		private bool isLivelyRunning;
		private bool canCheckLively;

		public Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			settings.Validate();
			QuickSetup.Run(settings, this.context);
			livelyService = new LivelyService(settings, context);
			commands = new CommandContainer();
			ResultsHelper.SetupScoreMultiplier(context);
			return Task.CompletedTask;
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
		{
			if (context.CurrentPluginMetadata.Disabled)
				return;

			if (args.IsVisible)
			{
				canCheckLively = true;
			}
			else
			{
				if (lastQuery == null)
					return;

				if (!string.IsNullOrWhiteSpace(lastQuery.ActionKeyword) //Wildcard action keyword check
				    && lastQuery.ActionKeyword != context.CurrentPluginMetadata.ActionKeyword)
					return;

				if (livelyService.UIRefreshRequired())
					context.API.ReQuery(false);
			}
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
		{
			if (settings.HasErrors)
				return ResultsHelper.SettingsErrors(settings);

			lastQuery = query;

			if (!initialised) // || query.IsReQuery
			{
				initialised = true;
				await livelyService.LoadWallpapers(token);
			}

			var results = GetResults(query);

			if (canCheckLively)
			{
				isLivelyRunning = Process.GetProcessesByName(nameof(Constants.Lively))
					.Any(p => p.MainModule?.FileName.EndsWith($"{nameof(Constants.Lively)}.exe") == true);
				livelyService.UpdateMonitorCount();
				canCheckLively = false;
			}

			if (!isLivelyRunning)
				results.Add(ResultsHelper.LivelyNotRunningResult());
			return results;
		}

		private List<Result> GetResults(Query query)
		{
			if (string.IsNullOrWhiteSpace(query.Search))
			{
				var results = livelyService.Wallpapers.OrderBy(x => x.Title)
					.ToResultList(context, livelyService);
				if (settings.ShowViewCommandsResult)
					results.Add(commands.ViewCommandResult(context));
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

		public Control CreateSettingPanel() => settings.GetSettingsView(context);

		public List<Result> LoadContextMenus(Result result) =>
			result.ContextData is IHasContextMenu data ? data.ToContextMenu(livelyService) : null;

		public async Task ReloadDataAsync()
		{
			await livelyService.LoadWallpapers(CancellationToken.None);
			ResultsHelper.SetupScoreMultiplier(context);
		}

		public void Dispose()
		{
			context.API.VisibilityChanged -= OnVisibilityChanged;
			livelyService.Dispose();
		}
	}
}