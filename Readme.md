<p align="center">
  <a href="https://flowlauncher.com">
    <img alt="flow launcher" src="docs/flow.png" width=12.5%>
  </a>
  <a href="https://www.rocksdanister.com/lively/">
    <img alt="lively wallpaper" src= "Flow.Launcher.Plugin.Lively/Images/icon.png" width=12.5%>
  </a>
</p>
<h1 align="center">Lively Wallpaper for Flow Launcher</h1>

A [Lively Wallpaper](https://www.rocksdanister.com/lively/) plugin for [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher), allowing for the ability to quickly change your wallpaper and more!

## Installation

Using Flow Launcher type:

```
pm install Lively Wallpaper Controller
```

The first time the plugin runs it will attempt to automatically find your Lively Wallpaper `Settings.json` file and installation type.

<details>
  <summary>Troubleshooting</summary>

If the plugin does not work out of the box, navigate to the plugin settings and make sure the `Settings.json` path and the type of installation is correct.

| Installation Type | Typical `Settings.json` Path                                                                                                |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------- |
| GitHub            | `%APPDATA%\Local\Lively Wallpaper\Settings.json`                                                                            |
| Microsoft Store   | `%APPDATA%\Local\Packages\12030rocksdanister.LivelyWallpaper_97hta09mmv6hy\LocalCache\Local\Lively Wallpaper\Settings.json` |

</details>

## Features

See all your lively wallpapers at a glance.

```
lw {your search}
```

<img alt="set wallpaper" src="docs/set_wallpaper.gif" width="640"/>

Use the preview panel (default shortcut: <kbd>F1</kbd>) to get a preview!

<img alt="preview panel gif of a gif" src="docs/preview_panel.gif" width="633"/>

For a multi monitor setup, you can quickly change the Lively wallpaper layout to `per monitor` with the `layout` command.

Then, when accessing the Flow Launcher context menu (default shortcut: <kbd>⇧ Shift</kbd> + <kbd>⏎ Enter</kbd>) on a wallpaper result you can choose a specific monitor to set that wallpaper on.

<img alt="wallpaper context menu" src="docs/context_menu.png" width="492"/>

### Commands

The plugin has a set of commands available so you don't have to navigate to Lively every time.

| Command Keyword | Action                                                                |
| --------------- | --------------------------------------------------------------------- |
| `setwp`         | Search and set wallpapers                                             |
| `random`        | Set a random wallpaper                                                |
| `closewp`       | Close a wallpaper                                                     |
| `volume`        | Set wallpaper volume                                                  |
| `layout`        | Change the layout of wallpapers (`per monitor`, `span` & `duplicate`) |
| `playback`      | Pause or play wallpaper playback                                      |
| `show`          | Unminimize the Lively Wallpaper application                           |
| `quit`          | Quit Lively Wallpaper                                                 |

<img alt="close wallpaper" src="docs/close_wallpaper.gif" width="640"/>

<img alt="set random wallpaper" src="docs/randomise_wallpaper.gif" width="640"/>

> [!IMPORTANT]
>
> Lively Wallpaper must be running (in the background) for the commands to function properly.

## Additional Information

This plugin comes with the [Lively Command Utility](https://github.com/rocksdanister/lively/releases/tag/v2.0.4.0) installed.
This allows easy use of the lively [command line](https://github.com/rocksdanister/lively/wiki/Command-Line-Controls) without needing the path to the Lively exe.
