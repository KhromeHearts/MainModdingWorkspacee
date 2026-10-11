using System;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using NeoModLoader.services;
using NeoModLoader.api;

namespace ShinobiBox
{
    public static class ShinobiActors
    {
        public const string HashiramaActorId = "hashirama_senju";
        public const string MadaraActorId = "madara_uchiha";
        public const string KuramaActorId = "kurama";
        public const string JuubiActorId = "juubi";
        public const string KakashiActorId = "kakashi_hatake";
        public const string SasukeActorId = "sasuke_uchiha";
        public const string OrochimaruActorId = "orochimaru";
        public const string NarutoActorId = "naruto_uzumaki";
        public const string SakuraActorId = "sakura_haruno";
        public const string ItachiActorId = "itachi_uchiha";
        public const string ObitoActorId = "obito_uchiha";
        public const string HagoromoActorId = "hagoromo_otsutsuki";
        public const string JinchurikiObitoActorId = "jinchuriki_obito";
        public const string JinchurikiMadaraActorId = "jinchuriki_madara";

        private static readonly Dictionary<string, string[]> RequiredTraitsByActor = new Dictionary<string, string[]>
        {
            [HashiramaActorId] = new[] { "immortal", "fire_proof", "senju_clan", "wood_release", "hashi_cells", "wood_sage_mode", "will_of_fire", "04_vast_chakra_reserve", "fire_release", "earth_release", "water_release", "lightning_release", "wind_release", "shadow_clonej", "09rank_god_of_shinobi" },
            [MadaraActorId] = new[] { "immortal", "fire_proof", "uchiha_clan", "madara_eternal_mangekyo", "04_vast_chakra_reserve", "curse_of_hatred", "fire_release", "earth_release", "water_release", "lightning_release", "wind_release", "08rank_ghost_of_uchiha" },
            [SasukeActorId] = new[] { "uchiha_clan", "fire_release", "lightning_release", "cursed_mark", "curse_of_hatred", "sasuke_rinnegan", "chidoriJ", "04_vast_chakra_reserve", "07_indra_chakra" },
            [OrochimaruActorId] = new[] { "regeneration", "cursed_mark", "snake_sage_mode", "wood_release", "shikotsumyaku", "04_vast_chakra_reserve", "07rank_sannin" },
            [KakashiActorId] = new[] { "05rank_anbu", "wind_release", "lightning_release", "earth_release", "fire_release", "water_release", "rasenganJ", "shadow_clonej", "rasenshurikenJ", "chidoriJ", "mangekyo_sharingan", "04_vast_chakra_reserve" },
            [ItachiActorId] = new[] { "regeneration", "uchiha_clan", "curse_of_hatred", "05rank_anbu", "mangekyo_sharingan", "04_vast_chakra_reserve" },
            [NarutoActorId] = new[] { "regeneration", "uzumaki_clan", "will_of_fire", "wind_release", "rasenganJ", "rasenshurikenJ", "shadow_clonej", "frog_sage_mode", "nine_tails_jinchuriki", "04_vast_chakra_reserve", "06rank_kage" },
            [ObitoActorId] = new[] { "ten_tails_jinchuriki", "uchiha_clan", "fire_release", "wood_release", "yinyang_release", "04_vast_chakra_reserve", "rinnegan", "mangekyo_sharingan" },
            [HagoromoActorId] = new[] { "regeneration", "otsutsuki_clan", "rinnegan", "04_vast_chakra_reserve", "05_hagoromo_chakra", "wind_release", "earth_release", "fire_release", "water_release", "lightning_release", "yinyang_release", "ten_tails_jinchuriki" },
            [SakuraActorId] = new[] { "slug_sage_mode", "regeneration", "will_of_fire", "04_vast_chakra_reserve", "earth_release" },
            [KuramaActorId] = new[] { "immortal", "strong", "regeneration", "04_vast_chakra_reserve", "nine_tails_jinchuriki" },
            [JuubiActorId] = new[] { "immortal", "strong", "regeneration", "04_vast_chakra_reserve", "05_hagoromo_chakra", "ten_tails_jinchuriki" }
        };

        private static readonly HashSet<string> CharacterIds = new HashSet<string>
        {
            HashiramaActorId, MadaraActorId, KakashiActorId, SasukeActorId, OrochimaruActorId, NarutoActorId,
            SakuraActorId, ItachiActorId, ObitoActorId, HagoromoActorId, JinchurikiObitoActorId, JinchurikiMadaraActorId
        };

        public static void Init()
        {
            loadAssets();
        }

        public static bool IsCharacter(Actor actor)
        {
            return actor?.asset != null && CharacterIds.Contains(actor.asset.id);
        }

        public static bool IsTailedBeast(Actor actor)
        {
            return actor?.asset != null && (actor.asset.id == KuramaActorId || actor.asset.id == JuubiActorId);
        }

        public static bool IsShinobi(Actor actor)
        {
            return actor != null && (actor.isSapient() || IsCharacter(actor));
        }

        public static void EnsureRequiredTraits(Actor actor)
        {
            if (actor?.asset == null || !RequiredTraitsByActor.TryGetValue(actor.asset.id, out string[] requiredTraits)) return;

            List<string> missingTraits = new List<string>();
            foreach (string traitId in requiredTraits)
            {
                if (!actor.hasTrait(traitId)) actor.addTrait(traitId);
                if (!actor.hasTrait(traitId)) missingTraits.Add(traitId);
            }

            if (missingTraits.Count > 0)
            {
                Debug.LogWarning($"[ShinobiBox] Actor '{actor.asset.id}' is missing required traits after spawn: {string.Join(", ", missingTraits)}");
            }
        }

        public static void loadAssets()
        {
            #region Shinobi Actors

            #region Hashirama
            if (AssetManager.actor_library.get(HashiramaActorId) == null)
            {
                ActorAsset hashirama = CreateCharacter(HashiramaActorId, "hashirama", "hashirama senju", "spawn_hashirama_senju", ShinobiKingdoms.KonohaKingdomId);
                hashirama.inspect_home = true;

                hashirama.base_stats["lifespan"] = 125f;
                hashirama.base_stats["health"] = 1200f;
                hashirama.base_stats["damage"] = 10f;
                hashirama.base_stats["speed"] = 25f;
                hashirama.base_stats["attack_speed"] = 2f;
                hashirama.base_stats["intelligence"] = 200f;
                hashirama.base_stats["stamina"] = 600f;
                hashirama.base_stats["mana"] = 3500f;
                hashirama.base_stats["armor"] = 25f;
                hashirama.base_stats["critical_chance"] = 0.40f;
                hashirama.base_stats["knockback"] = 0.2f;
                hashirama.base_stats["accuracy"] = 2f;
                hashirama.base_stats["targets"] = 1f;
                hashirama.base_stats["scale"] = 0.15f;
                hashirama.base_stats["chakra"] = 500f;
                hashirama.base_stats["mass_2"] = 45f;

                hashirama.addTrait("immortal");
                hashirama.addTrait("fire_proof");
                hashirama.addTrait("senju_clan");
                hashirama.addTrait("wood_release");
                hashirama.addTrait("hashi_cells");
                hashirama.addTrait("wood_sage_mode");
                hashirama.addTrait("will_of_fire");
                hashirama.addTrait("04_vast_chakra_reserve");
                hashirama.addTrait("fire_release");
                hashirama.addTrait("earth_release");
                hashirama.addTrait("water_release");
                hashirama.addTrait("lightning_release");
                hashirama.addTrait("wind_release");
                hashirama.addTrait("shadow_clonej");
                hashirama.addTrait("09rank_god_of_shinobi");

                hashirama.action_death = new WorldAction(HashiramaOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(hashirama);
            }

            #endregion

            #region Madara

            if (AssetManager.actor_library.get(MadaraActorId) == null)
            {
                ActorAsset madara = CreateCharacter(MadaraActorId, "madara", "madara uchiha", "spawn_madara_uchiha", ShinobiKingdoms.RogueKingdomId);
                madara.default_weapons = new string[] { "weapon_gunbai" };

                madara.base_stats["lifespan"] = 125f;
                madara.base_stats["health"] = 1000f;
                madara.base_stats["damage"] = 10f;
                madara.base_stats["speed"] = 25f;
                madara.base_stats["attack_speed"] = 2f;
                madara.base_stats["intelligence"] = 200f;
                madara.base_stats["stamina"] = 600f;
                madara.base_stats["mana"] = 3500f;
                madara.base_stats["armor"] = 25f;
                madara.base_stats["critical_chance"] = 0.40f;
                madara.base_stats["knockback"] = 0.2f;
                madara.base_stats["accuracy"] = 6f;
                madara.base_stats["targets"] = 1f;
                madara.base_stats["scale"] = 0.15f;
                madara.base_stats["chakra"] = 500f;
                madara.base_stats["mass_2"] = 40f;

                madara.addTrait("immortal");
                madara.addTrait("fire_proof");
                madara.addTrait("uchiha_clan");
                madara.addTrait("madara_eternal_mangekyo");
                madara.addTrait("04_vast_chakra_reserve");
                madara.addTrait("curse_of_hatred");
                madara.addTrait("fire_release");
                madara.addTrait("earth_release");
                madara.addTrait("water_release");
                madara.addTrait("lightning_release");
                madara.addTrait("wind_release");
                madara.addTrait("08rank_ghost_of_uchiha");

                madara.action_death = new WorldAction(MadaraOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(madara);
            }

            #endregion

            #region Sasuke

            if (AssetManager.actor_library.get(SasukeActorId) == null)
            {
                ActorAsset sasuke = CreateCharacter(SasukeActorId, "sasuke", "sasuke uchiha", "spawn_sasuke_uchiha", ShinobiKingdoms.RogueKingdomId);
                sasuke.default_weapons = new string[] { "weapon_kusanagi" };

                sasuke.base_stats["lifespan"] = 125f;
                sasuke.base_stats["health"] = 1500f;
                sasuke.base_stats["damage"] = 7f;
                sasuke.base_stats["speed"] = 26f;
                sasuke.base_stats["attack_speed"] = 1.5f;
                sasuke.base_stats["intelligence"] = 180f;
                sasuke.base_stats["stamina"] = 620f;
                sasuke.base_stats["mana"] = 3600f;
                sasuke.base_stats["armor"] = 13f;
                sasuke.base_stats["critical_chance"] = 0.40f;
                sasuke.base_stats["knockback"] = 0.2f;
                sasuke.base_stats["accuracy"] = 5f;
                sasuke.base_stats["targets"] = 1f;
                sasuke.base_stats["scale"] = 0.15f;
                sasuke.base_stats["chakra"] = 500f;
                sasuke.base_stats["mass_2"] = 40f;

                sasuke.addTrait("uchiha_clan");
                sasuke.addTrait("fire_release");
                sasuke.addTrait("lightning_release");
                sasuke.addTrait("cursed_mark");
                sasuke.addTrait("curse_of_hatred");
                sasuke.addTrait("sasuke_rinnegan");
                sasuke.addTrait("chidoriJ");
                sasuke.addTrait("04_vast_chakra_reserve");
                sasuke.addTrait("07_indra_chakra");

                sasuke.action_death = new WorldAction(SasukeOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(sasuke);
            }

            #endregion

            #region Orochimaru

            if (AssetManager.actor_library.get(OrochimaruActorId) == null)
            {
                ActorAsset orochimaru = CreateCharacter(OrochimaruActorId, "orochimaru", "orochimaru", "spawn_orochimaru", ShinobiKingdoms.RogueKingdomId);
                orochimaru.default_weapons = new string[] { "weapon_kusanagi" };

                orochimaru.base_stats["lifespan"] = 125f;
                orochimaru.base_stats["health"] = 1500f;
                orochimaru.base_stats["damage"] = 5f;
                orochimaru.base_stats["speed"] = 26f;
                orochimaru.base_stats["attack_speed"] = 1.5f;
                orochimaru.base_stats["intelligence"] = 180f;
                orochimaru.base_stats["stamina"] = 620f;
                orochimaru.base_stats["mana"] = 3600f;
                orochimaru.base_stats["armor"] = 13f;
                orochimaru.base_stats["critical_chance"] = 0.40f;
                orochimaru.base_stats["knockback"] = 0.2f;
                orochimaru.base_stats["accuracy"] = 5f;
                orochimaru.base_stats["targets"] = 1f;
                orochimaru.base_stats["scale"] = 0.15f;
                orochimaru.base_stats["chakra"] = 500f;
                orochimaru.base_stats["mass_2"] = 40f;

                orochimaru.addTrait("regeneration");
                orochimaru.addTrait("cursed_mark");
                orochimaru.addTrait("snake_sage_mode");
                orochimaru.addTrait("wood_release");
                orochimaru.addTrait("shikotsumyaku");
                orochimaru.addTrait("04_vast_chakra_reserve");
                orochimaru.addTrait("07rank_sannin");

                AssetManager.actor_library.loadTexturesAndSprites(orochimaru);
            }

            #endregion

            #region Kakashi

            if (AssetManager.actor_library.get(KakashiActorId) == null)
            {
                ActorAsset kakashi = CreateCharacter(KakashiActorId, "kakashi", "kakashi hatake", "spawn_kakashi_hatake", ShinobiKingdoms.KonohaKingdomId);
                kakashi.default_weapons = new string[] { "weapon_basic_kunai" };

                kakashi.base_stats["lifespan"] = 125f;
                kakashi.base_stats["health"] = 1200f;
                kakashi.base_stats["damage"] = 7f;
                kakashi.base_stats["speed"] = 25f;
                kakashi.base_stats["attack_speed"] = 2.5f;
                kakashi.base_stats["intelligence"] = 170f;
                kakashi.base_stats["stamina"] = 700f;
                kakashi.base_stats["mana"] = 3700f;
                kakashi.base_stats["armor"] = 27f;
                kakashi.base_stats["critical_chance"] = 0.40f;
                kakashi.base_stats["knockback"] = 0.2f;
                kakashi.base_stats["accuracy"] = 4f;
                kakashi.base_stats["targets"] = 1f;
                kakashi.base_stats["scale"] = 0.15f;
                kakashi.base_stats["chakra"] = 520f;
                kakashi.base_stats["mass_2"] = 40f;

                kakashi.addTrait("05rank_anbu");
                kakashi.addTrait("wind_release");
                kakashi.addTrait("lightning_release");
                kakashi.addTrait("earth_release");
                kakashi.addTrait("fire_release");
                kakashi.addTrait("water_release");
                kakashi.addTrait("rasenganJ");
                kakashi.addTrait("shadow_clonej");
                kakashi.addTrait("rasenshurikenJ");
                kakashi.addTrait("chidoriJ");
                kakashi.addTrait("mangekyo_sharingan");
                kakashi.addTrait("04_vast_chakra_reserve");

                AssetManager.actor_library.loadTexturesAndSprites(kakashi);
            }

            #endregion

            #region Itachi

            if (AssetManager.actor_library.get(ItachiActorId) == null)
            {
                ActorAsset itachi = CreateCharacter(ItachiActorId, "itachi", "itachi uchiha", "spawn_itachi_uchiha", ShinobiKingdoms.RogueKingdomId);
                itachi.default_weapons = new string[] { "weapon_basic_kunai" };

                itachi.base_stats["lifespan"] = 125f;
                itachi.base_stats["health"] = 1500f;
                itachi.base_stats["damage"] = 8f;
                itachi.base_stats["speed"] = 26f;
                itachi.base_stats["attack_speed"] = 1.5f;
                itachi.base_stats["intelligence"] = 180f;
                itachi.base_stats["stamina"] = 620f;
                itachi.base_stats["mana"] = 3600f;
                itachi.base_stats["armor"] = 13f;
                itachi.base_stats["critical_chance"] = 0.40f;
                itachi.base_stats["knockback"] = 0.2f;
                itachi.base_stats["accuracy"] = 5f;
                itachi.base_stats["targets"] = 1f;
                itachi.base_stats["scale"] = 0.15f;
                itachi.base_stats["chakra"] = 500f;
                itachi.base_stats["mass_2"] = 40f;

                itachi.addTrait("regeneration");
                itachi.addTrait("uchiha_clan");
                itachi.addTrait("curse_of_hatred");
                itachi.addTrait("05rank_anbu");
                itachi.addTrait("mangekyo_sharingan");
                itachi.addTrait("04_vast_chakra_reserve");

                AssetManager.actor_library.loadTexturesAndSprites(itachi);
            }

            #endregion

            #region Naruto

            if (AssetManager.actor_library.get(NarutoActorId) == null)
            {
                ActorAsset naruto = CreateCharacter(NarutoActorId, "naruto", "naruto uzumaki", "spawn_naruto_uzumaki", ShinobiKingdoms.KonohaKingdomId);
                naruto.base_stats["lifespan"] = 125f;
                naruto.base_stats["health"] = 1200f;
                naruto.base_stats["damage"] = 15f;
                naruto.base_stats["speed"] = 25f;
                naruto.base_stats["attack_speed"] = 2.5f;
                naruto.base_stats["intelligence"] = 170f;
                naruto.base_stats["stamina"] = 700f;
                naruto.base_stats["mana"] = 3700f;
                naruto.base_stats["armor"] = 27f;
                naruto.base_stats["critical_chance"] = 0.40f;
                naruto.base_stats["knockback"] = 0.2f;
                naruto.base_stats["accuracy"] = 4f;
                naruto.base_stats["targets"] = 1f;
                naruto.base_stats["scale"] = 0.15f;
                naruto.base_stats["chakra"] = 520f;
                naruto.base_stats["mass_2"] = 40f;

                naruto.addTrait("regeneration");
                naruto.addTrait("uzumaki_clan");
                naruto.addTrait("will_of_fire");
                naruto.addTrait("wind_release");
                naruto.addTrait("rasenganJ");
                naruto.addTrait("rasenshurikenJ");
                naruto.addTrait("shadow_clonej");
                naruto.addTrait("frog_sage_mode");
                naruto.addTrait("nine_tails_jinchuriki");
                naruto.addTrait("04_vast_chakra_reserve");
                naruto.addTrait("06rank_kage");

                naruto.action_death = new WorldAction(NarutoOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(naruto);
            }

            #endregion

            #region Obito

            if (AssetManager.actor_library.get(ObitoActorId) == null)
            {
                ActorAsset obito = CreateCharacter(ObitoActorId, "obito", "obito uchiha", "spawn_obito_uchiha", ShinobiKingdoms.RogueKingdomId);
                obito.default_weapons = new string[] { "weapon_yang_staff" };

                obito.base_stats["lifespan"] = 125f;
                obito.base_stats["health"] = 1500f;
                obito.base_stats["damage"] = 28f;
                obito.base_stats["speed"] = 26f;
                obito.base_stats["attack_speed"] = 1.5f;
                obito.base_stats["intelligence"] = 180f;
                obito.base_stats["stamina"] = 620f;
                obito.base_stats["mana"] = 3600f;
                obito.base_stats["armor"] = 13f;
                obito.base_stats["critical_chance"] = 0.40f;
                obito.base_stats["knockback"] = 0.2f;
                obito.base_stats["accuracy"] = 5f;
                obito.base_stats["targets"] = 1f;
                obito.base_stats["scale"] = 0.15f;
                obito.base_stats["chakra"] = 500f;
                obito.base_stats["mass_2"] = 40f;

                obito.addTrait("ten_tails_jinchuriki");
                obito.addTrait("uchiha_clan");
                obito.addTrait("fire_release");
                obito.addTrait("wood_release");
                obito.addTrait("yinyang_release");
                obito.addTrait("04_vast_chakra_reserve");
                obito.addTrait("rinnegan");
                obito.addTrait("mangekyo_sharingan");

                AssetManager.actor_library.loadTexturesAndSprites(obito);
            }

            #endregion

            #region Hagoromo

            if (AssetManager.actor_library.get(HagoromoActorId) == null)
            {
                ActorAsset hagoromo = CreateCharacter(HagoromoActorId, "hagoromo", "hagoromo otsutsuki", "spawn_hagoromo_otsutsuki", ShinobiKingdoms.KonohaKingdomId);
                hagoromo.default_weapons = new string[] { "weapon_hagoromo_staff" };

                hagoromo.base_stats["lifespan"] = 999f;
                hagoromo.base_stats["health"] = 10000f;
                hagoromo.base_stats["damage"] = 28f;
                hagoromo.base_stats["speed"] = 50f;
                hagoromo.base_stats["attack_speed"] = 2.5f;
                hagoromo.base_stats["intelligence"] = 1000f;
                hagoromo.base_stats["stamina"] = 10000f;
                hagoromo.base_stats["mana"] = 10000f;
                hagoromo.base_stats["armor"] = 27f;
                hagoromo.base_stats["critical_chance"] = 0.90f;
                hagoromo.base_stats["knockback"] = 0.2f;
                hagoromo.base_stats["accuracy"] = 4f;
                hagoromo.base_stats["targets"] = 1f;
                hagoromo.base_stats["scale"] = 0.20f;
                hagoromo.base_stats["chakra"] = 10000f;
                hagoromo.base_stats["mass_2"] = 40f;

                hagoromo.addTrait("regeneration");
                hagoromo.addTrait("otsutsuki_clan");
                hagoromo.addTrait("rinnegan");
                hagoromo.addTrait("04_vast_chakra_reserve");
                hagoromo.addTrait("05_hagoromo_chakra");
                hagoromo.addTrait("wind_release");
                hagoromo.addTrait("earth_release");
                hagoromo.addTrait("fire_release");
                hagoromo.addTrait("water_release");
                hagoromo.addTrait("lightning_release");
                hagoromo.addTrait("yinyang_release");
                hagoromo.addTrait("ten_tails_jinchuriki");

                hagoromo.action_death = new WorldAction(HagoromoOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(hagoromo);
            }

            #endregion

            #region Sakura

            if (AssetManager.actor_library.get(SakuraActorId) == null)
            {
                ActorAsset sakura = CreateCharacter(SakuraActorId, "sakura", "sakura haruno", "spawn_sakura_haruno", ShinobiKingdoms.KonohaKingdomId);
                sakura.base_stats["lifespan"] = 125f;
                sakura.base_stats["health"] = 950f;
                sakura.base_stats["damage"] = 150f;
                sakura.base_stats["speed"] = 24f;
                sakura.base_stats["attack_speed"] = 2.2f;
                sakura.base_stats["intelligence"] = 150f;
                sakura.base_stats["stamina"] = 560f;
                sakura.base_stats["mana"] = 3000f;
                sakura.base_stats["armor"] = 24f;
                sakura.base_stats["critical_chance"] = 0.30f;
                sakura.base_stats["knockback"] = 0.2f;
                sakura.base_stats["accuracy"] = 4f;
                sakura.base_stats["targets"] = 1f;
                sakura.base_stats["scale"] = 0.15f;
                sakura.base_stats["chakra"] = 350f;
                sakura.base_stats["mass_2"] = 36f;

                sakura.addTrait("slug_sage_mode");
                sakura.addTrait("regeneration");
                sakura.addTrait("will_of_fire");
                sakura.addTrait("04_vast_chakra_reserve");
                sakura.addTrait("earth_release");

                sakura.action_death = new WorldAction(SakuraOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(sakura);
            }

            #endregion

            #endregion

            #region Tailed Beasts

            if (AssetManager.actor_library.get(KuramaActorId) == null)
            {
                ActorAsset kurama = CreateCharacter(KuramaActorId, "kurama", "kurama", "spawn_kurama", ShinobiKingdoms.TailedBeastsKingdomId);
                kurama.is_humanoid = false;
                kurama.animation_walk_speed = 5f;
                kurama.animation_speed_based_on_walk_speed = false;
                kurama.use_items = false;
                kurama.actor_size = ActorSize.S17_Dragon;

                kurama.base_stats["lifespan"] = 999f;
                kurama.base_stats["health"] = 3000f;
                kurama.base_stats["damage"] = 123f;
                kurama.base_stats["speed"] = 25f;
                kurama.base_stats["attack_speed"] = -0.10f;
                kurama.base_stats["intelligence"] = 200f;
                kurama.base_stats["stamina"] = 600f;
                kurama.base_stats["mana"] = 3500f;
                kurama.base_stats["armor"] = 30f;
                kurama.base_stats["critical_chance"] = 0.40f;
                kurama.base_stats["knockback"] = 0.2f;
                kurama.base_stats["accuracy"] = 6f;
                kurama.base_stats["range"] = 80f;
                kurama.base_stats["targets"] = 5f;
                kurama.base_stats["scale"] = 0.15f;
                kurama.base_stats["chakra"] = 100000f;
                kurama.base_stats["mass_2"] = 1f;

                kurama.addTrait("immortal");
                kurama.addTrait("strong");
                kurama.addTrait("regeneration");
                kurama.addTrait("04_vast_chakra_reserve");
                kurama.addTrait("nine_tails_jinchuriki");

                kurama.action_death = new WorldAction(KuramaOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(kurama);
            }

            if (AssetManager.actor_library.get(JuubiActorId) == null)
            {
                ActorAsset juubi = CreateCharacter(JuubiActorId, "juubi", "juubi", "summon_ten_tails", ShinobiKingdoms.TailedBeastsKingdomId);
                juubi.is_humanoid = false;
                juubi.animation_walk_speed = 2.5f;
                juubi.animation_speed_based_on_walk_speed = false;
                juubi.use_items = false;
                juubi.actor_size = ActorSize.S17_Dragon;

                juubi.base_stats["lifespan"] = 999f;
                juubi.base_stats["health"] = 4200f;
                juubi.base_stats["damage"] = 180f;
                juubi.base_stats["speed"] = 25f;
                juubi.base_stats["attack_speed"] = -35f;
                juubi.base_stats["intelligence"] = 200f;
                juubi.base_stats["stamina"] = 900f;
                juubi.base_stats["mana"] = 5000f;
                juubi.base_stats["armor"] = 45f;
                juubi.base_stats["critical_chance"] = 0.45f;
                juubi.base_stats["knockback"] = 0.25f;
                juubi.base_stats["accuracy"] = 6f;
                juubi.base_stats["range"] = 80f;
                juubi.base_stats["targets"] = 6f;
                juubi.base_stats["scale"] = 0.20f;
                juubi.base_stats["chakra"] = 120000f;
                juubi.base_stats["mass_2"] = 1f;

                juubi.addTrait("immortal");
                juubi.addTrait("strong");
                juubi.addTrait("regeneration");
                juubi.addTrait("04_vast_chakra_reserve");
                juubi.addTrait("05_hagoromo_chakra");
                juubi.addTrait("ten_tails_jinchuriki");

                juubi.action_death = new WorldAction(JuubiOnDeath);
                AssetManager.actor_library.loadTexturesAndSprites(juubi);
            }

            #endregion
        }

        private static ActorAsset CreateCharacter(string id, string folder, string nameLocale, string powerId, string kingdomId)
        {
            ActorAsset unit = AssetManager.actor_library.clone(id, "$mob$");
            unit.id = id;
            unit.name_locale = nameLocale;
            unit.power_id = powerId;
            unit.kingdom_id_wild = kingdomId;
            unit.is_humanoid = true;
            unit.use_phenotypes = false;
            unit.unit_other = true;
            unit.has_advanced_textures = false;
            unit.check_flip = delegate { return true; };
            unit.name_template_sets = new string[] { "human_default_set" };
            unit.job = new string[] { "random_move" };
            unit.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
            unit.animation_swim = ActorAnimationSequences.walk_0;
            unit.texture_asset = new ActorTextureSubAsset("actors/" + folder + "/", unit.has_advanced_textures);
            unit._cached_sprite = Resources.Load<Sprite>("GameResources/actors/" + folder + "/Main/walk_0");
            unit.texture_id = folder;
            unit.icon = "units/" + folder;
            unit.can_be_killed_by_stuff = true;
            unit.can_be_killed_by_life_eraser = true;
            unit.can_attack_buildings = true;
            unit.can_be_moved_by_powers = true;
            unit.can_be_hurt_by_powers = true;
            unit.can_be_inspected = true;
            unit.visible_on_minimap = true;
            unit.use_items = true;
            unit.take_items = false;
            unit.can_talk_with = false;
            unit.control_can_talk = false;
            unit.can_evolve_into_new_species = false;
            unit.can_have_subspecies = false;
            unit.disable_jump_animation = true;
            unit.has_soul = true;
            unit.has_baby_form = false;
            unit.render_heads_for_babies = false;
            unit.body_separate_part_hands = true;
            unit.shadow = false;
            unit.force_land_creature = true;
            unit.force_ocean_creature = false;
            unit.flying = false;
            unit.very_high_flyer = false;
            unit.run_to_water_when_on_fire = true;
            unit.can_turn_into_zombie = false;
            unit.can_turn_into_demon_in_age_of_chaos = false;
            unit.can_turn_into_ice_one = false;
            unit.can_turn_into_tumor = false;
            unit.can_turn_into_mush = false;
            unit.can_be_killed_by_divine_light = true;
            unit.ignored_by_infinity_coin = false;
            unit.actor_size = ActorSize.S13_Human;
            unit.can_edit_equipment = true;
            unit.can_level_up = true;
            unit.can_receive_traits = false;
            return unit;
        }

        #region Heroes And Villains
        public static void CharacterBehaviour(Actor actor)
        {
            if (!IsCharacter(actor) || actor.has_attack_target || actor.current_tile == null) return;

            if (actor.asset.kingdom_id_wild == ShinobiKingdoms.KonohaKingdomId)
            {
                GuardVillage(actor);
            }
            else if (actor.asset.kingdom_id_wild == ShinobiKingdoms.RogueKingdomId && Randy.randomChance(0.05f))
            {
                City target = FindNearestCity(actor.current_tile, 100f);
                if (target != null && target.getTile() != null)
                {
                    actor.goTo(target.getTile());
                }
            }
        }

        private static void GuardVillage(Actor actor)
        {
            actor.data.get("home_village", out long homeId, -1L);
            City home = World.world.cities.get(homeId);

            if (home == null || !home.isAlive())
            {
                home = FindNearestCity(actor.current_tile, 100f);
                if (home == null) return;
                actor.data.set("home_village", home.getID());
            }

            WorldTile homeTile = home.getTile();
            if (homeTile == null) return;

            if (Toolbox.DistTile(actor.current_tile, homeTile) > 25f)
            {
                actor.goTo(homeTile);
            }
        }

        private static City FindNearestCity(WorldTile from, float maxDistance)
        {
            City nearest = null;
            float nearestDistance = maxDistance;

            foreach (City city in World.world.cities)
            {
                if (city == null || !city.isAlive()) continue;
                WorldTile cityTile = city.getTile();
                if (cityTile == null) continue;

                float distance = Toolbox.DistTile(from, cityTile);
                if (distance < nearestDistance)
                {
                    nearest = city;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }
        #endregion

        #region On Death Actions
        public static bool SasukeOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            Actor sasuke = pTarget.a;
            BaseSimObject attackerObject = sasuke.attackedBy;
            if (attackerObject == null || attackerObject.a == null || !attackerObject.a.isAlive()) return false;

            Actor attacker = attackerObject.a;
            if (attacker == sasuke) return false;

            if (!attacker.hasTrait("sasuke_rinnegan") && IsShinobi(attacker))
            {
                attacker.addTrait("sasuke_rinnegan");
            }
            else
            {
                ShinobiItems.EquipItem(attacker, "weapon_kusanagi");
            }

            return true;
        }

        public static bool ItachiOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            return true;
        }

        public static bool NarutoOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            return true;
        }

        public static bool SakuraOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            return true;
        }

        public static bool HagoromoOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.current_tile == null) return false;

            WorldTile tile = pTarget.current_tile;

            for (int i = 0; i < 4; i++)
            {
                ActionLibrary.castTornado(null, pTarget, null);
            }
            MapBox.spawnLightningMedium(tile, 0.15f);
            Earthquake.startQuake(tile, EarthquakeType.SmallDisaster);
            MapBox.spawnLightningBig(tile, 0.45f);

            return true;
        }

        public static bool HashiramaOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null || pTarget.a.data == null) return false;

            Actor hashirama = pTarget.a;
            BaseSimObject attackerObject = hashirama.attackedBy;
            if (attackerObject == null || attackerObject.a == null || !attackerObject.a.isAlive()) return false;

            Actor attacker = attackerObject.a;
            if (attacker == hashirama) return false;

            if (!attacker.hasTrait("hashi_cells") && IsShinobi(attacker) && Randy.randomChance(0.7f))
            {
                attacker.addTrait("hashi_cells");
            }
            else
            {
                ShinobiItems.EquipItem(attacker, "armor_warring_states");
            }

            return true;
        }

        public static bool MadaraOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null || pTarget.a.data == null) return false;

            Actor madara = pTarget.a;
            BaseSimObject attackerObject = madara.attackedBy;
            if (attackerObject == null || attackerObject.a == null || !attackerObject.a.isAlive()) return false;

            Actor attacker = attackerObject.a;
            if (attacker == madara) return false;

            if (!attacker.hasTrait("madara_eternal_mangekyo") && !attacker.hasTrait("eternal_mangekyo") && IsShinobi(attacker))
            {
                attacker.addTrait("madara_eternal_mangekyo");
            }
            else
            {
                ShinobiItems.EquipItem(attacker, "weapon_gunbai");
                ShinobiItems.EquipItem(attacker, "armor_warring_states");
            }

            return true;
        }
        public static bool KuramaOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null || pTarget.a.data == null) return false;

            Actor kurama = pTarget.a;
            BaseSimObject attackerObject = kurama.attackedBy;
            if (attackerObject == null || attackerObject.a == null || !attackerObject.a.isAlive()) return false;

            Actor attacker = attackerObject.a;
            if (attacker == kurama) return false;

            if (!attacker.hasTrait("nine_tails_jinchuriki") && IsShinobi(attacker))
            {
                attacker.addTrait("nine_tails_jinchuriki");
            }

            attacker.restoreHealth(attacker.getMaxHealth());
            ShinobiWorldLogs.AddWorldLog("log_kurama_sealed", "worldlog_kurama_sealed", "ui/icons/9Tails", attacker);

            return true;
        }

        public static bool JuubiOnDeath(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null || pTarget.a.data == null) return false;

            Actor juubi = pTarget.a;
            BaseSimObject attackerObject = juubi.attackedBy;
            if (attackerObject == null || attackerObject.a == null || !attackerObject.a.isAlive()) return false;

            Actor attacker = attackerObject.a;
            if (attacker == juubi) return false;

            if (!attacker.hasTrait("ten_tails_jinchuriki") && IsShinobi(attacker))
            {
                attacker.addTrait("ten_tails_jinchuriki");
            }

            attacker.restoreHealth(attacker.getMaxHealth());
            ShinobiWorldLogs.AddWorldLog("log_juubi_sealed", "worldlog_juubi_sealed", "ui/icons/10Tails", attacker);

            return true;
        }
        #endregion

    }
}
