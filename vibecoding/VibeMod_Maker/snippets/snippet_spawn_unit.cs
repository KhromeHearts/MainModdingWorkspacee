// snippet_spawn_unit.cs — Spawn a unit with traits and stats
// Source pattern: MoBox Mobunit, XMM MagicManager

public static void SpawnCustomUnit(WorldTile tile)
{
    // Option 1: Spawn existing unit type (simplest)
    Actor spawned = World.world.units.createNewUnit(
        "human",        // actor asset ID
        tile,           // where to spawn
        true,           // miracle spawn (visual effect)
        3f              // spawn height (drop animation)
    );

    if (spawned != null)
    {
        // Apply traits
        spawned.addTrait("strong", false);
        spawned.addTrait("fire_blood", false);  // custom trait

        // Set name
        spawned.setName("Champion");

        // Modify stats directly
        spawned.stats["health"] = 500f;
        spawned.stats["damage"] = 100f;
        spawned.setStatsDirty();

        // Set to full health after stat changes
        spawned.setHealth(spawned.getMaxHealth());

        // Store custom data on the actor (persists with save)
        spawned.data.set("my_mod_power_level", 9001);
    }

    // Option 2: Clone existing actor type with modifications
    ActorAsset customAsset = AssetManager.actor_library.clone("human", "super_human");
    customAsset.base_stats[S.health] = 999f;
    customAsset.base_stats[S.damage] = 50f;
    customAsset.base_stats[S.speed] = 80f;
    // customAsset is auto-registered by clone()

    Actor custom = World.world.units.createNewUnit("super_human", tile);
}
