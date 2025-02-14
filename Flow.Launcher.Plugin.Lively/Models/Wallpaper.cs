using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.Lively.UI;

namespace Flow.Launcher.Plugin.Lively.Models
{
	//Sourced from https://github.com/rocksdanister/lively/
	public record Wallpaper(
		//string AppVersion,
		string Title,
		string Thumbnail,
		string Preview,
		string Desc,
		string Author,
		//string License,
		string Contact
		//WallpaperType Type,
		//string FileName,
		//string Arguments,
		//bool IsAbsolutePath,
		//string Id,
		//List<string> Tags,
		//int Version
	) : ISearchableResult, IHasContextMenu
	{
		// Could move this to another type?
		/// <summary>
		/// Actual location of the wallpaper on disk. Used when opening the directory in windows.
		/// </summary>
		public string FolderPath { get; private set; }

		/// <summary>
		/// This can be different from <see cref="FolderPath"/> if the <see cref="LivelySettings.WallpaperDir"/> in the
		/// Lively settings is different from the actual location of the wallpaper. This can happen when Lively is installed
		/// from the Microsoft Store. Used with the commands in <see cref="LivelyCommandApi"/>.
		/// </summary>
		public string LivelyFolderPath { get; private set; }

		public string IconPath { get; private set; }
		public string PreviewPath { get; private set; }

		public void Init(string folderPath, string livelyWallpaperFolder)
		{
			FolderPath = folderPath;
			IconPath = Path.Combine(folderPath, Path.GetFileName(Thumbnail) ?? string.Empty);
			PreviewPath = Path.Combine(folderPath, Path.GetFileName(Preview) ?? string.Empty);
			if (!File.Exists(PreviewPath))
				PreviewPath = IconPath;

			LivelyFolderPath = Path.Combine(
				livelyWallpaperFolder,
				Constants.Folders.Wallpapers.Single(dir => Path.GetDirectoryName(folderPath)?.EndsWith(dir) == true),
				Path.GetFileName(Path.TrimEndingDirectorySeparator(folderPath)));
		}

		string ISearchableResult.SearchableString => Title;

		Result ISearchableResult.ToResult(PluginInitContext context, LivelyService livelyService,
			List<int> highlightData)
		{
			var title = Title;
			var score = 0;
			if (livelyService.IsActiveWallpaper(this, out var monitorIndexes))
			{
				var offset = livelyService.IsSingleDisplay
					? $"{Results.SelectedEmoji} | "
					: $"{Results.SelectedEmoji} {string.Join(", ", monitorIndexes)} | ";
				title = Results.OffsetTitle(title, offset, highlightData);
				score = 2 * Results.ScoreMultiplier;
			}

			return new Result
			{
				Title = title,
				SubTitle = Desc,
				IcoPath = IconPath,
				Score = score,
				ContextData = this,
				PreviewPanel = this.GetUserControl(),
				TitleHighlightData = highlightData,
				Action = _ =>
				{
					livelyService.Api.SetWallpaper(this); // TODO: change this livelyService.Commands.SetWallpaper(this)
					return true;
				}
			};
		}

		public List<Result> ToContextMenu(LivelyService livelyService)
		{
			//The readability here is pretty bad
			var results = new List<Result>();
			//Setting Wallpapers
			const string setPrefix = "Set as wallpaper";

			results.Add(new Result
			{
				Title = $"{setPrefix}{Results.AppendAllMonitors(livelyService.IsSingleDisplay)}",
				Score = (livelyService.MonitorCount + 2) * 2 * Results.ScoreMultiplier,
				IcoPath = Constants.Icons.Set,
				Action = _ =>
				{
					livelyService.Api.SetWallpaper(this);
					return true;
				}
			});

			if (!livelyService.IsSingleDisplay)
				livelyService.IterateMonitors(index =>
					results.Add(Results.GetMonitorIndexResult(setPrefix,
						Constants.Icons.Set,
						index,
						(livelyService.MonitorCount + 1) * 2 * Results.ScoreMultiplier,
						i => livelyService.Api.SetWallpaper(this, i))));

			//Closing wallpapers
			const string closePrefix = "Close wallpaper";
			if (!livelyService.IsActiveWallpaper(this, out var activeIndexes))
				return results;

			if (livelyService.IsSingleDisplay || activeIndexes.Count > 1)
				results.Add(new Result
				{
					Title = $"{closePrefix}{Results.AppendAllMonitors(activeIndexes.Count <= 1)}",
					Score = (livelyService.MonitorCount + 2) * Results.ScoreMultiplier,
					IcoPath = Constants.Icons.Close,
					Action = _ =>
					{
						livelyService.Api.CloseWallpaper();
						return true;
					}
				});

			if (livelyService.IsSingleDisplay)
				return results;

			results.AddRange(activeIndexes.Select(i =>
				Results.GetMonitorIndexResult(
					closePrefix,
					Constants.Icons.Close,
					i,
					(livelyService.MonitorCount + 1) * Results.ScoreMultiplier,
					index => livelyService.Api.CloseWallpaper(index))));
			return results;
		}
	}
}