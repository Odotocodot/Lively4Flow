using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.Lively.Commands;
using Flow.Launcher.Plugin.Lively.UI;

namespace Flow.Launcher.Plugin.Lively.Models
{
	public partial record Wallpaper : ISearchableResult, IHasContextMenu
	{
		/// <summary>
		/// Actual location of the wallpaper on disk. Used when opening the directory in windows.
		/// </summary>
		public string FolderPath { get; private set; }

		/// <summary>
		/// This can be different from <see cref="FolderPath"/> if the <see cref="LivelySettings.WallpaperDir"/> in the
		/// Lively settings is different from the actual location of the wallpaper. This can happen when Lively is installed
		/// from the Microsoft Store. Used with commands e.g. <see cref="SetWallpaperCommand"/>.
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
					? $"{ResultsHelper.SelectedEmoji} | "
					: $"{ResultsHelper.SelectedEmoji} {string.Join(", ", monitorIndexes)} | ";
				title = ResultsHelper.OffsetTitle(title, offset, highlightData);
				score = 2 * ResultsHelper.ScoreMultiplier;
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
					SetWallpaperCommand.Execute(livelyService, this);
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
				Title = $"{setPrefix}{ResultsHelper.AppendAllMonitors(livelyService.IsSingleDisplay)}",
				Score = (livelyService.MonitorCount + 2) * 2 * ResultsHelper.ScoreMultiplier,
				IcoPath = Constants.Icons.Set,
				Action = _ =>
				{
					SetWallpaperCommand.Execute(livelyService, this);
					return true;
				}
			});

			if (!livelyService.IsSingleDisplay)
				livelyService.IterateMonitors(index =>
					results.Add(ResultsHelper.GetMonitorIndexResult(setPrefix,
						Constants.Icons.Set,
						index,
						(livelyService.MonitorCount + 1) * 2 * ResultsHelper.ScoreMultiplier,
						i => SetWallpaperCommand.Execute(livelyService, this, i))));

			//Closing wallpapers
			const string closePrefix = "Close wallpaper";
			if (!livelyService.IsActiveWallpaper(this, out var activeIndexes))
				return results;

			if (livelyService.IsSingleDisplay || activeIndexes.Count > 1)
				results.Add(new Result
				{
					Title = $"{closePrefix}{ResultsHelper.AppendAllMonitors(activeIndexes.Count <= 1)}",
					Score = (livelyService.MonitorCount + 2) * ResultsHelper.ScoreMultiplier,
					IcoPath = Constants.Icons.Close,
					Action = _ =>
					{
						CloseWallpaperCommand.Execute();
						return true;
					}
				});

			if (livelyService.IsSingleDisplay)
				return results;

			results.AddRange(activeIndexes.Select(i =>
				ResultsHelper.GetMonitorIndexResult(
					closePrefix,
					Constants.Icons.Close,
					i,
					(livelyService.MonitorCount + 1) * ResultsHelper.ScoreMultiplier,
					CloseWallpaperCommand.Execute)));
			return results;
		}
	}
}