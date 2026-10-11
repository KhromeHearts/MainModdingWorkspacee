using System;
using System.Collections.Generic;
using NCMS.Utils;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using WarBox.UI.Windows;

namespace WarBox.UI;

internal static class WarBoxUI
{
    public static PowersTab tab;

    public static void Init()
    {
        tab = TabManager.CreateTab(
            "warbox_tab",
            "warbox_tab_name",
            "warbox_tab_description",
            SpriteTextureLoader.getSprite("ui/icons/warbox_tab_icon")
        );

        tab.SetLayout(new List<string>()
        {
            "builders",
            "spawners",
            "warriors",
        });

        CreateWindows();
        CreateButtons();

        tab.UpdateLayout();
    }

    private static void CreateWindows()
    {
        WarBoxAutoLayoutWindow.CreateWindow("warbox_auto_layout_window", "warbox_auto_layout_window_title");
    }

    private static void CreateButtons()
    {
        CreateBuilderButton("bunker_builder");
        CreateBuilderButton("artillery_bunker_builder");
        CreateBuilderButton("light_factory_builder");
        CreateBuilderButton("heavy_factory_builder");
        CreateBuilderButton("light_aircraft_factory_builder");
        CreateBuilderButton("heavy_aircraft_factory_builder");
        CreateBuilderButton("light_shipyard_builder");
        CreateBuilderButton("heavy_shipyard_builder");

        CreateVehicleButton("spawn_tank");
        CreateVehicleButton("spawn_apc");
        CreateVehicleButton("spawn_ifv");
        CreateVehicleButton("spawn_spg");
        CreateVehicleButton("spawn_helicopter");
        CreateVehicleButton("spawn_bomber");
        CreateVehicleButton("spawn_fighter");
        CreateVehicleButton("spawn_gunboat");

        CreateWarriorButton("pistol");
        CreateWarriorButton("smg");
        CreateWarriorButton("shotgunreplace");
        CreateWarriorButton("rifle");
        CreateWarriorButton("autorifle");
        CreateWarriorButton("sniperrifle");
        CreateWarriorButton("rpg");

    }

    private static void CreateWarriorButton(string equipment_id)
    {
        tab.AddPowerButton("warriors", PowerButtonCreator.CreateGodPowerButton(
            "spawn_warrior_" + equipment_id,
            SpriteTextureLoader.getSprite("ui/icons/items/icon_" + equipment_id)
        ));
    }

    private static void CreateVehicleButton(string id)
    {
        tab.AddPowerButton("spawners", PowerButtonCreator.CreateGodPowerButton(
            id,
            SpriteTextureLoader.getSprite("ui/icons/buttons/" + id)
        ));
    }

    private static void CreateBuilderButton(string building_id)
    {
        tab.AddPowerButton("builders", PowerButtonCreator.CreateGodPowerButton(
            building_id,
            SpriteTextureLoader.getSprite("ui/icons/buttons/" + building_id)
        )); ;
    }
}