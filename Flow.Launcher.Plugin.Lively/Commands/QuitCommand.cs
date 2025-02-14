using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class QuitCommand : CommandBase
	{
		public override string Shortcut { get; } = "quit";
		protected override string Description { get; } = "Quit Lively";
		protected override string IconPath { get; } = Constants.Icons.Quit;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) => ResultsHelper.SingleResult(Description, IconPath, () => Execute("--shutdown true"), true);
	}
}