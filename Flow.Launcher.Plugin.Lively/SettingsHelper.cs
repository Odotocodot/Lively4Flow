using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.Lively
{
	public static class SettingsHelper
	{
		private enum InstallType
		{
			None,
			GitHub,
			MicrosoftStore
		}


		public static void ForceSetup(Settings settings, PluginInitContext context)
		{
			settings.RunSetup = false;
			Setup(settings, context);
		}

		public static void Setup(Settings settings, PluginInitContext context)
		{
			if (settings.RunSetup) //TODO: if InstallType == None also run
				return;
			Log(context, "Starting Setup");

			InstallType installType = GetInstallLocation(context, out var exePath);

			string baseStoragePath;
			switch (installType)
			{
				case InstallType.GitHub:
					Log(context, $"Lively exe [GitHub Version] was found at: \"{exePath}\"");
					baseStoragePath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"Lively Wallpaper");
					break;
				case InstallType.MicrosoftStore:
					Log(context, $"Lively exe [Microsoft Store Version] was found at: \"{exePath}\"");
					baseStoragePath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						@"Packages\12030rocksdanister.LivelyWallpaper_97hta09mmv6hy\LocalCache\Local\Lively Wallpaper");
					break;
				default:
				case InstallType.None:
					Log(context, "No exe was NOT found, exiting quick setup.");
					//TODO: tell the user
					return;
			}

			settings.LivelyExePath = exePath;

			if (FindLivelySettings(context, baseStoragePath, out var settingsPath))
				settings.LivelySettingsJsonPath = settingsPath;

			if (FindLivelyLibraryFolder(context, baseStoragePath, out var libraryPath))
				settings.LivelyLibraryFolderPath = libraryPath;

			Log(context, "Finished quick setup");
			settings.RunSetup = true;
		}

		private static InstallType GetInstallLocation(PluginInitContext context, out string exePath)
		{
			Log(context, "Looking for Lively exe [GitHub Version]");
			var installType = InstallType.None;
			if (FindLivelyGitHub(out exePath))
			{
				installType = InstallType.GitHub;
			}
			else
			{
				Log(context, "Lively exe [GitHub Version] was NOT found");
				Log(context, "Looking for Lively exe [Microsoft Store Version]");
				if (FindLivelyMSStore(out exePath))
					installType = InstallType.MicrosoftStore;
				else
					Log(context, "Lively exe [Microsoft Store Version] was NOT found");
			}

			return installType;
		}


		private static bool FindLivelySettings(PluginInitContext context, string baseStoragePath,
			out string settingsPath)
		{
			Log(context, "Looking for Settings.json");
			settingsPath = Path.Combine(baseStoragePath, "Settings.json");
			if (File.Exists(settingsPath))
			{
				Log(context, $"Settings.json was found at: \"{settingsPath}\"");
				return true;
			}

			Log(context, "Settings.json was NOT found");
			return false;
		}

		private static bool FindLivelyLibraryFolder(PluginInitContext context, string baseStoragePath,
			out string libraryPath)
		{
			Log(context, "Looking for Lively Library Folder");
			libraryPath = Path.Combine(baseStoragePath, "Library");
			if (Directory.Exists(libraryPath) && Directory.Exists(Path.Combine(libraryPath, "wallpapers")))
			{
				Log(context, $"Lively Library Folder was found at: \"{libraryPath}\"");
				return true;
			}

			Log(context, "Lively Library Folder was NOT found");
			return false;
		}

		private static bool FindLivelyGitHub(out string exePath)
		{
			exePath = null;
			var baseRegistryKeys = new RegistryKey[] { Registry.CurrentUser, Registry.LocalMachine };
			var initialSubKeys = new string[]
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
				if (key != null)
					foreach (var name in key.GetSubKeyNames())
					{
						using RegistryKey subKey = key.OpenSubKey(name)!;
						if (((string?)subKey.GetValue("DisplayName"))?.Contains("Lively Wallpaper") == true)
						{
							var installLocation = (string?)subKey.GetValue("InstallLocation");
							if (installLocation != null)
							{
								path = Path.Combine(installLocation, "Lively.exe");
								return true;
							}
							else
							{
								return false;
							}
						}
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
					"Get-AppxPackage -Name 12030rocksdanister.LivelyWallpaper | Select -ExpandProperty InstallLocation",
				UseShellExecute = false,
				RedirectStandardOutput = true
			});
			exePath = process?.StandardOutput.ReadToEnd();

			return !string.IsNullOrWhiteSpace(exePath);
		}

		private static void Log(PluginInitContext context, string message, [CallerMemberName] string method = "")
		{
			context.API.LogInfo("LivelyWallpaperController." + nameof(SettingsHelper), message, method);
		}
	}
}