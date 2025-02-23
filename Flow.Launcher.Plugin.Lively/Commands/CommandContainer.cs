using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class CommandContainer : IEnumerable<CommandBase>
	{
		// TODO: initialise with reflection for ease?
		private readonly CommandKeyedCollection commands = new()
		{
			new SetWallpaperCommand(),
			new CloseWallpaperCommand(),
			new RandomiseCommand(),
			new LayoutCommand(),
			new VolumeCommand(),
			new PlaybackCommand(),
			new ShowCommand(),
			new QuitCommand()
		};


		public Result ViewCommandResult(PluginInitContext context)
		{
			var autoCompleteText = ResultsHelper.AutoCompleteText(context, Constants.Commands.Keyword);

			return new Result
			{
				Title = "View Lively commands",
				SubTitle = $"Type '{Constants.Commands.Keyword}' or select this result to view commands",
				Score = 100 * ResultsHelper.ScoreMultiplier,
				IcoPath = Constants.Icons.Lively,
				AutoCompleteText = autoCompleteText,
				Action = _ =>
				{
					context.API.ChangeQuery(autoCompleteText);
					return false;
				}
			};
		}

		public bool TryGetCommand(string query, out CommandBase command) => commands.TryGetValue(query, out command);

		public IEnumerator<CommandBase> GetEnumerator() => commands.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)commands).GetEnumerator();

		private class CommandKeyedCollection : KeyedCollection<string, CommandBase>
		{
			protected override string GetKeyForItem(CommandBase item) => item.Shortcut;
		}
	}
}