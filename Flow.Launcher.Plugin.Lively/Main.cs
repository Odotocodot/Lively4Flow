using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class Main : IAsyncPlugin, ISettingProvider, IDisposable, IContextMenu
	{
		private PluginInitContext context;
		private LivelyService livelyService;
		private IconProvider iconProvider;
		private Settings settings;


		public Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			SettingsHelper.Setup(settings, this.context);
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
			var isLivelyRunning = IsLivelyRunning();

			if (string.IsNullOrWhiteSpace(query.Search))
			{
				var results = livelyService.Wallpapers.ToResultList(livelyService);
				results.Insert(0, Results.ViewCommandResult(livelyService, isLivelyRunning));
				return results;
			}

			if (query.FirstSearch.StartsWith(Constants.Commands.Keyword))
			{
				if (query.FirstSearch.Length <= Constants.Commands.Keyword.Length)
					return livelyService.Commands.ToResultList(livelyService);

				var commandQuery = query.FirstSearch[1..].Trim();

				if (livelyService.TryGetCommand(commandQuery, out Command command))
					return command.ResultGetter(query.SecondToEndSearch);

				return livelyService.Commands.ToResultList(livelyService, commandQuery);
			}

			return livelyService.Wallpapers.Cast<ISearchable>()
				.Concat(livelyService.Commands)
				.ToResultList(livelyService, query.Search);
		}

		private bool IsLivelyRunning() => Process.GetProcessesByName("Lively")
			.FirstOrDefault(p => p.MainModule?.FileName.StartsWith(settings.LivelyExePath) == true) != null;

		public void Dispose() => context.API.VisibilityChanged -= OnVisibilityChanged;

		public Control CreateSettingPanel() => throw new NotImplementedException();

		public List<Result> LoadContextMenus(Result result) => Results.ContextMenu(result, livelyService);
	}

	public class IconProvider { }
}