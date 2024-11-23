using System.Collections.Generic;
using System.IO;

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

		Result ISearchableResult.ToResult(LivelyService livelyService, List<int> highlightData = null) => new()
		{
			Title = Title,
			SubTitle = Desc,
			IcoPath = IconPath,
			ContextData = this,
			TitleHighlightData = highlightData,
			Action = _ =>
			{
				//Depends on WallpaperArrangement, if it is PER need to iterate all screens otherwise its fine.
				livelyService.Api.SetWallpaper(this);
				return true;
			}
		};
	}
}