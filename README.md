# Add A Species Description
A quick repo for a .NET tool that creates species description patches for objects.

Instead of using the generic object descriptions, a new species description will be added to all objects for the species of your choice. By default, the human description is copied as a base value.

This tool is targeted at adding descriptions for modded species; it will not replace existing descriptions.

## Usage
The tool should be executed from the Command Prompt (`cmd.exe`). There's no GUI.
```
AddADescription.exe asset_folder species_name output_folder
```

### asset_folder
Full path to unpacked game assets. All subdirectories will be scanned. You should **not** enter the `objects` subfolder here.

EG. `D:\Steam\SteamApps\common\Starbound\assets\unpacked`.

Files are only read, but they should not be locked while the application is running to prevent any issues or skipped files.

### species_name
The name of the species to add descriptions for

EG. `avali` => `"avaliDescription" : "desc"`

### output_folder
**Note:** Patch files will be created without checking if files already exist. It is highly recommended to use a new and empty folder to prevent the tool from overwriting existing files.  
Base folder to generate the object patches in. Folders are created to match the folder structure of the game assets.

EG. `D:\Steam\SteamApps\common\Starbound\mods\MyMod`.
