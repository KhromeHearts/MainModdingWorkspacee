using System.Collections.Generic;
using HarmonyLib;

namespace ShinobiBox
{
    public enum JutsuTrigger
    {
        Attack,
        Hit,
        Combat
    }

    public class Jutsu
    {
        public string id = "";
        public JutsuTrigger trigger = JutsuTrigger.Attack;
        public float cost = 0;
        public bool tailedBeastChakra = false;
        public bool sageChakra = false;
        public float cooldown = 0;
        public float chance = 1f;
        public float range = 0;
        public bool signature = false;
        public string sound = "jutsu_use.wav";
        public string[] traits;
        public string[] statuses;
        public string[] units;
        public AttackAction action;

        public bool CanBeUsedBy(Actor a)
        {
            if (traits != null)
            {
                foreach (string trait in traits)
                {
                    if (a.hasTrait(trait)) return true;
                }
            }

            if (statuses != null)
            {
                foreach (string status in statuses)
                {
                    if (a.hasStatus(status)) return true;
                }
            }

            if (units != null)
            {
                foreach (string unit in units)
                {
                    if (a.asset.id == unit) return true;
                }
            }

            return false;
        }

        public bool Cast(Actor a, BaseSimObject pTarget, WorldTile pTile)
        {
            if (!ShinobiJutsu.IsReady(a, id)) return false;
            if (!HasEnoughChakra(a)) return false;
            if (range > 0 && (pTarget == null || pTarget.current_tile == null || a.distanceToActorTile(pTarget.current_tile) > range)) return false;
            if (!Randy.randomChance(chance)) return false;

            ShinobiJutsu.StartCooldown(a, id, cooldown);
            if (!action(a, pTarget, pTile))
            {
                ShinobiJutsu.StartCooldown(a, id, 0f);
                return false;
            }

            UseChakra(a);

            if (signature && a.isAlive() && a.current_tile != null)
            {
                a.addStatusEffect("jutsuuse", 1f);
                JutsuLibrary.PlayWavSound(sound, a.current_tile.posV3);
            }
            return true;
        }

        private bool HasEnoughChakra(Actor a)
        {
            if (cost <= 0) return true;
            if (tailedBeastChakra) return ChakraSystem.GetTailedBeastChakra(a) >= cost;
            if (sageChakra) return ChakraSystem.GetSageChakra(a) >= cost;
            if (a.hasStatus("chakra_exhaustion")) return false;
            return ChakraSystem.GetCurrent(a) >= ChakraSystem.GetJutsuCost(a, cost);
        }

        private void UseChakra(Actor a)
        {
            if (cost <= 0) return;

            if (tailedBeastChakra) ChakraSystem.TryConsumeTailedBeastChakra(a, cost);
            else if (sageChakra) ChakraSystem.TryConsumeSageChakra(a, cost);
            else ChakraSystem.TryConsume(a, ChakraSystem.GetJutsuCost(a, cost));
        }
    }

    public static class ShinobiJutsu
    {
        public static List<Jutsu> AllJutsu = new List<Jutsu>();

        #region Sharingan
        public static Jutsu Genjutsu1 = new Jutsu()
        {
            id = "genjutsu_1",
            cost = 20f,
            cooldown = 12f,
            chance = 0.25f,
            statuses = new string[] { "status_sharingan_1t" },
            action = JutsuLibrary.Sharingan1Action
        };

        public static Jutsu Genjutsu2 = new Jutsu()
        {
            id = "genjutsu_2",
            cost = 30f,
            cooldown = 12f,
            chance = 0.25f,
            statuses = new string[] { "status_sharingan_2t" },
            action = JutsuLibrary.Sharingan2Action
        };

        public static Jutsu Genjutsu3 = new Jutsu()
        {
            id = "genjutsu_3",
            cost = 50f,
            cooldown = 12f,
            chance = 0.25f,
            statuses = new string[] { "status_sharingan_3t" },
            action = JutsuLibrary.Sharingan3Action
        };

        public static Jutsu Amaterasu = new Jutsu()
        {
            id = "amaterasu",
            cost = 120f,
            cooldown = 30f,
            chance = 0.30f,
            signature = true,
            sound = "sharingan_sound.wav",
            traits = new string[] { "mangekyo_sharingan", "eternal_mangekyo", "sasuke_rinnegan" },
            action = JutsuLibrary.AmaterasuAction
        };

        public static Jutsu Kamui = new Jutsu()
        {
            id = "kamui",
            trigger = JutsuTrigger.Hit,
            cost = 100f,
            cooldown = 60f,
            chance = 0.15f,
            signature = true,
            sound = "kamui.wav",
            traits = new string[] { "mangekyo_sharingan" },
            action = JutsuLibrary.KamuiAction
        };

        public static Jutsu SusanooRibcage = new Jutsu()
        {
            id = "susanoo_ribcage",
            trigger = JutsuTrigger.Hit,
            cost = 75f,
            cooldown = 30f,
            signature = true,
            traits = new string[] { "mangekyo_sharingan", "eternal_mangekyo", "sasuke_rinnegan" },
            action = JutsuLibrary.SusanooRibcage
        };

        public static Jutsu BaseSusanoo = new Jutsu()
        {
            id = "base_susanoo",
            cost = 100f,
            cooldown = 60f,
            chance = 0.20f,
            signature = true,
            traits = new string[] { "mangekyo_sharingan" },
            action = JutsuLibrary.BaseSusanooAction
        };

        public static Jutsu PerfectSusanoo = new Jutsu()
        {
            id = "perfect_susanoo",
            cost = 150f,
            cooldown = 60f,
            chance = 0.25f,
            signature = true,
            traits = new string[] { "eternal_mangekyo", "sasuke_rinnegan" },
            action = JutsuLibrary.PerfectSusanooAction
        };

        public static Jutsu SusanooStrike = new Jutsu()
        {
            id = "susanoo_strike",
            cooldown = 3f,
            chance = 0.50f,
            statuses = new string[] { "status_base_susanoo", "status_perfect_susanoo", "madara_ps" },
            action = JutsuLibrary.SusanooStrikeAction
        };

        public static Jutsu Amenotejikara = new Jutsu()
        {
            id = "amenotejikara",
            trigger = JutsuTrigger.Hit,
            cost = 25f,
            cooldown = 20f,
            signature = true,
            sound = "sharingan_sound.wav",
            traits = new string[] { "sasuke_rinnegan" },
            action = JutsuLibrary.Amenotejikara
        };
        #endregion

        #region Madara
        public static Jutsu MadaraPerfectSusanoo = new Jutsu()
        {
            id = "madara_perfect_susanoo",
            cost = 150f,
            cooldown = 60f,
            chance = 0.25f,
            signature = true,
            traits = new string[] { "madara_eternal_mangekyo" },
            action = JutsuLibrary.MadaraPerfectSusanoo
        };

        public static Jutsu MadaraSusanooRibcage = new Jutsu()
        {
            id = "madara_susanoo_ribcage",
            trigger = JutsuTrigger.Hit,
            cost = 75f,
            cooldown = 30f,
            signature = true,
            traits = new string[] { "madara_eternal_mangekyo" },
            action = JutsuLibrary.MadaraSusanooRibcage
        };

        public static Jutsu TengaiShinsei = new Jutsu()
        {
            id = "tengai_shinsei",
            cost = 500f,
            cooldown = 120f,
            chance = 0.15f,
            signature = true,
            traits = new string[] { "madara_eternal_mangekyo" },
            action = JutsuLibrary.MadaraTengaiShinsei
        };

        public static Jutsu TailedBeastGenjutsu = new Jutsu()
        {
            id = "tailed_beast_genjutsu",
            cost = 50f,
            cooldown = 30f,
            chance = 0.30f,
            signature = true,
            sound = "sharingan_sound.wav",
            traits = new string[] { "madara_eternal_mangekyo", "sasuke_rinnegan" },
            action = JutsuLibrary.MadaraEMSGenjutsu
        };
        #endregion

        #region Rinnegan
        public static Jutsu RinneganMeteor = new Jutsu()
        {
            id = "rinnegan_meteor",
            cost = 300f,
            cooldown = 120f,
            chance = 0.10f,
            signature = true,
            traits = new string[] { "rinnegan", "sasuke_rinnegan" },
            action = JutsuLibrary.RinneganMeteorAction
        };

        public static Jutsu NarakaPath = new Jutsu()
        {
            id = "naraka_path",
            trigger = JutsuTrigger.Hit,
            cost = 300f,
            cooldown = 90f,
            chance = 0.50f,
            signature = true,
            traits = new string[] { "rinnegan", "sasuke_rinnegan" },
            action = JutsuLibrary.NarakaPathHealAction
        };

        public static Jutsu SixPaths = new Jutsu()
        {
            id = "six_paths",
            cost = 100f,
            cooldown = 20f,
            chance = 0.20f,
            signature = true,
            traits = new string[] { "rinnegan", "sasuke_rinnegan" },
            action = JutsuLibrary.SixPathsTechnique
        };

        public static Jutsu ShinraTensei = new Jutsu()
        {
            id = "shinra_tensei",
            trigger = JutsuTrigger.Hit,
            cost = 80f,
            cooldown = 15f,
            chance = 0.25f,
            signature = true,
            traits = new string[] { "rinnegan", "sasuke_rinnegan" },
            action = JutsuLibrary.DevaShinraTensei
        };

        public static Jutsu InfiniteTsukuyomi = new Jutsu()
        {
            id = "infinite_tsukuyomi",
            cost = 400f,
            cooldown = 600f,
            chance = 0.005f,
            signature = true,
            traits = new string[] { "rinnesharingan" },
            action = JutsuLibrary.InfiniteTsukuyomiAction
        };

        public static Jutsu HeavenlyIceChamber = new Jutsu()
        {
            id = "heavenly_ice_chamber",
            cost = 200f,
            cooldown = 45f,
            chance = 0.15f,
            signature = true,
            traits = new string[] { "rinnesharingan" },
            action = JutsuLibrary.HeavenlyIceChamberAction
        };

        public static Jutsu TruthSeekingOrb = new Jutsu()
        {
            id = "truth_seeking_orb",
            trigger = JutsuTrigger.Combat,
            cooldown = 6f,
            chance = 0.50f,
            range = 24f,
            statuses = new string[] { "status_six_paths_senjutsu" },
            action = JutsuLibrary.TruthSeekingOrbAction
        };
        #endregion

        #region Byakugan And Ketsuryugan
        public static Jutsu PalmRotation = new Jutsu()
        {
            id = "palm_rotation",
            cost = 80f,
            cooldown = 15f,
            chance = 0.15f,
            signature = true,
            traits = new string[] { "byakugan", "golden_byakugan" },
            action = JutsuLibrary.PalmRotationAttackAction
        };

        public static Jutsu SixtyFourPalms = new Jutsu()
        {
            id = "64_palms",
            cost = 80f,
            cooldown = 20f,
            chance = 0.20f,
            signature = true,
            traits = new string[] { "byakugan", "golden_byakugan" },
            action = JutsuLibrary.EightTrigrams64PalmsAction
        };

        public static Jutsu KetsuryuganAwaken = new Jutsu()
        {
            id = "ketsuryugan",
            trigger = JutsuTrigger.Hit,
            cost = 50f,
            cooldown = 45f,
            traits = new string[] { "ketsuryugan" },
            action = Ketsuryugan.KetsuryuganAction
        };

        public static Jutsu ExplodingHuman = new Jutsu()
        {
            id = "exploding_human",
            cost = 75f,
            cooldown = 30f,
            chance = 0.15f,
            signature = true,
            traits = new string[] { "ketsuryugan" },
            action = Ketsuryugan.KetsuryuganEHT
        };
        #endregion

        #region Clans
        public static Jutsu AdamantiteChains = new Jutsu()
        {
            id = "adamantite_chains",
            cost = 100f,
            cooldown = 25f,
            chance = 0.20f,
            signature = true,
            traits = new string[] { "uzumaki_clan" },
            action = JutsuLibrary.AdamantiteChainsAction
        };

        public static Jutsu Expansion = new Jutsu()
        {
            id = "akimichi_expansion",
            cost = 100f,
            cooldown = 40f,
            chance = 0.30f,
            signature = true,
            traits = new string[] { "akimichi_clan" },
            action = JutsuLibrary.AkimichiSizeManipulationAction
        };

        public static Jutsu InsectSwarm = new Jutsu()
        {
            id = "insect_swarm",
            trigger = JutsuTrigger.Hit,
            cost = 100f,
            cooldown = 25f,
            chance = 0.30f,
            signature = true,
            traits = new string[] { "aburame_clan" },
            action = JutsuLibrary.AburameInsectSwarm
        };

        public static Jutsu BoneDefense = new Jutsu()
        {
            id = "bone_defense",
            trigger = JutsuTrigger.Hit,
            cost = 45f,
            cooldown = 15f,
            chance = 0.35f,
            traits = new string[] { "shikotsumyaku" },
            action = Shikotsumyaku.ShikoDefense
        };

        public static Jutsu BoneShot = new Jutsu()
        {
            id = "bone_shot",
            cost = 25f,
            cooldown = 4f,
            chance = 0.35f,
            traits = new string[] { "shikotsumyaku" },
            action = Shikotsumyaku.BoneShot
        };
        #endregion

        #region Natures
        public static Jutsu FireBall = new Jutsu()
        {
            id = "fire_ball",
            trigger = JutsuTrigger.Combat,
            cost = 50f,
            cooldown = 8f,
            chance = 0.30f,
            range = 24f,
            traits = new string[] { "fire_release" },
            action = JutsuLibrary.FireReleaseAction
        };

        public static Jutsu WindCutter = new Jutsu()
        {
            id = "wind_cutter",
            trigger = JutsuTrigger.Combat,
            cost = 50f,
            cooldown = 8f,
            chance = 0.30f,
            range = 24f,
            traits = new string[] { "wind_release" },
            action = JutsuLibrary.WindReleaseAction
        };

        public static Jutsu WeightRock = new Jutsu()
        {
            id = "weight_rock",
            cost = 60f,
            cooldown = 20f,
            chance = 0.20f,
            traits = new string[] { "earth_release" },
            action = JutsuLibrary.EarthReleaseAction
        };

        public static Jutsu WaterPrison = new Jutsu()
        {
            id = "water_prison",
            cost = 50f,
            cooldown = 15f,
            chance = 0.20f,
            traits = new string[] { "water_release" },
            action = JutsuLibrary.WaterReleaseAction
        };

        public static Jutsu LightningStrike = new Jutsu()
        {
            id = "lightning_strike",
            cost = 60f,
            cooldown = 10f,
            chance = 0.20f,
            traits = new string[] { "lightning_release" },
            action = JutsuLibrary.LightningReleaseAction
        };

        public static Jutsu StormLaser = new Jutsu()
        {
            id = "storm_release",
            cost = 80f,
            cooldown = 12f,
            chance = 0.25f,
            traits = new string[] { "storm_release" },
            action = JutsuLibrary.StormReleaseAction
        };

        public static Jutsu WoodBind = new Jutsu()
        {
            id = "wood_bind",
            cost = 50f,
            cooldown = 15f,
            chance = 0.25f,
            traits = new string[] { "wood_release" },
            action = JutsuLibrary.WoodReleaseAction
        };
        #endregion

        #region Ninjutsu And Taijutsu
        public static Jutsu Rasengan = new Jutsu()
        {
            id = "rasengan",
            trigger = JutsuTrigger.Combat,
            cost = 50f,
            cooldown = 10f,
            chance = 0.60f,
            signature = true,
            sound = "rasengan.wav",
            traits = new string[] { "rasenganJ" },
            action = JutsuLibrary.RasenganAction
        };

        public static Jutsu Chidori = new Jutsu()
        {
            id = "chidori",
            cost = 50f,
            cooldown = 10f,
            chance = 0.40f,
            signature = true,
            sound = "chidori.wav",
            traits = new string[] { "chidoriJ" },
            action = JutsuLibrary.ChidoriAction
        };

        public static Jutsu Rasenshuriken = new Jutsu()
        {
            id = "rasenshuriken",
            trigger = JutsuTrigger.Combat,
            cost = 100f,
            cooldown = 20f,
            chance = 0.40f,
            range = 24f,
            signature = true,
            sound = "rasenshuriken.wav",
            traits = new string[] { "rasenshurikenJ" },
            action = JutsuLibrary.RasenshurikenAction
        };

        public static Jutsu ShadowClone = new Jutsu()
        {
            id = "shadow_clone",
            trigger = JutsuTrigger.Hit,
            cooldown = 30f,
            chance = 0.30f,
            signature = true,
            sound = "shadow_clone_spawn.wav",
            traits = new string[] { "shadow_clonej" },
            action = JutsuLibrary.ShadowCloneAction
        };

        public static Jutsu ArmorBreak = new Jutsu()
        {
            id = "armor_break",
            cost = 40f,
            cooldown = 10f,
            chance = 0.15f,
            traits = new string[] { "taijutsu_master", "ten_tails_jinchuriki" },
            statuses = new string[] { "status_jinchuriki_initial_release", "status_jinchuriki_v1_cloak", "status_jinchuriki_v2_cloak", "status_jinchuriki_incomplete_beast", "status_jinchuriki_kcm1", "status_jinchuriki_kcm2", "status_jinchuriki_baryon_mode" },
            action = JutsuLibrary.ArmorBreakAction
        };

        public static Jutsu StrongPunch = new Jutsu()
        {
            id = "strong_punch",
            trigger = JutsuTrigger.Hit,
            cost = 10f,
            cooldown = 6f,
            chance = 0.20f,
            traits = new string[] { "taijutsu_master" },
            statuses = new string[] { "status_jinchuriki_initial_release", "status_jinchuriki_v1_cloak" },
            action = JutsuLibrary.StrongPunch
        };
        #endregion

        #region Tailed Beasts
        public static Jutsu TailedBeastBomb = new Jutsu()
        {
            id = "tailed_beast_bomb",
            trigger = JutsuTrigger.Combat,
            cost = 140f,
            tailedBeastChakra = true,
            cooldown = 20f,
            chance = 0.30f,
            range = 40f,
            signature = true,
            traits = new string[] { "ten_tails_jinchuriki" },
            statuses = new string[] { "status_jinchuriki_v2_cloak", "status_jinchuriki_incomplete_beast", "status_jinchuriki_kcm1", "status_jinchuriki_kcm2" },
            units = new string[] { "kurama" },
            action = JutsuLibrary.TailedBeastBombAction
        };

        public static Jutsu BeastBlast = new Jutsu()
        {
            id = "beast_blast",
            cost = 70f,
            tailedBeastChakra = true,
            cooldown = 12f,
            chance = 0.25f,
            signature = true,
            traits = new string[] { "ten_tails_jinchuriki" },
            statuses = new string[] { "status_jinchuriki_v2_cloak", "status_jinchuriki_incomplete_beast" },
            action = JutsuLibrary.KCMBlastAction
        };

        public static Jutsu ChakraArmSwipe = new Jutsu()
        {
            id = "chakra_arm_swipe",
            cost = 50f,
            tailedBeastChakra = true,
            cooldown = 10f,
            chance = 0.30f,
            signature = true,
            statuses = new string[] { "status_jinchuriki_kcm1", "status_jinchuriki_kcm2" },
            units = new string[] { "kurama" },
            action = JutsuLibrary.ChakraArmSwipeAction
        };

        public static Jutsu BaryonDrain = new Jutsu()
        {
            id = "baryon_drain",
            cooldown = 8f,
            chance = 0.30f,
            statuses = new string[] { "status_jinchuriki_baryon_mode" },
            action = JutsuLibrary.BaryonLifespanDrainAction
        };
        #endregion

        #region Senjutsu
        public static Jutsu FrogKumite = new Jutsu()
        {
            id = "frog_kumite",
            cost = 15f,
            sageChakra = true,
            cooldown = 4f,
            chance = 0.25f,
            statuses = new string[] { "status_frog_sage" },
            action = SageProgression.FrogKumite
        };

        public static Jutsu SnakeBite = new Jutsu()
        {
            id = "snake_bite",
            cost = 15f,
            sageChakra = true,
            cooldown = 6f,
            chance = 0.20f,
            statuses = new string[] { "status_snake_sage" },
            action = SageProgression.SnakeBite
        };

        public static Jutsu WoodSageBind = new Jutsu()
        {
            id = "wood_sage_bind",
            cost = 15f,
            sageChakra = true,
            cooldown = 12f,
            chance = 0.15f,
            signature = true,
            statuses = new string[] { "status_wood_sage" },
            action = SageProgression.WoodBind
        };
        #endregion

        public static void Init()
        {
            AllJutsu = new List<Jutsu>()
            {
                TengaiShinsei, RinneganMeteor, InfiniteTsukuyomi,
                TailedBeastGenjutsu, TailedBeastBomb, NarakaPath,
                SusanooRibcage, MadaraSusanooRibcage, Kamui, ShinraTensei,
                Amenotejikara, HeavenlyIceChamber, ShadowClone, BaryonDrain, Rasenshuriken,
                Amaterasu, PalmRotation, AdamantiteChains, Rasengan, Chidori, BeastBlast, ChakraArmSwipe,
                BaseSusanoo, PerfectSusanoo, MadaraPerfectSusanoo, SixtyFourPalms,
                SixPaths, TruthSeekingOrb, ExplodingHuman, Expansion, InsectSwarm, SusanooStrike,
                FrogKumite, SnakeBite, WoodSageBind,
                StormLaser, WoodBind,
                KetsuryuganAwaken, BoneDefense, FireBall, WindCutter, WaterPrison, LightningStrike,
                WeightRock, BoneShot, ArmorBreak, StrongPunch,
                Genjutsu1, Genjutsu2, Genjutsu3
            };
        }

        public static bool TryCast(Actor a, BaseSimObject pTarget, JutsuTrigger trigger)
        {
            if (a == null || !a.isAlive() || a.current_tile == null) return false;
            if (!ChakraSystem.CanUseChakra(a)) return false;
            if (a.hasStatus("status_ability_cooldown")) return false;
            if (!IsReady(a, "jutsu_recovery")) return false;

            WorldTile tile = a.current_tile;
            if (trigger != JutsuTrigger.Hit && pTarget != null && pTarget.current_tile != null)
            {
                tile = pTarget.current_tile;
            }

            foreach (Jutsu jutsu in AllJutsu)
            {
                if (jutsu.trigger != trigger) continue;
                if (!jutsu.CanBeUsedBy(a)) continue;

                if (jutsu.Cast(a, pTarget, tile))
                {
                    StartCooldown(a, "jutsu_recovery", 1.5f);
                    return true;
                }
            }

            return false;
        }

        public static bool IsReady(Actor a, string cooldownId)
        {
            a.data.get("cd_" + cooldownId, out float readyAt);
            return World.world.getCurWorldTime() >= readyAt;
        }

        public static void StartCooldown(Actor a, string cooldownId, float seconds)
        {
            a.data.set("cd_" + cooldownId, (float)World.world.getCurWorldTime() + seconds);
        }
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

            BaseSimObject attacker = pAttacker;
            if (attacker == __instance) attacker = null;

            ShinobiJutsu.TryCast(__instance, attacker, JutsuTrigger.Hit);
        }
    }
}
