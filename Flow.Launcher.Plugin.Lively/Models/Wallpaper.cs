using System.Collections.Generic;
using System.IO;
using System.Linq;

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
		string License,
		string Contact
		//WallpaperType Type,
		//string FileName,
		//string Arguments,
		//bool IsAbsolutePath,
		//string Id,
		//List<string> Tags,
		//int Version
	) : ISearchableResult
	{
		//Could move this to another type?
		public string FolderPath { get; private set; }
		public string IconPath { get; private set; }
		public string PreviewPath { get; private set; }

		public void Init(string folderPath)
		{
			FolderPath = folderPath;
			IconPath = Path.Combine(folderPath, Thumbnail);
			PreviewPath = Path.Combine(folderPath, Preview);
		}

		string ISearchableResult.SearchableString => Title;

		Result ISearchableResult.ToResult(LivelyService livelyService, List<int> highlightData)
		{
			var title = Title;
			if (livelyService.IsActiveWallpaper(this, out var monitorIndexes))
				title = ResultFrom.OffsetTitle(title,
					$"[{ResultFrom.SelectedEmoji} {string.Join(", ", monitorIndexes.Order())}] ",
					highlightData);

			return new Result
			{
				Title = title,
				SubTitle = Desc,
				IcoPath = IconPath,
				ContextData = this,
				TitleHighlightData = highlightData,
				Action = _ =>
				{
					livelyService.Api.SetWallpaper(this);
					livelyService.Context.API.ReQuery();
					return true;
				}
			};
		}
	}
}