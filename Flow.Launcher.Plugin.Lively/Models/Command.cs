using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public class Command : ISearchableResult
	{
		public const string Keyword = "!";

		//public Command(string name, string shortcut, List<Result> results): this()
		public Command(string shortcut, string description, ResultGetter resultGetter)
		{
			Shortcut = shortcut;
			Description = description;
			ResultGetter = resultGetter;
		}

		public string Shortcut { get; private set; }
		public string Description { get; private set; }
		public ResultGetter ResultGetter { get; private set; }
		string ISearchableResult.SearchableString => Shortcut;

		Result ISearchableResult.ToResult(PluginInitContext context, List<int> highlightData = null) => new()
		{
			Title = Shortcut,
			SubTitle = Description,
			ContextData = this,
			TitleHighlightData = highlightData,
			Action = _ =>
			{
				context.API.ChangeQuery(context.CurrentPluginMetadata.ActionKeyword + " " + Keyword + Shortcut);
				return false;
			}
		};
	}

	public delegate List<Result> ResultGetter(Query query);
}