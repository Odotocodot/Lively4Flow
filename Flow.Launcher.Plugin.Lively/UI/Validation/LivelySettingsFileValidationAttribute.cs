using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Flow.Launcher.Plugin.Lively.UI.Validation
{
	public class LivelySettingsFileValidationAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var settingsPath = (string)value;

			var exists = File.Exists(settingsPath)
			             && Path.GetFileName(settingsPath) == Constants.Files.LivelySettings
			             && File.Exists(Path.Combine(
				             Path.GetDirectoryName(settingsPath) ?? string.Empty,
				             Constants.Files.WallpaperLayout));

			return exists
				? ValidationResult.Success
				: new ValidationResult("Invalid settings file path");
		}
	}
}