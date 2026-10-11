# GAME_SYSTEMS_INDEX

Dense lookup map for WorldBox game systems. Each section: library class, what it manages, key methods with signatures, gotchas.

---

## The Two Pillars — Read This First

### AssetManager — the rules engine
Holds every library (static definitions, templates, configurations). Used to **look up what exists**: traits, statuses, spells, actor types, building types, etc.

**Access pattern**: `AssetManager.<library_field>`

### MapBox / World.world — the live simulation
Holds every active entity currently on the map. Used to **interact with what is happening right now**: spawning, iterating, modifying live objects.

**Access pattern**: `World.world.<manager_field>` (equivalent to `MapBox.instance.<field>`)

### The Rule

| I need to... | Use |
|---|---|
| Define something or look up a template | `AssetManager` |
| Find, iterate, or modify something alive on the map | `World.world` |

> **⚠️ Confusing these two is the single most common mistake in WorldBox modding.**

All system entries below show both the **AssetManager accessor** (definition/template) and the **World.world accessor** (live instance) where both exist.

---

## Actors (Units)
- **AssetManager**: `AssetManager.actor_library` → `ActorAssetLibrary` (actor type definitions)
- **World.world**: `World.world.units` → `ActorManager` (all live actors on map)

**Key methods — ActorManager (World.world.units):**
```csharp
Actor spawnNewUnit(string pActorAssetID, WorldTile pTile, bool pSpawnSound = false, bool pMiracleSpawn = false, float pSpawnHeight = 6f, Subspecies pSubspecies = null, bool pGiveOwnerlessItems = false, bool pAdultAge = false)
Actor createNewUnit(string pStatsID, WorldTile pTile, bool pMiracleSpawn = false, float pSpawnHeight = 0f, Subspecies pSubspecies = null, Subspecies pSubspeciesMutateFrom = null, bool pSpawnWithItems = true, bool pAdultAge = false, bool pGiveOwnerlessItems = false, bool pSapientSubspecies = false)
bool cloneUnit(Actor pCloneFrom, WorldTile pTileTarget = null)
```

**Key methods — Actor instance:**
```csharp
string getName()
void dieAndDestroy(AttackType pType)
int getAge()  // via .age property
```

**Key properties on Actor:** `.city`, `.kingdom`, `.clan`, `.culture`, `.family`, `.religion`, `.subspecies`, `.army`, `.equipment`, `.data`, `.asset`

**Gotcha**: Use `spawnNewUnit` for god-power style spawns (with drop animation). Use `createNewUnit` for programmatic spawns (no visual effect).

---

## Actor Traits
- **AssetManager**: `AssetManager.traits` → `ActorTraitLibrary`
- **AssetManager**: `AssetManager.trait_groups` → `ActorTraitGroupLibrary`

**Key methods — on Actor instance:**
```csharp
bool addTrait(string pTraitID, bool pRemoveOpposites = false)  // [overloaded: also accepts ActorTrait]
void removeTrait(string pTraitID)  // [overloaded: also accepts ActorTrait]
bool hasTrait(string pTraitID)     // [overloaded: also accepts ActorTrait]
int countTraits()
void clearTraits()
bool hasTraits()
```

**ActorAsset (`AssetManager.actor_library.get(id)`)** — defines per-species traits. Line 1672 in API ref, massive class with all actor stats.

**Gotcha**: `addTrait` returns `false` if the trait was already present or couldn't be added (opposite exists).

---

## Status Effects
- **AssetManager**: `AssetManager.status` → `StatusLibrary`
- **World.world**: `World.world.statuses` → `StatusManager`

**Key methods — on BaseSimObject (Actor/Building):**
```csharp
bool addStatusEffect(string pID, float pOverrideTimer = 0f, bool pColorEffect = true)  // [direct call confirmed via XMM]
bool hasStatus(string pID)                    // [direct call confirmed]
void finishStatusEffect(string pID)           // removes a specific status
void finishAllStatusEffects()                 // removes all statuses
int countStatusEffects()
Dictionary<string, Status>.ValueCollection getStatuses()
```

**Lookup:**
```csharp
StatusAsset statusDef = AssetManager.status.get("burning");
```

**Gotcha**: `addStatusEffect` and `hasStatus` are marked `internal` in the source but **are callable directly** from NML mods (NML compiles source against the game assembly). Confirmed working in XMM mod. `ActionLibrary.giveCursed()` etc. also work as higher-level wrappers.

---

## Spells
- **AssetManager**: `AssetManager.spells` → `SpellLibrary` (extends `AssetLibrary<SpellAsset>`)

**Lookup:**
```csharp
SpellAsset spell = AssetManager.spells.get("spell_id");
```

**Gotcha**: SpellLibrary is a thin wrapper — most spell logic lives in `ActionLibrary` static methods (e.g., `castFire`, `castLightning`, `castCure`, `castTornado`). Spells are attached to actors via the `SpellHolder` on `Actor._spells`.

> **⚠️ SILENT FAILURE**: After setting `trait.spells_ids = new List<string>{...}`, you **MUST** call `trait.linkSpells()` to resolve IDs to SpellAsset references. Without this call, spells will silently fail to fire during combat. This is the #1 missed step in spell registration.

---

## Powers (God Powers)
- **AssetManager**: `AssetManager.powers` → `PowerLibrary` (extends `AssetLibrary<GodPower>`)
- **AssetManager**: `AssetManager.power_tab_library` → `PowerTabLibrary`

**Lookup & config:**
```csharp
GodPower power = AssetManager.powers.get("power_id");
```

**GodPower is config-based** — key fields to set:
- `click_power_action` / `click_power_brush_action` — `PowerAction` delegate
- `click_action` / `click_brush_action` — `PowerActionWithID` delegate
- `actor_asset_id` — for spawn powers
- `type` — `PowerActionType` enum
- `rank` — `PowerRank` (0-4)
- `path_icon` — sprite path
- `hold_action`, `click_interval`, `show_tool_sizes`, `terraform`

**Gotcha**: Powers are configured declaratively via field assignment, not method calls. The `select_button_action` fires on selection, `click_power_action` fires on map click.

---

## Kingdoms
- **AssetManager**: `AssetManager.kingdoms` → `KingdomLibrary`
- **AssetManager**: `AssetManager.kingdoms_traits` → `KingdomTraitLibrary`
- **World.world**: `World.world.kingdoms` → `KingdomManager`
- **World.world**: `World.world.kingdoms_wild` → `WildKingdomsManager`

**Key methods — KingdomManager:**
```csharp
// KingdomManager extends MetaSystemManager<Kingdom, KingdomData>
// Standard iteration: foreach (Kingdom k in World.world.kingdoms) { ... }
```

**Kingdom class** (line 16427): extends `MetaObjectWithTraits<KingdomData, KingdomTrait>` — has `.getCities()`, `.data`, properties for diplomacy.

**Gotcha**: Wild/neutral kingdoms use `World.world.kingdoms_wild`, not `World.world.kingdoms`. Use `kingdoms_wild.get("animals")` etc.

---

## Cities
- **World.world**: `World.world.cities` → `CityManager`

**Key methods — CityManager:**
```csharp
City newCity(Kingdom pKingdom, TileZone pZone, Actor pOriginalActor)
City buildNewCity(Actor pActor, TileZone pZone)
City buildFirstCivilizationCity(Actor pActor)
// Standard iteration: foreach (City c in World.world.cities) { ... }
```

**City class** (line 10167): extends `MetaObject<CityData>` — has `.buildings`, `.getPopulation()`, `.kingdom`, `.zones`.

**CityResources:**
```csharp
int city.resources.get(string pRes)
int city.resources.change(string pRes, int pAmount = 1)
```

---

## Buildings
- **AssetManager**: `AssetManager.buildings` → `BuildingLibrary`
- **World.world**: `World.world.buildings` → `BuildingManager`

**Key methods — BuildingManager:**
```csharp
Building addBuilding(string pID, WorldTile pTile, bool pCheckForBuild = false, bool pSfx = false, BuildPlacingType pType = BuildPlacingType.New)  // [overloaded: also accepts BuildingAsset]
bool canBuildFrom(WorldTile pTile, BuildingAsset pAsset, City pCity, BuildPlacingType pType = BuildPlacingType.New, bool pFloraGrowth = false)
```

---

## BaseSimObject (shared Actor/Building base)
Both `Actor` and `Building` inherit from `BaseSimObject`. Key shared methods:

```csharp
void getHit(float pDamage, bool pFlash = true, AttackType pAttackType = AttackType.Other, BaseSimObject pAttacker = null, bool pSkipIfShake = true, bool pMetallicWeapon = false, bool pCheckDamageReduction = true)
void setHealth(int pValue, bool pClamp = true)
void changeHealth(int pValue)
int getHealth()
int getMaxHealth()  // returns (int)stats["health"]
bool hasHealth()
bool isActor()
bool isBuilding()
bool canAttackTarget(BaseSimObject pTarget, bool pCheckForFactions = true, bool pAttackBuildings = true)
bool areFoes(BaseSimObject pTarget)
```

**Key properties:** `.current_tile`, `.current_position`, `.kingdom`, `.stats` (BaseStats dict-like accessor)

---

## Items & Equipment
- **AssetManager**: `AssetManager.items` → `ItemLibrary`
- **AssetManager**: `AssetManager.items_modifiers` → `ItemModifierLibrary`
- **AssetManager**: `AssetManager.item_groups` → `ItemGroupLibrary`
- **AssetManager**: `AssetManager.unit_hand_tools` → `UnitHandToolLibrary`
- **World.world**: `World.world.items` → `ItemManager`

[see MISSING_SYSTEMS.md — ActorEquipment.txt is a stub, ask user for full file if needed]

---

## AI & Behavior
- **AssetManager**: `AssetManager.decisions_library` → `DecisionsLibrary`
- **AssetManager**: `AssetManager.tasks_actor` → `BehaviourTaskActorLibrary`
- **AssetManager**: `AssetManager.tasks_city` → `BehaviourTaskCityLibrary`
- **AssetManager**: `AssetManager.tasks_kingdom` → `BehaviourTaskKingdomLibrary`
- **AssetManager**: `AssetManager.job_actor` → `ActorJobLibrary`
- **AssetManager**: `AssetManager.job_city` → `CityJobLibrary`
- **AssetManager**: `AssetManager.job_kingdom` → `KingdomJobLibrary`
- **AssetManager**: `AssetManager.citizen_job_library` → `CitizenJobLibrary`
- **AssetManager**: `AssetManager.personalities` → `PersonalityLibrary`

**Actor AI navigation:**
```csharp
// ActorMove (static helper)
ExecuteEvent ActorMove.goTo(Actor pActor, WorldTile pTileTarget, bool pPathOnLiquid = false, bool pWalkOnBlocks = false, bool pPathOnLava = false, int pLimitPathfindingRegions = 0)
```

[see Game_code_essential/DecisionAsset.txt, Game_code_essential/BrainsLibrary.txt]

---

## Disasters
- **AssetManager**: `AssetManager.disasters` → `DisasterLibrary`

[see Game_code_essential/DisasterAsset.txt]

---

## Biomes & Terrain
- **AssetManager**: `AssetManager.biome_library` → `BiomeLibrary`
- **AssetManager**: `AssetManager.tiles` → `TileLibrary`
- **AssetManager**: `AssetManager.top_tiles` → `TopTileLibrary`
- **AssetManager**: `AssetManager.terraform` → `TerraformLibrary`

**Tile access:**
```csharp
WorldTile tile = World.world.GetTile(int pX, int pY)  // returns null if out of bounds
WorldTile tile = World.world.GetTileSimple(int pX, int pY)  // no bounds check
```

---

## Resources & Drops
- **AssetManager**: `AssetManager.resources` → `ResourceLibrary`
- **AssetManager**: `AssetManager.drops` → `DropsLibrary`
- **World.world**: `World.world.drop_manager` → `DropManager`

[see MISSING_SYSTEMS.md]

---

## Projectiles
- **AssetManager**: `AssetManager.projectiles` → `ProjectileLibrary`
- **World.world**: `World.world.projectiles` → `ProjectileManager`

```csharp
World.world.projectiles.spawn(BaseSimObject pSelf, BaseSimObject pTarget, string pProjectileID, Vector3 pStart, Vector3 pEnd)
```

---

## Alliances & Diplomacy
- **World.world**: `World.world.alliances` → `AllianceManager`
- **World.world**: `World.world.diplomacy` → `DiplomacyManager`

[see Game_code_essential/DiplomacyManager.txt — also covered by snippet_harmony_hooks.cs]

---

## Wars
- **AssetManager**: `AssetManager.war_types_library` → `WarTypeLibrary`
- **World.world**: `World.world.wars` → `WarManager`

[covered by snippet_harmony_hooks.cs — WarManager.newWar Prefix pattern]

---

## Armies
- **World.world**: `World.world.armies` → `ArmyManager`

[see MISSING_SYSTEMS.md]

---

## Religions
- **AssetManager**: `AssetManager.religion_traits` → `ReligionTraitLibrary`
- **AssetManager**: `AssetManager.religion_trait_groups` → `ReligionTraitGroupLibrary`
- **World.world**: `World.world.religions` → `ReligionManager`

[standard AssetLibrary<T> pattern — see MISSING_SYSTEMS.md for ReligionTrait details]

---

## Cultures
- **AssetManager**: `AssetManager.culture_traits` → `CultureTraitLibrary`
- **AssetManager**: `AssetManager.culture_trait_groups` → `CultureTraitGroupLibrary`
- **World.world**: `World.world.cultures` → `CultureManager`

[see MISSING_SYSTEMS.md — CultureTrait.txt, CultureManager.txt]

---

## Clans
- **AssetManager**: `AssetManager.clan_traits` → `ClanTraitLibrary`
- **AssetManager**: `AssetManager.clan_trait_groups` → `ClanTraitGroupLibrary`
- **World.world**: `World.world.clans` → `ClanManager`

[see MISSING_SYSTEMS.md — ClanTrait.txt, ClanManager.txt]

---

## Languages
- **AssetManager**: `AssetManager.language_traits` → `LanguageTraitLibrary`
- **World.world**: `World.world.languages` → `LanguageManager`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Subspecies
- **AssetManager**: `AssetManager.subspecies_traits` → `SubspeciesTraitLibrary`
- **World.world**: `World.world.subspecies` → `SubspeciesManager`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Families
- **World.world**: `World.world.families` → `FamilyManager`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Plots
- **AssetManager**: `AssetManager.plots_library` → `PlotsLibrary`
- **World.world**: `World.world.plots` → `PlotManager`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## World Ages (Eras)
- **AssetManager**: `AssetManager.era_library` → `WorldAgeLibrary`
- **World.world**: `World.world.era_manager` → `WorldAgeManager`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## World Laws
- **AssetManager**: `AssetManager.world_laws_library` → `WorldLawLibrary`
- **World.world**: `World.world.world_laws` → `WorldLaws`

[covered by snippet_worldlaw_iteration.cs]

---

## World Behaviours
- **AssetManager**: `AssetManager.world_behaviours` → `WorldBehaviourLibrary`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Effects & Visuals
- **AssetManager**: `AssetManager.effects_library` → `EffectsLibrary`

```csharp
// Static spawn methods:
EffectsLibrary.spawnAt(string pEffectID, Vector3 pPos, float pScale)
EffectsLibrary.spawnAtTile(string pEffectID, WorldTile pTile, float pScale)
EffectsLibrary.spawn(string pEffectID, WorldTile pTile, ...)
```

---

## Combat
- **AssetManager**: `AssetManager.combat_action_library` → `CombatActionLibrary`

**Key static methods (on MapBox):**
```csharp
AttackDataResult MapBox.newAttack(AttackData pData)
void MapBox.spawnLightningBig(WorldTile pTile, float pScale = 0.25f, Actor pActor = null)
void MapBox.spawnLightningSmall(WorldTile pTile, float pScale = 0.25f, Actor pActor = null)
```

---

## Happiness, Loyalty & Opinion
- **AssetManager**: `AssetManager.loyalty_library` → `LoyaltyLibrary`
- **AssetManager**: `AssetManager.opinion_library` → `OpinionLibrary`
- **AssetManager**: `AssetManager.happiness_library` → `HappinessLibrary`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Name Generation
- **AssetManager**: `AssetManager.name_generator` → `NameGeneratorLibrary`
- **AssetManager**: `AssetManager.name_sets` → `NameSetsLibrary`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Base Stats
- **AssetManager**: `AssetManager.base_stats_library` → `BaseStatsLibrary`

**Stat access on any BaseSimObject:**
```csharp
float value = actor.stats["health"];     // or "damage", "speed", "armor", etc.
float value = actor.stats["intelligence"];
```

---

## Genetics (Chromosomes & Genes)
- **AssetManager**: `AssetManager.chromosome_type_library` → `ChromosomeTypeLibrary`
- **AssetManager**: `AssetManager.gene_library` → `GeneLibrary`
- **AssetManager**: `AssetManager.phenotype_library` → `PhenotypeLibrary`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## Rarity
- **AssetManager**: `AssetManager.rarity_library` → `RarityLibrary`

[standard AssetLibrary<T> pattern — no class file needed for basic usage]

---

## ActionLibrary (important helper class)
Not a system, but critical for modding — static methods for game actions:
```csharp
bool ActionLibrary.zombieInfectAttack(BaseSimObject pSelf, BaseSimObject pTarget, ...)
bool ActionLibrary.turnIntoZombie(BaseSimObject pTarget, ...)
bool ActionLibrary.turnIntoDemon(BaseSimObject pTarget, ...)
bool ActionLibrary.metamorphInto(Actor pTarget, string pAsset, bool pRemoveAcquiredTraits = false, ...)
Actor ActionLibrary.turnIntoMetamorph(BaseSimObject pTarget, string pAssetID)
bool ActionLibrary.giveCursed(WorldTile pTile, Actor pActor)
bool ActionLibrary.giveEnchanted(WorldTile pTile, Actor pActor)
void ActionLibrary.removeUnit(Actor pActor)
Actor ActionLibrary.getActorNearPos(Vector2 pPos)
```

---

## AssetLibrary base pattern
All libraries extend `AssetLibrary<T>`. Standard methods available on every library:
```csharp
T get(string pID)        // lookup by string ID
List<T> list             // all assets in the library
bool has(string pID)     // check if ID exists
T add(T asset)           // register new asset
```

Access any library by string ID:
```csharp
BaseAssetLibrary lib = AssetManager.get("units");  // returns the ActorAssetLibrary
bool exists = AssetManager.has("units");
```

---

## Remaining Systems (definition-only, no key API methods)
These follow the standard `AssetLibrary<T>` pattern — use `.get(id)`, `.list`, `.add()`:

| System | Accessor |
|---|---|
| Communication | `AssetManager.communication_library`, `AssetManager.communication_topic_library` |
| Books | `AssetManager.book_types` / `World.world.books` |
| Clouds | `AssetManager.clouds` |
| Map Generation | `AssetManager.map_gen_templates`, `AssetManager.map_gen_settings`, `AssetManager.map_sizes` |
| Brushes | `AssetManager.brush_library` |
| Achievements | `AssetManager.achievements`, `AssetManager.achievement_groups` |
| Architecture | `AssetManager.architecture_library`, `AssetManager.architect_mood_library` |
| Colors | `AssetManager.color_style_library`, `AssetManager.kingdom_colors_library`, ... |
| Banners | `AssetManager.kingdom_banners_library`, `AssetManager.clan_banners_library`, ... |
| Signals | `AssetManager.signals` |
| Professions | `AssetManager.professions` |
| Trait Rains | `AssetManager.trait_rains` |
| Sim Globals | `AssetManager.sim_globals_library` |
| Tile Effects | `AssetManager.tile_tile_effects` |
| UI/Windows | `AssetManager.window_library`, `AssetManager.tooltips`, `AssetManager.hotkey_library` |
| Debug | `AssetManager.debug_tool_library` |

---

## Other Runtime Managers (World.world only)
- `World.world.delayed_actions_manager` → `DelayedActionsManager`
- `World.world.era_manager` → `WorldAgeManager`
- `World.world.tile_manager` → `TileManager`
- `World.world.save_manager` → `SaveManager`
- `World.world.map_stats` → `MapStats`
- `World.world.world_laws` → `WorldLaws`
