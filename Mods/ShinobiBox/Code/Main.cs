using System;
using UnityEngine;
using HarmonyLib;
using NeoModLoader.api;

namespace ShinobiBox
{
    public class Main : BasicMod<Main>
    {
        public const string HarmonyId = "com.shinobibox.mod";

        protected override void OnModLoad()
        {
            var harmony = new Harmony(HarmonyId);
            try
            {
                harmony.PatchAll();

                Debug.Log("[ShinobiBox] All systems loaded.");
            }
            catch (Exception e)
            {
                Debug.LogError("[ShinobiBox] Patch Error: " + e.Message);
            }

            ShinobiTraits.Init();
            ChakraSystem.RegisterChakraBaseStat();
            ShinobiActors.Init();
            ShinobiProjectiles.Init();
            ShinobiItems.Init();
            ShinobiTab.Init();
            ShinobiStatus.Init();
            Shikotsumyaku.Init();
            ShinobiEffects.Init();
            ShinobiKingdoms.Init();

            if (GameObject.Find("ShinobiBox_Logic") == null)
            {
                var go = new GameObject("ShinobiBox_Logic");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<ShinobiLoop>();
            }
        }
    }

    public class ShinobiLoop : MonoBehaviour
    {
        private float timer = 0f;
        private float regenTimer = 0f;
        private float progressionTimer = 0f;
        private bool startupChecksDone = false;

        void Update()
        {
            if (!Config.game_loaded) return;

            if (!startupChecksDone)
            {
                startupChecksDone = true;
                BurningEffectPatch.SkipIfStandaloneModLoaded();
            }

            if (World.world.isPaused()) return;

            JutsuLibrary.TickWorldAgeRevert(Time.deltaTime);

            timer += Time.deltaTime;
            if (timer < 0.1f) return;

            float elapsed = timer;
            timer = 0f;

            regenTimer += elapsed;
            bool doRegen = regenTimer >= 1f;
            if (doRegen) regenTimer = 0f;

            progressionTimer += elapsed;
            bool doProgression = progressionTimer >= 3f;
            if (doProgression) progressionTimer = 0f;

            int akatsukiMembers = 0;

            foreach (Actor actor in MapBox.instance.units)
            {
                if (actor == null || !actor.isAlive()) continue;

                if (actor.hasStatusStunned())
                {
                    actor.finishStatusEffect("stunned");
                }

                if (ChakraSystem.CanUseChakra(actor))
                {
                    ChakraSystem.Tick(actor, elapsed);
                }

                if (doRegen)
                {
                    ApplyClanRegeneration(actor);
                }

                if (doProgression)
                {
                    ShinobiProgression.CheckProgression(actor);
                    ShinobiProgression.removeWeapon(actor);
                    if (actor.hasTrait("trait_akatsuki")) akatsukiMembers++;
                }
            }

            if (doProgression)
            {
                ShinobiProgression.AkatsukiMembers = akatsukiMembers;
            }
        }

        private static void ApplyClanRegeneration(Actor a)
        {
            if (a.data.health >= a.getMaxHealth()) return;

            if (a.hasTrait("uzumaki_clan")) a.restoreHealth(4);
            if (a.hasTrait("senju_clan")) a.restoreHealth(1);
            if (a.hasTrait("hashi_cells")) a.restoreHealth(5);
            if (a.hasTrait("06rank_kage")) a.restoreHealth(1);
        }
    }
}