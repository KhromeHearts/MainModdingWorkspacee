using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.api.features;
using ReflectionUtility;

namespace WarBox.Content;

internal static class WarBoxGodPowers
{
    private static FieldInfo dropField;

    public static void Init()
    {
        dropField = typeof(GodPower).GetField("cached_drop_asset", BindingFlags.NonPublic | BindingFlags.Instance);

        AddDrops();
        AddPowers();
        //Cache();
    }

    private static void AddDrops()
    {
        CreateDrop("spawn_bunker", "bunker");
        CreateDrop("spawn_artillery_bunker", "artillery_bunker");

        CreateDrop("spawn_light_factory", "light_factory");
        CreateDrop("spawn_heavy_factory", "heavy_factory");
        CreateDrop("spawn_light_aircraft_factory", "light_aircraft_factory");
        CreateDrop("spawn_heavy_aircraft_factory", "heavy_aircraft_factory");
        CreateDrop("spawn_light_shipyard", "light_shipyard");
        CreateDrop("spawn_heavy_shipyard", "heavy_shipyard");
    }
    
    private static void AddPowers()
    {
        CreateBuilderPower("bunker_builder", "Bunker", "spawn_bunker");
        CreateBuilderPower("artillery_bunker_builder", "Artillery Bunker", "spawn_artillery_bunker");
        CreateBuilderPower("heavy_factory_builder", "Heavy Factory", "spawn_heavy_factory");
        CreateBuilderPower("light_factory_builder", "Light Factory", "spawn_light_factory");
        CreateBuilderPower("light_aircraft_factory_builder", "Light Aircraft Factory", "spawn_light_aircraft_factory");
        CreateBuilderPower("heavy_aircraft_factory_builder", "Heavy Aircraft Factory", "spawn_heavy_aircraft_factory");
        CreateBuilderPower("light_shipyard_builder", "Light Shipyard", "spawn_light_shipyard");
        CreateBuilderPower("heavy_shipyard_builder", "Heavy Shipyard", "spawn_heavy_shipyard");

        CreateVehiclePower("spawn_tank", "warbox_tank");
        CreateVehiclePower("spawn_apc", "warbox_apc");
        CreateVehiclePower("spawn_ifv", "warbox_ifv");
        CreateVehiclePower("spawn_spg", "warbox_spg");
        CreateVehiclePower("spawn_helicopter", "warbox_helicopter");
        CreateVehiclePower("spawn_bomber", "warbox_bomber");
        CreateVehiclePower("spawn_fighter", "warbox_fighter");
        CreateVehiclePower("spawn_gunboat", "warbox_gunboat");

        CreateWarriorPower("spawn_warrior_pistol", "pistol");
        CreateWarriorPower("spawn_warrior_smg", "smg");
        CreateWarriorPower("spawn_warrior_shotgunreplace", "shotgunreplace");
        CreateWarriorPower("spawn_warrior_rifle", "rifle");
        CreateWarriorPower("spawn_warrior_autorifle", "autorifle");
        CreateWarriorPower("spawn_warrior_sniperrifle", "sniperrifle");
        CreateWarriorPower("spawn_warrior_rpg", "rpg");
    }

    private static bool StuffDrop(WorldTile pTile, GodPower pPower)
    {
        AssetManager.powers.CallMethod("spawnDrops", pTile, pPower);
        return true;
    }

    private static bool SpawnVehicle(WorldTile pTile, string pPowerID)
    {
        City city = pTile.zone.city;
        if (pTile.zone.city == null)
        {
            WorldTip.showNow("cant_spawn_unit_kingdom", true, "top", 3f);
            return false;
        }

        Kingdom kingdom = city.kingdom;
        if (kingdom == null)
        {
            WorldTip.showNow("cant_spawn_unit_kingdom", true, "top", 3f);
            return false;
        }

        GodPower power = AssetManager.powers.get(pPowerID);
        if (power == null) return false;

        MusicBox.playSound("event:/SFX/UNIQUE/SpawnWhoosh", (float)pTile.pos.x, (float)pTile.pos.y, false, false);
        EffectsLibrary.spawn("fx_spawn", pTile, null, null, 0f, -1f, -1f);

        Actor unit = World.world.units.createNewUnit(
               power.actor_asset_id,
               pTile,
               pMiracleSpawn: false,
               0f,
               null,
               null,
               pSpawnWithItems: false
        );
        unit.makeWait(1f);
        unit.setKingdom(kingdom);
        unit.setCity(city);

        return true;
    }

    private static bool SpawnWarrior(WorldTile pTile, string item)
    {
        City city = pTile.zone.city;
        if (pTile.zone.city == null)
        {
            WorldTip.showNow("cant_spawn_unit_kingdom", true, "top", 3f);
            return false;
        }

        Kingdom kingdom = city.kingdom;
        if (kingdom == null)
        {
            WorldTip.showNow("cant_spawn_unit_kingdom", true, "top", 3f);
            return false;
        }

        MusicBox.playSound("event:/SFX/UNIQUE/SpawnWhoosh", (float)pTile.pos.x, (float)pTile.pos.y, false, false);
        EffectsLibrary.spawn("fx_spawn", pTile, null, null, 0f, -1f, -1f);

        Subspecies subspecies = city.getMainSubspecies();

        if (subspecies == null)
        {
            WorldTip.showNow("cant_spawn_unit_kingdom", true, "top", 3f);
            return false;
        }
        
        Actor unit = World.world.units.createNewUnit(
            subspecies.getActorAsset().id,
            pTile,
            pMiracleSpawn: false,
            0f,
            subspecies,
            null,
            false,
            true
        );

        string[] equipmentItems = { item, "armor_steel" };

        foreach (string equipment in equipmentItems)
        {
            EquipmentAsset equipmentAsset = AssetManager.items.get(equipment);
            Item item_asset = World.world.items.generateItem(equipmentAsset);
            unit.equipment.setItem(item_asset, unit);
        }

        unit.makeWait(1f);
        unit.setKingdom(kingdom);
        unit.setCity(city);
        city.makeWarrior(unit);

        return true;
    }

    private static void CreateWarriorPower(string id, string equipment_id)
    {
        GodPower spawn_warrior = new GodPower()
        {
            id = id,
            type = PowerActionType.PowerSpecial,
            rank = PowerRank.Rank0_free,
            unselect_when_window = true,
            show_spawn_effect = true,
            actor_spawn_height = 3f,
            multiple_spawn_tip = true,
            show_unit_stats_overview = true,
            set_used_camera_drag_on_long_move = true
        };
        spawn_warrior.name = id;
        spawn_warrior.click_action = new PowerActionWithID((pTile, pPower) =>
        {
            return SpawnWarrior(pTile, equipment_id);
        });

        AssetManager.powers.add(spawn_warrior);
    }

    private static void CreateVehiclePower(string id, string actor_id)
    {
        GodPower vehicle = AssetManager.powers.clone(id, "$template_spawn_actor$");
        vehicle.name = id;
        vehicle.actor_asset_id = actor_id;
        vehicle.click_action = new PowerActionWithID(SpawnVehicle);
    }
    
    private static void CreateBuilderPower(string id, string name, string drop_id)
    {
        GodPower builder = AssetManager.powers.clone(id, "$template_drop_building$");
        builder.name = name;
        builder.rank = PowerRank.Rank0_free;
        builder.drop_id = drop_id;
        builder.falling_chance = 0f;
        builder.force_brush = "circ_0";
        builder.click_power_action = StuffDrop;
        builder.click_power_brush_action = new PowerAction((pTile, pPower) =>
        {
            return (bool)AssetManager.powers.CallMethod("loopWithCurrentBrushPowerForDropsFull", pTile, pPower);
        });
        dropField.SetValue(AssetManager.powers.get(id), AssetManager.drops.get(drop_id));
    }

    private static void CreateDrop(string id, string building_id){
        DropAsset drop = new DropAsset
        {
            id = id,
            path_texture = "drops/drop_stone",
            default_scale = 0.2f,
            random_frame = true,
            random_flip = true,
            type = DropType.DropBuilding,
            building_asset = building_id,
            action_landed = DropsLibrary.action_spawn_building
        };
        AssetManager.drops.add(drop);
    }
}