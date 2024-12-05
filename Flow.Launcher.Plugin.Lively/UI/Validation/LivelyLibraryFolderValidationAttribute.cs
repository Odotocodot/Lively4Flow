using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Flow.Launcher.Plugin.Lively.UI.Validation
{
	using Folders = Constants.Folders;

	public class LivelyLibraryFolderValidationAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var libraryPath = (string)value;
			return Directory.Exists(libraryPath)
			       && Directory.Exists(Path.Combine(libraryPath, Folders.LocalWallpapers))
			       && Directory.Exists(Path.Combine(libraryPath, Folders.SaveData))
			       && Directory.Exists(Path.Combine(libraryPath, Folders.WebWallpapers))
			       && Directory.Exists(Path.Combine(libraryPath, Folders.WpData))
				? ValidationResult.Success
				: new ValidationResult("Invalid Lively library folder path");
		}
	}
}