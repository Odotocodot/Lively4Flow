using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public class CommandResult
	{
		public const string Keyword = "!";
		private readonly PluginInitContext context;

		//public Command(string name, string shortcut, List<Result> results): this()
		public CommandResult(PluginInitContext context, string shortcut, string description, ResultGetter resultGetter)
		{
			this.context = context;
			Description = description;
			Shortcut = shortcut;
			ResultGetter = resultGetter;
		}

		public string Description { get; private set; }
		public string Shortcut { get; private set; }
		public ResultGetter ResultGetter { get; private set; }

		public Result ToResult() => new()
		{
			Title = Shortcut,
			SubTitle = Description,
			ContextData = this,
			Action = _ =>
			{
				context.API.ChangeQuery(context.CurrentPluginMetadata.ActionKeyword + " " + Keyword + Shortcut);
				return false;
			}
		};
	}

	public delegate List<Result> ResultGetter(CommandResult commandResult, Query query);
}