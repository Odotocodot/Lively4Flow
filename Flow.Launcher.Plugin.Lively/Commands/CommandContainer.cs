using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.Lively.Commands
{
	public class CommandContainer : IEnumerable<CommandBase>
	{
		private readonly CommandKeyedCollection commands;

		public CommandContainer()
		{
			commands = new CommandKeyedCollection
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

			for (var i = 0; i < commands.Count; i++)
				commands[i].Score = (commands.Count - i) * Results.ScoreMultiplier;
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