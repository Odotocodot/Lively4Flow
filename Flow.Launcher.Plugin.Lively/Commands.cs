using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	using Icons = Constants.Icons;

	public class Commands : IEnumerable<Command>
	{
		private readonly CommandKeyedCollection commands;

		public Commands(LivelyService livelyService, PluginInitContext context)
		{
			commands = new CommandKeyedCollection
			{
				new Command("setwp", "Search and set wallpapers", Icons.Set,
					q => livelyService.Wallpapers.ToResultList(livelyService, context, q)),
				new Command("random", "Set a random Wallpaper", Icons.Random,
					_ => Results.For.RandomiseCommand(livelyService)),
				new Command("closewp", "Close a wallpaper", Icons.Close,
					_ => Results.For.CloseCommand(livelyService)),
				new Command("volume", "Set the volume of a wallpaper", Icons.Volume,
					q => Results.For.VolumeCommand(livelyService, q)),
				new Command("layout", "Change the wallpaper layout", Icons.Layout,
					_ => Results.For.WallpaperArrangements(livelyService)),
				new Command("playback", "Pause or play wallpaper playback", Icons.Playback,
					_ => Results.For.PlaybackCommand(livelyService)),
				//new Command("seek", "Set wallpaper playback position", null),
				new Command(Constants.Commands.Open, "Show Lively", Icons.Open, livelyService.Api.OpenLively),
				new Command("quit", "Quit Lively", Icons.Quit, livelyService.Api.QuitLively)
			};
		}

		public bool TryGetCommand(string query, out Command command) => commands.TryGetValue(query, out command);

		public IEnumerator<Command> GetEnumerator() => commands.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)commands).GetEnumerator();

		private class CommandKeyedCollection : KeyedCollection<string, Command>
		{
			protected override string GetKeyForItem(Command item) => item.Shortcut;
		}
	}
}