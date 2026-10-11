using System;
using UnityEngine;
using HarmonyLib;
using NeoModLoader.api;

namespace ShinobiBox
{
    public class Main : BasicMod<Main>
    {
        protected override void OnModLoad()
        {
            var harmony = new Harmony("com.shinobibox.mod");
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
        private float clearTimer = 0f;
        private float regenTimer = 0f;
        private float progressionTimer = 0f;
        private bool startupChecksDone = false;

        void Update()
        {
            if (!Config.game_loaded) return;

            // Runs once, after every mod and the game itself have finished loading.
            if (!startupChecksDone)
            {
                startupChecksDone = true;
                ShinobiSelfCheck.Run();
            }

            if (World.world.isPaused()) return;

            JutsuLibrary.TickWorldAgeRevert(Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                foreach (Actor actor in MapBox.instance.units)
                {
                    if (actor == null || !actor.isAlive()) continue;

                    // Anti-stun (same as the AntiStun mod): stuns can chain forever and decide a fight on the first hit.
                    if (actor.hasStatusStunned())
                    {
                        actor.finishStatusEffect(StatusIds.Stunned);
                    }

                    if (!ChakraSystem.IsChakraUser(actor)) continue;

                    ChakraSystem.UpdateMax(actor);
                    ChakraSystem.ProcessDrain(actor, 0.1f);
                    ChakraSystem.UpdateSpecialResources(actor, 0.1f);

                    // Chakra Regen. Exhaustion only slows it down (see GetRegenRate), so a unit always recovers.
                    if (!ShinobiProgression.IsModeActive(actor))
                    {
                        ChakraSystem.AddChakra(actor, ChakraSystem.GetRegenRate(actor) * 0.1f);
                    }
                }
                timer = 0f;
            }

            regenTimer += Time.deltaTime;
            if (regenTimer >= 1.0f)
            {
                foreach (Actor a in MapBox.instance.units)
                {
                    if (a.isAlive())
                    {
                        if (a.hasTrait(TraitIds.UzumakiClan) && a.data.health < a.getMaxHealth()) a.restoreHealth(4);
                        if (a.hasTrait(TraitIds.SenjuClan) && a.data.health < a.getMaxHealth()) a.restoreHealth(1);
                        if (a.hasTrait(TraitIds.HashiramaCells) && a.data.health < a.getMaxHealth()) a.restoreHealth(5);
                        if (a.hasTrait(TraitIds.Kage) && a.data.health < a.getMaxHealth()) a.restoreHealth(1);
                    }
                }
                regenTimer = 0f;
            }

            progressionTimer += Time.deltaTime;
            if (progressionTimer >= 3.0f)
            {
                int akatsukiMembers = 0;
                foreach (Actor a in MapBox.instance.units)
                {
                    if (a.isAlive())
                    {
                        ShinobiProgression.CheckProgression(a);
                        if (a.hasTrait(TraitIds.Akatsuki)) akatsukiMembers++;
                    }
                }
                ShinobiProgression.AkatsukiMembers = akatsukiMembers;
                progressionTimer = 0f;
            }

            clearTimer += Time.deltaTime;
            if (clearTimer >= 5f)
            {
                foreach (Actor a in MapBox.instance.units)
                {
                    ShinobiProgression.removeWeapon(a);
                }

                clearTimer = 0f;
            }

        }
    }
}