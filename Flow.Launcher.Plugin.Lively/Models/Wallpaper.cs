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
	) : ISearchable, ISearchableResult // TODO: maybe IResultContextMenu?
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

		string ISearchable.SearchableString => Title;

		string ISearchableResult.SearchableString => Title;

		Result ISearchableResult.ToResult(PluginInitContext context, LivelyService livelyService,
			List<int> highlightData = null)
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
	}
}