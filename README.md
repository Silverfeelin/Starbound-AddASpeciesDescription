# Add A Species Description
A quick repo for a .NET tool that creates species description patches for objects and tiles.

A better tool with a GUI: http://community.playstarbound.com/threads/hey-race-modders-a-little-something-to-use-for-racial-descriptions.136135/

Instead of using the generic object and tile descriptions, a new species description will be added to all objects and tiles for the species of your choice. By default, the human description is copied as a base value. For objects that lack this value, you can choose whether to use the default description to still create patches or to skip these files.

This tool is targeted at adding descriptions for modded species; it will not replace existing descriptions.

### Example
A file containing the below descriptions:
```
"description" : "Embers dance away from the campfire, warming the air. The heat can be used for cooking.",
"humanDescription" : "I've always enjoyed watching the flames of a campfire dance.",
"hylotlDescription" : "A campfire, I enjoy the simplicity of flame-cooked food.",
```
Will create the following patch file:
```json
[
  {
    "op": "add",
    "path": "/avaliDescription",
    "value": "I've always enjoyed watching the flames of a campfire dance."
  }
]
```

## Usage
The tool should be executed from the Command Prompt (`cmd.exe`). You can also open the tool directly to input parameters one by one.
```
AddADescription.exe asset_folder species_name output_folder skip_missing
```

### asset_folder
Full path to unpacked game assets. All subdirectories will be scanned. You should **not** enter the `objects` or `tiles` subfolder here.

EG. `D:\Steam\SteamApps\common\Starbound\assets\unpacked`.

Files are only read, but they should not be locked while the application is running to prevent any issues or skipped files.

### species_name
The name of the species to add descriptions for.

EG. `avali` => `"avaliDescription" : "desc"`

### output_folder
Base folder to generate the object patches in. Folders are created to match the folder structure of the game assets.  
If patch files already exist, you may choose to overwrite them. I still recommend using a new and empty folder though.

EG. `D:\Steam\SteamApps\common\Starbound\mods\MyMod`.

### skip_missing
`true` or `false`.
Indicates whether objects that have no `humanDescription` should be ignored or not. If these files are not ignored, the default `description` will be used and patch files will still be created for the species description. If the files are ignored, no patch files will be created for these objects and tiles.
