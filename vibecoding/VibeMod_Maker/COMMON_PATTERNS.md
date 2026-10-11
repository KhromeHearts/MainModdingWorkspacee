# COMMON_PATTERNS

Recurring modding patterns extracted from real WorldBox mods. Each pattern is 3-5 lines of context pointing to a snippet.

---

## 1. Add a Custom Trait with an Effect

**When:** You want to define a new trait with stat bonuses and/or a periodic effect (heal, damage, transform, etc.)

**Pattern:** Create `ActorTrait` → set `id`, `group_id`, `base_stats`, `path_icon` → assign `action_special_effect` lambda → `AssetManager.traits.add()` → add locale keys → `LM.ApplyLocale()` or apply it to an en.json if available.

**Gotcha:** Locale keys use `trait_TRAITID` for name and `trait_TRAITID_info` for description. Call `trait.unlock(true)` to make it visible in the editor.

→ See: `snippets/snippet_add_trait.cs`

---

## 2. Apply a Status Effect

**When:** You want to burn, freeze, curse, or apply any status to an actor.

**Pattern:** `addStatusEffect` is `internal` but callable from NML mods (compiled against game assembly). Direct call: `actor.addStatusEffect("burning", 5f, true)`. For checking: `actor.hasStatus("burning")`. For removal: `actor.finishStatusEffect("burning")`.

**Gotcha:** If direct call won't compile, use `AccessTools.Method()` reflection or route through `ActionLibrary` helpers (`giveCursed`, `giveEnchanted`). These are public and handle everything internally.

→ See: `snippets/snippet_status_effect.cs`

---

## 3. Spawn a Unit

**When:** You want to create a new actor on the map, either from an existing type or a custom clone.

**Pattern:** `World.world.units.createNewUnit("human", tile)` for quiet spawn. `World.world.units.spawnNewUnit("human", tile, true)` for god-power style (with drop animation). Clone assets with `AssetManager.actor_library.clone("source", "newId")` to create variant types.

**Gotcha:** After modifying stats on a spawned actor, call `actor.setStatsDirty()` then `actor.setHealth(actor.getMaxHealth())` or stats won't take effect. Use `actor.data.set(key, value)` for custom persistent data.

→ See: `snippets/snippet_spawn_unit.cs`

---

## 4. Register a God Power

**When:** You want a clickable power button that does something on the map.

**Pattern:** Create `GodPower` → set `id`, icon, delegates → `AssetManager.powers.add()`. Two delegate types: `click_power_action` (receives tile + power object) and `click_action` (receives tile + power ID string). Return `true` if the action succeeded.

**Gotcha:** Powers are field-based config, not method calls. `actor_asset_id` auto-spawns that unit type. Custom actions override the default spawn. Use `PowerButtonCreator.CreateGodPowerButton()` to add to a tab.

→ See: `snippets/snippet_god_power.cs`

---

## 5. Hook Actor Behaviour with Harmony

**When:** You need to intercept game events: birth, death, stat updates, combat, wars, save/load.

**Pattern:** `[HarmonyPatch(typeof(TargetClass), "methodName")]` on a static class. Use `Postfix` to run after the original (safe, compatible). Use `Prefix` returning `false` to block/replace the original (breaks other mods — use sparingly). Initialize with `new Harmony("mod.id").PatchAll()` in `OnModLoad`.

**Key hooks from real mods:**
- `ActorManager.finalizeActor` → on unit spawn
- `Actor.die` → on death
- `Actor.updateStats` → modify stats dynamically
- `BabyMaker.makeBaby` → on birth
- `WarManager.newWar` → intercept wars
- `MapBox.saveSave/loadSave` → persist custom data

**Gotcha:** `HarmonyPrefix` returning `false` blocks ALL other mods patching that method. Prefer `HarmonyTranspiler` for compatibility. Always null-check `__instance` in Postfix.

→ See: `snippets/snippet_harmony_hooks.cs`

---

## 6. Use ActionLibrary (Transformations & Spells)

**When:** You need to transform units, cast spells, or trigger game actions without reimplementing the logic.

**Pattern:** `ActionLibrary` has static methods for common actions. Transformations: `turnIntoZombie`, `turnIntoDemon`, `metamorphInto(actor, "dragon")`. Spell actions: `castFire`, `castLightning`, `castTornado`, `castCure`, `castShieldOnHimself`, `teleportRandom`. These are used as `AttackAction` delegates on traits and spells.

**Gotcha:** `ActionLibrary.removeUnit(actor)` is the clean way to remove a unit (not `dieAndDestroy`). Use `ActionLibrary.getActorNearPos(pos)` to find nearby units.

→ See: `snippets/snippet_action_library.cs`

---

## 7. Register a Custom Spell

**When:** You want an actor to cast a spell in combat, linked to a trait.

**Pattern:** Create `SpellAsset` → set `id`, `action` (AttackAction delegate), `cast_target`, `chance`, `cost_mana` → `AssetManager.spells.add()`. Then create a trait with `trait.spells_ids = new List<string>{ "spell_id" }` → `trait.linkSpells()` to connect.

**Gotcha:** `trait.linkSpells()` is REQUIRED — it resolves string IDs to SpellAsset references. Without it, the spell won't trigger. `SpellAsset.action` wraps an `AttackAction` — use `new AttackAction(ActionLibrary.castFire)` to wrap existing methods.

→ See: `snippets/snippet_register_spell.cs`

---

## 8. World Laws & Live Entity Iteration

**When:** You want to modify game rules (world laws) or loop through all living units/kingdoms/cities.

**Pattern:** Modify laws: `AssetManager.world_laws_library.get("law_id").default_state = true`. Check: `WorldLawLibrary.world_law_name.isEnabled()`. Iterate units: `foreach (Actor a in World.world.units)`. Iterate kingdoms: `foreach (Kingdom k in World.world.kingdoms)`. Access city resources: `city.resources.get("food")`.

**Gotcha:** Use `InvokeRepeating("method", delay, interval)` for periodic checks instead of `Update()` — much cheaper. Access tile under mouse with `World.world.getMouseTilePos()`. Store custom data with `kingdom.data.set(key, value)` / `.get(key, out value)`.

→ See: `snippets/snippet_worldlaw_iteration.cs`

---

## Cross-Cutting Patterns

### Initialization Order
Most mods use a coroutine to wait for `AssetManager` to be ready:
```csharp
IEnumerator InitializeWhenReady() {
    while (AssetManager.traits == null) yield return null;
    RegisterTraits();
}
```

### Custom Per-Actor Data
Use `actor.data.set("key", value)` / `actor.data.get("key", out T value)` for persistent custom data that survives save/load.

### Localization Convention
| Asset Type | Name Key | Description Key |
|---|---|---|
| Trait | `trait_TRAITID` | `trait_TRAITID_info` |
| Power | `POWERID` | `POWERID_description` |
| Status | `status_STATUSID` | — |
| Item | `item_ITEMID` | `item_ITEMID_description` |
