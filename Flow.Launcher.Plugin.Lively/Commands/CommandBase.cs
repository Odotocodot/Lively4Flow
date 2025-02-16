using System.Collections.Generic;
using System.Diagnostics;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public abstract class CommandBase : ISearchableResult
	{
		public abstract string Shortcut { get; }
		protected abstract string Description { get; }
		protected abstract string IconPath { get; }
		public int Score { private get; set; }
		public string SearchableString => Shortcut;

		// Maybe make async?
		private protected static void Execute(string args)
		{
			var psi = new ProcessStartInfo(Constants.CommandUtility, args) { CreateNoWindow = true };
			using Process process = Process.Start(psi);
		}

		public Result ToResult(PluginInitContext context, LivelyService livelyService, List<int> highlightData = null)
		{
			var autoCompleteText =
				$"{context.CurrentPluginMetadata.ActionKeyword} {Constants.Commands.Keyword}{Shortcut} ";
			return new Result
			{
				Title = Shortcut,
				SubTitle = Description,
				Score = Score,
				ContextData = this,
				TitleHighlightData = highlightData,
				IcoPath = IconPath,
				AutoCompleteText = autoCompleteText,
				Action = _ =>
				{
					context.API.ChangeQuery(autoCompleteText);
					return false;
				}
			};
		}

		public abstract List<Result> CommandResults(PluginInitContext context, LivelyService livelyService,
			string query = null);
	}
}