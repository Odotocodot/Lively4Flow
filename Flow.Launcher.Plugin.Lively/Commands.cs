using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flow.Launcher.Plugin.Lively.Models;

namespace Flow.Launcher.Plugin.Lively
{
	public class Commands : IEnumerable<Command>
	{
		private readonly CommandKeyedCollection commands;

		public Commands(LivelyService livelyService, PluginInitContext context)
		{
			commands = new CommandKeyedCollection
			{
				new Command("setwp", "Search and set wallpapers", Constants.Icons.Set,
					q => livelyService.Wallpapers.ToResultList(livelyService, context, q)),
				new Command("random", "Set a random Wallpaper", Constants.Icons.Random,
					_ => Results.For.RandomiseCommand(livelyService)),
				new Command("closewp", "Close a wallpaper", Constants.Icons.Close,
					_ => Results.For.CloseCommand(livelyService)),
				new Command("volume", "Set the volume of a wallpaper", Constants.Icons.Volume,
					q => Results.For.VolumeCommand(livelyService, q)),
				new Command("layout", "Change the wallpaper layout", Constants.Icons.Layout,
					_ => Results.For.WallpaperArrangements(livelyService)),
				new Command("playback", "Pause or play wallpaper playback", Constants.Icons.Playback,
					_ => Results.For.PlaybackCommand(livelyService)),
				//new Command("seek", "Set wallpaper playback position", null),
				new Command("open", "Open or Show Lively", Constants.Icons.Open, livelyService.Api.OpenLively),
				new Command("quit", "Quit Lively", Constants.Icons.Quit, livelyService.Api.QuitLively)
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