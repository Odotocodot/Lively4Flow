using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public class Command : ISearchable //TODO convert to record
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
		string ISearchable.SearchableString => Shortcut;
	}

	public delegate List<Result> ResultGetter(string query);
}