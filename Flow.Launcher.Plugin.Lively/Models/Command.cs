using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public record Command(string Shortcut, string Description, ResultGetter ResultGetter) : ISearchable
	{
		string ISearchable.SearchableString => Shortcut;
	}

	public delegate List<Result> ResultGetter(string query);
}