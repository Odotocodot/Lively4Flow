using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class ShowCommand : CommandBase
	{
		public override string Shortcut { get; } = "show";
		protected override string Description { get; } = "Show Lively";
		protected override string IconPath { get; } = Constants.Icons.Open;

		public override List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null) => ResultsHelper.SingleResult(Description, IconPath, () => Execute("--showApp true"), true);
	}
}