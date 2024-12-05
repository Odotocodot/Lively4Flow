using System.ComponentModel.DataAnnotations;
using System.IO;
using Flow.Launcher.Plugin.Lively.Models;
using Flow.Launcher.Plugin.Lively.UI.ViewModels;

namespace Flow.Launcher.Plugin.Lively.UI.Validation
{
	public class LivelyExePathValidationAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var instance = (SettingsViewModel)validationContext.ObjectInstance;
			if (instance.LivelyInstallType == LivelyInstallType.MicrosoftStore)
				return ValidationResult.Success;

			var exePath = (string)value;
			return File.Exists(exePath)
			       && Path.GetExtension(exePath) == ".exe"
				? ValidationResult.Success
				: new ValidationResult("Invalid Lively exe path");
		}
	}
}