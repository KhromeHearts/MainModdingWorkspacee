// snippet_god_power.cs — Register a god power button that spawns a unit
// Source pattern: MoBox Mobunit

using NeoModLoader.General;

public static void RegisterSpawnPower()
{
    // 1. Create the power
    GodPower power = new GodPower();
    power.id = "spawn_zombie_bear";
    power.name = "spawn_zombie_bear";            // matches locale key
    power.actor_asset_id = "bear";               // base actor type
    power.show_spawn_effect = true;
    power.multiple_spawn_tip = true;
    power.actor_spawn_height = 3f;

    // 2. Define what happens on map click
    power.click_action = new PowerActionWithID((WorldTile pTile, string pPower) =>
    {
        if (pTile == null) return false;

        Actor spawned = World.world.units.createNewUnit("bear", pTile, true, 3f);
        if (spawned != null)
        {
            spawned.addTrait("infected", false);
            return true;
        }
        return false;
    });

    // 3. Register the power
    AssetManager.powers.add(power);

    // 4. Add localization
    LM.AddToCurrentLocale("spawn_zombie_bear", "Zombie Bear");
    LM.AddToCurrentLocale("spawn_zombie_bear_description", "Spawn an infected bear");

    // 5. Create a button on the creatures tab
    var creaturesTab = PowerButtonCreator.GetTab("units");
    Sprite icon = Resources.Load<Sprite>("ui/Icons/iconBear");  // or custom sprite

    PowerButton btn = PowerButtonCreator.CreateGodPowerButton(
        "spawn_zombie_bear",
        icon,
        creaturesTab
    );
}
