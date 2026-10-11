using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace WarBox.Content;

internal static class WarBoxBuildings
{
    public static void Init()
    {
        AddBuildings();
        AddBuildingOrders();
        AddArchitectures();

        PreloadHelpers.preloadBuildingSprites();
    }

    private static void AddBuildings()
    {
        BuildingAsset bunker = AssetManager.buildings.clone("bunker", "$building_civ_human$");
        bunker.upgrade_level = 2;
        bunker.can_be_upgraded = false;
        bunker.has_sprite_construction = false;
        bunker.burnable = false;
        bunker.has_sprites_main_disabled = false;
        bunker.has_sprites_main = true;
        bunker.has_sprites_ruin = true;
        bunker.draw_light_area = true;
        bunker.draw_light_size = 1f;
        bunker.build_road_to = false;
        bunker.base_stats["health"] = 10000f;
        bunker.base_stats["targets"] = 1f;
        bunker.base_stats["area_of_effect"] = 1f;
        bunker.base_stats["damage"] = 20f;
        bunker.base_stats["knockback"] = 1f;
        bunker.priority = 100;
        bunker.type = "type_watch_tower";
        bunker.fundament = new BuildingFundament(2, 2, 1, 0);
        bunker.cost = new ConstructionCost(0, 10, 5, 5);
        bunker.tower = true;
        bunker.sprite_path = "buildings/bunker";
        bunker.tower_projectile = "shotgun_bullet";
        bunker.tower_projectile_offset = 2f;
        bunker.tower_projectile_amount = 1;
        bunker.tower_projectile_reload = 0f;
        bunker.build_place_borders = true;
        bunker.build_place_batch = false;
        bunker.build_place_single = true;
        bunker.setShadow(0.5f, 0.23f, 0.27f);
        bunker.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
        bunker.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
        bunker.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
        bunker.has_sprites_special = false;
        bunker.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");

        BuildingAsset artillery_bunker = AssetManager.buildings.clone("artillery_bunker", "bunker");
        artillery_bunker.sprite_path = "buildings/artillery_bunker";
        artillery_bunker.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        artillery_bunker.priority = 50;
        artillery_bunker.has_sprite_construction = true;
        artillery_bunker.cost = new ConstructionCost(0, 12, 8, 5);
        artillery_bunker.base_stats["damage"] = 1000f;
        artillery_bunker.tower_projectile = "cannon_shell";
        artillery_bunker.tower_projectile_offset = 4f;
        artillery_bunker.tower_projectile_reload = 5f;
        artillery_bunker.setShadow(0.5f, 0.23f, 0.27f);
        artillery_bunker.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
        artillery_bunker.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
        artillery_bunker.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
        artillery_bunker.max_houses = 1;
        artillery_bunker.type = "type_artillerybunker";

        BuildingAsset heavy_factory = AssetManager.buildings.clone("heavy_factory", "$building_civ_human$");
        heavy_factory.can_be_upgraded = false;
        heavy_factory.has_sprite_construction = true;
        heavy_factory.burnable = false;
        heavy_factory.has_sprites_main_disabled = false;
        heavy_factory.has_sprites_main = true;
        heavy_factory.draw_light_area = true;
        heavy_factory.draw_light_size = 0.25f;
        heavy_factory.build_road_to = true;
        heavy_factory.base_stats["health"] = 2000f;
        heavy_factory.smoke = true;
        heavy_factory.smoke_interval = 1f;
        heavy_factory.smoke_offset = new Vector2Int(100, 100);
        heavy_factory.priority = 9999;
        heavy_factory.type = "type_heavyfactory";
        heavy_factory.fundament = new BuildingFundament(5, 5, 4, 1);
        heavy_factory.cost = new ConstructionCost(20, 80, 0, 0);
        heavy_factory.tower = false;
        heavy_factory.sprite_path = "buildings/heavy_factory";
        heavy_factory.build_place_borders = true;
        heavy_factory.build_place_batch = false;
        heavy_factory.build_place_single = true;
        heavy_factory.setShadow(0.7f, 0.32f, 0.38f);
        heavy_factory.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTownHall";
        heavy_factory.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
        heavy_factory.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
        heavy_factory.has_sprites_special = false;
        heavy_factory.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        heavy_factory.construction_progress_needed = 100;
        heavy_factory.max_houses = 2;

        BuildingAsset light_factory = AssetManager.buildings.clone("light_factory", "heavy_factory");
        light_factory.sprite_path = "buildings/light_factory";
        light_factory.cost = new ConstructionCost(10, 45, 0, 0);
        light_factory.fundament = new BuildingFundament(3, 3, 3, 1);
        light_factory.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        light_factory.type = "type_lightfactory";

        BuildingAsset laf = AssetManager.buildings.clone("light_aircraft_factory", "heavy_factory"); // LAF: Light aircraft factory
        laf.sprite_path = "buildings/light_aircraft_factory";
        laf.cost = new ConstructionCost(10, 45, 0, 0);
        laf.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        laf.smoke = false;
        laf.type = "type_lightaircraftfactory";

        BuildingAsset haf = AssetManager.buildings.clone("heavy_aircraft_factory", "light_aircraft_factory"); // HAF: Heavy aircraft factory
        haf.sprite_path = "buildings/heavy_aircraft_factory";
        haf.cost = new ConstructionCost(20, 60, 0, 0);
        haf.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        haf.smoke = false;
        haf.type = "type_heavyaircraftfactory";

        BuildingAsset light_shipyard = AssetManager.buildings.clone("light_shipyard", "heavy_factory");
        light_shipyard.sprite_path = "buildings/light_shipyard";
        light_shipyard.cost = new ConstructionCost(10, 45, 0, 0);
        light_shipyard.fundament = new BuildingFundament(3, 3, 3, 1);
        light_shipyard.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        light_shipyard.type = "type_lightshipyard";
        light_shipyard.can_be_upgraded = true;
        light_shipyard.upgrade_level = 0;
        light_shipyard.upgrade_to = "heavy_shipyard";

        BuildingAsset heavy_shipyard = AssetManager.buildings.clone("heavy_shipyard", "heavy_factory");
        heavy_shipyard.sprite_path = "buildings/heavy_shipyard";
        heavy_shipyard.cost = new ConstructionCost(20, 80, 0, 0);
        heavy_shipyard.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
        heavy_shipyard.type = "type_heavyshipyard";
        heavy_shipyard.can_be_upgraded = false;
        heavy_shipyard.upgrade_level = 1;
        heavy_shipyard.upgraded_from = "light_shipyard";

        foreach (ActorAsset race in AssetManager.actor_library.list.Where(race => race.architecture_id != string.Empty && race.canBecomeSapient() == true)) // this does double work for actors that have both non-spaient and sapient
        {
            BuildingAsset watch_tower = AssetManager.buildings.get($"watch_tower_{race.architecture_id}");
            if (watch_tower != null)
            {
                watch_tower.can_be_upgraded = true;
                watch_tower.upgrade_level = 1;
                watch_tower.upgrade_to = "bunker";
            }
        }
    }

    private static void AddBuildingOrders()
    {
        foreach (CityBuildOrderAsset civ in AssetManager.city_build_orders.list)
        {
            BuildOrder order;

            civ.addUpgrade("order_watch_tower");
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire");

            civ.addBuilding("order_artillery_bunker", 1);
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_bonfire", "order_watch_tower");

            civ.addBuilding("order_heavy_factory", 1);
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_0");

            civ.addBuilding("order_light_factory", 1);
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_0");

            civ.addBuilding("order_light_aircraft_factory", 1);
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_0");

            civ.addBuilding("order_heavy_aircraft_factory", 1);
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_hall_0");

            civ.addBuilding("order_light_shipyard", 1);
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_docks_0");

            civ.addUpgrade("order_light_shipyard");
            order = civ.list.Last();
            order.requirements_orders = AssetLibrary<CityBuildOrderAsset>.a<string>("order_docks_0");
        }
    }

    private static void AddArchitectures()
    {
        var customOrders = new Dictionary<string, string>
        {
            {"order_artillery_bunker", "artillery_bunker"},
            {"order_light_shipyard", "light_shipyard"},
            {"order_heavy_factory", "heavy_factory"},
            {"order_light_factory", "light_factory"},
            {"order_light_aircraft_factory", "light_aircraft_factory"},
            {"order_heavy_aircraft_factory", "heavy_aircraft_factory"}
        };

        foreach (var arch in AssetManager.architecture_library.list)
        {
            arch.building_ids_for_construction ??= new Dictionary<string, string>();
            foreach (var kvp in customOrders)
            {
                if (!arch.building_ids_for_construction.ContainsKey(kvp.Key))
                {
                    arch.building_ids_for_construction.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}