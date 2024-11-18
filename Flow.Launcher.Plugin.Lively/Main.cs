using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Lively
{
	public class Main : IAsyncPlugin, ISettingProvider, IDisposable
	{
		private PluginInitContext context;
		private LivelyPlugin plugin;
		private ResultCreator resultCreator;
		private IconProvider iconProvider;
		private Settings settings;

		public async Task InitAsync(PluginInitContext context)
		{
			this.context = context;
			context.API.VisibilityChanged += OnVisibilityChanged;
			settings = context.API.LoadSettingJsonStorage<Settings>();
			SettingsHelper.Setup(settings, this.context);
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs args)
		{
			if (args.IsVisible && !context.CurrentPluginMetadata.Disabled) { }
		}

		public async Task<List<Result>> QueryAsync(Query query, CancellationToken token) =>
			throw new NotImplementedException();

		public void Dispose()
		{
			context.API.VisibilityChanged -= OnVisibilityChanged;
		}

		public Control CreateSettingPanel() => throw new NotImplementedException();
	}

	public class IconProvider { }

	public class ResultCreator { }

	public class LivelyPlugin { }
}