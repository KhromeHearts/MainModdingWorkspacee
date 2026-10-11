using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ShinobiBox
{
    public enum JutsuTrigger
    {
        Attack,
        Hit,
        Combat
    }

    public enum JutsuChakra
    {
        Normal,
        TailedBeast,
        Sage,
        None
    }

    public class Jutsu
    {
        public string id;
        public JutsuTrigger trigger = JutsuTrigger.Attack;
        public JutsuChakra chakra = JutsuChakra.Normal;
        public float cost;
        public float cooldown;
        public float chance = 1f;
        public float range;
        public int priority;
        public bool signature;
        public string sound;
        public AttackAction action;
    }

    public static class ShinobiJutsu
    {
        private const float RecoveryTime = 1.5f;

        private enum SourceKind
        {
            Trait,
            Status,
            Unit
        }

        private class JutsuSource
        {
            public string id;
            public SourceKind kind;
            public Jutsu[] jutsu;
        }

        private static readonly List<JutsuSource> Sources = new List<JutsuSource>();
        private static readonly Stack<List<Jutsu>> CandidatePool = new Stack<List<Jutsu>>();

        #region Sharingan Line
        public static readonly Jutsu Genjutsu1 = new Jutsu { id = "genjutsu_1", cost = 20f, cooldown = 12f, chance = 0.25f, priority = 20, action = JutsuLibrary.Sharingan1Action };
        public static readonly Jutsu Genjutsu2 = new Jutsu { id = "genjutsu_2", cost = 30f, cooldown = 12f, chance = 0.25f, priority = 20, action = JutsuLibrary.Sharingan2Action };
        public static readonly Jutsu Genjutsu3 = new Jutsu { id = "genjutsu_3", cost = 50f, cooldown = 12f, chance = 0.25f, priority = 20, action = JutsuLibrary.Sharingan3Action };
        public static readonly Jutsu Amaterasu = new Jutsu { id = "amaterasu", cost = 120f, cooldown = 30f, chance = 0.30f, priority = 60, signature = true, sound = "sharingan_sound.wav", action = JutsuLibrary.AmaterasuAction };
        public static readonly Jutsu BaseSusanoo = new Jutsu { id = "base_susanoo", cost = 100f, cooldown = 25f, chance = 0.20f, priority = 55, signature = true, action = JutsuLibrary.BaseSusanooAction };
        public static readonly Jutsu PerfectSusanoo = new Jutsu { id = "perfect_susanoo", cost = 40f, cooldown = 15f, chance = 0.30f, priority = 55, signature = true, action = JutsuLibrary.PerfectSusanooAction };
        public static readonly Jutsu SusanooRibcage = new Jutsu { id = "susanoo_ribcage", trigger = JutsuTrigger.Hit, cost = 75f, cooldown = 30f, priority = 80, signature = true, action = (pSelf, pTarget, pTile) => JutsuLibrary.SusanooRibcage(pSelf, pTile) };
        public static readonly Jutsu Kamui = new Jutsu { id = "kamui", trigger = JutsuTrigger.Hit, cost = 100f, cooldown = 60f, chance = 0.15f, priority = 80, signature = true, sound = "kamui.wav", action = JutsuLibrary.KamuiAction };
        public static readonly Jutsu Amenotejikara = new Jutsu { id = "amenotejikara", trigger = JutsuTrigger.Hit, cost = 25f, cooldown = 20f, priority = 70, signature = true, sound = "sharingan_sound.wav", action = (pSelf, pTarget, pTile) => JutsuLibrary.Amenotejikara(pSelf, pTile) };
        public static readonly Jutsu MadaraPerfectSusanoo = new Jutsu { id = "madara_perfect_susanoo", cost = 40f, cooldown = 15f, chance = 0.30f, priority = 55, signature = true, action = JutsuLibrary.MadaraPerfectSusanoo };
        public static readonly Jutsu MadaraSusanooRibcage = new Jutsu { id = "madara_susanoo_ribcage", trigger = JutsuTrigger.Hit, cost = 75f, cooldown = 30f, priority = 80, signature = true, action = (pSelf, pTarget, pTile) => JutsuLibrary.MadaraSusanooRibcage(pSelf, pTile) };
        public static readonly Jutsu TengaiShinsei = new Jutsu { id = "tengai_shinsei", cost = 500f, cooldown = 120f, chance = 0.15f, priority = 100, signature = true, action = JutsuLibrary.MadaraTengaiShinsei };
        public static readonly Jutsu TailedBeastGenjutsu = new Jutsu { id = "tailed_beast_genjutsu", cost = 50f, cooldown = 30f, chance = 0.30f, priority = 90, signature = true, sound = "sharingan_sound.wav", action = JutsuLibrary.MadaraEMSGenjutsu };
        #endregion

        #region Rinnegan Line
        public static readonly Jutsu RinneganMeteor = new Jutsu { id = "rinnegan_meteor", cost = 300f, cooldown = 120f, chance = 0.10f, priority = 100, signature = true, action = JutsuLibrary.RinneganMeteorAction };
        public static readonly Jutsu NarakaPath = new Jutsu { id = "naraka_path", trigger = JutsuTrigger.Hit, cost = 300f, cooldown = 90f, chance = 0.50f, priority = 85, signature = true, action = JutsuLibrary.NarakaPathHealAction };
        public static readonly Jutsu SixPaths = new Jutsu { id = "six_paths", cost = 100f, cooldown = 20f, chance = 0.20f, priority = 50, signature = true, action = JutsuLibrary.SixPathsTechnique };
        public static readonly Jutsu ShinraTensei = new Jutsu { id = "shinra_tensei", trigger = JutsuTrigger.Hit, cost = 80f, cooldown = 15f, chance = 0.25f, priority = 75, signature = true, action = JutsuLibrary.DevaShinraTensei };
        public static readonly Jutsu InfiniteTsukuyomi = new Jutsu { id = "infinite_tsukuyomi", cost = 400f, cooldown = 600f, chance = 0.005f, priority = 100, signature = true, action = JutsuLibrary.InfiniteTsukuyomiAction };
        public static readonly Jutsu HeavenlyIceChamber = new Jutsu { id = "heavenly_ice_chamber", cost = 200f, cooldown = 45f, chance = 0.15f, priority = 70, signature = true, action = JutsuLibrary.HeavenlyIceChamberAction };
        public static readonly Jutsu TruthSeekingOrb = new Jutsu { id = "truth_seeking_orb", trigger = JutsuTrigger.Combat, chakra = JutsuChakra.None, cooldown = 6f, chance = 0.50f, range = 24f, priority = 50, action = JutsuLibrary.TruthSeekingOrbAction };
        #endregion

        #region Byakugan And Other Dojutsu
        public static readonly Jutsu PalmRotation = new Jutsu { id = "palm_rotation", cost = 80f, cooldown = 15f, chance = 0.15f, priority = 60, signature = true, action = JutsuLibrary.PalmRotationAttackAction };
        public static readonly Jutsu SixtyFourPalms = new Jutsu { id = "64_palms", cost = 80f, cooldown = 20f, chance = 0.20f, priority = 55, signature = true, action = JutsuLibrary.EightTrigrams64PalmsAction };
        public static readonly Jutsu KetsuryuganAwaken = new Jutsu { id = "ketsuryugan", trigger = JutsuTrigger.Hit, cost = 50f, cooldown = 45f, priority = 40, action = Ketsuryugan.KetsuryuganAction };
        public static readonly Jutsu ExplodingHuman = new Jutsu { id = "exploding_human", cost = 75f, cooldown = 30f, chance = 0.15f, priority = 50, signature = true, action = Ketsuryugan.KetsuryuganEHT };
        #endregion

        #region Clans And Bloodlines
        public static readonly Jutsu AdamantiteChains = new Jutsu { id = "adamantite_chains", cost = 100f, cooldown = 25f, chance = 0.20f, priority = 60, signature = true, action = JutsuLibrary.AdamantiteChainsAction };
        public static readonly Jutsu Expansion = new Jutsu { id = "akimichi_expansion", cost = 100f, cooldown = 40f, chance = 0.30f, priority = 50, signature = true, action = JutsuLibrary.AkimichiSizeManipulationAction };
        public static readonly Jutsu InsectSwarm = new Jutsu { id = "insect_swarm", trigger = JutsuTrigger.Hit, cost = 100f, cooldown = 25f, chance = 0.30f, priority = 50, signature = true, action = JutsuLibrary.AburameInsectSwarm };
        public static readonly Jutsu BoneDefense = new Jutsu { id = "bone_defense", trigger = JutsuTrigger.Hit, cost = 45f, cooldown = 15f, chance = 0.35f, priority = 40, action = Shikotsumyaku.ShikoDefense };
        public static readonly Jutsu BoneShot = new Jutsu { id = "bone_shot", cost = 25f, cooldown = 4f, chance = 0.35f, priority = 30, action = Shikotsumyaku.BoneShot };
        #endregion

        #region Natures
        public static readonly Jutsu FireBall = new Jutsu { id = "fire_ball", trigger = JutsuTrigger.Combat, cost = 50f, cooldown = 8f, chance = 0.30f, range = 24f, priority = 40, action = JutsuLibrary.FireReleaseAction };
        public static readonly Jutsu WindCutter = new Jutsu { id = "wind_cutter", trigger = JutsuTrigger.Combat, cost = 50f, cooldown = 8f, chance = 0.30f, range = 24f, priority = 40, action = JutsuLibrary.WindReleaseAction };
        public static readonly Jutsu WeightRock = new Jutsu { id = "weight_rock", cost = 60f, cooldown = 20f, chance = 0.20f, priority = 35, action = JutsuLibrary.EarthReleaseAction };
        public static readonly Jutsu WaterPrison = new Jutsu { id = "water_prison", cost = 50f, cooldown = 15f, chance = 0.20f, priority = 40, action = JutsuLibrary.WaterReleaseAction };
        public static readonly Jutsu LightningStrike = new Jutsu { id = "lightning_strike", cost = 60f, cooldown = 10f, chance = 0.20f, priority = 40, action = JutsuLibrary.LightningReleaseAction };
        public static readonly Jutsu StormLaser = new Jutsu { id = "storm_release", cost = 80f, cooldown = 12f, chance = 0.25f, priority = 45, action = JutsuLibrary.StormReleaseAction };
        public static readonly Jutsu WoodBind = new Jutsu { id = "wood_bind", cost = 50f, cooldown = 15f, chance = 0.25f, priority = 45, action = JutsuLibrary.WoodReleaseAction };
        #endregion

        #region Ninjutsu And Taijutsu
        public static readonly Jutsu Rasengan = new Jutsu { id = "rasengan", trigger = JutsuTrigger.Combat, cost = 50f, cooldown = 10f, chance = 0.60f, priority = 60, signature = true, sound = "rasengan.wav", action = (pSelf, pTarget, pTile) => JutsuLibrary.RasenganAction(pSelf, pTile) };
        public static readonly Jutsu Chidori = new Jutsu { id = "chidori", cost = 50f, cooldown = 10f, chance = 0.40f, priority = 60, signature = true, sound = "chidori.wav", action = JutsuLibrary.ChidoriAction };
        public static readonly Jutsu Rasenshuriken = new Jutsu { id = "rasenshuriken", trigger = JutsuTrigger.Combat, cost = 100f, cooldown = 20f, chance = 0.40f, range = 24f, priority = 65, signature = true, sound = "rasenshuriken.wav", action = JutsuLibrary.RasenshurikenAction };
        public static readonly Jutsu ShadowClone = new Jutsu { id = "shadow_clone", trigger = JutsuTrigger.Hit, chakra = JutsuChakra.None, cooldown = 30f, chance = 0.30f, priority = 70, signature = true, sound = "shadow_clone_spawn.wav", action = JutsuLibrary.ShadowCloneAction };
        public static readonly Jutsu ArmorBreak = new Jutsu { id = "armor_break", cost = 40f, cooldown = 10f, chance = 0.15f, priority = 30, action = JutsuLibrary.ArmorBreakAction };
        public static readonly Jutsu StrongPunch = new Jutsu { id = "strong_punch", trigger = JutsuTrigger.Hit, cost = 10f, cooldown = 6f, chance = 0.20f, priority = 30, action = JutsuLibrary.StrongPunch };
        #endregion

        #region Tailed Beasts
        public static readonly Jutsu TailedBeastBomb = new Jutsu { id = "tailed_beast_bomb", trigger = JutsuTrigger.Combat, chakra = JutsuChakra.TailedBeast, cost = 140f, cooldown = 20f, chance = 0.30f, range = 40f, priority = 90, signature = true, action = JutsuLibrary.TailedBeastBombAction };
        public static readonly Jutsu BeastBlast = new Jutsu { id = "beast_blast", chakra = JutsuChakra.TailedBeast, cost = 70f, cooldown = 12f, chance = 0.25f, priority = 60, signature = true, action = JutsuLibrary.KCMBlastAction };
        public static readonly Jutsu ChakraArmSwipe = new Jutsu { id = "chakra_arm_swipe", chakra = JutsuChakra.TailedBeast, cost = 50f, cooldown = 10f, chance = 0.30f, priority = 60, signature = true, action = JutsuLibrary.ChakraArmSwipeAction };
        public static readonly Jutsu BaryonDrain = new Jutsu { id = "baryon_drain", chakra = JutsuChakra.None, cooldown = 8f, chance = 0.30f, priority = 70, action = JutsuLibrary.BaryonLifespanDrainAction };
        #endregion

        #region Senjutsu
        public static readonly Jutsu FrogKumite = new Jutsu { id = "frog_kumite", chakra = JutsuChakra.Sage, cost = 15f, cooldown = 4f, chance = 0.25f, priority = 50, action = SageProgression.FrogKumite };
        public static readonly Jutsu SnakeBite = new Jutsu { id = "snake_bite", chakra = JutsuChakra.Sage, cost = 15f, cooldown = 6f, chance = 0.20f, priority = 50, action = SageProgression.SnakeBite };
        public static readonly Jutsu WoodSageBind = new Jutsu { id = "wood_sage_bind", chakra = JutsuChakra.Sage, cost = 15f, cooldown = 12f, chance = 0.15f, priority = 50, signature = true, action = SageProgression.WoodBind };
        #endregion

        public static void Init()
        {
            Sources.Clear();
            Status("status_sharingan_1t", Genjutsu1);
            Status("status_sharingan_2t", Genjutsu2);
            Status("status_sharingan_3t", Genjutsu3);
            Trait("mangekyo_sharingan", Amaterasu, BaseSusanoo, SusanooRibcage, Kamui);
            Trait("eternal_mangekyo", Amaterasu, PerfectSusanoo, SusanooRibcage, Kamui);
            Trait("madara_eternal_mangekyo", TengaiShinsei, MadaraPerfectSusanoo, TailedBeastGenjutsu, MadaraSusanooRibcage);
            Trait("sasuke_rinnegan", RinneganMeteor, NarakaPath, SixPaths, ShinraTensei, Amaterasu, PerfectSusanoo, TailedBeastGenjutsu, SusanooRibcage, Kamui, Amenotejikara);

            Trait("rinnegan", RinneganMeteor, NarakaPath, SixPaths, ShinraTensei);
            Trait("rinnesharingan", InfiniteTsukuyomi, HeavenlyIceChamber);
            Status("status_six_paths_senjutsu", TruthSeekingOrb);

            Trait("byakugan", PalmRotation, SixtyFourPalms);
            Trait("golden_byakugan", PalmRotation, SixtyFourPalms);
            Trait("ketsuryugan", KetsuryuganAwaken, ExplodingHuman);

            Trait("uzumaki_clan", AdamantiteChains);
            Trait("akimichi_clan", Expansion);
            Trait("aburame_clan", InsectSwarm);
            Trait("shikotsumyaku", BoneDefense, BoneShot);

            Trait("fire_release", FireBall);
            Trait("wind_release", WindCutter);
            Trait("earth_release", WeightRock);
            Trait("water_release", WaterPrison);
            Trait("lightning_release", LightningStrike);
            Trait("storm_release", StormLaser);
            Trait("wood_release", WoodBind);

            Trait("rasenganJ", Rasengan);
            Trait("chidoriJ", Chidori);
            Trait("rasenshurikenJ", Rasenshuriken);
            Trait("shadow_clonej", ShadowClone);
            Trait("taijutsu_master", ArmorBreak, StrongPunch);

            Unit(ShinobiActors.KuramaActorId, TailedBeastBomb, ChakraArmSwipe);
            Unit(ShinobiActors.JuubiActorId, TailedBeastBomb, BeastBlast, ArmorBreak);
            Trait("ten_tails_jinchuriki", TailedBeastBomb, BeastBlast, ArmorBreak);
            Status("status_jinchuriki_initial_release", StrongPunch, ArmorBreak);
            Status("status_jinchuriki_v1_cloak", StrongPunch, ArmorBreak);
            Status("status_jinchuriki_v2_cloak", TailedBeastBomb, BeastBlast, ArmorBreak);
            Status("status_jinchuriki_incomplete_beast", TailedBeastBomb, BeastBlast, ArmorBreak);
            Status("status_jinchuriki_kcm1", TailedBeastBomb, ChakraArmSwipe, ArmorBreak);
            Status("status_jinchuriki_kcm2", TailedBeastBomb, ChakraArmSwipe, ArmorBreak);
            Status("status_jinchuriki_baryon_mode", BaryonDrain, ArmorBreak);

            Status("status_frog_sage", FrogKumite);
            Status("status_snake_sage", SnakeBite);
            Status("status_wood_sage", WoodSageBind);
        }

        private static void Trait(string traitId, params Jutsu[] jutsu)
        {
            Sources.Add(new JutsuSource { id = traitId, kind = SourceKind.Trait, jutsu = jutsu });
        }

        private static void Status(string statusId, params Jutsu[] jutsu)
        {
            Sources.Add(new JutsuSource { id = statusId, kind = SourceKind.Status, jutsu = jutsu });
        }

        private static void Unit(string assetId, params Jutsu[] jutsu)
        {
            Sources.Add(new JutsuSource { id = assetId, kind = SourceKind.Unit, jutsu = jutsu });
        }

        #region Casting
        public static bool TryCast(Actor actor, BaseSimObject target, JutsuTrigger trigger)
        {
            if (actor == null || !actor.isAlive() || actor.current_tile == null) return false;
            if (!ChakraSystem.CanUseChakra(actor)) return false;
            if (actor.hasStatus("status_ability_cooldown")) return false;

            float now = (float)World.world.getCurWorldTime();
            actor.data.get("jutsu_recovery", out float recoveryUntil);
            if (now < recoveryUntil) return false;

            List<Jutsu> candidates = CandidatePool.Count > 0 ? CandidatePool.Pop() : new List<Jutsu>();
            try
            {
                CollectCandidates(actor, trigger, candidates);
                return CastFirstReady(actor, target, trigger, candidates, now, recoveryUntil);
            }
            finally
            {
                candidates.Clear();
                CandidatePool.Push(candidates);
            }
        }

        private static bool CastFirstReady(Actor actor, BaseSimObject target, JutsuTrigger trigger, List<Jutsu> candidates, float now, float recoveryUntil)
        {
            WorldTile tile = trigger == JutsuTrigger.Hit ? actor.current_tile : (target?.current_tile ?? actor.current_tile);

            foreach (Jutsu jutsu in candidates)
            {
                if (!IsReady(actor, jutsu.id, now)) continue;
                if (!CanAfford(actor, jutsu)) continue;
                if (jutsu.range > 0f && (target == null || target.current_tile == null || actor.distanceToActorTile(target.current_tile) > jutsu.range)) continue;
                if (!Randy.randomChance(jutsu.chance)) continue;

                actor.data.set("jutsu_recovery", now + RecoveryTime);
                if (!jutsu.action(actor, target, tile))
                {
                    actor.data.set("jutsu_recovery", recoveryUntil);
                    continue;
                }

                Pay(actor, jutsu);
                StartCooldown(actor, jutsu.id, jutsu.cooldown);

                if (jutsu.signature && actor.isAlive() && actor.current_tile != null)
                {
                    actor.addStatusEffect("jutsuuse", 1f);
                    JutsuLibrary.PlayWavSound(jutsu.sound ?? "jutsu_use.wav", actor.current_tile.posV3);
                }
                return true;
            }

            return false;
        }

        private static void CollectCandidates(Actor actor, JutsuTrigger trigger, List<Jutsu> candidates)
        {
            foreach (JutsuSource source in Sources)
            {
                if (!HasSource(actor, source)) continue;

                foreach (Jutsu jutsu in source.jutsu)
                {
                    if (jutsu.trigger == trigger && !candidates.Contains(jutsu)) candidates.Add(jutsu);
                }
            }

            candidates.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        private static bool HasSource(Actor actor, JutsuSource source)
        {
            switch (source.kind)
            {
                case SourceKind.Status:
                    return actor.hasStatus(source.id);
                case SourceKind.Unit:
                    return actor.asset.id == source.id;
                default:
                    return actor.hasTrait(source.id);
            }
        }

        public static bool IsReady(Actor actor, string cooldownId)
        {
            return IsReady(actor, cooldownId, (float)World.world.getCurWorldTime());
        }

        private static bool IsReady(Actor actor, string cooldownId, float now)
        {
            actor.data.get("cd_" + cooldownId, out float readyAt);
            return now >= readyAt;
        }

        public static void StartCooldown(Actor actor, string cooldownId, float seconds)
        {
            actor.data.set("cd_" + cooldownId, (float)World.world.getCurWorldTime() + seconds);
        }

        private static bool CanAfford(Actor actor, Jutsu jutsu)
        {
            switch (jutsu.chakra)
            {
                case JutsuChakra.Normal:
                    if (actor.hasStatus("chakra_exhaustion")) return false;
                    return ChakraSystem.GetCurrent(actor) >= ChakraSystem.GetJutsuCost(actor, jutsu.cost);
                case JutsuChakra.TailedBeast:
                    return ChakraSystem.GetTailedBeastChakra(actor) >= jutsu.cost;
                case JutsuChakra.Sage:
                    return ChakraSystem.GetSageChakra(actor) >= jutsu.cost;
                default:
                    return true;
            }
        }

        private static void Pay(Actor actor, Jutsu jutsu)
        {
            switch (jutsu.chakra)
            {
                case JutsuChakra.Normal:
                    ChakraSystem.TryConsume(actor, ChakraSystem.GetJutsuCost(actor, jutsu.cost));
                    break;
                case JutsuChakra.TailedBeast:
                    ChakraSystem.TryConsumeTailedBeastChakra(actor, jutsu.cost);
                    break;
                case JutsuChakra.Sage:
                    ChakraSystem.TryConsumeSageChakra(actor, jutsu.cost);
                    break;
            }
        }
        #endregion
    }

    [HarmonyPatch(typeof(MapBox), "applyAttack")]
    public static class JutsuAttackPatch
    {
        [HarmonyPostfix]
        public static void Postfix(AttackData pData, BaseSimObject pTargetToCheck, AttackDataResult __result)
        {
            if (__result.state != ApplyAttackState.Hit) return;
            if (pData.initiator == null || !pData.initiator.isActor()) return;
            if (pTargetToCheck == null || !pTargetToCheck.isActor() || !pTargetToCheck.isAlive()) return;

            ShinobiJutsu.TryCast(pData.initiator.a, pTargetToCheck, JutsuTrigger.Attack);
        }
    }

    [HarmonyPatch(typeof(Actor), "getHit")]
    public static class JutsuHitPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Actor __instance, BaseSimObject pAttacker)
        {
            if (!__instance.isAlive() || !__instance.hasHealth()) return;

            BaseSimObject attacker = pAttacker == __instance ? null : pAttacker;
            ShinobiJutsu.TryCast(__instance, attacker, JutsuTrigger.Hit);
        }
    }
}
