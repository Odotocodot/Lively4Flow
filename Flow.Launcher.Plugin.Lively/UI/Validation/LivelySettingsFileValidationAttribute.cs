using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Flow.Launcher.Plugin.Lively.UI.Validation
{
	public class LivelySettingsFileValidationAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var settingsPath = (string)value;
			return File.Exists(settingsPath)
			       && Path.GetFileName(settingsPath) == Constants.Files.LivelySettings
				? ValidationResult.Success
				: new ValidationResult("Invalid settings file path");
		}
	}
}