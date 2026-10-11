using System;
using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
using System.Collections.Generic;
using ReflectionUtility;
using HarmonyLib;
using ai.behaviours;
using ai.behaviours.conditions;
using UnityEngine.EventSystems;
using System.Threading;
using NCMS;
using UnityEngine;
using System.Linq;
using System.Text;
using ai;
using Beebyte.Obfuscator;

namespace GOWB {
    public class GOWCommands : MonoBehaviour {
        public static Actor pActor;
        public static bool isPossessed = false;
        public static GOWCommands instance;

        public static void Init()
        {
            //create a new gameobject with this component attached (required to use Update)
            GameObject holder = new GameObject();
            holder.AddComponent<GOWCommands>();
        }

        private void Update() {
             instance = this;
             isPossessed = ControllableUnit.isControllingUnit();

            //Check if unit is possessed before reading key press

            if (isPossessed)
            {
                pActor = ControllableUnit._unit_main;
                if (pActor == null)
                {
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Alpha3) && pActor.hasTrait("Kratos"))
                {
                    if (pActor.asset.id == "Kratos")
                    {
                        return;
                    }
                    switchUnitTo(pActor, "Kratos", null);
                    // WorldTip.showNow("Leviathan Axe", false, "top", 2f, "#00d9ff");
                }

                if (Input.GetKeyDown(KeyCode.Alpha4) && pActor.hasTrait("Kratos"))
                {
                    if (pActor.asset.id == "Kratosblades")
                    {
                        return;
                    }
                    switchUnitTo(pActor, "Kratosblades", null);
                    // WorldTip.showNow("Blades of Chaos", false, "top", 2f, "#ff5e00");
                }

                if (Input.GetKeyDown(KeyCode.Alpha5) && pActor.hasTrait("Kratos"))
                {
                    if (pActor.asset.id == "Kratosspear")
                    {
                        return;
                    }
                    switchUnitTo(pActor, "Kratosspear", null);
                    // WorldTip.showNow("Draupnir Spear", false, "top", 2f, "#ffe100");
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (pActor.hasTrait("Odin") || pActor.hasTrait("Odin Phase 2"))
                    {
                        castRavenTeleport(pActor);
                    }

                    if (pActor.hasTrait("Thor"))
                    {
                        castThunderTeleport(pActor);
                    }

                    if (pActor.hasTrait("Baldur"))
                    {
                        superJump(pActor);
                    }
                }
            }

        }

        public static bool switchUnitTo(Actor pActor, string unitID, WorldTile pTile = null)
        {
            pTile = pActor.current_tile;
            var act = World.world.units.createNewUnit(unitID, pTile);
            if (act == null) return false;
            if (pActor.kingdom != null && pActor.kingdom.isAlive())
            {
                act.kingdom = pActor.kingdom;
                ActorTool.copyUnitToOtherUnit(pActor, act);
                act.data.health = pActor.data.health;
                act.data.level = pActor.data.level;

                if (pActor.hasStatus("possessed"))
                {
                    ControllableUnit.setControllableCreature(act);
                    ActionLibrary.removeUnit(pActor);
                }
                else
                {
                    ActionLibrary.removeUnit(pActor);
                }
            }
            return false;
        }

        public static bool castRavenTeleport(Actor caster)
        {
            WorldTile worldTile;
            if (caster == null)
            {
                return false;
            }
            worldTile = MapBox.instance.getMouseTilePos();
            Vector3 tPos = caster.cur_transform_position;
            Vector2 tHeadPos = caster.getRenderedItemPosition();
            tPos.y += tHeadPos.y * 9f * caster.current_scale.y -2f;
            EffectsLibrary.spawnAt("fx_odin_teleport", tPos, caster.actor_scale * 0.4f);
            EffectsLibrary.spawnAt("fx_odin_teleport", worldTile.pos, caster.actor_scale * 0.4f);
            caster.spawnOn(worldTile, -1f);

            return false;
        }

        public static bool castThunderTeleport(Actor caster)
        {
            WorldTile worldTile;
            if (caster == null)
            {
                return false;
            }
            worldTile = MapBox.instance.getMouseTilePos();
            MapBox.spawnLightningMedium(caster.current_tile, 0.25f);
            MapBox.spawnLightningMedium(worldTile, 0.25f);
            caster.spawnOn(worldTile, -1f);

            return false;
        }

        private static bool superJump(Actor pActor)
        {
            float tForceAmount = SimGlobals.m.gravity * 0.5f;
            tForceAmount = Mathf.Clamp(tForceAmount, 1.5f, 2.5f);
            tForceAmount += tForceAmount * 0.5f;
            float tForceHeight = tForceAmount;
            Vector2 tTargetPosition = pActor.current_position - ControllableUnit.getMovementVector();
            Vector2 tCurPosition = pActor.current_position;
            EffectsLibrary.spawnAt("fx_jump", tCurPosition, 0.1f);
            pActor.calculateForce(tCurPosition.x, tCurPosition.y, tTargetPosition.x, tTargetPosition.y, tForceAmount, tForceHeight, true);
            return false;
        }
    }
}