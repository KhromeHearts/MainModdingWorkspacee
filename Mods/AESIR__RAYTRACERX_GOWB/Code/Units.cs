using System;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using NeoModLoader.services;
using NeoModLoader.api;

namespace GOWB
{
  class Units
  {
    public const string OdinActorId = "Odin";
    public const string ThorActorId = "Thor";
    public const string BaldurActorId = "Baldur";
    public const string HeimdallActorId = "Heimdall";
    public const string KratosActorId = "Kratos";
    public const string KratosSpearActorId = "Kratosspear";
    public const string KratosBladesActorId = "Kratosblades";
    public const string JormungandrActorId = "Jormungandr";
    public const string AtreusActorId = "Atreus";
    public const string FreyaActorId = "Freya";


    public static void Init()
    {

      loadAssets();

    }

    public static void loadAssets()
    {
      if (AssetManager.actor_library.get(OdinActorId) == null)
      {
        ActorAsset odin = AssetManager.actor_library.clone(OdinActorId, "$mob$");
        odin.id = OdinActorId;
        odin.name_locale = "Odin";
        odin.power_id = "spawn_odin";
        odin.name_template_sets = new string[] { "human_default_set" };

        odin.is_humanoid = true;
        odin.use_phenotypes = false;
        odin.unit_other = true;
        odin.has_advanced_textures = false;
        odin.check_flip = delegate { return true; };

        odin.job = new string[] { "random_move" };
        odin.kingdom_id_wild = "Aesir";
        odin.icon = "ui_odin";
        odin.animation_swim = new string[] { "swim_0", "swim_1", "swim_2" };
        odin.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        odin.texture_asset = new ActorTextureSubAsset("actors/odin/", odin.has_advanced_textures);
        odin._cached_sprite = Resources.Load<Sprite>("GameResources/actors/odin/Main/walk_0");
        odin.texture_id = "odin";
        odin.default_attack = "Gungnir";
        odin.run_to_water_when_on_fire = false;
        odin.can_be_killed_by_stuff = true;
        odin.can_be_killed_by_life_eraser = true;
        odin.can_attack_buildings = true;
        odin.can_be_moved_by_powers = true;
        odin.can_be_hurt_by_powers = true;
        odin.can_turn_into_zombie = false;
        odin.can_be_inspected = true;
        odin.visible_on_minimap = false;
        odin.use_items = true;
        odin.take_items = false;
        odin.source_meat = false;
        odin.inspect_home = true;
        odin.disable_jump_animation = true;
        odin.has_soul = true;
        odin.force_ocean_creature = false;
        odin.force_land_creature = true;
        odin.can_turn_into_demon_in_age_of_chaos = false;
        odin.can_turn_into_ice_one = false;
        odin.can_turn_into_tumor = false;
        odin.can_turn_into_mush = false;
        odin.die_in_lava = false;
        odin.die_on_blocks = false;
        odin.die_by_lightning = true;
        odin.damaged_by_ocean = false;
        odin.flying = false;
        odin.very_high_flyer = false;
        odin.hide_favorite_icon = false;
        odin.can_edit_traits = true;
        odin.can_be_killed_by_divine_light = false;
        odin.ignored_by_infinity_coin = true;
        odin.can_level_up = false;
        odin.ignore_blocks = false;
        odin.move_from_block = true;
        odin.immune_to_injuries = true;
        odin.actor_size = ActorSize.S13_Human;
        odin.shadow = false;
        odin.body_separate_part_hands = false;
        odin.has_baby_form = false;
        odin.render_heads_for_babies = false;
        odin.can_evolve_into_new_species = false;
        odin.can_have_subspecies = false;
        odin.can_talk_with = false;
        odin.control_can_talk = false;
        odin.spell_ids = List.Of<string>(new string[]
        {
             "Dynja eitr",
             "Drifa",
             "All fireball"
        });
        odin.effect_cast_top = "fx_odin_cast_top";
        odin.effect_cast_ground = "fx_odin_cast_ground";

        odin.base_stats["lifespan"] = 100000f;
        odin.base_stats["health"] = 1000;
        odin.base_stats["damage"] = 60;
        odin.base_stats["speed"] = 20f;
        odin.base_stats["armor"] = 1;
        odin.base_stats["attack_speed"] = 1f;
        odin.base_stats["critical_chance"] = 0.1f;
        odin.base_stats["intelligence"] = 100f;
        odin.base_stats["knockback"] = 0.1f;
        odin.base_stats["accuracy"] = 1f;
        odin.base_stats["range"] = 3;
        odin.base_stats["targets"] = 1f;
        odin.base_stats["mass_2"] = 40f;

        odin.addTrait("immortal");
        odin.addTrait("Odin");
        odin.addTrait("evil");
        odin.addTrait("strong_minded");

        setDefaultResourcePools(odin);
        AssetManager.actor_library.loadTexturesAndSprites(odin);
      }


      if (AssetManager.actor_library.get(ThorActorId) == null)
      {
        ActorAsset thor = AssetManager.actor_library.clone(ThorActorId, "$mob$");
        thor.id = ThorActorId;
        thor.name_locale = "Thor";
        thor.power_id = "spawn_thor";
        thor.name_template_sets = new string[] { "human_default_set" };

        thor.is_humanoid = true;
        thor.use_phenotypes = false;
        thor.unit_other = true;
        thor.has_advanced_textures = false;
        thor.check_flip = delegate { return true; };

        thor.job = new string[] { "random_move" };
        thor.kingdom_id_wild = "Aesir";
        thor.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        thor.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        thor.texture_asset = new ActorTextureSubAsset("actors/thor/", thor.has_advanced_textures);
        thor._cached_sprite = Resources.Load<Sprite>("GameResources/actors/thor/Main/walk_0");
        thor.default_attack = "Mjolnir";
        thor.texture_id = "thor";
        thor.icon = "ui_thor";

        thor.can_be_killed_by_stuff = true;
        thor.can_be_killed_by_life_eraser = true;
        thor.can_attack_buildings = true;
        thor.can_be_moved_by_powers = true;
        thor.can_be_hurt_by_powers = true;
        thor.can_be_inspected = true;
        thor.visible_on_minimap = true;
        thor.use_items = true;
        thor.take_items = true;
        thor.can_talk_with = false;
        thor.control_can_talk = false;
        thor.can_evolve_into_new_species = false;
        thor.can_have_subspecies = false;
        thor.disable_jump_animation = true;
        thor.has_soul = true;
        thor.has_baby_form = false;
        thor.render_heads_for_babies = false;
        thor.body_separate_part_hands = true;
        thor.shadow = false;

        thor.force_land_creature = true;
        thor.force_ocean_creature = false;
        thor.flying = false;
        thor.very_high_flyer = false;
        thor.run_to_water_when_on_fire = true;

        thor.can_turn_into_zombie = false;
        thor.can_turn_into_demon_in_age_of_chaos = false;
        thor.can_turn_into_ice_one = false;
        thor.can_turn_into_tumor = false;
        thor.can_turn_into_mush = false;
        thor.can_be_killed_by_divine_light = true;
        thor.ignored_by_infinity_coin = false;
        thor.can_level_up = false;
        thor.immune_to_injuries = true;
        thor.actor_size = ActorSize.S13_Human;
        thor.can_edit_equipment = true;

        thor.base_stats["lifespan"] = 100000f;
        thor.base_stats["health"] = 1243;
        thor.base_stats["critical_damage_multiplier"] = 0.30f;
        thor.base_stats["damage"] = 120;
        thor.base_stats["speed"] = 15f;
        thor.base_stats["armor"] = 10;
        thor.base_stats["attack_speed"] = 3f;
        thor.base_stats["critical_chance"] = 0.1f;
        thor.base_stats["knockback"] = 0.1f;
        thor.base_stats["accuracy"] = 1f;
        thor.base_stats["range"] = 10;
        thor.base_stats["targets"] = 1f;
        thor.base_stats["mass_2"] = 40f;

        thor.addTrait("immortal");
        thor.addTrait("Thor");
        thor.addTrait("fire_proof");
        thor.addTrait("regeneration");

        setDefaultResourcePools(thor);
        AssetManager.actor_library.loadTexturesAndSprites(thor);
      }


      if (AssetManager.actor_library.get(BaldurActorId) == null)
      {
        ActorAsset baldur = AssetManager.actor_library.clone(BaldurActorId, "$mob$");
        baldur.id = BaldurActorId;
        baldur.name_locale = "Baldur";
        baldur.power_id = "spawn_baldur";
        baldur.name_template_sets = new string[] { "human_default_set" };

        baldur.is_humanoid = true;
        baldur.use_phenotypes = false;
        baldur.unit_other = true;
        baldur.has_advanced_textures = false;
        baldur.check_flip = delegate { return true; };

        baldur.job = new string[] { "random_move" };
        // baldur.kingdom_id_wild = "rouge";
        baldur.kingdom_id_wild = "Aesir";
        baldur.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        baldur.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        baldur.texture_asset = new ActorTextureSubAsset("actors/baldur/", baldur.has_advanced_textures);
        baldur._cached_sprite = Resources.Load<Sprite>("GameResources/actors/baldur/Main/walk_0");
        baldur.spell_ids = List.Of<string>(new string[]
        {
          "Baldur punch"
        });
        baldur.texture_id = "baldur";
        baldur.icon = "ui_baldur";

        baldur.can_be_killed_by_stuff = true;
        baldur.can_be_killed_by_life_eraser = true;
        baldur.can_attack_buildings = true;
        baldur.can_be_moved_by_powers = true;
        baldur.can_be_hurt_by_powers = true;
        baldur.can_be_inspected = true;
        baldur.visible_on_minimap = true;
        baldur.use_items = true;
        baldur.take_items = true;
        baldur.can_talk_with = false;
        baldur.control_can_talk = false;
        baldur.can_evolve_into_new_species = false;
        baldur.can_have_subspecies = false;
        baldur.disable_jump_animation = false;
        baldur.has_soul = true;
        baldur.has_baby_form = false;
        baldur.render_heads_for_babies = false;
        baldur.body_separate_part_hands = true;
        baldur.shadow = false;

        baldur.force_land_creature = true;
        baldur.force_ocean_creature = false;
        baldur.flying = false;
        baldur.very_high_flyer = false;
        baldur.run_to_water_when_on_fire = true;

        baldur.can_turn_into_zombie = false;
        baldur.can_turn_into_demon_in_age_of_chaos = false;
        baldur.can_turn_into_ice_one = false;
        baldur.can_turn_into_tumor = false;
        baldur.can_turn_into_mush = false;
        baldur.can_be_killed_by_divine_light = true;
        baldur.ignored_by_infinity_coin = false;
        baldur.can_level_up = false;
        baldur.immune_to_injuries = true;
        baldur.actor_size = ActorSize.S13_Human;
        baldur.can_edit_equipment = true;

        baldur.base_stats["lifespan"] = 100000f;
        baldur.base_stats["health"] = 1000f;
        baldur.base_stats["damage"] = 50f;
        baldur.base_stats["speed"] = 25f;
        baldur.base_stats["armor"] = 10f;
        baldur.base_stats["attack_speed"] = 5f;
        baldur.base_stats["critical_chance"] = 0.1f;
        baldur.base_stats["knockback"] = 0.1f;
        baldur.base_stats["accuracy"] = 1f;
        baldur.base_stats["range"] = 2f;
        baldur.base_stats["targets"] = 1f;
        baldur.base_stats["mass_2"] = 40f;

        baldur.addTrait("immortal");
        baldur.addTrait("Baldur");
        baldur.addTrait("mistletoe_curse");
        baldur.addTrait("freeze_proof");

        setDefaultResourcePools(baldur);
        AssetManager.actor_library.loadTexturesAndSprites(baldur);
      }

      if (AssetManager.actor_library.get(HeimdallActorId) == null)
      {
        ActorAsset heimdall = AssetManager.actor_library.clone(HeimdallActorId, "$mob$");
        heimdall.id = HeimdallActorId;
        heimdall.name_locale = "Heimdall";
        heimdall.power_id = "spawn_heimdall";
        heimdall.name_template_sets = new string[] { "human_default_set" };

        heimdall.is_humanoid = true;
        heimdall.use_phenotypes = false;
        heimdall.unit_other = true;
        heimdall.has_advanced_textures = false;
        heimdall.check_flip = delegate { return true; };

        heimdall.job = new string[] { "random_move" };
        heimdall.kingdom_id_wild = "Aesir";
        heimdall.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        heimdall.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        heimdall.texture_asset = new ActorTextureSubAsset("actors/heimdall/", heimdall.has_advanced_textures);
        heimdall._cached_sprite = Resources.Load<Sprite>("GameResources/actors/heimdall/Main/walk_0");
        heimdall.texture_id = "heimdall";
        heimdall.icon = "ui_heimdall";

        heimdall.can_be_killed_by_stuff = true;
        heimdall.can_be_killed_by_life_eraser = true;
        heimdall.can_attack_buildings = true;
        heimdall.can_be_moved_by_powers = true;
        heimdall.can_be_hurt_by_powers = true;
        heimdall.can_be_inspected = true;
        heimdall.visible_on_minimap = true;
        heimdall.use_items = true;
        heimdall.take_items = true;
        heimdall.can_talk_with = false;
        heimdall.control_can_talk = false;
        heimdall.can_evolve_into_new_species = false;
        heimdall.can_have_subspecies = false;
        heimdall.disable_jump_animation = true;
        heimdall.has_soul = true;
        heimdall.has_baby_form = false;
        heimdall.render_heads_for_babies = false;
        heimdall.body_separate_part_hands = true;
        heimdall.shadow = false;

        heimdall.force_land_creature = true;
        heimdall.force_ocean_creature = false;
        heimdall.flying = false;
        heimdall.very_high_flyer = false;
        heimdall.run_to_water_when_on_fire = true;

        heimdall.can_turn_into_zombie = false;
        heimdall.can_turn_into_demon_in_age_of_chaos = false;
        heimdall.can_turn_into_ice_one = false;
        heimdall.can_turn_into_tumor = false;
        heimdall.can_turn_into_mush = false;
        heimdall.can_be_killed_by_divine_light = true;
        heimdall.ignored_by_infinity_coin = false;
        heimdall.can_level_up = false;
        heimdall.immune_to_injuries = true;
        heimdall.actor_size = ActorSize.S13_Human;
        heimdall.can_edit_equipment = true;

        heimdall.base_stats["lifespan"] = 100000f;
        heimdall.base_stats["health"] = 1000f;
        heimdall.base_stats["damage"] = 50f;
        heimdall.base_stats["speed"] = 30f;
        heimdall.base_stats["armor"] = 1f;
        heimdall.base_stats["attack_speed"] = 3f;
        heimdall.base_stats["critical_chance"] = 0.1f;
        heimdall.base_stats["knockback"] = 0.1f;
        heimdall.base_stats["accuracy"] = 1f;
        heimdall.base_stats["range"] = 2f;
        heimdall.base_stats["targets"] = 1f;
        heimdall.base_stats["mass_2"] = 40f;

        heimdall.addTrait("immortal");
        heimdall.addTrait("Heimdall");
        heimdall.addTrait("evil");

        setDefaultResourcePools(heimdall);
        AssetManager.actor_library.loadTexturesAndSprites(heimdall);
      }
      if (AssetManager.actor_library.get(KratosActorId) == null)
      {
        ActorAsset kratos = AssetManager.actor_library.clone(KratosActorId, "$mob$");
        kratos.id = KratosActorId;
        kratos.name_locale = "Kratos";
        kratos.power_id = "spawn_kratos";
        kratos.name_template_sets = new string[] { "human_default_set" };

        kratos.is_humanoid = true;
        kratos.use_phenotypes = false;
        kratos.unit_other = true;
        kratos.has_advanced_textures = false;
        kratos.check_flip = delegate { return true; };

        kratos.job = new string[] { "random_move" };
        kratos.kingdom_id_wild = "Vanir";
        kratos.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        kratos.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        kratos.texture_asset = new ActorTextureSubAsset("actors/kratos/", kratos.has_advanced_textures);
        kratos._cached_sprite = Resources.Load<Sprite>("GameResources/actors/kratos/Main/walk_0");
        kratos.default_attack = "Leviathan";
        kratos.texture_id = "kratos";
        kratos.icon = "ui_kratos";

        kratos.can_be_killed_by_stuff = true;
        kratos.can_be_killed_by_life_eraser = true;
        kratos.can_attack_buildings = true;
        kratos.can_be_moved_by_powers = true;
        kratos.can_be_hurt_by_powers = true;
        kratos.can_be_inspected = true;
        kratos.visible_on_minimap = true;
        kratos.use_items = true;
        kratos.take_items = true;
        kratos.can_talk_with = false;
        kratos.control_can_talk = false;
        kratos.can_evolve_into_new_species = false;
        kratos.can_have_subspecies = false;
        kratos.disable_jump_animation = true;
        kratos.has_soul = true;
        kratos.has_baby_form = false;
        kratos.render_heads_for_babies = false;
        kratos.body_separate_part_hands = true;
        kratos.shadow = false;
        kratos.die_in_lava = false;

        kratos.force_land_creature = true;
        kratos.force_ocean_creature = false;
        kratos.flying = false;
        kratos.very_high_flyer = false;
        kratos.run_to_water_when_on_fire = true;

        kratos.can_turn_into_zombie = false;
        kratos.can_turn_into_demon_in_age_of_chaos = false;
        kratos.can_turn_into_ice_one = false;
        kratos.can_turn_into_tumor = false;
        kratos.can_turn_into_mush = false;
        kratos.can_be_killed_by_divine_light = true;
        kratos.ignored_by_infinity_coin = false;
        kratos.immune_to_injuries = true;
        kratos.actor_size = ActorSize.S13_Human;
        kratos.can_edit_equipment = true;

        kratos.spell_ids = List.Of<string>(new string[]
        {
             "Cast throw status",
             "Throw leviathan axe"
        });
        kratos.base_stats["lifespan"] = 100000f;
        kratos.base_stats["health"] = 1000f;
        kratos.base_stats["damage"] = 80f;
        kratos.base_stats["speed"] = 15f;
        kratos.base_stats["armor"] = 1f;
        kratos.base_stats["attack_speed"] = 20f;
        kratos.base_stats["critical_chance"] = 0.1f;
        kratos.base_stats["knockback"] = 0.1f;
        kratos.base_stats["accuracy"] = 1f;
        kratos.base_stats["range"] = 1f;
        kratos.base_stats["targets"] = 1f;
        kratos.base_stats["mass_2"] = 40f;

        kratos.addTrait("immortal");
        kratos.addTrait("Kratos");
        kratos.addTrait("regeneration");
        kratos.addTrait("fire_proof");
        kratos.addTrait("poison_immune");
        kratos.addTrait("strong_minded");

        setDefaultResourcePools(kratos);
        AssetManager.actor_library.loadTexturesAndSprites(kratos);
      }

      if (AssetManager.actor_library.get(KratosSpearActorId) == null)
      {
        ActorAsset kratosspear = AssetManager.actor_library.clone("Kratosspear", "$mob$");
        kratosspear.id = "Kratosspear";
        kratosspear.name_locale = "Kratos";
        kratosspear.power_id = "spawn_kratosspear";
        kratosspear.name_template_sets = new string[] { "human_default_set" };

        kratosspear.is_humanoid = true;
        kratosspear.use_phenotypes = false;
        kratosspear.unit_other = true;
        kratosspear.has_advanced_textures = false;
        kratosspear.check_flip = delegate { return true; };

        kratosspear.job = new string[] { "random_move" };
        kratosspear.kingdom_id_wild = "Vanir";
        kratosspear.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        kratosspear.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        kratosspear.texture_asset = new ActorTextureSubAsset("actors/kratosspear/", kratosspear.has_advanced_textures);
        kratosspear._cached_sprite = Resources.Load<Sprite>("GameResources/actors/kratosspear/Main/walk_0");
        kratosspear.default_attack = "Draupnir";
        kratosspear.texture_id = "kratosspear";
        kratosspear.icon = "ui_kratos";

        kratosspear.can_be_killed_by_stuff = true;
        kratosspear.can_be_killed_by_life_eraser = true;
        kratosspear.can_attack_buildings = true;
        kratosspear.can_be_moved_by_powers = true;
        kratosspear.can_be_hurt_by_powers = true;
        kratosspear.can_be_inspected = true;
        kratosspear.visible_on_minimap = true;
        kratosspear.use_items = true;
        kratosspear.take_items = true;
        kratosspear.can_talk_with = false;
        kratosspear.control_can_talk = false;
        kratosspear.can_evolve_into_new_species = false;
        kratosspear.can_have_subspecies = false;
        kratosspear.disable_jump_animation = true;
        kratosspear.has_soul = true;
        kratosspear.has_baby_form = false;
        kratosspear.render_heads_for_babies = false;
        kratosspear.body_separate_part_hands = true;
        kratosspear.shadow = false;
        kratosspear.die_in_lava = false;

        kratosspear.force_land_creature = true;
        kratosspear.force_ocean_creature = false;
        kratosspear.flying = false;
        kratosspear.very_high_flyer = false;
        kratosspear.run_to_water_when_on_fire = true;

        kratosspear.can_turn_into_zombie = false;
        kratosspear.can_turn_into_demon_in_age_of_chaos = false;
        kratosspear.can_turn_into_ice_one = false;
        kratosspear.can_turn_into_tumor = false;
        kratosspear.can_turn_into_mush = false;
        kratosspear.can_be_killed_by_divine_light = true;
        kratosspear.ignored_by_infinity_coin = false;
        kratosspear.immune_to_injuries = true;
        kratosspear.actor_size = ActorSize.S13_Human;
        kratosspear.can_edit_equipment = true;

        kratosspear.base_stats["lifespan"] = 100000f;
        kratosspear.base_stats["health"] = 1500;
        kratosspear.base_stats["damage"] = 100f;
        kratosspear.base_stats["speed"] = 10f;
        kratosspear.base_stats["armor"] = 1f;
        kratosspear.base_stats["attack_speed"] = 20f;
        kratosspear.base_stats["critical_chance"] = 0.1f;
        kratosspear.base_stats["knockback"] = 0.1f;
        kratosspear.base_stats["accuracy"] = 1f;
        kratosspear.base_stats["range"] = 1f;
        kratosspear.base_stats["targets"] = 1f;
        kratosspear.base_stats["mass_2"] = 40f;

        kratosspear.addTrait("immortal");
        kratosspear.addTrait("Kratos");
        kratosspear.addTrait("regeneration");
        kratosspear.addTrait("fire_proof");
        kratosspear.addTrait("poison_immune");
        kratosspear.addTrait("strong_minded");

        setDefaultResourcePools(kratosspear);
        AssetManager.actor_library.loadTexturesAndSprites(kratosspear);
      }

      if (AssetManager.actor_library.get(KratosBladesActorId) == null)
      {
        ActorAsset kratosblades = AssetManager.actor_library.clone(KratosBladesActorId, "$mob$");
        kratosblades.id = KratosBladesActorId;
        kratosblades.name_locale = "Kratos";
        kratosblades.power_id = "spawn_kratosblades";
        kratosblades.name_template_sets = new string[] { "human_default_set" };

        kratosblades.is_humanoid = true;
        kratosblades.use_phenotypes = false;
        kratosblades.unit_other = true;
        kratosblades.has_advanced_textures = false;
        kratosblades.check_flip = delegate { return true; };

        kratosblades.job = new string[] { "random_move" };
        kratosblades.kingdom_id_wild = "Vanir";
        kratosblades.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        kratosblades.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        kratosblades.texture_asset = new ActorTextureSubAsset("actors/kratosblades/", kratosblades.has_advanced_textures);
        kratosblades._cached_sprite = Resources.Load<Sprite>("GameResources/actors/kratosblades/Main/walk_0");
        kratosblades.default_attack = "ChaosBlades";
        kratosblades.texture_id = "kratosblades";
        kratosblades.icon = "ui_kratos";

        kratosblades.can_be_killed_by_stuff = true;
        kratosblades.can_be_killed_by_life_eraser = true;
        kratosblades.can_attack_buildings = true;
        kratosblades.can_be_moved_by_powers = true;
        kratosblades.can_be_hurt_by_powers = true;
        kratosblades.can_be_inspected = true;
        kratosblades.visible_on_minimap = true;
        kratosblades.use_items = true;
        kratosblades.take_items = true;
        kratosblades.can_talk_with = false;
        kratosblades.control_can_talk = false;
        kratosblades.can_evolve_into_new_species = false;
        kratosblades.can_have_subspecies = false;
        kratosblades.disable_jump_animation = true;
        kratosblades.has_soul = true;
        kratosblades.has_baby_form = false;
        kratosblades.render_heads_for_babies = false;
        kratosblades.body_separate_part_hands = true;
        kratosblades.shadow = false;
        kratosblades.die_in_lava = false;

        kratosblades.force_land_creature = true;
        kratosblades.force_ocean_creature = false;
        kratosblades.flying = false;
        kratosblades.very_high_flyer = false;
        kratosblades.run_to_water_when_on_fire = true;

        kratosblades.can_turn_into_zombie = false;
        kratosblades.can_turn_into_demon_in_age_of_chaos = false;
        kratosblades.can_turn_into_ice_one = false;
        kratosblades.can_turn_into_tumor = false;
        kratosblades.can_turn_into_mush = false;
        kratosblades.can_be_killed_by_divine_light = true;
        kratosblades.ignored_by_infinity_coin = false;
        kratosblades.immune_to_injuries = true;
        kratosblades.actor_size = ActorSize.S13_Human;
        kratosblades.can_edit_equipment = true;

        kratosblades.base_stats["lifespan"] = 100000f;
        kratosblades.base_stats["health"] = 1500f;
        kratosblades.base_stats["damage"] = 100f;
        kratosblades.base_stats["speed"] = 10f;
        kratosblades.base_stats["armor"] = 1f;
        kratosblades.base_stats["attack_speed"] = 10f;
        kratosblades.base_stats["critical_chance"] = 0.1f;
        kratosblades.base_stats["knockback"] = 0.1f;
        kratosblades.base_stats["accuracy"] = 1f;
        kratosblades.base_stats["range"] = 1f;
        kratosblades.base_stats["targets"] = 1f;
        kratosblades.base_stats["mass_2"] = 40f;

        kratosblades.addTrait("immortal");
        kratosblades.addTrait("Kratos");
        kratosblades.addTrait("regeneration");
        kratosblades.addTrait("fire_proof");
        kratosblades.addTrait("poison_immune");
        kratosblades.addTrait("strong_minded");

        setDefaultResourcePools(kratosblades);
        AssetManager.actor_library.loadTexturesAndSprites(kratosblades);
      }

      if (AssetManager.actor_library.get(JormungandrActorId) == null)
      {
        ActorAsset jormungandr = AssetManager.actor_library.clone(JormungandrActorId, "$mob$");
        jormungandr.id = JormungandrActorId;
        jormungandr.name_locale = "Jormungandr";
        jormungandr.power_id = "spawn_jormungandr";
        jormungandr.job = new string[] { "random_move" };
        jormungandr.name_template_sets = new string[] { "human_default_set" };

        jormungandr.kingdom_id_wild = "Jotnar";
        jormungandr.icon = "ui_jormungandr";
        jormungandr.animation_swim = new string[] { "swim_1", "swim_2", "swim_3", "swim_4", "swim_5", "swim_6", "swim_7", "swim_8" };
        jormungandr.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7", "walk_8" };
        jormungandr.has_advanced_textures = false;
        jormungandr.texture_asset = new ActorTextureSubAsset("actors/jormungandr/", jormungandr.has_advanced_textures);
        jormungandr._cached_sprite = Resources.Load<Sprite>("GameResources/actors/jormungandr/Main/walk_0");
        jormungandr.texture_id = "jormungandr";
        jormungandr.run_to_water_when_on_fire = true;
        jormungandr.can_be_killed_by_stuff = true;
        jormungandr.can_be_killed_by_life_eraser = true;
        jormungandr.unit_other = true;
        jormungandr.check_flip = delegate { return true; };
        jormungandr.use_phenotypes = false;
        jormungandr.can_attack_buildings = false;
        jormungandr.can_be_moved_by_powers = true;
        jormungandr.can_be_hurt_by_powers = true;
        jormungandr.can_turn_into_zombie = false;
        jormungandr.immune_to_slowness = true;
        jormungandr.can_be_inspected = true;
        jormungandr.visible_on_minimap = true;
        jormungandr.use_items = true;
        jormungandr.take_items = false;
        jormungandr.inspect_home = true;
        jormungandr.disable_jump_animation = true;
        jormungandr.has_soul = false;
        jormungandr.force_ocean_creature = true;
        jormungandr.force_land_creature = true;
        jormungandr.can_turn_into_demon_in_age_of_chaos = false;
        jormungandr.can_turn_into_ice_one = false;
        jormungandr.can_turn_into_tumor = false;
        jormungandr.can_turn_into_mush = false;
        jormungandr.can_flip = true;
        jormungandr.die_in_lava = true;
        jormungandr.die_on_blocks = false;
        jormungandr.die_by_lightning = true;
        jormungandr.damaged_by_ocean = false;
        jormungandr.flying = false;
        jormungandr.very_high_flyer = false;
        jormungandr.hide_favorite_icon = true;
        jormungandr.can_edit_traits = true;
        jormungandr.can_be_killed_by_divine_light = true;
        jormungandr.ignored_by_infinity_coin = false;
        jormungandr.can_level_up = false;
        jormungandr.immune_to_injuries = true;
        jormungandr.ignore_blocks = true;
        jormungandr.move_from_block = false;
        jormungandr.actor_size = ActorSize.S17_Dragon;
        jormungandr.shadow = false;
        jormungandr.body_separate_part_hands = false;
        jormungandr.has_baby_form = false;
        jormungandr.render_heads_for_babies = false;
        jormungandr.can_evolve_into_new_species = false;
        jormungandr.can_have_subspecies = false;
        jormungandr.can_talk_with = false;
        jormungandr.control_can_talk = false;

        jormungandr.base_stats["lifespan"] = 100000f;
        jormungandr.base_stats["health"] = 3000f;
        jormungandr.base_stats["damage"] = 100f;
        jormungandr.base_stats["speed"] = 10f;
        jormungandr.base_stats["armor"] = 1f;
        jormungandr.base_stats["attack_speed"] = 1.0f;
        jormungandr.base_stats["critical_chance"] = 0.1f;
        jormungandr.base_stats["knockback"] = 0.1f;
        jormungandr.base_stats["accuracy"] = 1f;
        jormungandr.base_stats["range"] = 20f;
        jormungandr.base_stats["targets"] = 20f;
        jormungandr.base_stats["mass_2"] = 40f;
        jormungandr.inspect_avatar_scale = 0.8f;

        jormungandr.addTrait("immortal");
        jormungandr.addTrait("pacifist");
        jormungandr.addTrait("Jormungandr");
        jormungandr.addTrait("freeze_proof");
        jormungandr.addTrait("regeneration");
        jormungandr.addTrait("poison_immune");
        jormungandr.addTrait("tough");

        // setDefaultResourcePools(jormungandr);
        // AssetManager.actor_library.loadTexturesAndSprites(jormungandr);
      }

      if (AssetManager.actor_library.get(AtreusActorId) == null)
      {
        ActorAsset atreus = AssetManager.actor_library.clone(AtreusActorId, "$mob$");
        atreus.id = AtreusActorId;
        atreus.name_locale = "Atreus";
        atreus.power_id = "spawn_atreus";
        atreus.name_template_sets = new string[] { "human_default_set" };
        atreus.job = new string[] { "random_move" };
        atreus.kingdom_id_wild = "Vanir";
        atreus.icon = "ui_atreus";
        atreus.animation_swim = new string[] { "swim_0", "swim_1", "swim_2", "swim_3" };
        atreus.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        atreus.has_advanced_textures = false;
        atreus.texture_asset = new ActorTextureSubAsset("actors/atreus/", atreus.has_advanced_textures);
        atreus._cached_sprite = Resources.Load<Sprite>("GameResources/actors/atreus/Main/walk_0");
        atreus.texture_id = "atreus";
        atreus.default_attack = "talon";
        atreus.run_to_water_when_on_fire = true;
        atreus.can_be_killed_by_stuff = true;
        atreus.can_be_killed_by_life_eraser = true;
        atreus.can_attack_buildings = false;
        atreus.can_be_moved_by_powers = true;
        atreus.can_be_hurt_by_powers = true;
        atreus.can_turn_into_zombie = false;
        atreus.can_be_inspected = true;
        atreus.visible_on_minimap = false;
        atreus.use_items = true;
        atreus.take_items = false;
        atreus.source_meat = false;
        atreus.inspect_home = true;
        atreus.disable_jump_animation = true;
        atreus.has_soul = true;
        atreus.force_ocean_creature = false;
        atreus.force_land_creature = true;
        atreus.can_turn_into_demon_in_age_of_chaos = false;
        atreus.can_turn_into_ice_one = false;
        atreus.can_turn_into_tumor = false;
        atreus.can_turn_into_mush = false;
        atreus.die_in_lava = true;
        atreus.die_on_blocks = false;
        atreus.die_by_lightning = true;
        atreus.damaged_by_ocean = false;
        atreus.flying = false;
        atreus.very_high_flyer = false;
        atreus.hide_favorite_icon = false;
        atreus.can_edit_traits = true;
        atreus.can_be_killed_by_divine_light = true;
        atreus.ignored_by_infinity_coin = true;
        atreus.can_level_up = false;
        atreus.immune_to_injuries = true;
        atreus.actor_size = ActorSize.S13_Human;
        atreus.shadow = false;
        atreus.body_separate_part_hands = false;
        atreus.has_baby_form = false;
        atreus.render_heads_for_babies = false;
        atreus.can_evolve_into_new_species = false;
        atreus.can_have_subspecies = false;
        atreus.can_talk_with = false;
        atreus.control_can_talk = false;
        atreus.unit_other = true;
        atreus.use_phenotypes = false;
        atreus.check_flip = delegate { return true; };

        atreus.base_stats["lifespan"] = 1000f;
        atreus.base_stats["health"] = 500f;
        atreus.base_stats["damage"] = 10f;
        atreus.base_stats["speed"] = 25f;
        atreus.base_stats["armor"] = 1f;
        atreus.base_stats["attack_speed"] = 30f;
        atreus.base_stats["critical_chance"] = 0.3f;
        atreus.base_stats["knockback"] = 0.1f;
        atreus.base_stats["accuracy"] = 1f;
        atreus.base_stats["range"] = 1f;
        atreus.base_stats["targets"] = 1f;
        atreus.base_stats["mass_2"] = 40f;

        atreus.addTrait("immortal");
        atreus.addTrait("strong_minded");
        atreus.addTrait("fire_proof");
        atreus.addTrait("Atreus");

        setDefaultResourcePools(atreus);
        AssetManager.actor_library.loadTexturesAndSprites(atreus);
      }

      if (AssetManager.actor_library.get(FreyaActorId) == null)
      {
        ActorAsset freya = AssetManager.actor_library.clone(FreyaActorId, "$mob$");
        freya.id = FreyaActorId;
        freya.name_locale = "Freya";
        freya.name_template_sets = new string[] { "human_default_set" };
        freya.job = new string[] { "random_move" };
        freya.kingdom_id_wild = "Vanir";
        freya.icon = "ui_freya";
        freya.animation_swim = new string[] { "swim_0", "swim_0", "swim_1", "swim_1", "swim_1", "swim_2", "swim_2", "swim_2", "swim_2" };
        freya.animation_walk = new string[] { "walk_0", "walk_1", "walk_2", "walk_3" };
        freya.has_advanced_textures = false;
        freya.texture_asset = new ActorTextureSubAsset("actors/freya/", freya.has_advanced_textures);
        freya._cached_sprite = Resources.Load<Sprite>("GameResources/actors/freya/Main/walk_0");
        freya.check_flip = delegate { return true; };
        freya.texture_id = "freyaa";
        freya.default_attack = "VanirGoddessSword";
        freya.run_to_water_when_on_fire = true;
        freya.can_be_killed_by_stuff = true;
        freya.can_be_killed_by_life_eraser = true;
        freya.can_attack_buildings = false;
        freya.can_be_moved_by_powers = true;
        freya.can_be_hurt_by_powers = true;
        freya.can_turn_into_zombie = false;
        freya.can_be_inspected = true;
        freya.visible_on_minimap = false;
        freya.use_items = true;
        freya.take_items = false;
        freya.source_meat = false;
        freya.inspect_home = true;
        freya.disable_jump_animation = true;
        freya.has_soul = true;
        freya.force_ocean_creature = false;
        freya.force_land_creature = true;
        freya.can_turn_into_demon_in_age_of_chaos = false;
        freya.can_turn_into_ice_one = false;
        freya.can_turn_into_tumor = false;
        freya.can_turn_into_mush = false;
        freya.die_in_lava = false;
        freya.die_on_blocks = false;
        freya.die_by_lightning = true;
        freya.damaged_by_ocean = false;
        freya.flying = false;
        freya.very_high_flyer = false;
        freya.hide_favorite_icon = false;
        freya.can_edit_traits = true;
        freya.can_be_killed_by_divine_light = false;
        freya.ignored_by_infinity_coin = true;
        freya.can_level_up = false;
        freya.immune_to_injuries = true;
        freya.actor_size = ActorSize.S13_Human;
        freya.shadow = false;
        freya.body_separate_part_hands = false;
        freya.has_baby_form = false;
        freya.render_heads_for_babies = false;
        freya.can_evolve_into_new_species = false;
        freya.can_have_subspecies = false;
        freya.can_talk_with = false;
        freya.control_can_talk = false;
        freya.check_flip = delegate { return true; };
        freya.unit_other = true;
        freya.use_phenotypes = false;
        freya.spell_ids = List.Of<string>(new string[]
        {
             "Cast living plants"
        });
        freya.effect_cast_top = "fx_cast_top_green";
        freya.base_stats["lifespan"] = 100000f;
        freya.base_stats["health"] = 500f;
        freya.base_stats["damage"] = 225f;
        freya.base_stats["speed"] = 15f;
        freya.base_stats["armor"] = 1f;
        freya.base_stats["attack_speed"] = 5f;
        freya.base_stats["critical_chance"] = 0.1f;
        freya.base_stats["knockback"] = 0.8f;
        freya.base_stats["accuracy"] = 1f;
        freya.base_stats["range"] = 20f;
        freya.base_stats["targets"] = 6f;
        freya.base_stats["mass_2"] = 40f;

        freya.addTrait("immortal");
        freya.addTrait("Freya");
        freya.addTrait("fire_proof");
        freya.addTrait("strong_minded");
        freya.addTrait("poison_immune");

        setDefaultResourcePools(freya);
        AssetManager.actor_library.loadTexturesAndSprites(freya);
      }
    }

    public static void setDefaultResourcePools(ActorAsset actor)
    {
      if (actor?.base_stats == null)
      {
        return;
      }

      actor.base_stats["mana"] = 500f;
      actor.base_stats["stamina"] = 500f;
    }
  }
}


