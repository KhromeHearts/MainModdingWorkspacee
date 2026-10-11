# PossessLib design

PossessLib is a library mod for WorldBox and NeoModLoader (NML). When you possess a unit you play
it directly: you get a Terraria-style hotbar, inventory, equipment column and HUD. Other mods
(SurvivalBox, a biome-ambience mod and so on) build on it through `PossessLib.Api`.

This file covers the architecture, the data model, how PossessLib stays in sync with the game,
how it saves, the public API, the Harmony patches, and every assumption I could not check
against the decompiled source. Section 10 lists the extras I added beyond the brief.

---

## 1. Ground rules

1. **The game owns the truth wherever it has a concept.** Equipped gear lives in `ActorEquipment`.
   Carried resources live in the `ActorBag` (`actor.data.inventory`). Money lives in
   `actor.data.money`. PossessLib only owns what the game has no concept of: the slot layout,
   unequipped items the unit carries, the hotbar selection, the trash slot and items on the ground.
2. **Prefer polling over patching.** Possession start and end, death, world reloads, attacks and level-ups are all
   detected by comparing cheap values once per frame (or on a slow tick). There are 8 Harmony
   patches in total (section 8), all postfixes or non-skipping prefixes.
3. **No per-frame allocation and no `FindObjectOfType`.** Views only redraw when an
   `Inventory.Version` counter changes. References are cached once.
4. **No collisions with the old mod.** PossessLib uses the namespace `PossessLib`, the Harmony ID
   `com.raytracerx.possesslib`, the GUID `RAYTRACERX_POSSESSLIB`, save keys prefixed with
   `possesslib.` and the resource folder `ui/possesslib/`. It can be installed alongside
   MainCharacterMod. Both mods will draw a HUD if both are enabled, so turn one off when you
   want a clean comparison.

## 2. File layout

```
PossessLib/
  mod.json, icon.png, default_config.json, README.md
  docs/DESIGN.md                 this file
  Locales/en.json                UI strings and config labels
  Data/food.json                 eating table
  Data/gathering.json            gathering rules per building type
  Data/recipes.json              crafting recipes (plus auto-recipes from the game's item costs)
  GameResources/ui/possesslib/   sprites (gold health frame and slot frame reused from MainCharacterMod)
  Code/
    PossessLibMod.cs             NML entry point (BasicMod), patch bootstrap, runtime object
    Core/      config, logging, localisation, JSON data tables, key parsing, runtime driver
    Model/     ItemStack, Inventory, SlotAddress, custom item and equipment slot registries
    Sync/      InventoryRegistry (actor -> inventory and item protection), ActorSync, InventoryOps
    Persistence/ InventorySerializer (versioned DTOs), WorldPersistence (save flush and load relink)
    Gameplay/  PossessionController, HandItem, Consumables, Gathering, Crafting, GroundItems, MultiWeaponAI
    UI/        HUD root, slot widget, hotbar, inventory, equipment, health/XP, statuses, crafting,
               cursor stack, toasts, unit inventory window, tooltips
    Patches/   one file per Harmony patch, each with a comment saying why it exists
    Api/       PossessLibApi (functions), PossessLibEvents (events), public definition types
    Debug/     SelfTest (in-game automated test, off by default)
```

## 3. Data model

```
ItemStack (class)
  Kind   : Item | Resource | Custom | Unresolved
  Item   : the game's Item object (weapons, armor, accessories). Count is always 1.
  Id     : resource id, custom item id, or (for Kind=Item) the asset id, cached for display
  Count  : stack size
  Data   : optional Dictionary<string,string> payload for custom items from other mods
```

* **Every item type the game has is covered.** `EquipmentType` has six values: Weapon, Helmet,
  Armor, Boots, Ring and Amulet. All six are `EquipmentAsset` items, and the game stores them as
  `Item` objects in `World.world.items`. Every `ResourceAsset` (food, minerals, gold and so on) is
  a stackable `Resource`. There are no other carryable item classes in the game. Mod items are
  `Custom` stacks, registered through the API.
* An `Item` keeps its modifiers, quality, durability, kills and name because PossessLib never
  copies an item. It holds the same `Item` reference the game uses.
* `Unresolved` holds a custom item whose mod is not loaded right now. It is kept as raw data, so
  disabling a mod for one session does not delete its items, and it comes back when the mod is
  re-enabled.

```
Inventory (class), one per actor that has PossessLib data
  slots[ HotbarSize + MainSize ]   hotbar first, then the main grid (null = empty)
  Selected                         selected hotbar index
  Cursor                           the stack held on the mouse while editing (Terraria's "mouse item")
  Trash                            Terraria-style trash slot. Recoverable until something replaces it.
  CustomEquipment[slotId]          contents of extra equipment slots registered by other mods
  Version                          bumped on every change. The UI redraws only when it changes.
```

Inventories are created lazily, only for units that are possessed, opened in the unit window, or
touched through the API. Ordinary AI units carry no PossessLib data, so there is no cost per unit.

**Slot counts.** The default is a 10-slot hotbar plus 3 rows of 10 (40 slots). The config allows
6 to 10 hotbar slots and 1 to 5 rows. Why 10 and 3 rows:

* Ten slots map one-to-one onto the number keys 1 to 0, as in Terraria.
* With multi-weapon carrying (section 6.4), six slots run out quickly.
* WorldBox's main UI canvas is only about 500 units tall. Terraria's 4 storage rows plus crafting
  would cover most of the map, and 3 rows keep the open inventory inside the top-left third of the
  screen.

## 4. Keeping the game in sync

### 4.1 Resources: the ActorBag holds the totals, PossessLib holds the layout
* The total of each resource across the slots and the cursor always equals
  `actor.data.inventory.getResource(id)`.
* When PossessLib itself changes a resource (trash, drop, eat, craft, gather, split or merge), it
  updates the bag and the slots together, so the reconcile step finds nothing to fix.
* When the game changes the bag (kill loot, AI unloading resources at a stockpile, stealing,
  another mod), `ActorSync.ReconcileResources` notices the difference. It adds the surplus to slots
  (topping up existing stacks first) or removes the shortfall, starting from the last main slot.
  For the possessed unit this runs every 0.2 s. For other units it runs when their inventory is
  shown.
* If the bag holds more than the slots can fit, the extra stays in the bag (so the game stays
  correct) and the HUD shows an "overflow" hint. It moves into slots as soon as space frees up.

**Why:** the bag is what the game's AI, unit window, kill loot, saving and stockpile unloading
read. If PossessLib kept its own copy instead, the two would drift apart.

### 4.2 Armor and accessories: ActorEquipment holds the truth
The equipment column shows `actor.equipment.getSlot(type)` directly. It is a view, not a copy.
Equipping or unequipping calls the game's `ActorEquipmentSlot.setItem` or `takeAwayItem`, then
`setStatsDirty()`, so the game recalculates stats.

### 4.3 Weapon: the selected hotbar slot decides
Invariant for the **possessed** unit:
`equipment.weapon` = the selected hotbar stack if that stack is a weapon, otherwise empty.
The weapon `Item` is the same reference in both places. Selecting food therefore puts the weapon
away (the unit punches), the same way Terraria works.

**The cursed-weapon bug from the old mod.** The old mod skipped syncing whenever
`canChangeSlot()` was false, so the HUD and the hand disagreed. PossessLib keeps the game's rule
(a cursed item cannot be removed) and makes the HUD tell the truth:
* The cursed weapon stays in the hand. Its slot gets a red "in hand" marker, and the hand-render
  override switches off, so the sprite you see is the weapon you are using.
* Selecting a different weapon is refused with a toast ("Cursed! You can't let go of X"). Selecting
  food or tools is still allowed, so you can eat while cursed.
* Once the curse is gone (reforge, or another mod removes it), normal syncing resumes on the next
  frame.

Other weapon changes the game makes on its own (steal or take-items, god powers, other mods) are
caught in two places:
1. The prefix on `ActorEquipmentSlot.setItem` stores the item being displaced into the inventory
   when the unit has PossessLib data.
2. The per-frame weapon check notices a weapon that is equipped but not in the inventory, and
   imports it into the selected slot, or the first free slot.

### 4.4 Item lifetime (why items never disappear)
`ItemManager.checkDeadObjects` deletes any item with no actor and no city (`Item.isReadyForRemoval`).
Unequipped items carried in an inventory, the trash slot and items on the ground have no owner as
far as the game knows. A postfix on `isReadyForRemoval` returns false for every item in
PossessLib's protection set, a `HashSet<Item>` that is updated on every add and remove. PossessLib
also calls `item.setUnitHasIt(owner)` on carried items, so `getActor()` still points at the
carrier.

### 4.5 Death
A slow tick (every 0.5 s) checks the owners of registered inventories. When an owner is dead:
* Unequipped `Item`s fall to the ground at the last known position (when ground items are on, the
  default). Otherwise they go to the zone's city, the same rule the game uses for equipment, or
  they become ownerless and the game removes them.
* Equipped gear follows the game's normal death rules. PossessLib does not change those.
* The game handles resources (they are in the bag).
* The inventory is unregistered and its custom-data key is cleared.
* If the dead unit was possessed, polling sees `ControllableUnit.getControllableUnit()` change and
  the HUD shuts down cleanly (see 6.1).

## 5. Persistence

**Choice: the game's own custom-data containers. No sidecar file.**
* `actor.data.custom_data_string["possesslib.inventory"]` holds that unit's inventory as JSON.
  `BaseSystemData` custom data is part of `ActorData` and is saved inside the world file.
* `World.world.map_stats.custom_data.custom_data_string["possesslib.ground"]` holds items on the
  ground. `MapStats.custom_data` is a `SaveCustomData : BaseSystemData` and is saved with the
  world.

Why this beats a sidecar file keyed by world and unit id:
* Data cannot leak between worlds and cannot go stale. It lives inside the save it belongs to, so
  "save as", autosaves, cloud saves and copied save folders all just work.
* Unit ids are only unique within one world. A sidecar would need a reliable world identity, and
  WorldBox does not provide one.
* Deleting a save deletes its PossessLib data too.

Format (versioned, compact, and open to other mods' items):
```json
{"v":1,"h":10,"m":30,"sel":2,
 "s":[{"i":0,"k":"item","item":81234},
      {"i":1,"k":"res","id":"wood","n":40},
      {"i":5,"k":"custom","id":"survivalbox:boss_trophy","n":1,"d":{"boss":"wither"}}],
 "c":{"k":"res","id":"bread","n":3},
 "t":{"k":"item","item":81300},
 "eq":{"mymod:back":{"k":"custom","id":"mymod:backpack","n":1}}}
```
* Items are stored by `Item.data.id`. The game already saves every live item in `SavedMap.items`,
  and the protection postfix (4.4) keeps PossessLib's items alive, so they are in the save.
* **When it is written:** a prefix on `SavedMap.create`, the method that collects everything for a
  save, writes every registered inventory and the ground items. The same prefix removes stale keys
  from units PossessLib does not track. This covers clones: miracle-born clones copy the
  parent's custom data through `cloneCustomDataFrom`.
* **When it is read:** a postfix on `Actor.loadFromSave`, which `ActorManager.loadObject` calls for
  each actor after `equipment.load`. Items are already loaded at that point ("Loading Items" runs
  before "Finish Loading Actors" in `SaveManager`). Each item is claimed by at most one inventory,
  and the first claim wins. Ground items load at the same moment, on the first actor or at
  `MapBox.on_world_loaded` if the world has no units.
* **World switches:** `MapBox.current_world_seed_id` goes up by one in every `clearWorld()`.
  `InventoryRegistry.EnsureWorld()` compares it and clears all in-memory state when it changes, so
  nothing carries over to the next world.
* Resource stacks are saved for their layout only. After loading, the bag is the authority (4.1).

## 6. Gameplay systems

### 6.1 Possession lifecycle (`PossessionController`, polled every frame)
* **Start:** `ControllableUnit.getControllableUnit()` changes from null to a unit. PossessLib then:
  * Builds the HUD lazily, on the first possession, once `CanvasMain.instance` exists.
  * Zooms the camera to the configured level (default 11, the old mod's value) and remembers the
    previous zoom.
  * Hides the native key-tips panel ("Right") and the native health bar inside
    `PossessionUnitInfo`. Your restyled bar replaces it.
  * Sets `ignore_mod_keys` on the up, down, left and right hotkeys, so you can move while holding
    Shift or Ctrl. This is the old mod's hotkey fix.
  * Disables wheel zoom while possessing, so the wheel scrolls the hotbar. Ctrl+wheel still zooms.
  * Imports the unit's current weapon into the hotbar.
* **Switch:** the main unit changes. This happens when it dies while you control a group, because
  `ControllableUnit.remove` picks a new random main unit. The HUD rebinds to the new unit.
* **End:** the controllable unit becomes null (released, dead, or the world was cleared). The
  controller closes the inventory, returns the cursor stack to the inventory or the ground, restores
  the hotkeys, zoom, native tips and health bar, hides the HUD, and equips the best carried weapon
  for the AI.

### 6.2 Hand item (`HandItem` and a postfix on `Actor.getHandRendererAsset`)
The game renders hand items inside a `Parallel.For`, so the postfix runs on worker threads. The
main thread works out one `(actor, IHandRenderer)` pair whenever the selection changes. The
postfix only reads those two static references, so it is thread-safe. It overrides the result
only when the selected stack can be drawn (it has `getSprites()[0]`). Otherwise the game's own
choice is kept, which avoids a null `IHandRenderer` that would crash the renderer.
`checkHasRenderedItem()` and `getRenderedItemSprite()` are marked `AggressiveInlining`, so patching
them would be unreliable. PossessLib never patches them.

### 6.3 Using items (eating and custom uses)
With food (or a custom item that has a use callback) selected, left-click uses it instead of
attacking. The `updateControllableUnit` postfix clears the attack flags for that frame. Eating
calls the game's own `Actor.consumeFoodResource`, which applies nutrition, health percent,
favourite food, and trait or status rolls. `Data/food.json` adds a flat or percent bonus heal and
a cooldown per food.

### 6.4 Multiple weapons
A unit carries any number of weapons in its hotbar or inventory and switches between them with
the number keys or the scroll wheel. That is the Terraria model: one weapon is active in the hand
at a time. True simultaneous dual-wielding is not possible, because the game's combat reads exactly
one `equipment.weapon`. Faking it would mean rewriting combat, which is out of scope. AI-controlled
units with PossessLib data use their extra weapons too (`MultiWeaponAI`, extra):
* When their weapon is replaced by looting, the old weapon is kept instead of destroyed.
* About once a second, a unit that has an attack target switches to a ranged weapon when the
  target is out of melee reach, and back to melee when the target is close.
* When possession ends, the unit equips its best weapon.

### 6.5 Gathering (`Gathering`, refined from ResourceMining.cs)
* Triggered by the possessed unit's real attack. Polling `Actor._possession_attack_happened_frame`
  catches each left-click swing, so the game's own attack animation and cooldown pace the
  gathering, and there is no duplicate input loop.
* The target is the building under the cursor, or a neighbouring tile, within reach.
* `Data/gathering.json` sets, for each `BuildingType`: the hits needed, the faster tool groups, and
  the hits needed with a tool. Each hit shakes the building. The last hit calls the game's
  `building.extractResources(actor)` and gives `resources_given`, matching
  `BehExtractResourcesFromBuilding`, including the miner trait bonus.
* Fixed from the old mod: WorldBox has no pickaxe *items* (pickaxes are task hand-tools only), so
  the old "requires pickaxe" check made mining impossible. Tools now speed gathering up rather than
  being required. Axes are faster on trees and hammers are faster on minerals.

### 6.6 Crafting (`Crafting`)
* **Auto-recipes come from the game's own item costs.** Any `EquipmentAsset` in the configured
  groups that is unlocked (`isAvailable()`) and has `cost_resource_id_1/2` or a gold cost gets a
  recipe for those resources plus `get_total_cost` coins. The result is made with
  `items.generateItem(...)`, so it rolls modifiers like the game's blacksmiths. This covers armor
  and tools without hand-writing about 200 recipes.
* `Data/recipes.json` adds or overrides recipes. Optional `near_building` and `near_building_type`
  fields require a crafting station nearby. Mods add recipes with `PossessLibApi.RegisterRecipe`.

### 6.7 Ground items (`GroundItems`)
* WorldBox has no item-on-ground entities, so PossessLib adds a small one. Ways to drop: click the
  world while holding a stack on the cursor, release a drag outside any UI, or press the drop key.
* Drawing uses the game's own `QuantumSpriteAsset` system (copied from the `draw_parabolic_unload`
  asset the game uses for thrown resources). The quantum sprite asset is registered at load time.
* The possessed unit picks items up automatically within 1.5 tiles, after a short delay so you
  don't instantly re-pick what you just dropped. Items despawn after a configurable number of
  world-minutes (favourite items never despawn), with a cap of 300 stacks.
* Saved in `map_stats.custom_data` (section 5).

### 6.8 Coins
The coin readout shows `actor.data.money`, the money units really spend on food and crafting.
Gold is a normal stackable resource. There is no separate coin item, because WorldBox's money is a
number, not an item, and a fake coin item would split one value across two places.

## 7. UI

* Everything is parented to `CanvasMain.instance.canvas_ui`. That canvas uses the game's
  `CanvasScaler` (ScaleWithScreenSize, reference height 500 multiplied by the player's "UI size"
  setting), so the HUD keeps its size at every resolution and follows the game's UI-size option.
  Each HUD group is anchored to a screen corner and scaled by the `pl_ui_scale` config value.
  Positions are offsets from that corner, computed from the slot-size constants in `UIConst`.
  There are no absolute pixel positions.
* The cursor stack is drawn on `canvas_tooltip`, so it shows above windows. The unit inventory
  window is a real `ScrollWindow` (NML `AbstractWindow`), so it gets the game's frame, title, close
  button and back navigation.
* The font is `LocalizedTextManager.current_font` (the game's pixel font). Panels use
  `ui/special/windowInnerSliced`. Icons come from the game (`iconArmor`, `iconLevels`,
  `iconMoney` and so on). The gold health frame and the slot frame are your MainCharacterMod sprites.
* Item tooltips use the game's own `"equipment"` tooltip with `TooltipData.item` (stats,
  modifiers, durability, rarity colour, history). Resources and custom items use PossessLib's
  own tooltip assets, registered in `AssetManager.tooltips`.
* Each slot shows: the icon (`asset.getSprite()` for items, `getSpriteIcon()` for resources), the
  count, the hotbar number, a frame tinted by rarity, a durability bar when the item is damaged,
  and a red marker when the item is cursed and in the hand. Fixed from the old mod: icons are
  bound from the data on every version change, the icon `Image` is enabled with
  `preserveAspect` set, slots are positioned by explicit anchors (no layout groups to collapse
  them), and the HUD is built only after the real UI canvas exists.

**Controls (defaults, rebindable in the config):**

| Action | Input |
|---|---|
| Open or close the inventory | `I` |
| Select hotbar slot | `1` to `0`, or the mouse wheel (Ctrl+wheel zooms) |
| Pick up or place a stack | Left click, or drag and drop |
| Take half a stack | Right click on a stack |
| Place one item | Right click while holding a matching stack |
| Equip or unequip | Right click on armor or an accessory, or drag it to its slot |
| Quick move (hotbar and main) or quick equip | Shift + left click |
| Send to trash | Ctrl + left click, or drop onto the trash slot |
| Drop on the ground | Click the world while holding a stack, drag outside the UI, or press `X` (drops the hovered or selected stack). `Q` is the game's steal key, so it is not used. |
| Use or eat the selected item | Left click on the world |

Slot clicks and Shift while the pointer is over PossessLib UI don't attack or dash.

## 8. Harmony patches (all of them)

| # | Target | Kind | Why |
|---|---|---|---|
| 1 | `Actor.getHandRendererAsset` | postfix | Shows the selected hotbar item in the hand (6.2). Thread-safe. |
| 2 | `ControllableUnit.updateControllableUnit` | postfix | Clears attack and action flags while the pointer is over PossessLib UI or a stack is on the cursor. Uses consumables on left click. Drops the cursor stack on the world. |
| 3 | `Item.isReadyForRemoval` | postfix | Keeps carried, trashed and ground items alive (4.4). |
| 4 | `SavedMap.create` | prefix (never skips) | Writes inventories and ground items into custom data before the game serializes them (5). |
| 5 | `Actor.loadFromSave` | postfix | Rebuilds the inventory from custom data at the same moment the game relinks equipment (5). |
| 6 | `ActorEquipmentSlot.setItem` | prefix (never skips) | Stores items the game displaces into the inventory instead of losing them (4.3, 6.4). Does nothing for units without PossessLib data. |
| 7 | `Actor.updateStats` | prefix and postfix | Adds `BaseStats` from items in extra equipment slots registered by other mods. Fast path: returns at once when no such slot has stats. |
| 8 | `UnitWindow.OnEnable` | postfix | Adds the "Inventory" button to the unit window. |

Everything else (possession start and end, HUD visibility, native HUD hiding, hotkey changes,
death, world change, gathering, level-up) is polling or plain field changes.
`PossessLibMod` applies each patch class separately and catches errors, so if a game update breaks
one patch, it logs an error and the rest of the mod keeps working.

## 9. Public API (`PossessLib.Api`)

* `PossessLibApi.Version` ("0.1.0"), `PossessLibApi.ApiVersion` (an int, bumped on breaking
  changes), and `PossessLibApi.IsApiAtLeast(int)`.
* Possession: `GetPossessedActor()`, `IsPossessing`, `IsPossessed(Actor)`.
* Inventory: `GetInventory(actor, create)`, `AddItem(actor, Item)`,
  `AddResource(actor, id, n)`, `RemoveResource(actor, id, n)`, `CountResource(actor, id)`,
  `AddCustomItem(actor, id, n, data)`, `RemoveCustomItem(actor, id, n)`,
  `CountCustomItem(actor, id)`, `OpenInventory()`, `CloseInventory()`, `IsInventoryOpen`.
* Registration: `RegisterCustomItem(CustomItemDefinition)`, `RegisterRecipe(RecipeDefinition)`,
  `RegisterEquipmentSlot(EquipmentSlotDefinition)`, `RegisterHudElement(HudElementDefinition)`,
  `RegisterInventoryPanel(InventoryPanelDefinition)`.
* World: `DropOnGround(stack, position)` and `ShowToast(text)`.
* Camera (`PossessLibApi.Camera`): `SetZoom(float, instant)`, `GetZoom()`, `ResetZoom()`,
  `ZoomLimits`. These are thin wrappers over `MoveCamera.setTargetZoom` and `forceZoom` that
  clamp to the game's limits and remember the zoom from before possession.
* Events (`PossessLibEvents`): `UnitPossessed`, `UnitReleased`, `InventoryOpened`,
  `InventoryClosed`, `ItemAdded`, `ItemRemoved`, `ItemMoved`, `ItemUsed`, `HotbarSlotSelected`,
  `EquipmentChanged`, `ItemDropped`, `ItemPickedUp`, `ItemCrafted`, `PossessedLevelUp`. WorldBox has
  none of these as hooks: equipment and bag changes are plain field writes, and possession only
  toggles a UI.
* Event handlers are wrapped in try/catch, so a broken handler in another mod logs an error and
  does not break PossessLib.

**Already in the game or NML, so reused rather than rewritten:** `BasicMod`, `ModConfig`, `LM`,
`AbstractWindow`/`WindowCreator`, `SimpleButton`, `MoveCamera.setTargetZoom`, the game's
`Tooltip` system, `consumeFoodResource`, `extractResources`, `generateItem`, `QuantumSprite`
rendering and `MusicBox.playSoundUI`.

**Depending on PossessLib from another mod** (see `Mods/PossessLibExample`):
```json
{ "name": "My Mod", "author": "Me", "version": "1.0.0", "GUID": "ME_MYMOD",
  "Dependencies": ["RAYTRACERX_POSSESSLIB"] }
```
NML compiles PossessLib first and adds its DLL as a reference. For an optional dependency, use
`"OptionalDependencies"` and wrap calls in `#if RAYTRACERX_POSSESSLIB`. NML defines that symbol
when PossessLib is present.

## 10. Extras (small, optional, beyond the brief)
1. **Terraria-style trash slot**, recoverable until something replaces it.
2. **Sort button**: stacks and sorts the main grid. It leaves the hotbar alone.
3. **Durability bar and rarity tint** on slots.
4. **Level-up popup**, plus an XP bar under the health bar.
5. **AI weapon switching** for units that carry several weapons (6.4). Toggle: `pl_ai_weapon_switch`.
6. **Overflow hint** when the bag holds more than the slots can fit.
7. **In-game self-test** (`Debug/SelfTest.cs`). It runs only when the `selftest.flag` file is in
   the mod folder, and logs PASS or FAIL lines to `Player.log`.

## 11. Assumptions I could not verify from the code

* **Cross-mod type references at runtime.** NML loads mods with `Assembly.Load(byte[])` and
  registers no `AssemblyResolve` handler. I assume Mono resolves the dependent mod's reference to
  `RAYTRACERX_POSSESSLIB` from the assemblies already loaded. Other NML mods rely on this too.
  `PossessLibExample` checks it.
* **Sprite sizes and import settings.** NML imports PNGs with PixelsPerUnit 1 and no border, so the
  gold frame is drawn as a `Simple` (stretched) image at its native aspect ratio, not as a 9-slice.
* **Ground-item drawing.** The `p_mapSprite` prefab, z = 4 and the scale formula are copied from
  the game's thrown-resource renderer. Whether the sprites look right on screen still has to be
  checked.
* **The position of the unit-window button.** The button is anchored to the right edge of the
  window's `Background`. The prefab layout is not in the code, so it may need nudging.
* **The native HUD names.** `PossessionUI` contains a child named "Right" (the key tips). The old
  mod used this name and it worked. The health bar is the `StatBar` inside `PossessionUnitInfo`.
* **Whether QuantumSprite systems are already initialised at mod load.** The code handles both
  orders.
* **Esc while the inventory is open.** The game's own `cancel` hotkey may also act on Esc during
  possession, so PossessLib does not bind Esc. Use `I` to close.

## 12. What was checked in the running game

The self-test ran 7 times inside your real game, with all your other mods loaded. The last three
runs passed **45 of 45** checks.

**Confirmed:**

* **Loading:** NML compiles and loads PossessLib, all 8 patches apply, and the example mod loads
  and calls the API (cross-mod references work).
* **Possession:** possession start, release and death are detected. The HUD is built, then hidden
  again on release or death.
* **Icons:** items show their icons in the slots. Window captures show the herb pouch, wood ×27,
  weapons, bread and helmet in their slots.
* **Resources:** the bag stays in sync with the slots through gain, split, merge, trash, take-back
  and drop.
* **Weapons and gear:** the selected weapon is equipped, and selecting a non-weapon empties the
  hand. A cursed weapon cannot be swapped for another weapon, stays in the hand and is reported
  on the HUD. Helmets equip and unequip.
* **Saving:** the custom-data round trip works, and so does a real save to disk followed by a load
  through the game's own loader.
* **Gameplay:** eating heals. Ground drops are picked back up. Crafting works from the data file
  (118 recipes in total). Custom items work through the API. When a unit dies, its items fall to
  the ground and its inventory is released.
* **Unit window:** the Inventory button is added to the unit window and opens the PossessLib window
  with its slots.
* **Native HUD:** the native health bar and key tips are hidden.

**Not covered by the test:**

* Gathering: it needs a real mouse target.
* The hand-item sprite and ground-item sprites: the code paths run, but I could not confirm them
  in a screenshot.
* Drag and drop by real mouse.
* AI weapon switching.
* Level-up popup.
* Changing config values at runtime.

**Known conflict:** **MainCharacterMod** must be *disabled* while PossessLib is enabled. Its
`MCInventory.SyncSelectedWeaponToActor` takes the weapon out of the hand every frame, and its HUD
overlaps PossessLib's. PossessLib logs a warning and shows a toast when both are enabled.
Avb's Expanded Armory replaces `ActorEquipmentSlot.setItem`, so PossessLib's prefix on that method
runs with `Priority.First`.
