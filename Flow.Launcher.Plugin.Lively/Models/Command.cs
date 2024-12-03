using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public record Command(string Shortcut, string Description, string IconPath, ResultGetter ResultGetter) : ISearchable
	{
		public Command(string shortcut, string description, string iconPath, Action action) :
			this(shortcut, description, iconPath, _ => Results.SingleResult(description, iconPath, action, true)) { }

		string ISearchable.SearchableString => Shortcut;
	}

	public delegate List<Result> ResultGetter(string query);
}