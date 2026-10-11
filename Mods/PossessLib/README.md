# PossessLib

Play the unit you possess. PossessLib turns WorldBox's possession mode into a small action RPG
with a Terraria-style hotbar, inventory, equipment column and HUD. It is also a **library**:
other mods (SurvivalBox, a biome-ambience mod and so on) build on its API.

* Mod GUID: `RAYTRACERX_POSSESSLIB` · version 0.1.0 · API level 1
* Requires: NeoModLoader (NML)
* Can be installed next to MainCharacterMod. They share no IDs, save keys or resource paths. If
  both are enabled you will see two HUDs, so disable one of them to compare.

## Install

1. Copy the `PossessLib` folder into `WorldBox/Mods/`.
2. (Optional) Copy `PossessLibExample` too. It is a tiny mod that depends on PossessLib and adds
   a Herb Pouch item, a Lucky Charm slot and a kill counter. Delete it whenever you like.
3. Start the game. NML compiles the mod on first launch. Compile errors, if any, appear in
   `%USERPROFILE%\AppData\LocalLow\mkarpenko\WorldBox\Player.log`.

## How to play

Possess a unit as usual (the possession power, or `G`). The HUD appears:

* **Top left:** the hotbar, with the selected item's name above it and status effects below it.
* **Top right:** the gold health bar, your level and XP, and your armor (defense).
* **Bottom:** the game's own unit bar (name, age, kills, level).

Press `I` to open the inventory. That shows the grid, coins, the trash slot, a sort button, the
crafting list and the equipment column (helmet, armor, boots, ring, amulet).

| Action | Input |
|---|---|
| Open or close the inventory | `I` (rebindable) |
| Select hotbar slot | `1` to `0`, the mouse wheel, or a click on the slot while the inventory is closed |
| Zoom while possessing | `Ctrl` + mouse wheel |
| Pick up or place a stack | Left click, or drag and drop |
| Take half a stack | Right click |
| Place one item | Right click while holding a stack |
| Equip or unequip gear | Right click, Shift + left click, or drag it to its slot |
| Quick move (hotbar and inventory) | Shift + left click |
| Trash | Ctrl + left click, or drop onto the trash slot |
| Drop on the ground | Click the world while holding a stack, drag outside the window, or press `X` |
| Eat food or use an item | Select it, then left click on the world |
| Attack, kick, dash, jump... | The game's normal possession controls |

What the game does with it:

* **Weapons:** the weapon in the selected hotbar slot is the one in your hand. Carry as many
  weapons as you like and switch with the number keys. A **cursed** weapon stays in your hand
  (that is the game's rule). Its slot gets a red marker and the HUD says so. You can still
  switch to food or tools, but not to another weapon.
* **Resources** are your unit's real carried resources, the same ones the game's AI unloads at
  stockpiles. If you carry more than the slots can hold, the extra shows as "+N carried".
* **Gathering:** attack a tree, bush, plant, hive or ore. A few hits harvest it. An axe makes
  trees faster and a hammer makes ore faster.
* **Crafting:** every unlocked weapon, armor piece and accessory can be crafted from the game's
  own costs (resources plus coins). More recipes can go in `Data/recipes.json`.
* **Ground items:** dropped items lie on the map and are picked up when you walk over them. They
  are saved with the world and despawn after a while (10 minutes by default).
* **Saving:** everything is stored inside the world save. Nothing is written to separate files,
  so nothing leaks between worlds.
* **Any unit:** open a unit's window and click the new **Inventory** button on its right edge to
  view or edit its inventory without possessing it.

## Config (NML mod settings)

Inventory and drop keys, mouse-wheel mode, HUD scale, hotbar size (6 to 10), inventory rows (1 to
5), resource stack size, possession zoom, and switches for eating, gathering, crafting, ground
items, dropping on death, AI weapon switching and debug logging.

The data tables live in `Data/` (`food.json`, `gathering.json`, `recipes.json`). Edit them while
the game is closed.

## For mod authors

Read `docs/DESIGN.md` (sections 5 and 9) and the example in `Mods/PossessLibExample`.
In short:

```json
"Dependencies": ["RAYTRACERX_POSSESSLIB"]
```

```csharp
using PossessLib.Api;
using PossessLib.Model;

PossessLibApi.RegisterCustomItem(new CustomItemDefinition { Id = "mymod:gem", Name = "Gem", IconPath = "ui/icons/mymod/gem" });
PossessLibApi.RegisterRecipe(new RecipeDefinition { Id = "mymod:gem", ResultKind = RecipeResultKind.Custom, ResultId = "mymod:gem",
    Resources = new Dictionary<string, int> { { "gems", 2 } } });
PossessLibEvents.UnitPossessed += actor => PossessLibApi.ShowToast("Hello " + actor.getName());
Actor me = PossessLibApi.GetPossessedActor();
PossessLibApi.AddResource(me, "wood", 10);
PossessLibApi.Camera.SetZoom(20f);
```

## Self-test (for development)

Create an empty file `selftest.flag` in this folder, start the game and wait about a minute.
`Player.log` will contain `[PossessLib]: [SelfTest] PASS/FAIL ...` lines and a summary. If the
file contains the word `quit`, the game closes when the test is done. Delete the flag file
afterwards: the test spawns and possesses a unit on the startup world.
