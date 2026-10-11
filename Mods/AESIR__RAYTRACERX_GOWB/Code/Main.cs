using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;
using HarmonyLib;
using NeoModLoader.api;

namespace GOWB
{
    public class Main : BasicMod<Main>
    {
        protected override void OnModLoad()
        {
            var harmony = new Harmony("GOWB");
            try
            {
                harmony.PatchAll(typeof(CheckLightningActionPatch));
                harmony.PatchAll(typeof(DontMoveIfPetrified));
                harmony.PatchAll(typeof(DropManagerSpawnByIdGuardPatch));
                harmony.PatchAll(typeof(DropUpdateGuardPatch));
                Debug.Log("RTX is soooo cooooool~");
            }
            catch (Exception e)
            {

                Debug.LogError("[GOWB] Patch Error: " + e.Message);
            }

            GOWStatus.Init();
            GOWProjectiles.Init();
            GOWEffects.Init();
            Clouds.Init();
            Spells.Init();
            GOWCombat.Init();
            updateSpellLinkAsset();
            GOWTraits.Init();
            GOWConfig.ApplyTraitConfigSettings();
            GOWItems.Init();
            Units.Init();
            JormOverrideSprite.init();
            Terraforms.Init();
            GOWTabs.Init();
            GOWKingdoms.Init();
            GOWCommands.Init();
        }

        public static void updateSpellLinkAsset()
        {
            foreach (SpellAsset tAsset in AssetManager.spells.list)
            {
                if (tAsset.decision_ids != null)
                {
                    tAsset.decisions_assets = new DecisionAsset[tAsset.decision_ids.Count];
                    for (int i = 0; i < tAsset.decision_ids.Count; i++)
                    {
                        string tDecisionID = tAsset.decision_ids[i];
                        DecisionAsset tDecisionAsset = AssetManager.decisions_library.get(tDecisionID);
                        tAsset.decisions_assets[i] = tDecisionAsset;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MapAction), "checkLightningAction")]
    public static class CheckLightningActionPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Vector2Int pPos, int pRad)
        {
            bool grantedImmortalThisStrike = false;
            var actors = World.world.units.getSimpleList();

            foreach (var actor in actors)
            {
                if (actor == null || actor.current_tile == null)
                {
                    continue;
                }

                if (Toolbox.DistVec2(actor.current_tile.pos, pPos) > pRad)
                {
                    continue;
                }

                if (!grantedImmortalThisStrike && !actor.hasTrait("immortal") && Randy.randomChance(0.2f))
                {
                    actor.addTrait("immortal", false);
                    actor.addTrait("energized", false);
                    grantedImmortalThisStrike = true;
                }

                if (actor.hasTrait("Thor"))
                {
                    actor.addStatusEffect("Thunder god's rage");
                }
            }
        }
    }

    [HarmonyPatch(typeof(Actor), "updateParallelChecks")]
    [HarmonyPatch(new Type[] { typeof(float) })]
    public static class DontMoveIfPetrified
    {
        [HarmonyPostfix]
        static void Postfix(Actor __instance)
        {
            if (CantMove(__instance))
            {
                __instance.stopMovement();
            }
        }

        public static bool CantMove(Actor a)
        {
            return a != null && (
                a.hasStatus("frozen") ||
                a.hasStatus("Vines Spell") ||
                a.hasStatus("Binding Spell")
            );
        }
    }

    [HarmonyPatch(typeof(DropManager), "spawn", new Type[] { typeof(WorldTile), typeof(string), typeof(float), typeof(float), typeof(long) })]
    public static class DropManagerSpawnByIdGuardPatch
    {
        private static readonly HashSet<string> MissingDropIdsLogged = new HashSet<string>();

        [HarmonyPrefix]
        public static bool Prefix(string pDropID, ref Drop __result)
        {
            if (string.IsNullOrEmpty(pDropID))
            {
                __result = null;
                return false;
            }

            if (AssetManager.drops.get(pDropID) != null)
            {
                return true;
            }

            if (!MissingDropIdsLogged.Contains(pDropID))
            {
                MissingDropIdsLogged.Add(pDropID);
                Debug.LogWarning("[GOWB] Prevented spawn of missing drop id: " + pDropID);
            }

            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Drop), "update", new Type[] { typeof(float) })]
    public static class DropUpdateGuardPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Drop __instance, ref DropAsset ____asset)
        {
            if (____asset != null)
            {
                return true;
            }

            __instance.makeInactive();
            return false;
        }
    }
}