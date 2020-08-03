# KuruTools

This repository contains the following tools for the Kururin GBA ROMs:

- `KuruRomExtractor`: A console application that can extract and decode map data and tiles from the ROM. It can also replace the maps of the ROM with your modified maps.
- `KuruLevelEditor`: A GUI level editor. Depends on `KuruRomExtractor`.
- `Misc`: Some additional ROM patches.

The supported ROMs are:
- Kuru Kuru Kururin (Europe)
- Kururin Paradise (Japan)

## Installation

You can simply download the binaries under the `Release` section,
or build it from source (tested on Visual Studio 2019).

Prerequisite:
- Windows, Linux or macOS
- .NET core 3.1
- A supported ROM (format: `.gba`)

See this link to install .NET core: https://dotnet.microsoft.com/download

## Configuration

If you want to use the level editor:

- The rom extractor binaries must be placed in the same directory as the level editor,
with the name `KuruRomExtractor.dll`
- The *Kuru Kuru Kururin* or *Kururin Paradise* ROM must be placed in the working directory (most of time, it is the same directory as the level editor),
with the name `input.gba`
- The patched ROM will be saved in the working directory, with the name `output.gba`

These settings can be changed by editing the file `config.ini`.

If you want to be able to test your levels by clicking on the corresponding buttons in the level editor, you must configure the command that runs the emulator in `config.ini`.

## Running the level editor

You can run the level editor by opening `KuruLevelEditor.exe` (Windows)
or with the command `dotnet KuruLevelEditor.dll` (Windows, Linux, macOS).

Some notes about the level editor:

- Depending on the input ROM, the *Kuru Kuru Kururin* or *Kururin Paradise* editor will be loaded.
- When running it for the first time, a new directory `tiles` will be created. It contains all the tiles of the game. If the level editor exits as soon as you open it, please ensure the ROM is present (see previous section).
- The level editor saves your levels under the directory `levels`. This is the directory that you should copy if you want to backup your levels.
- The level editor saves the patched ROM as `output.gba` (by default). You should not modify this file manually as it will be overriden each time you click on the `Build` button. If you want to apply custom patches to the ROM, you should apply them to `input.gba` (the patches will be replicated on `output.gba`).

## License and contributions

Made by Mickael Laurent (E-Sh4rk).

You can freely use, share and modify these tools, as long as you give me some credits
(you can mention my name, username, or a link to this repository).

If you have any question or if you find any issue, please feel free to open a ticket under the `Issues` tab.

Any contribution is very welcome. Please feel free to fork this repository and submit a pull request.

Enjoy :)
