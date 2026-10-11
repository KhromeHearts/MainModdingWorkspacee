// snippet_worldlaw_iteration.cs — Modify world laws and iterate live entities
// Source pattern: RULERBOX Main

// ---- Modify a world law at init ----
public static void ModifyWorldLaw()
{
    // Get an existing world law and change its default
    var cursedLaw = AssetManager.world_laws_library.get("world_law_cursed_world");
    cursedLaw.default_state = true;  // enable by default

    // Check if a world law is currently active
    if (WorldLawLibrary.world_law_cursed_world.isEnabled())
    {
        Debug.Log("Cursed world is active!");
    }
}

// ---- Iterate all live units ----
public static void IterateUnits()
{
    foreach (Actor unit in World.world.units)
    {
        if (unit == null) continue;

        // Check traits
        if (unit.hasTrait("immortal"))
        {
            unit.setHealth(unit.getMaxHealth());
        }

        // Access kingdom
        if (unit.kingdom != null && unit.kingdom.data.name == "Humans")
        {
            unit.addExperience(10);
        }
    }
}

// ---- Iterate all kingdoms ----
public static void IterateKingdoms()
{
    foreach (Kingdom k in World.world.kingdoms)
    {
        Debug.Log($"Kingdom: {k.data.name}, Cities: {k.getCities()?.Count}");

        // Add custom data to kingdom (persists with save)
        k.data.set("my_mod_gold", 1000);
    }
}

// ---- Iterate all cities ----
public static void IterateCities()
{
    foreach (City city in World.world.cities)
    {
        int pop = city.getPopulation();
        int food = city.resources.get("food");
        Debug.Log($"City: pop={pop}, food={food}");
    }
}

// ---- Periodic check using MonoBehaviour Update or InvokeRepeating ----
// In your BasicMod class:
//   InvokeRepeating("CheckUnits", 2f, 1f);  // every 1 second after 2s delay
//
// void CheckUnits() { IterateUnits(); }

// ---- Get tile under mouse (for interactive tools) ----
public static void GetMouseTile()
{
    WorldTile tile = World.world.getMouseTilePos();
    if (tile != null)
    {
        var city = tile.zone?.city;
        var kingdom = city?.kingdom;
    }
}
