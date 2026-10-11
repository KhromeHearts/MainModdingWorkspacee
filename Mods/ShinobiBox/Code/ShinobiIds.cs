using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ShinobiBox
{
    // Every trait / status / effect / projectile / item id ShinobiBox uses, in one place.
    // Always use these instead of typing "some_id" by hand: a typo in a constant name is a compile error,
    // while a typo in a string just makes the feature silently never happen.
    // ShinobiSelfCheck (bottom of this file) checks at startup that every id below really exists in the game.

    public static class TraitIds
    {
        #region Clans
        public const string HyugaClan = "hyuga_clan";
        public const string UzumakiClan = "uzumaki_clan";
        public const string KaguyaClan = "kaguya_clan";
        public const string OtsutsukiClan = "otsutsuki_clan";
        public const string SenjuClan = "senju_clan";
        public const string UchihaClan = "uchiha_clan";
        public const string AkimichiClan = "akimichi_clan";
        public const string LeeClan = "lee_clan";
        public const string ChinoikeClan = "chinoike_clan";
        public const string AburameClan = "aburame_clan";
        #endregion

        #region Dojutsu
        public const string Byakugan = "byakugan";
        public const string GoldenByakugan = "golden_byakugan";
        public const string Rinnegan = "rinnegan";
        public const string RinneSharingan = "rinnesharingan";
        public const string Sharingan = "sharingan";
        public const string Mangekyo = "mangekyo_sharingan";
        public const string MadaraEternalMangekyo = "madara_eternal_mangekyo";
        public const string EternalMangekyo = "eternal_mangekyo";
        public const string SasukeRinnegan = "sasuke_rinnegan";
        public const string Tenseigan = "tenseigan";
        public const string Ketsuryugan = "ketsuryugan";
        public const string Blind = "trait_blind";
        #endregion

        #region Ranks
        public const string AcademyStudent = "01rank_academy_student";
        public const string Genin = "02rank_genin";
        public const string Chunin = "03rank_chunin";
        public const string Jonin = "04rank_jonin";
        public const string Anbu = "05rank_anbu";
        public const string Kage = "06rank_kage";
        public const string Sannin = "07rank_sannin";
        public const string GhostOfUchiha = "08rank_ghost_of_uchiha";
        public const string GodOfShinobi = "09rank_god_of_shinobi";
        #endregion

        #region Chakra Reserves And Special Chakra
        public const string LowChakraReserve = "01_low_chakra_reserve";
        public const string HighChakraReserve = "02_high_chakra_reserve";
        public const string GreaterChakraReserve = "03_greater_chakra_reserve";
        public const string VastChakraReserve = "04_vast_chakra_reserve";
        public const string HagoromoChakra = "05_hagoromo_chakra";
        public const string HamuraChakra = "06_hamura_chakra";
        public const string IndraChakra = "07_indra_chakra";
        public const string AsuraChakra = "08_asura_chakra";
        #endregion

        #region Shinobi Traits
        public const string WillOfFire = "will_of_fire";
        public const string CurseOfHatred = "curse_of_hatred";
        public const string HashiramaCells = "hashi_cells";
        public const string TaijutsuMaster = "taijutsu_master";
        public const string Akatsuki = "trait_akatsuki";
        #endregion

        #region Chakra Natures And Releases
        public const string FireNature = "fireN";
        public const string EarthNature = "earthN";
        public const string WaterNature = "waterN";
        public const string WindNature = "windN";
        public const string LightningNature = "lightningN";
        public const string StormRelease = "stormR";
        public const string WoodRelease = "wood_release";
        public const string YinRelease = "yin_release";
        public const string YangRelease = "yang_release";
        public const string YinYangRelease = "yinyang_release";
        #endregion

        #region Jinchuriki
        public const string NineTailsJinchuriki = "nine_tails_jinchuriki";
        public const string TenTailsJinchuriki = "ten_tails_jinchuriki";
        #endregion

        #region Transformations
        public const string EightInnerGates = "eight_inner_gates";
        public const string Shikotsumyaku = "shikotsumyaku";
        public const string CursedMark = "cursed_mark";
        #endregion

        #region Jutsu
        public const string Rasengan = "rasenganJ";
        public const string Rasenshuriken = "rasenshurikenJ";
        public const string Chidori = "chidoriJ";
        public const string ShadowClone = "shadow_clonej";
        #endregion

        #region Senjutsu
        public const string FrogSage = "frog_sage_mode";
        public const string SlugSage = "slug_sage_mode";
        public const string SnakeSage = "snake_sage_mode";
        public const string WoodSage = "wood_sage_mode";
        #endregion

        #region Vanilla WorldBox Traits We Use
        public const string Clone = "clone";
        public const string Crippled = "crippled";
        public const string Evil = "evil";
        public const string Eyepatch = "eyepatch";
        public const string FireProof = "fire_proof";
        public const string Genius = "genius";
        public const string Immortal = "immortal";
        public const string Regeneration = "regeneration";
        public const string SkinBurns = "skin_burns";
        public const string Strong = "strong";
        public const string StrongMinded = "strong_minded";
        #endregion
    }

    public static class StatusIds
    {
        #region Chakra
        public const string ChakraExhaustion = "chakra_exhaustion";
        public const string AbilityCooldown = "status_ability_cooldown";
        public const string JutsuUse = "jutsuuse";
        public const string ChakraBreak = "chakra_break";
        #endregion

        #region Sharingan Line
        public const string Sharingan1Tomoe = "status_sharingan_1t";
        public const string Sharingan2Tomoe = "status_sharingan_2t";
        public const string Sharingan3Tomoe = "status_sharingan_3t";
        public const string Mangekyo = "status_mangekyo";
        public const string Genjutsu1 = "genjutsu_1";
        public const string Genjutsu2 = "genjutsu_2";
        public const string Genjutsu3 = "genjutsu_3";
        public const string Amaterasu = "status_amaterasu";
        public const string WeakEye = "weak_eye";
        public const string SusanooRibcage = "susanoo_ribcage";
        public const string BaseSusanoo = "status_base_susanoo";
        public const string PerfectSusanoo = "status_perfect_susanoo";
        public const string Kamui = "kamui_intangibility";
        public const string KamuiCooldown = "kamui_cooldown";
        public const string MadaraPerfectSusanoo = "madara_ps";
        public const string MadaraSusanooRibcage = "susanoo_ribcage_madara";
        public const string TengaiShinsei = "tengai_shinsei";
        public const string MadaraTamed = "status_madara_tamed";
        #endregion

        #region Rinnegan Line
        public const string RinneCooldown = "rinne_cooldown";
        public const string RinneganControlled = "status_rinnegan_controlled";
        public const string KingOfHell = "koh_sprite";
        public const string InfiniteTsukuyomi = "status_infinite_tsukuyomi";
        public const string SixPathsSenjutsu = "status_six_paths_senjutsu";
        public const string TruthSeekingShield = "status_tso_shield";
        public const string TruthSeekingBalls = "tsballs";
        #endregion

        #region Byakugan Line
        public const string ByakuganActive = "byakuganP";
        public const string Kaiten = "status_kaiten";
        public const string KaitenCooldown = "kaiten_cooldown";
        public const string Hit64Palms = "status_getting_hit_64";
        public const string Attack64Palms = "eightAttack";
        public const string TenseiganChakraMode = "tenseigan_cm";
        #endregion

        #region Other Dojutsu And Clans
        public const string Ketsuryugan = "status_ketsuryugan";
        public const string KetsuryuganExplodingHuman = "status_ketsuryuganEHT";
        public const string AdamantiteChains = "status_adamantite_chains";
        public const string AkimichiGrown = "status_akimichi_grown";
        public const string AkimichiShrunk = "status_akimichi_shrunk";
        public const string AburameMark = "status_aburame_mark";
        public const string ShikotsumyakuDefense = "shiko_defense";
        #endregion

        #region Nine Tails And Ten Tails
        public const string NineTailsInitialRelease = "status_jinchuriki_initial_release";
        public const string NineTailsV1Cloak = "status_jinchuriki_v1_cloak";
        public const string NineTailsV2Cloak = "status_jinchuriki_v2_cloak";
        public const string NineTailsIncompleteBeast = "status_jinchuriki_incomplete_beast";
        public const string NineTailsKcm1 = "status_jinchuriki_kcm1";
        public const string NineTailsKcm2 = "status_jinchuriki_kcm2";
        public const string NineTailsBaryon = "status_jinchuriki_baryon_mode";
        public const string BaryonLifeDecay = "baryon_life_decay";
        public const string KcmAura = "kcm_aura";
        public const string V1Aura = "v1_aura";
        public const string KuramaSageMode = "kurama_sage_mode";
        public const string Juubi1 = "juubi1";
        public const string Juubi2 = "juubi2";
        #endregion

        #region Transformations
        public const string InnerGate1 = "inner_gate_1";
        public const string InnerGate2 = "inner_gate_2";
        public const string InnerGate3 = "inner_gate_3";
        public const string InnerGate4 = "inner_gate_4";
        public const string InnerGate5 = "inner_gate_5";
        public const string InnerGate6 = "inner_gate_6";
        public const string InnerGate7 = "inner_gate_7";
        public const string InnerGate8 = "inner_gate_8";
        public const string EightGateStrain = "eight_gate_strain";
        public const string CursedMark1 = "cursed_mark1";
        public const string CursedMark2 = "cursed_mark2";
        public const string CursedMark3 = "cursed_mark3";
        #endregion

        #region Senjutsu
        public const string FrogSage = "status_frog_sage";
        public const string SlugSage = "status_slug_sage";
        public const string SnakeSage = "status_snake_sage";
        public const string HundredSeals = "status_100_sealings";
        #endregion

        #region Jutsu And Natures
        public const string Rasengan = "status_rasengan";
        public const string Chidori = "status_chidori";
        public const string ShadowClone = "shadow_clone";
        public const string MokutonTree = "status_mokuton_tree";
        public const string WaterPrison = "water_prison";
        public const string KatonBurn = "katon_burn";
        public const string WeightRock = "weight_rock";
        public const string WillOfFireBuff = "power_up";
        #endregion

        #region Vanilla WorldBox Statuses We Use
        public const string Angry = "angry";
        public const string Cursed = "cursed";
        public const string Drowning = "drowning";
        public const string Frozen = "frozen";
        public const string HadGoodDream = "had_good_dream";
        public const string Invincible = "invincible";
        public const string PowerUp = "powerup";
        public const string Sleeping = "sleeping";
        public const string Slowness = "slowness";
        public const string Stunned = "stunned";
        #endregion
    }

    public static class EffectIds
    {
        public const string ShinobiAntimatter = "fx_shinobi_antimatter";
        public const string BijuuBite = "fx_bijuu_bite";
        public const string ChakraArm = "fx_chakra_arm";
        public const string KingOfHell = "fx_king_of_hell";
        public const string TeleportYellow = "fx_teleport_yellow";
        public const string Katon = "fx_katon";
        public const string BeastCharge = "fx_beastcharge";
        public const string Smoke = "fx_smoke";

        // Vanilla WorldBox effects we use
        public const string ExplosionSmall = "fx_explosion_small";
        public const string ExplosionTiny = "fx_explosion_tiny";
    }

    public static class ProjectileIds
    {
        public const string FtgKunai = "projectile_ftg_kunai";
        public const string BasicKunai = "projectile_basic_kunai";
        public const string PaperBombKunai = "projectile_paper_bomb_kunai";
        public const string Rasenshuriken = "projectile_rasenshuriken";
        public const string WindCutter = "projectile_wind_cutter";
        public const string TailedBeastBomb = "projectile_tailed_beast_bomb";
        public const string TruthSeekingOrb = "projectile_truth_seeking_orb";

        // Vanilla WorldBox projectiles we use
        public const string Arrow = "arrow";
        public const string RedOrb = "red_orb";
        public const string Torch = "torch";
    }

    public static class ItemIds
    {
        public const string TripleScythe = "weapon_triple_scythe";
        public const string Kusanagi = "weapon_kusanagi";
        public const string BasicKunai = "weapon_basic_kunai";
        public const string Gunbai = "weapon_gunbai";
        public const string PaperBombKunai = "weapon_bomb_kunai";
        public const string ExecutionerBlade = "weapon_executioner_blade";
        public const string HagoromoStaff = "weapon_hagoromo_staff";
        public const string YinStaff = "weapon_yin_staff";
        public const string YangStaff = "weapon_yang_staff";
        public const string FtgKunai = "weapon_ftg_kunai";
        public const string FlakJacket = "armor_shinobi_flak_jacket";
        public const string WarringStatesArmor = "armor_warring_states";
        public const string MadaraSixPathsCloak = "armor_madara_sixpaths_cloak";
    }

    // Runs once, a moment after the game has finished loading (called from ShinobiLoop).
    // Every problem is written to the log as "[ShinobiBox] Self-check: ...", so a typo shows up the first
    // time you launch the game instead of months later as "this feature never works".
    public static class ShinobiSelfCheck
    {
        private static int problems;

        public static void Run()
        {
            problems = 0;

            CheckIds(typeof(TraitIds), "trait", id => AssetManager.traits.has(id));
            CheckIds(typeof(StatusIds), "status", id => AssetManager.status.has(id));
            CheckIds(typeof(EffectIds), "effect", id => AssetManager.effects_library.has(id));
            CheckIds(typeof(ProjectileIds), "projectile", id => AssetManager.projectiles.has(id));
            CheckIds(typeof(ItemIds), "item", id => AssetManager.items.has(id));
            CheckIds(typeof(ShinobiActors), "actor", id => AssetManager.actor_library.has(id));

            CheckTraits();
            CheckStatuses();
            CheckItems();

            if (problems == 0)
            {
                Debug.Log("[ShinobiBox] Self-check passed: every id, text and icon was found.");
            }
            else
            {
                Debug.LogWarning($"[ShinobiBox] Self-check found {problems} problem(s). See the lines above.");
            }
        }

        // Only checks the ids listed in our *Ids classes, never vanilla content we don't own.
        private static void CheckIds(Type idClass, string kind, Func<string, bool> exists)
        {
            foreach (FieldInfo field in idClass.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.IsLiteral || field.FieldType != typeof(string)) continue;

                string id = (string)field.GetValue(null);
                if (!exists(id))
                {
                    Report($"{kind} '{id}' ({idClass.Name}.{field.Name}) is not registered.");
                }
            }
        }

        private static void CheckTraits()
        {
            foreach (string id in GetOwnIds(typeof(TraitIds)))
            {
                ActorTrait trait = AssetManager.traits.get(id);
                if (trait == null || !IsShinobiGroup(trait.group_id)) continue;

                CheckText(trait.getLocaleID(), $"trait '{id}' name");
                CheckText(trait.getDescriptionID(), $"trait '{id}' description");
                CheckIcon(trait.path_icon, $"trait '{id}'");
            }
        }

        private static void CheckStatuses()
        {
            foreach (string id in GetOwnIds(typeof(StatusIds)))
            {
                StatusAsset status = AssetManager.status.get(id);
                if (status == null) continue;

                if (string.IsNullOrEmpty(status.locale_id)) Report($"status '{id}' has no locale_id, so it shows no name in game.");
                CheckText(status.locale_id, $"status '{id}' name");
                CheckIcon(status.path_icon, $"status '{id}'");

                if (status.need_visual_render && (status.sprite_list == null || status.sprite_list.Length == 0))
                {
                    Report($"status '{id}' should show an animation, but effects/{status.texture} has no sprites.");
                }
            }
        }

        private static void CheckItems()
        {
            foreach (string id in GetOwnIds(typeof(ItemIds)))
            {
                EquipmentAsset item = AssetManager.items.get(id);
                if (item == null) continue;

                CheckIcon(item.path_icon, $"item '{id}'");
            }
        }

        // World logs are created on first use, so ShinobiWorldLogs calls this when it builds one.
        public static void CheckWorldLog(string localeId, string iconPath)
        {
            CheckText(localeId, $"world log '{localeId}'");
            CheckIcon(iconPath, $"world log '{localeId}'");
        }

        private static void CheckText(string key, string what)
        {
            if (string.IsNullOrEmpty(key) || LocalizedTextManager.stringExists(key)) return;
            Report($"{what}: text key '{key}' is missing from Locales/en.json.");
        }

        private static void CheckIcon(string path, string what)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (SpriteTextureLoader.getSprite(path) != null) return;
            if (SpriteTextureLoader.getSprite("ui/icons/" + path) != null) return;
            Report($"{what}: icon '{path}' was not found.");
        }

        // The "Vanilla WorldBox ..." ids are skipped here: we check that they exist, but their text/icons aren't ours.
        private static IEnumerable<string> GetOwnIds(Type idClass)
        {
            foreach (FieldInfo field in idClass.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.IsLiteral || field.FieldType != typeof(string)) continue;
                if (VanillaIds.Contains((string)field.GetValue(null))) continue;
                yield return (string)field.GetValue(null);
            }
        }

        private static bool IsShinobiGroup(string groupId)
        {
            return !string.IsNullOrEmpty(groupId) && groupId.StartsWith("Shinobi_Box_");
        }

        private static readonly HashSet<string> VanillaIds = new HashSet<string>
        {
            TraitIds.Clone, TraitIds.Crippled, TraitIds.Evil, TraitIds.Eyepatch, TraitIds.FireProof, TraitIds.Genius,
            TraitIds.Immortal, TraitIds.Regeneration, TraitIds.SkinBurns, TraitIds.Strong, TraitIds.StrongMinded,
            StatusIds.Angry, StatusIds.Cursed, StatusIds.Drowning, StatusIds.Frozen, StatusIds.HadGoodDream,
            StatusIds.Invincible, StatusIds.PowerUp, StatusIds.Sleeping, StatusIds.Slowness, StatusIds.Stunned,
            EffectIds.ExplosionSmall, EffectIds.ExplosionTiny,
            ProjectileIds.Arrow, ProjectileIds.RedOrb, ProjectileIds.Torch
        };

        private static void Report(string message)
        {
            problems++;
            Debug.LogWarning("[ShinobiBox] Self-check: " + message);
        }
    }
}
