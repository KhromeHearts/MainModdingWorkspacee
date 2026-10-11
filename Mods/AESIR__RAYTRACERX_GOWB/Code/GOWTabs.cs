using System;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General.UI.Tab;
using System.Runtime.InteropServices;

namespace GOWB
{
    public static class GOWTabs
    {
        private static PowersTab tab;

        public static void Init()
        {
            Sprite tabIcon = SpriteTextureLoader.getSprite("ui/icons/gow_ragnarok");
            if (tabIcon == null)
            {
                Debug.LogWarning("[GOWB] Tab icon not found, skipping tab creation");
                return;
            }

            tab = TabManager.CreateTab(
                "GOWB",
                "Button_Tab_GOWB",
                "Button_Tab_GOWB Description",
                tabIcon,
                "GOWB"
            );
            if (tab == null)
            {
                Debug.LogError("[GOWB] Failed to create GOWB tab");
                return;
            }

            LoadButtons();
        }

        private static void LoadButtons()
        {
            if (tab == null) return;

            RegisterSpawnPower("spawn_odin", Units.OdinActorId, "ui/icons/ui_odin", "Odin", "Spawn Odin", new Vector2(254f, 18f));
            RegisterSpawnPower("spawn_thor", Units.ThorActorId, "ui/icons/ui_thor", "Thor", "Spawn Thor", new Vector2(290f, 18f));
            RegisterSpawnPower("spawn_baldur", Units.BaldurActorId, "ui/icons/ui_baldur", "Baldur", "Spawn Baldur", new Vector2(326f, 18f));
            RegisterSpawnPower("spawn_heimdall", Units.HeimdallActorId, "ui/icons/ui_heimdall", "Heimdall", "Spawn Heimdall", new Vector2(362f, 18f));
            RegisterSpawnPower("spawn_kratos", Units.KratosActorId, "ui/icons/ui_kratos", "Kratos", "Spawn Kratos", new Vector2(398f, 18f));
            RegisterSpawnPower("spawn_kratosspear", Units.KratosSpearActorId, "ui/icons/ui_kratosspear", "Kratos Spear", "Spawn Kratos Spear", new Vector2(434f, 18f));
            RegisterSpawnPower("spawn_kratosblades", Units.KratosBladesActorId, "ui/icons/ui_kratosblades", "Kratos Blades", "Spawn Kratos Blades", new Vector2(470f, 18f));
            RegisterSpawnPower("spawn_jormungandr", Units.JormungandrActorId, "ui/icons/ui_jormungandr", "Jormungandr", "Spawn Jormungandr", new Vector2(506f, 18f));
            RegisterSpawnPower("spawn_atreus", Units.AtreusActorId, "ui/icons/ui_atreus", "Atreus", "Spawn Atreus", new Vector2(542f, 18f));
            RegisterSpawnPower("spawn_freya", Units.FreyaActorId, "ui/icons/ui_freya", "Freya", "Spawn Freya", new Vector2(578f, 18f));
        }

        private static void RegisterSpawnPower(string powerId, string actorId, string iconPath, string title, string description, Vector2 buttonPos)
        {
            GodPower power = AssetManager.powers.get(powerId);
            if (power == null)
            {
                power = new GodPower();
                power.id = powerId;
                power.name = powerId;
                power.actor_asset_id = actorId;
                power.show_spawn_effect = true;
                power.actor_spawn_height = 3f;
                power.sound_event = "spawn";
                power.multiple_spawn_tip = true;
                power.click_action = new PowerActionWithID(CallSpawnUnit);
                AssetManager.powers.add(power);
            }

            Sprite icon = SpriteTextureLoader.getSprite(iconPath);
            if (icon == null)
            {
                Debug.LogWarning($"[GOWB] Missing icon '{iconPath}' for power '{powerId}'.");
                return;
            }

            PowerButtons.CreateButton(
                powerId,
                icon,
                title,
                description,
                buttonPos,
                ButtonType.GodPower,
                tab.transform,
                null
            );
        }

        public static bool CallSpawnUnit(WorldTile pTile, string pPowerID)
        {
            if (pTile == null || string.IsNullOrEmpty(pPowerID)) return false;
            try
            {
                var power = AssetManager.powers.get(pPowerID);
                if (power == null) return false;
                if (string.IsNullOrEmpty(power.actor_asset_id)) return false;
                if (AssetManager.actor_library.get(power.actor_asset_id) == null)
                {
                    Debug.LogWarning($"[GOWB] Spawn failed: actor asset '{power.actor_asset_id}' is not registered for power '{pPowerID}'.");
                    return false;
                }

                float spawnHeight = power.actor_spawn_height > 0f ? power.actor_spawn_height : 3f;

                Actor spawned = World.world.units.spawnNewUnit(
                    power.actor_asset_id,
                    pTile,
                    pSpawnSound: true,
                    pMiracleSpawn: true,
                    pSpawnHeight: spawnHeight,
                    pSubspecies: null,
                    pGiveOwnerlessItems: false,
                    pAdultAge: true
                );

                if (spawned == null)
                {
                    spawned = World.world.units.createNewUnit(power.actor_asset_id, pTile, true, spawnHeight);
                }
                if (spawned == null) return false;

                spawned.data.level = 1;


                if (spawned.kingdom == null)
                {
                    Kingdom fallbackKingdom = null;
                    if (pTile.zone != null && pTile.zone.city != null)
                    {
                        fallbackKingdom = pTile.zone.city.kingdom;
                    }
                    if (fallbackKingdom == null)
                    {
                        fallbackKingdom = World.world?.kingdoms?.getRandom();
                    }
                    if (fallbackKingdom != null)
                    {
                        spawned.setKingdom(fallbackKingdom);
                    }
                }

                if (pPowerID == "spawn_odin")
                {
                    spawned.setName("Odin", true);
                    spawned.data.name = "Odin";
                }
                else if (pPowerID == "spawn_thor")
                {
                    spawned.setName("Thor", true);
                    spawned.data.name = "Thor";
                }
                else if (pPowerID == "spawn_baldur")
                {
                    spawned.setName("Baldur", true);
                    spawned.data.name = "Baldur";
                }
                else if (pPowerID == "spawn_heimdall")
                {
                    spawned.setName("Heimdall", true);
                    spawned.data.name = "Heimdall";
                }
                else if (pPowerID == "spawn_kratos")
                {
                    spawned.setName("Kratos", true);
                    spawned.data.name = "Kratos";
                }
                else if (pPowerID == "spawn_kratosspear")
                {
                    spawned.setName("Kratos", true);
                    spawned.data.name = "Kratos";
                }
                else if (pPowerID == "spawn_kratosblades")
                {
                    spawned.setName("Kratos", true);
                    spawned.data.name = "Kratos";
                }
                else if (pPowerID == "spawn_jormungandr")
                {
                    spawned.setName("Jormungandr", true);
                    spawned.data.name = "Jormungandr";
                }
                else if (pPowerID == "spawn_atreus")
                {
                    spawned.setName("Atreus", true);
                    spawned.data.name = "Atreus";
                }
                else if (pPowerID == "spawn_freya")
                {
                    spawned.setName("Freya", true);
                    spawned.data.name = "Freya";
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}