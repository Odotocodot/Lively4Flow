using System;
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
		//string License,
		string Contact
		//WallpaperType Type,
		//string FileName,
		//string Arguments,
		//bool IsAbsolutePath,
		//string Id,
		//List<string> Tags,
		//int Version
	) : ISearchable
	{
		//Could move this to another type?
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

			var partialPath = Path.GetDirectoryName(folderPath) switch
			{
				string path when path.EndsWith(Constants.Folders.LocalWallpapers) => Constants.Folders.LocalWallpapers,
				string path when path.EndsWith(Constants.Folders.WebWallpapers) => Constants.Folders.WebWallpapers,
				_ => throw new ArgumentException("Invalid wallpaper path")
			};
			var folderName = Path.GetFileName(Path.TrimEndingDirectorySeparator(folderPath));
			LivelyFolderPath = Path.Combine(livelyWallpaperFolder, partialPath, folderName);
		}

		string ISearchable.SearchableString => Title;
	}
}