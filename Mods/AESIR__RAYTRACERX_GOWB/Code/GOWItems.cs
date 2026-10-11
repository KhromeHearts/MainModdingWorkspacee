using System;
using System.Threading;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Linq;
using ai;
using NeoModLoader.services;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using NCMS.Utils;
using UnityEngine.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace GOWB
{
  internal static class GOWItems
  {
    private const string GroupId_GOWEquipment = "gow_equipment";

    public static void Init()
    {
      loadItemGroup();
      loadCustomItems();
    }

    private static void loadItemGroup()
    {
      AssetManager.item_groups.add(new ItemGroupAsset
      {
        id = GroupId_GOWEquipment,
        name = "gow_equipment_group",
        color = "#0b73b8"
      });
    }

    private static void loadCustomItems()
    {
      // ─── GUNGNIR ───────────────────────────────────────────────
      EquipmentAsset Gungnir = AssetManager.items.clone("Gungnir", "$range");
      Gungnir.id = "Gungnir";
      Gungnir.translation_key = "weapon_gungnir";
      Gungnir.material = "basic";
      Gungnir.equipment_subtype = "spear";
      Gungnir.quality = Rarity.R3_Legendary;
      Gungnir.is_pool_weapon = false;
      Gungnir.unlock(true);
      Gungnir.name_templates = new List<string>();
      Gungnir.equipment_type = EquipmentType.Weapon;
      Gungnir.attack_type = WeaponType.Range;
      Gungnir.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(Gungnir, "spear");
      Gungnir.projectile = "plasma_ball";
      Gungnir.path_slash_animation = "effects/slashes/slash_spear";
      Gungnir.equipment_value = 10000;
      Gungnir.special_effect_interval = 0.4f;
      Gungnir.base_stats = new BaseStats();
      Gungnir.base_stats.set("damage", 100f);
      Gungnir.base_stats.set("health", 100f);
      Gungnir.base_stats.set("accuracy", 1000f);
      Gungnir.base_stats.set("range", 20f);
      Gungnir.base_stats.set("projectiles", 2f);
      Gungnir.base_stats.set("intelligence", 100f);
      Gungnir.base_stats.set("warfare", 50f);
      Gungnir.base_stats.set("diplomacy", 50f);
      Gungnir.base_stats.set("stewardship", 50f);
      Gungnir.base_stats.set("opinion", 100f);
      Gungnir.action_attack_target = new AttackAction(Gungnirasa);
      Gungnir.group_id = GroupId_GOWEquipment;
      Gungnir.path_icon = "ui/items/icon_Gungnir";
      Gungnir.path_gameplay_sprite = "weapons/weapon_Gungnir";
      assignGameplaySpritesIfExists(Gungnir, "weapon_Gungnir");

      AssetManager.items.list.AddItem(Gungnir);

      // ─── MJOLNIR ───────────────────────────────────────────────
      EquipmentAsset Mjolnir = AssetManager.items.clone("Mjolnir", "$weapon");
      Mjolnir.id = "Mjolnir";
      Mjolnir.translation_key = "weapon_mjolnir";
      Mjolnir.material = "basic";
      Mjolnir.equipment_subtype = "hammer";
      Mjolnir.quality = Rarity.R3_Legendary;
      Mjolnir.is_pool_weapon = false;
      Mjolnir.unlock(true);
      Mjolnir.name_templates = new List<string>();
      Mjolnir.equipment_type = EquipmentType.Weapon;
      Mjolnir.attack_type = WeaponType.Melee;
      Mjolnir.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(Mjolnir, "hammer");
      Mjolnir.path_slash_animation = "effects/slashes/slash_hammer";
      Mjolnir.equipment_value = 10000;
      Mjolnir.special_effect_interval = 0.4f;
      Mjolnir.base_stats = new BaseStats();
      Mjolnir.base_stats.set("damage", 20f);
      Mjolnir.base_stats.set("range", 10f);
      Mjolnir.base_stats.set("critical_chance", 0.2f);
      Mjolnir.base_stats.set("knockback", 1f);
      Mjolnir.base_stats.set("warfare", 90f);
      Mjolnir.action_attack_target = new AttackAction(Mjolnirasa);
      Mjolnir.group_id = GroupId_GOWEquipment;
      Mjolnir.path_icon = "ui/items/icon_Mjolnir";
      Mjolnir.path_gameplay_sprite = "weapons/weapon_Mjolnir";
      assignGameplaySpritesIfExists(Mjolnir, "weapon_Mjolnir");
      
      AssetManager.items.list.AddItem(Mjolnir);

      // ─── LEVIATHAN ─────────────────────────────────────────────
      EquipmentAsset Leviathan = AssetManager.items.clone("Leviathan", "$weapon");
      Leviathan.id = "Leviathan";
      Leviathan.translation_key = "weapon_leviathan";
      Leviathan.material = "basic";
      Leviathan.equipment_subtype = "axe";
      Leviathan.quality = Rarity.R3_Legendary;
      Leviathan.is_pool_weapon = false;
      Leviathan.unlock(true);
      Leviathan.name_templates = new List<string>();
      Leviathan.equipment_type = EquipmentType.Weapon;
      Leviathan.attack_type = WeaponType.Melee;
      Leviathan.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(Leviathan, "axe");
      Leviathan.item_modifier_ids = new string[] { "ice" };
      Leviathan.path_slash_animation = "effects/slashes/slash_axe";
      Leviathan.equipment_value = 9000;
      Leviathan.special_effect_interval = 0.4f;
      Leviathan.base_stats = new BaseStats();
      Leviathan.base_stats.set("damage", 20f);
      Leviathan.base_stats.set("range", 1f);
      Leviathan.base_stats.set("accuracy", 10f);
      Leviathan.base_stats.set("critical_chance", 0.3f);
      Leviathan.base_stats.set("knockback", 0.1f);
      Leviathan.base_stats.set("warfare", 40f);
      Leviathan.action_attack_target = new AttackAction(Leviathanasa);
      Leviathan.group_id = GroupId_GOWEquipment;
      Leviathan.path_icon = "ui/items/icon_Leviathan";
      Leviathan.path_gameplay_sprite = "weapons/weapon_Leviathan";
      assignGameplaySpritesIfExists(Leviathan, "weapon_Leviathan");

      AssetManager.items.list.AddItem(Leviathan);

      // ─── DRAUPNIR ──────────────────────────────────────────────
      EquipmentAsset Draupnir = AssetManager.items.clone("Draupnir", "$range");
      Draupnir.id = "Draupnir";
      Draupnir.translation_key = "weapon_draupnir";
      Draupnir.material = "basic";
      Draupnir.equipment_subtype = "spear";
      Draupnir.quality = Rarity.R3_Legendary;
      Draupnir.is_pool_weapon = false;
      Draupnir.unlock(true);
      Draupnir.name_templates = new List<string>();
      Draupnir.equipment_type = EquipmentType.Weapon;
      Draupnir.attack_type = WeaponType.Range;
      Draupnir.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(Draupnir, "spear");
      Draupnir.projectile = "DraupClone";
      Draupnir.path_slash_animation = "effects/slashes/slash_spear";
      Draupnir.equipment_value = 10000;
      Draupnir.special_effect_interval = 0.4f;
      Draupnir.base_stats = new BaseStats();
      Draupnir.base_stats.set("attack_speed", 20f);
      Draupnir.base_stats.set("damage", 10f);
      Draupnir.base_stats.set("range", 10f);
      Draupnir.base_stats.set("targets", 10f);
      Draupnir.base_stats.set("critical_chance", 0.2f);
      Draupnir.base_stats.set("projectiles", 1f);
      Draupnir.base_stats.set("intelligence", 100f);
      Draupnir.base_stats.set("warfare", 60f);
      Draupnir.base_stats.set("stewardship", 100f);
      Draupnir.action_attack_target = new AttackAction(Draupnirasa);
      Draupnir.group_id = GroupId_GOWEquipment;
      Draupnir.path_icon = "ui/items/icon_Draupnir";
      Draupnir.path_gameplay_sprite = "weapons/weapon_Draupnir";
      assignGameplaySpritesIfExists(Draupnir, "weapon_Draupnir");

      AssetManager.items.list.AddItem(Draupnir);

      // ─── TALON ─────────────────────────────────────────────────
      EquipmentAsset talon = AssetManager.items.clone("talon", "$range");
      talon.id = "talon";
      talon.translation_key = "weapon_talon";
      talon.material = "basic";
      talon.equipment_subtype = "bow";
      talon.quality = Rarity.R3_Legendary;
      talon.is_pool_weapon = false;
      talon.unlock(true);
      talon.name_templates = new List<string>();
      talon.equipment_type = EquipmentType.Weapon;
      talon.attack_type = WeaponType.Range;
      talon.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(talon, "bow");
      talon.projectile = "TalonArrow";
      talon.path_slash_animation = "effects/slashes/slash_bow";
      talon.equipment_value = 1000;
      talon.special_effect_interval = 0.4f;
      talon.base_stats = new BaseStats();
      talon.base_stats.set("damage", 5f);
      talon.base_stats.set("range", 10f);
      talon.base_stats.set("accuracy", 50f);
      talon.base_stats.set("targets", 3f);
      talon.base_stats.set("projectiles", 1f);
      talon.action_attack_target = new AttackAction(talonasa);
      talon.group_id = GroupId_GOWEquipment;
      talon.path_icon = "ui/items/icon_talon";
      talon.path_gameplay_sprite = "weapons/weapon_talon";
      assignGameplaySpritesIfExists(talon, "weapon_talon");

      AssetManager.items.list.AddItem(talon);

      // ─── VANIR GODDESS SWORD ───────────────────────────────────
      EquipmentAsset VanirGoddessSword = AssetManager.items.clone("VanirGoddessSword", "$range");
      VanirGoddessSword.id = "VanirGoddessSword";
      VanirGoddessSword.translation_key = "weapon_vanirgoddesssword";
      VanirGoddessSword.material = "basic";
      VanirGoddessSword.equipment_subtype = "spear";
      VanirGoddessSword.quality = Rarity.R3_Legendary;
      VanirGoddessSword.is_pool_weapon = false;
      VanirGoddessSword.unlock(true);
      VanirGoddessSword.name_templates = new List<string>();
      VanirGoddessSword.equipment_type = EquipmentType.Weapon;
      VanirGoddessSword.attack_type = WeaponType.Range;
      VanirGoddessSword.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(VanirGoddessSword, "spear");
      VanirGoddessSword.projectile = "FriggProjectile";
      VanirGoddessSword.path_slash_animation = "effects/slashes/slash_spear";
      VanirGoddessSword.equipment_value = 1000;
      VanirGoddessSword.special_effect_interval = 0.4f;
      VanirGoddessSword.base_stats = new BaseStats();
      VanirGoddessSword.base_stats.set("multiplier_offspring", 0.3f);
      VanirGoddessSword.base_stats.set("lifespan", 0.5f);
      VanirGoddessSword.base_stats.set("attack_speed", 10f);
      VanirGoddessSword.base_stats.set("damage", 10f);
      VanirGoddessSword.base_stats.set("range", 10f);
      VanirGoddessSword.base_stats.set("accuracy", 5f);
      VanirGoddessSword.base_stats.set("targets", 5f);
      VanirGoddessSword.base_stats.set("critical_chance", 0.1f);
      VanirGoddessSword.base_stats.set("intelligence", 100f);
      VanirGoddessSword.base_stats.set("warfare", 20f);
      VanirGoddessSword.base_stats.set("projectiles", 3f);
      VanirGoddessSword.action_attack_target = new AttackAction(VanirGoddessSwordasa);
      VanirGoddessSword.group_id = GroupId_GOWEquipment;
      VanirGoddessSword.path_icon = "ui/items/icon_VanirGoddessSword";
      VanirGoddessSword.path_gameplay_sprite = "weapons/weapon_VanirGoddessSword";
      assignGameplaySpritesIfExists(VanirGoddessSword, "weapon_VanirGoddessSword");

      AssetManager.items.list.AddItem(VanirGoddessSword);

      // ─── CHAOS BLADES ──────────────────────────────────────────
      EquipmentAsset ChaosBlades = AssetManager.items.clone("ChaosBlades", "$weapon");
      ChaosBlades.id = "ChaosBlades";
      ChaosBlades.translation_key = "weapon_chaosblades";
      ChaosBlades.material = "basic";
      ChaosBlades.equipment_subtype = "sword";
      ChaosBlades.quality = Rarity.R3_Legendary;
      ChaosBlades.is_pool_weapon = false;
      ChaosBlades.unlock(true);
      ChaosBlades.name_templates = new List<string>();
      ChaosBlades.equipment_type = EquipmentType.Weapon;
      ChaosBlades.attack_type = WeaponType.Melee;
      ChaosBlades.name_class = "item_class_weapon";
      ensureWeaponNameTemplates(ChaosBlades, "sword");
      ChaosBlades.item_modifier_ids = new string[] { "flame" };
      ChaosBlades.path_slash_animation = "effects/slashes/chaos_slash3";
      ChaosBlades.equipment_value = 11000;
      ChaosBlades.special_effect_interval = 0.4f;
      ChaosBlades.base_stats = new BaseStats();
      ChaosBlades.base_stats.set("attack_speed", 5f);
      ChaosBlades.base_stats.set("damage", 40f);
      ChaosBlades.base_stats.set("speed", 5f);
      ChaosBlades.base_stats.set("range", 4f);
      ChaosBlades.base_stats.set("accuracy", 20f);
      ChaosBlades.base_stats.set("targets", 10f);
      ChaosBlades.base_stats.set("critical_chance", 0.5f);
      ChaosBlades.base_stats.set("knockback", 0.1f);
      ChaosBlades.base_stats.set("warfare", 80f);
      ChaosBlades.action_attack_target = new AttackAction(ChaosBladesasa);
      ChaosBlades.group_id = GroupId_GOWEquipment;
      ChaosBlades.path_icon = "ui/items/icon_ChaosBlades";
      ChaosBlades.path_gameplay_sprite = "weapons/weapon_ChaosBlades";
      assignGameplaySpritesIfExists(ChaosBlades, "weapon_ChaosBlades");

      AssetManager.items.list.AddItem(ChaosBlades);
    }

    public static bool Gungnirasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null && pTarget.isActor())
      {
        if (Randy.randomChance(0.1f))
        {
          ActionLibrary.addPoisonedEffectOnTarget(null, pTarget, null);
        }
        if (Randy.randomChance(0.05f))
        {
          pTarget.a.addStatusEffect("Binding Spell", 60f);
        }
      }
      return false;
    }

    public static bool Mjolnirasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null && pTile != null)
      {
        if (Randy.randomChance(0.40f))
        {
          MapBox.spawnLightningMedium(pTile, 0.15f);
          EffectsLibrary.spawnExplosionWave(pTile.posV3, 0.5f, 3f);
          World.world.applyForceOnTile(pTarget.current_tile, 3, 0.3f, true, 0, null, pTarget);
          MapBox.spawnLightningMedium(pTile, 0.15f);
        }
      }
      return false;
    }

    public static bool Leviathanasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null) return false;

      if (Randy.randomChance(0.2f))
      {
        if (pTarget.hasStatus("Dodge Effect")) return false;

        pSelf.a.addStatusEffect("No friendly fire", 3f);
        WorldTile targetTile = pTarget.current_tile;
        if (targetTile != null)
        {
          for (int i = 0; i < 30; ++i)
          {
            World.world.loopWithBrush(targetTile, Brush.get(3, "circ_"), new PowerActionWithID(PowerLibrary.drawTemperatureMinus), "temperatureMinus");
          }
        }
        if (pTile != null)
        {
          World.world.applyForceOnTile(pTarget.current_tile, 3, 0.3f, true, 0, null, pTarget);
          World.world.drop_manager.spawnParabolicDrop(pTile, "snow", 0f, 0.1f, 3.5f, 0.5f, 4f, Randy.randomFloat(0.025f, 0.2f));
        }
      }

      if (Randy.randomChance(1f))
      {
        ActionLibrary.addPoisonedEffectOnTarget(null, pTarget, null);
      }
      return false;
    }

    public static bool Draupnirasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null && pTarget.isActor())
      {
        if (Randy.randomChance(0.01f))
        {
          pSelf.a.addStatusEffect("No friendly fire", 15f);
          WorldTile targetTile = pTarget.a.current_tile;
          if (targetTile != null)
          {
            // PowerLibrary pb = new PowerLibrary();
            // pb.spawnCloud(targetTile, "DraupnirCloud");
            GOWB.Spells.CastDraupnirRain(pSelf, pTarget);
          }
        }
      }
      return false;
    }

    public static bool talonasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null && pTarget.isActor())
      {
        if (Randy.randomChance(0.05f))
        {
          pTarget.a.addStatusEffect("Talon Effect", 10f);
          pTarget.a.addTrait("cursed");
        }
      }
      return false;
    }

    public static bool VanirGoddessSwordasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null && pTarget.isActor())
      {
        if (pTile != null)
        {
          World.world.drop_manager.spawnParabolicDrop(pTile, "acid", 0f, 0.1f, 3.5f, 0.5f, 4f, Randy.randomFloat(0.025f, 0.2f));
        }
        if (Randy.randomChance(0.1f))
        {
          pTarget.a.addStatusEffect("Vines Spell", 30f);
        }
      }
      return false;
    }

    public static bool ChaosBladesasa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null && pTarget.isActor())
      {
        if (pTile != null)
        {
          World.world.drop_manager.spawnParabolicDrop(pTile, "fire", 0f, 0.1f, 3.5f, 0.5f, 4f, Randy.randomFloat(0.025f, 0.2f));
          pTarget.a.addStatusEffect("burning", 10f);
          if (Randy.randomChance(0.15f))
          {
            MapAction.damageWorld(pTile, 5, AssetManager.terraform.get("dragon_attack"), null);
          }
        }
      }
      return false;
    }

    private static void assignGameplaySpritesIfExists(EquipmentAsset item, string id)
    {
      Sprite[] sprites = getWeaponSprites(id);
      if (sprites.Length > 0)
      {
        item.gameplay_sprites = sprites;
      }
    }

    private static void ensureWeaponNameTemplates(EquipmentAsset item, string weaponSubtype)
    {
      if (item == null)
      {
        return;
      }

      if (item.name_templates == null)
      {
        item.name_templates = new List<string>();
      }

      item.name_templates.RemoveAll(string.IsNullOrEmpty);
      if (item.name_templates.Count > 0)
      {
        return;
      }

      string baseTemplate = "sword_name";
      switch (weaponSubtype)
      {
        case "spear":
          baseTemplate = "spear_name";
          break;
        case "hammer":
          baseTemplate = "hammer_name";
          break;
        case "axe":
          baseTemplate = "axe_name";
          break;
        case "bow":
          baseTemplate = "bow_name";
          break;
      }

      item.name_templates.Add(baseTemplate);
      item.name_templates.Add("weapon_name_city");
      item.name_templates.Add("weapon_name_kingdom");
      item.name_templates.Add("weapon_name_culture");
    }

    public static Sprite[] getWeaponSprites(string id)
    {
      var sprite = Resources.Load<Sprite>("Weapons/" + id);
      if (sprite != null)
        return new Sprite[] { sprite };
      else
        return Array.Empty<Sprite>();
    }
  }
}