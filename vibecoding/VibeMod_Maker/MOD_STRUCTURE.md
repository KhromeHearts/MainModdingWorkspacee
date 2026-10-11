# MOD_STRUCTURE

Everything needed to scaffold a WorldBox NML mod from zero. Patterns over explanations.

---

## File Structure

```
GAMEPATH/Mods/YourMod/
├── mod.json                  ← REQUIRED — mod metadata
├── ModClass.cs               ← REQUIRED — entry point
├── *.cs                      ← additional source files
├── YourMod.csproj            ← optional but recommended — IDE support
├── icon.png                  ← optional — mod icon
├── default_config.json       ← optional — user-configurable settings
├── Locales/                  ← optional — auto-loaded translations
│   ├── en.json               ← English
│   ├── cz.json               ← Simplified Chinese
│   └── lang.csv              ← multi-language CSV
└── GameResources/            ← optional — auto-loaded sprites/textures
    ├── sprites.json           ← sprite config (pivot, border, PPU)
    └── ui/icons/myIcon.png    ← placed as if in Unity Resources/
```

NML finds `mod.json` recursively and uses its parent as the mod folder.

---

## mod.json (Required)

```json
{
    "name": "My Mod",
    "author": "AuthorName",
    "version": "1.0.0",
    "description": "Short description",
    "iconPath": "icon.png",
    "GUID": "AUTHORNAME_MY_MOD",
    "Dependencies": [],
    "OptionalDependencies": []
}
```

**Rules:**
- If `GUID` is empty → NML auto-generates from `AUTHOR_MODNAME` (uppercased, ASCII special chars → underscores)
- `Dependencies` = hard deps (mod won't load if missing)
- `OptionalDependencies` = soft deps (NML adds dep ID as compile-time `#if` macro)

---

## Entry Point — Two Options

### Option A: BasicMod<T> (Recommended)

```csharp
using NeoModLoader.api;

namespace MyNamespace;

class MyMod : BasicMod<MyMod>
{
    protected override void OnModLoad()
    {
        // Your init code here
        // Locales/ folder loaded automatically before this
        // default_config.json loaded automatically before this
        LogInfo("Mod loaded!");
    }
}
```

**What BasicMod gives you for free:**
- `OnModLoad()` instead of raw `OnLoad()`
- `Instance` static property (singleton access)
- `LogInfo()` / `LogWarning()` / `LogError()` (prefixed with mod name)
- `GetDeclaration()` → `ModDeclare` (mod folder, GUID, etc.)
- `GetConfig()` → `ModConfig` (user settings)
- Auto-loads `Locales/` folder
- Auto-loads `default_config.json`
- Inherits `MonoBehaviour` — can use `Update()`, `Awake()`, etc.

### Option B: Raw IMod

```csharp
using UnityEngine;
using NeoModLoader.api;
using NeoModLoader.services;

namespace MyNamespace;

public class ModClass : MonoBehaviour, IMod
{
    private ModDeclare _declare;
    private GameObject _gameObject;

    public ModDeclare GetDeclaration() => _declare;
    public GameObject GetGameObject() => _gameObject;
    public string GetUrl() => "";

    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        _declare = pModDecl;
        _gameObject = pGameObject;
        LogService.LogInfo($"[{pModDecl.Name}]: Hello World!");
    }
}
```

**Call order:** `OnLoad` → `Awake` → `OnEnable` → `Start` → `Update`

---

## Optional Interfaces

| Interface | What it does | Required method |
|---|---|---|
| `IConfigurable` | Adds config button in Mods window | `ModConfig GetConfig()` |
| `ILocalizable` | Auto-loads locale files before `OnLoad` | `string GetLocaleFilesDirectory(ModDeclare)` |
| `IReloadable` | Adds reload button (requires `Config.isEditor=true`) | `void Reload()` |

BasicMod<T> already implements `IMod` + `IConfigurable` + `ILocalizable`.

---

## Asset Registration

### The Pattern
All assets use the same flow: **create object → set fields → add to library**.

### Traits
```csharp
// Create + register a new trait
ActorTrait trait = new ActorTrait();
trait.id = "my_custom_trait";
trait.path_icon = "ui/icons/myTraitIcon";
trait.base_stats[S.health] = 50f;           // +50 health
trait.base_stats[S.damage] = 10f;           // +10 damage
trait.action_special_effect = (target, tile) => {
    // periodic effect logic
    return false;
};
trait.special_effect_interval = 1f;         // every 1 second
AssetManager.traits.add(trait);

// Add to a trait group (makes it appear in UI)
// Use existing or create custom group
```

### Actor Types
```csharp
ActorAsset actor = AssetManager.actor_library.clone("human", "my_creature");
actor.base_stats[S.health] = 200f;
actor.base_stats[S.damage] = 25f;
actor.base_stats[S.speed] = 60f;
// actor is auto-added by clone()
```

### God Powers
```csharp
GodPower power = new GodPower();
power.id = "my_power";
power.name = "my_power";
power.rank = PowerRank.Rank0_free;
power.path_icon = "ui/icons/myPowerIcon";
power.click_power_action = (tile, pPowerID) => {
    World.world.units.spawnNewUnit("human", tile);
    return true;
};
AssetManager.powers.add(power);
```

### Items / Equipment (NML Helper)
NML provides `ItemAssetCreator` in `NeoModLoader.General.Game`:
```csharp
ItemAssetCreator.CreateMeleeWeapon("my_sword", base_stats: myStats);        // auto-added to AssetManager.items
ItemAssetCreator.CreateRangeWeapon("my_bow", projectile: "arrow");          // auto-added
ItemAssetCreator.CreateArmorOrAccessory("my_helm", EquipmentType.Helmet);   // auto-added
ItemAssetCreator.CreateWeaponMaterial("my_material", base_stats: matStats); // must manually add to AssetManager.items_material_weapon
ItemAssetCreator.CreateAndAddModifier("my_mod", mod_type: "sharp", mod_rank: 1, translation_key: "my_mod_name"); // auto-added
```

### Status Effects
```csharp
StatusAsset status = new StatusAsset();
status.id = "my_status";
status.duration = 10f;  // seconds
status.path_icon = "ui/icons/myStatusIcon";
// set stat modifiers, callbacks, etc.
AssetManager.status.add(status);
```

### Standard Library Pattern
Every `AssetLibrary<T>` supports:
```csharp
T asset = library.get("id");     // lookup
library.add(asset);              // register
library.list                     // all assets
T clone = library.clone("source_id", "new_id");  // some libraries support clone
```

---

## NML Native vs Harmony Required

### ⚠️ Critical: Internal Methods Need Harmony

Many game methods are `internal`, not `public`. **NML does not wrap them.** You need raw Harmony patches to call or intercept them.

| Method | Visibility | NML Wrapper? | What to do |
|---|---|---|---|
| `addStatusEffect(string)` | `internal` | ❌ No | **Direct call works** — confirmed via XMM mod |
| `hasStatus(string)` | `internal` | ❌ No | **Direct call works** — same as above |
| `finishStatusEffect(string)` | `public` | — | Call directly |
| `addTrait(string)` | `public` | — | Call directly |
| `hasTrait(string)` | `public` | — | Call directly |
| `removeTrait(string)` | `public` | — | Call directly |
| `spawnNewUnit(...)` | `public` | — | Call directly |
| `getHit(...)` | `internal` | ❌ No | Harmony |
| `addBuilding(...)` | `internal` | ❌ No | Harmony |

### `addStatusEffect` — Direct Call Confirmed
Despite being `internal`, NML compiles mod source against `Assembly-CSharp.dll` directly, so internal methods **are callable**. Confirmed in XMM mod (MagicPatches.cs line 141):
```csharp
actor.addStatusEffect("burning", 5f, true);   // direct call — works
actor.hasStatus("burning");                    // direct call — works
actor.finishStatusEffect("burning");           // public — always works
```

Alternatively, use `ActionLibrary` static helpers:
```csharp
ActionLibrary.giveCursed(tile, actor);      // applies "cursed" status
ActionLibrary.giveEnchanted(tile, actor);   // applies "enchanted" status
```

---

## Hooks vs Harmony Patches

### What NML Provides Natively
- **Asset registration** — add traits, powers, statuses, actors to libraries
- **UI** — power buttons/tabs, config windows, auto-layout windows
- **Resources** — auto-load sprites from `GameResources/`
- **Localization** — auto-load from `Locales/`
- **Item creation** — `ItemAssetCreator` helpers
- **Mod lifecycle** — `OnModLoad`, `Reload`, `[Hotfixable]`
- **Logging** — `LogService` / `BasicMod.LogInfo`
- **Mod dependencies** — hard and soft with compile-time macros

### What Requires Harmony
- **Intercepting game methods** — Prefix/Postfix/Transpiler on any game method
- **Calling internal methods** — `getHit`, `addBuilding`, etc. (note: `addStatusEffect` and `hasStatus` work via direct call despite being internal)
- **Modifying game loop** — hooking into `Update`, `LateUpdate` on game managers
- **Custom AI decisions** — inserting into the decision tree
- **Listener-Handler system** — NML's alternative to Harmony (STILL IN DEVELOPMENT, not reliable yet)

### Harmony Patch Pattern
```csharp
using HarmonyLib;

[HarmonyPatch(typeof(Actor), "updateAge")]
public class PatchActorAge
{
    [HarmonyPrefix]
    static bool Prefix(Actor __instance)
    {
        // Return false to skip original method
        // Return true to continue to original
        return true;
    }

    [HarmonyPostfix]
    static void Postfix(Actor __instance)
    {
        // Runs after original method
    }
}
```

**Initialize Harmony in OnModLoad:**
```csharp
protected override void OnModLoad()
{
    Harmony harmony = new Harmony("my.mod.id");
    harmony.PatchAll();  // auto-finds all [HarmonyPatch] classes
}
```

> **⚠️ HarmonyPrefix that returns `false` blocks ALL other mods from patching the same method.** Use `HarmonyTranspiler` for precise IL injection when possible — it's more compatible but harder to write.

---

## Localization

### JSON (per-language files)
```json
// Locales/en.json
{
    "trait_my_custom_trait": "My Cool Trait",
    "trait_my_custom_trait_description": "Makes units super strong"
}
```

### CSV (all languages in one file)
```csv
key,en,cz,ch
trait_my_custom_trait,My Cool Trait,我的酷特性,我的酷特性
```

### Runtime
```csharp
LM.Get("trait_my_custom_trait");           // returns localized string
LM.AddToCurrentLocale("key", "value");     // add at runtime
LM.ApplyLocale();                          // apply changes
```

**Convention:** WorldBox uses `trait_TRAITID` for trait names, `trait_TRAITID_description` for descriptions. Same pattern for `power_`, `status_`, `item_`, etc.

---

## Sprites & Resources

### Auto-loading (recommended)
Place files in `GameResources/` — mirrors Unity's `Resources/` folder structure:
```
GameResources/
├── ui/icons/myIcon.png      → SpriteTextureLoader.getSprite("ui/icons/myIcon")
└── actors/races/myrace/     → walk/swim/idle sprites
```

### Manual loading
```csharp
Sprite icon = SpriteTextureLoader.getSprite("ui/icons/iconOption");
Sprite[] list = SpriteTextureLoader.getSpriteList("effects/projectiles/arrow");
```

### From mod folder
```csharp
string modPath = GetDeclaration().FolderPath;
Sprite sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(modPath, "mySprite.png"));
```

---

## Mod Config (optional)

`default_config.json`:
```json
{
    "Default": {
        "EnableFeatureX": {
            "Id": "EnableFeatureX",
            "Type": "SWITCH",
            "BoolVal": true
        },
        "SpawnRate": {
            "Id": "SpawnRate",
            "Type": "SLIDER",
            "FloatVal": 0.5,
            "MinFloatVal": 0.0,
            "MaxFloatVal": 1.0
        }
    }
}
```

Read in code:
```csharp
ModConfig config = GetConfig();
bool enabled = (bool)config.GetConfig()["Default"]["EnableFeatureX"].GetValue();
float rate = (float)config.GetConfig()["Default"]["SpawnRate"].GetValue();
```

---

## Mod Reload (dev only)

1. Implement `IReloadable` (or get it free with `BasicMod`)
2. Set `Config.isEditor = true`
3. Mark methods with `[Hotfixable]`
4. Modify code → click Reload in game

```csharp
[Hotfixable]
void MyMethod() { /* can be hot-reloaded */ }

// Also works on lambdas:
trait.action_special_effect = [Hotfixable] (target, tile) => { ... };
```

**Cannot hotfix:** constructors, `MonoBehaviour` methods (`Awake`, `Update`).

---

## Known NML Limitations

1. **No native hooks API** — Listener-Handler system is documented but still in development. Use Harmony.
2. **No wrappers for internal methods** — `getHit`, `addBuilding` require Harmony/reflection. However, `addStatusEffect` and `hasStatus` work via direct call despite being `internal`.
3. **Event system not ready** — example code exists but system labeled "(Developing)". Don't rely on it.
4. **Button/Tab docs are incomplete** — in Chinese only, point to example repos. Use existing mod code as reference.
5. **No support for pre-0.22.9 game versions.**
6. **`IncompatibleWith` in mod.json not implemented yet.**
7. **BepInEx mods**: NML recognizes them but does NOT load them — BepInEx handles its own loading.
8. **No automatic DLL compilation** — NML compiles `.cs` files at runtime from source, not precompiled DLLs.
9. **`HarmonyPrefix` returning false breaks other mods** — use Transpiler for compatibility.
10. **`trait.linkSpells()` MUST be called** after setting `trait.spells_ids` — without it, spells silently fail to fire during combat. This is the #1 missed step in spell registration.
