# StS2 Exporter
This is a mod for Slay the Spire 2 which adds a tool to export data for all its items (cards, relics potions, etc.) to text files, and render images of them to files.

It can also dump all textures from the game, including exporting each texture from atlases as individual image files.

Its output format is meant to be mostly compatible with [sts-exporter](https://github.com/OceanUwU/sts-exporter) for Slay the Spire 1, though some features are missing at the moment. Check the issues tab for missing features / progress on them.

# Running the mod (pre-workshop)
- *Please do not contact the game's devs if something modding-related isn't working, ask in the [Slay the Spire Discord](https://discord.gg/slaythespire)'s #sts2-modding channel instead.*
1. In steam, right click on Slay the Spire 2, click Properties, click Installed Files, click Browse
1. In the folder that just opened, create a folder called `mods` if it doesn't exist
1. Download the latest release from the [releases page](https://github.com/oceanuwu/sts2-exporter/releases)
1. Extract the contents of the release zip into your `mods` folder
1. Run the game
1. Click "Exporter" on the game's main menu
1. Tweak the settings to your needs and click "Export!"
1. Click "Open Folder" to view the output

# Compiling
1. Open `project.godot` with [Megadot 4.5.1-m.5](https://megadot.megacrit.com/)
1. Click Project > Export, select Windows Desktop, select Export PCK/ZIP, and export to `STS2Export.pck`
    - Alternatively, you can run `megadot --headless --export-pack "Windows Desktop" ./STS2Export.pck` in a command line
1. In a command line, run `dotnet build` in this project's folder
1. Before running the game, copy the following files to your Slay the Spire 2 `mods` folder:
    - `.godot/mono/temp/bin/STS2Export.dll`
    - `.godot/mono/temp/bin/Scriban.dll`
    - `STS2Export.pck`