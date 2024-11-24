using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public class Command : ISearchableResult //TODO convert to record
	{
		public const string Keyword = "!"; //TODO move to Constants.cs

		public Command(string shortcut, string description, ResultGetter resultGetter)
		{
			Shortcut = shortcut;
			Description = description;
			ResultGetter = resultGetter;
		}

		public string Shortcut { get; }
		public string Description { get; }
		public ResultGetter ResultGetter { get; }
		string ISearchableResult.SearchableString => Shortcut;

		Result ISearchableResult.ToResult(LivelyService livelyService, List<int> highlightData = null)
		{
			var autoCompleteText = $"{livelyService.Context.CurrentPluginMetadata.ActionKeyword} {Keyword}{Shortcut} ";
			return new Result
			{
				Title = Shortcut,
				SubTitle = Description,
				ContextData = this,
				TitleHighlightData = highlightData,
				AutoCompleteText = autoCompleteText,
				Action = _ =>
				{
					livelyService.Context.API.ChangeQuery(autoCompleteText);
					return false;
				}
			};
		}
	}

	public delegate List<Result> ResultGetter(string query);
}