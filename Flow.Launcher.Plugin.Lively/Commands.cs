using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public class Command
	{
		//public Command(string name, string shortcut, List<Result> results): this()
		public Command(string name, string shortcut, string description, ResultGetter resultGetter)
		{
			Name = name;
			Shortcut = shortcut;
			Description = description;
			ResultGetter = resultGetter;
		}
		public string Name { get; private set; }
		public string Shortcut { get; private set; }
		public string Description { get; private set; }
		public ResultGetter ResultGetter { get; private set; }

		public Result ToResult()
		{
			return new Result
			{
				Title = Shortcut,
				SubTitle = Description,
			};
		}
	}

	public delegate List<Result> ResultGetter();

}