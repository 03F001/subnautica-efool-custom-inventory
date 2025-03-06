# efool's Custom Inventory Mod for Subnautica

- Configure size of inventories:
	- Player inventory
	- Locker inventories (waterproof locker, wall locker, locker, lifepod locker, cyclops lockers)
	- Seamoth storage
	- Prawn suit storage (including additional size per storage module)
	- Scanner room upgrades
	- Bioreactor
	- Filtration machine (including salt/water ratio)
	- Carry-all bags
	- Trash can
	- Nuclear waste can
	- Planters (it don't make sense, but you can do it)
- Inventory sizes are per-save
- Preset inventory sizes
- Bulk inventory operations (switch, drop, eat, etc) modifier keys
	- `left shift` all items
	- `left ctrl` all items of selected tech type
	- Modifier keys are configurable
- Pin inventory items to prevent move, drop, eat, etc
	- `left alt` click item inside an inventory to toggle pin
	- Modifier key is configurable
	- Complements bulk operations
	- Bulk pin all items with `left alt` + `left shift`
	- Bulk pin items by tech type with `left alt` + `left ctrl`

# Requirements

- Subnautica Living Large Update 71288
- Tobey's BepInEx Pack v5
	- [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1108)
	- [Github](https://github.com/toebeann/BepInEx.Subnautica)
- Nautilus v1
	- [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1262)
	- [Github](https://github.com/SubnauticaModding/Nautilus)

# Installation

- Install Tobey's BepInEx Pack v5
- Install Nautilus v1
- Extract `efool-custom-inventory_#.#.#.zip` to `BepInEx/plugins`
	- `[game]/BepInEx/plugins/efool-custom-inventory/efool-custom-inventory.dll`
	- `[game]/BepInEx/plugins/efool-custom-inventory/presets.json`

Note: `[game]` is the directory containing `Subnautica.exe`

# Custom Presets

There is no UI option to modify presets. Manually modify `presets.json` at the installed location `[game]/BepInEx/plugins/efool-custom-inventory/presets.json` to add your own entry. See `efool's Hoarder Rules` as a reference.

# Console Commands

| Description                           | Command                            |
| ------------------------------------- | ---------------------------------- |
| Player inventory width                | set_inventory_width                |
| Player inventory height               | set_inventory_height               |
| Locker width                          | set_locker_width                   |
| Locker height                         | set_locker_height                  |
| Wall locker width                     | set_smalllocker_width              |
| Wall locker height                    | set_smalllocker_height             |
| Waterproof locker width               | set_smallstorage_width             |
| Waterproof locker height              | set_smallstorage_height            |
| Carry-all width                       | set_luggagebag_width               |
| Carry-all height                      | set_luggagebag_height              |
| Lifepod 5 storage width               | set_lifepodLocker_width            |
| Lifepod 5 storage height              | set_lifepodLocker_height           |
| Seamoth storage width                 | set_seamoth_width                  |
| Seamoth storage height                | set_seamoth_height                 |
| Prawn suit storage width              | set_exosuit_width                  |
| Prawn suit storage height             | set_exosuit_height                 |
| Prawn suit height per storage module  | set_exosuitStorageModule_height    |
| Cyclops storage locker default width  | set_cyclopsLocker_width            |
| Cyclops sotrage locker default height | set_cyclopsLocker_height           |
| Cyclops storage locker 1 width        | set_cyclopsLocker1_width           |
| Cyclops storage locker 1 height       | set_cyclopsLocker1_height          |
| Cyclops storage locker 2 width        | set_cyclopsLocker2_width           |
| Cyclops storage locker 2 height       | set_cyclopsLocker2_height          |
| Cyclops storage locker 3 width        | set_cyclopsLocker3_width           |
| Cyclops storage locker 3 height       | set_cyclopsLocker3_height          |
| Cyclops storage locker 4 width        | set_cyclopsLocker4_width           |
| Cyclops storage locker 4 height       | set_cyclopsLocker4_height          |
| Cyclops storage locker 5 width        | set_cyclopsLocker5_width           |
| Cyclops storage locker 5 height       | set_cyclopsLocker5_height          |
| Scanner room upgrades width           | set_maproomupgrades_width          |
| Scanner room upgrades height          | set_maproomupgrades_height         |
| Bioreactor width                      | set_basebioreactor_width           |
| Bioreactor height                     | set_basebioreactor_height          |
| Filtration machine max salt           | set_basefiltrationMachine_maxSalt  |
| Filtration machine max water          | set_basefiltrationMachine_maxWater |
| Nuclear waste disposal width          | set_labtrashcan_width              |
| Nuclear waste disposal height         | set_labtrashcan_height             |
| Trash can width                       | set_trashcans_width                |
| Trash can height                      | set_trashcans_height               |
| Basic plant pot width                 | set_planterpot_width               |
| Basic plant pot height                | set_planterpot_height              |
| Composite plant pot width             | set_planterpot2_width              |
| Composite plant pot height            | set_planterpot2_height             |
| Chic plant pot width                  | set_planterpot3_width              |
| Chic plant pot height                 | set_planterpot3_height             |
| Plant shelf width                     | set_plantershelf_width             |
| Plant shelf height                    | set_plantershelf_height            |
| Exterior growbed width                | set_farmingtray_width              |
| Exterior growbed height               | set_farmingtray_height             |
| Indoor growbed width                  | set_planterbox_width               |
| Indoor growbed height                 | set_planterbox_height              |
| Alien containment planter width       | set_planter_width                  |
| Alien containment planter height      | set_planter_height                 |

# Known Issues

- Decreasing inventory size can lose stored items
	- Wordaround: choose inventory sizes at the beginning of a save
- Planter inventory sizes have no effect
- Bulk item drop occludes PDA
	- Workaround: close PDA to allow items to move away or manually look elsewhere before opening PDA again
- Only tested on keyboard/mouse

# Other Mods

- efool's custom inventory mod replaces these popular mods:
	- [Advanced inventory](https://www.nexusmods.com/subnautica/mods/490)
	- [Inventory size](https://www.nexusmods.com/subnautica/mods/1206)
		- This mod supports dynamically recognized storages that efool's mod does not

- efool's custom inventory mod goes well with these mods:
	- [EasyCraft](https://www.nexusmods.com/subnautica/mods/24)
	- [More Modified Items (BepInEx)](https://www.nexusmods.com/subnautica/mods/398)
		- Scuba Manifold connects to all the tanks in your giant inventory