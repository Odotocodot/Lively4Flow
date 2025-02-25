using System.Collections.Generic;
using System.Threading;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class IndexWallpapersCommand : CommandBase
	{
		public override string Shortcut { get; } = "index";
		protected override string Description { get; } = "Reindex Lively wallpapers";
		protected override string IconPath { get; } = Constants.Icons.Lively;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) => new()
		{
			new Result
			{
				Title = Description + "  [Shortcut: F5]",
				SubTitle = "Reindexing is needed if you have added/removed a wallpaper from Lively",
				IcoPath = Constants.Icons.Lively,
				AsyncAction = async _ =>
				{
					await livelyService.LoadWallpapers(CancellationToken.None);
					return true;
				}
			}
		};
	}
}