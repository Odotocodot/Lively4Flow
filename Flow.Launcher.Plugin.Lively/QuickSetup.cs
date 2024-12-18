using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Flow.Launcher.Plugin.Lively.Models;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.Lively
{
	public static class QuickSetup
	{
		public static void ForceRun(Settings settings, PluginInitContext context)
		{
			settings.HasRunQuickSetup = false;
			Run(settings, context);
		}

		public static void Run(Settings settings, PluginInitContext context)
		{
			if (settings.InstallType != LivelyInstallType.None || settings.HasRunQuickSetup)
				return;
			Log(context, "Starting Setup");

			settings.InstallType = GetInstallLocation(context, out var exePath);

			string baseStoragePath;
			switch (settings.InstallType)
			{
				case LivelyInstallType.GitHub:
					Log(context, $"Lively exe [GitHub Version] was found at: \"{exePath}\"");
					baseStoragePath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"Lively Wallpaper");
					break;
				case LivelyInstallType.MicrosoftStore:
					Log(context, $"Lively exe [Microsoft Store Version] was found at: \"{exePath}\"");
					baseStoragePath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						$@"Packages\{Constants.Lively.AppId}\LocalCache\Local\Lively Wallpaper");
					break;
				default:
				case LivelyInstallType.None:
					Log(context, "No exe was NOT found, exiting quick setup.");
					//TODO: tell the user
					return;
			}

			if (FindLivelySettings(context, baseStoragePath, out var settingsPath))
				settings.LivelySettingsJsonPath = settingsPath;

			Log(context, "Finished quick setup");
			settings.HasRunQuickSetup = true;
		}

		private static LivelyInstallType GetInstallLocation(PluginInitContext context, out string exePath)
		{
			Log(context, "Looking for Lively exe [GitHub Version]");
			var installType = LivelyInstallType.None;
			if (FindLivelyGitHub(out exePath))
			{
				installType = LivelyInstallType.GitHub;
			}
			else
			{
				Log(context, "Lively exe [GitHub Version] was NOT found");
				Log(context, "Looking for Lively exe [Microsoft Store Version]");
				if (FindLivelyMSStore(out exePath))
					installType = LivelyInstallType.MicrosoftStore;
				else
					Log(context, "Lively exe [Microsoft Store Version] was NOT found");
			}

			return installType;
		}


		private static bool FindLivelySettings(PluginInitContext context, string baseStoragePath,
			out string settingsPath)
		{
			Log(context, $"Looking for {Constants.Files.LivelySettings}");
			settingsPath = Path.Combine(baseStoragePath, Constants.Files.LivelySettings);
			if (File.Exists(settingsPath))
			{
				Log(context, $"{Constants.Files.LivelySettings} was found at: \"{settingsPath}\"");
				return true;
			}

			Log(context, $"{Constants.Files.LivelySettings} was NOT found");
			return false;
		}

		private static bool FindLivelyGitHub(out string exePath)
		{
			exePath = null;
			var baseRegistryKeys = new[]
			{
				Registry.CurrentUser,
				Registry.LocalMachine
			};
			var initialSubKeys = new[]
			{
				@"SOFTWARE\WoW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
				@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
			};
			for (var i = 0; i < initialSubKeys.Length; i++)
			{
				for (var j = 0; j < baseRegistryKeys.Length; j++)
					if (TryGetLively(baseRegistryKeys[j], initialSubKeys[i], out exePath))
						return true;
			}

			return false;

			static bool TryGetLively(RegistryKey baseKey, string initialSubKey, out string path)
			{
				path = null;
#nullable enable
				using RegistryKey? key = baseKey.OpenSubKey(initialSubKey);
				if (key == null)
					return false;

				foreach (var name in key.GetSubKeyNames())
				{
					using RegistryKey subKey = key.OpenSubKey(name)!;
					if (((string?)subKey.GetValue("DisplayName"))?.Contains("Lively Wallpaper") != true)
						continue;

					var installLocation = (string?)subKey.GetValue("InstallLocation");

					if (installLocation == null)
						return false;
					path = Path.Combine(installLocation, "Lively.exe");
					return true;
				}

				return false;
#nullable restore
			}
		}

		private static bool FindLivelyMSStore(out string exePath)
		{
			exePath = null;
			var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			var powerShellPath = @$"{systemRoot}\System32\WindowsPowerShell\v1.0\powershell.exe";
			if (!File.Exists(powerShellPath))
			{
				powerShellPath = @$"{systemRoot}\SysWOW64\WindowsPowerShell\v1.0\powershell.exe";
				if (!File.Exists(powerShellPath))
					return false;
			}

			using Process process = Process.Start(new ProcessStartInfo
			{
				FileName = powerShellPath,
				Arguments =
					$"Get-AppxPackage -Name {Constants.Lively.AppName} | Select -ExpandProperty InstallLocation",
				UseShellExecute = false,
				RedirectStandardOutput = true
			});
			exePath = process?.StandardOutput.ReadToEnd().TrimEnd();

			return !string.IsNullOrWhiteSpace(exePath);
		}

		private static void Log(PluginInitContext context, string message, [CallerMemberName] string method = "")
		{
			context.API.LogInfo($"{Constants.PluginName}.{nameof(QuickSetup)}", message, method);
		}
	}
}