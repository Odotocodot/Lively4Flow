using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.Lively.Commands;
using Flow.Launcher.Plugin.Lively.UI;
using static Flow.Launcher.Plugin.Lively.ResultsHelper;

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

		public virtual bool Equals(Wallpaper other)
		{
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return FolderPath == other.FolderPath;
		}

		public override int GetHashCode() => FolderPath != null ? FolderPath.GetHashCode() : 0;

		string ISearchableResult.SearchableString => Title;

		Result ISearchableResult.ToResult(PluginInitContext context, LivelyService livelyService,
			List<int> highlightData)
		{
			var title = Title;
			var score = 0;
			if (livelyService.IsActiveWallpaper(this, out var monitorIndexes))
			{
				var offset = livelyService.IsSingleDisplay
					? $"{SelectedEmoji} "
					: $"[{SelectedEmoji} {string.Join(", ", monitorIndexes)}] ";
				title = OffsetTitle(title, offset, highlightData);
				score = (livelyService.MonitorCount - monitorIndexes.Min() + 1) * ScoreMultiplier;
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
					Task.Run(() => SetWallpaperCommand.Execute(livelyService, this));
					//SetWallpaperCommand.Execute(livelyService, this);
					return true;
				}
			};
		}

		List<Result> IHasContextMenu.ToContextMenu(LivelyService livelyService)
		{
			//The readability here is pretty bad
			var results = new List<Result>();
			//Setting Wallpapers
			const string setPrefix = "Set as wallpaper";

			results.Add(new Result
			{
				Title = $"{setPrefix}{AppendAllMonitors(livelyService.IsSingleDisplay)}",
				Score = (livelyService.MonitorCount + 2) * 2 * ScoreMultiplier,
				IcoPath = Constants.Icons.Set,
				Action = _ =>
				{
					//Task.Run(() => SetWallpaperCommand.Execute(livelyService, this));
					SetWallpaperCommand.Execute(livelyService, this);
					return true;
				}
			});

			if (!livelyService.IsSingleDisplay)
				livelyService.IterateMonitors(index =>
					results.Add(GetMonitorIndexResult(setPrefix,
						Constants.Icons.Set,
						index,
						(livelyService.MonitorCount + 1) * 2 * ScoreMultiplier,
						i => SetWallpaperCommand.Execute(livelyService, this, i))));

			//Closing wallpapers
			const string closePrefix = "Close wallpaper";
			if (!livelyService.IsActiveWallpaper(this, out var activeIndexes))
				return results;

			if (livelyService.IsSingleDisplay || activeIndexes.Count > 1)
				results.Add(new Result
				{
					Title = $"{closePrefix}{AppendAllMonitors(activeIndexes.Count <= 1)}",
					Score = (livelyService.MonitorCount + 2) * ScoreMultiplier,
					IcoPath = Constants.Icons.Close,
					Action = _ =>
					{
						CloseWallpaperCommand.Execute(livelyService);
						return true;
					}
				});

			if (livelyService.IsSingleDisplay)
				return results;

			results.AddRange(activeIndexes.Select(i =>
				GetMonitorIndexResult(
					closePrefix,
					Constants.Icons.Close,
					i,
					(livelyService.MonitorCount + 1) * ScoreMultiplier,
					monitorIndex => CloseWallpaperCommand.Execute(livelyService, monitorIndex))));
			return results;
		}
	}
}