using System;
using System.Threading;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai;
using Beebyte.Obfuscator;

namespace GOWB
{
  internal static class GOWTraits
  {
    private static string GroupId_GOWR = "GOWRagnarok";
    private static string GroupId_GOWR2 = "GOWRagnarok2";
    private const string BirthRateEnabled = "2";
    private const string BirthRateDisabled = "0";

    private static List<ActorTrait> myListTraits = new List<ActorTrait>();

    //Group Ids
    public static void Init()
    {
      loadGroups();
      loadTraits();
    }
    public static void loadGroups()
    {
      if (AssetManager.trait_groups.get(GroupId_GOWR) == null)
      {
        ActorTraitGroupAsset GOWRagnarok = new ActorTraitGroupAsset();
        GOWRagnarok.id = GroupId_GOWR;
        GOWRagnarok.name = "Ragnarok";
        GOWRagnarok.color = "#790000";
        AssetManager.trait_groups.add(GOWRagnarok);
      }

      if (AssetManager.trait_groups.get(GroupId_GOWR2) == null)
      {
        ActorTraitGroupAsset GOWRagnarok2 = new ActorTraitGroupAsset();
        GOWRagnarok2.id = GroupId_GOWR2;
        GOWRagnarok2.name = "Ragnarok 2";
        GOWRagnarok2.color = "#0059ff";
        AssetManager.trait_groups.add(GOWRagnarok2);
      }
    }

    public static void loadTraits()
    {
      #region Thor
      ActorTrait thor = new ActorTrait
      {
        id = "Thor",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/thor",
        needs_to_be_explored = false,
        can_be_given = true,
        rate_birth = 0,
        rate_inherit = 0
      };
      thor.base_stats = new BaseStats();
      thor.base_stats.set("lifespan", 200f);
      thor.base_stats.set("damage", 300);
      thor.base_stats.set("health", 14000);
      thor.base_stats.set("range", 10);
      thor.base_stats.set("armor", 30);
      thor.base_stats.set("scale", 0.1f);
      thor.base_stats.set("targets", 5f);
      thor.base_stats.set("critical_chance", 0.20f);
      thor.base_stats.set("knockback", 1f);
      thor.base_stats.set("intelligence", 20);
      thor.base_stats.set("warfare", 200);
      thor.base_stats.set("multiplier_speed", 0.20f);
      thor.base_stats.set("attack_speed", -0.20f);
      thor.base_stats.set("skill_combat", 0.9f);
      thor.addCombatAction("combat_block");
      thor.addCombatAction("combat_dash");
      thor.addCombatAction("combat_deflect_projectile");
      thor.linkCombatActions();
      thor.action_death = new WorldAction(Thorda);
      thor.action_special_effect = (WorldAction)Delegate.Combine(thor.action_special_effect, new WorldAction(thorProof));
      thor.action_attack_target = (AttackAction)Delegate.Combine(thor.action_attack_target, new AttackAction(thoraa));
      thor.action_get_hit = (GetHitAction)Delegate.Combine(thor.action_get_hit, new GetHitAction(thorgha));

      addTraitToGame(thor);
      #endregion

      #region Odin
      ActorTrait odin = new ActorTrait
      {
        id = "Odin",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/odin",
        needs_to_be_explored = false,
        can_be_given = true,
        rate_birth = 0,
        rate_inherit = 0
      };
      odin.base_stats = new BaseStats();
      odin.base_stats.set("lifespan", 200f);
      odin.base_stats.set("damage", 200);
      odin.base_stats.set("speed", 15f);
      odin.base_stats.set("health", 6000);
      odin.base_stats.set("accuracy", 10f);
      odin.base_stats.set("range", 15);
      odin.base_stats.set("armor", 10);
      odin.base_stats.set("targets", 3f);
      odin.base_stats.set("intelligence", 500);
      odin.base_stats.set("warfare", 100);
      odin.base_stats.set("diplomacy", 100);
      odin.base_stats.set("stewardship", 100);
      odin.base_stats.set("opinion", 90f);
      odin.base_stats.set("attack_speed", 0.20f);
      odin.base_stats.set("mana", 3000f);
      odin.base_stats.set("skill_combat", 0.3f);
      odin.base_stats.set("skill_spell", 3f);
      odin.addSpell("Drifa");
      odin.addSpell("All fireball");
      odin.linkSpells();
      odin.addCombatAction("combat_backstep");
      odin.addCombatAction("combat_block");
      odin.linkCombatActions();
      odin.action_death = new WorldAction(OdinPhaseTwo);
      odin.action_special_effect = (WorldAction)Delegate.Combine(odin.action_special_effect, new WorldAction(Odinwa));
      odin.action_attack_target = (AttackAction)Delegate.Combine(odin.action_attack_target, new AttackAction(Odinaa));

      addTraitToGame(odin);
      #endregion

      #region Odin Phase 2
      ActorTrait phase2odin = new ActorTrait
      {
        id = "Odin Phase 2",
        group_id = GroupId_GOWR2,
        path_icon = "ui/Icons/odinphase2",
        needs_to_be_explored = false,
        can_be_given = false,
        rate_birth = 0,
        rate_inherit = 0

      };
      phase2odin.base_stats = new BaseStats();
      phase2odin.base_stats.set("lifespan", 200f);
      phase2odin.base_stats.set("damage", 300);
      phase2odin.base_stats.set("speed", 20f);
      phase2odin.base_stats.set("health", 7000);
      phase2odin.base_stats.set("accuracy", 100f);
      phase2odin.base_stats.set("range", 20);
      phase2odin.base_stats.set("targets", 3f);
      phase2odin.base_stats.set("intelligence", 100);
      phase2odin.base_stats.set("warfare", 100);
      phase2odin.base_stats.set("diplomacy", 100);
      phase2odin.base_stats.set("stewardship", 100);
      phase2odin.base_stats.set("opinion", 100f);
      phase2odin.base_stats.set("mana", 10000f);
      phase2odin.base_stats.set("skill_combat", 1f);
      phase2odin.base_stats.set("skill_spell", 10f);
      phase2odin.addSpell("Dynja eitr");
      phase2odin.addSpell("All shield");
      phase2odin.addSpell("Drifa");
      phase2odin.addSpell("All fireball");
      phase2odin.linkSpells();
      phase2odin.addCombatAction("combat_backstep");
      phase2odin.addCombatAction("combat_block");
      phase2odin.addCombatAction("combat_dash");
      phase2odin.addCombatAction("combat_deflect_projectile");
      phase2odin.linkCombatActions();
      phase2odin.action_special_effect = new WorldAction(Odinwa);
      phase2odin.action_attack_target = (AttackAction)Delegate.Combine(phase2odin.action_attack_target, new AttackAction(Odinaa));
      phase2odin.action_get_hit = (GetHitAction)Delegate.Combine(phase2odin.action_get_hit, new GetHitAction(OdinPhaseTwoGetHit));

      addTraitToGame(phase2odin);
      #endregion

      #region Baldur
      ActorTrait baldur = new ActorTrait
      {
        id = "Baldur",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/baldur",
        needs_to_be_explored = false,
        can_be_given = true,
        rate_birth = 0,
        rate_inherit = 0
      };
      baldur.base_stats = new BaseStats();
      baldur.base_stats.set("lifespan", 200f);
      baldur.base_stats.set("attack_speed", 0.35f);
      baldur.base_stats.set("damage", 200);
      baldur.base_stats.set("speed", 25f);
      baldur.base_stats.set("health", 5500);
      baldur.base_stats.set("accuracy", 30f);
      baldur.base_stats.set("critical_chance", 0.4f);
      baldur.base_stats.set("knockback", 1.2f);
      baldur.base_stats.set("warfare", 50);
      baldur.base_stats.set("stewardship", 100);
      baldur.base_stats.set("opinion", 10f);
      baldur.base_stats.set("skill_combat", 0.6f);
      baldur.base_stats.set("skill_spell", 1f);
      baldur.addSpell("Baldur punch");
      baldur.addSpell("Godly uppercut");
      baldur.linkSpells();
      baldur.addCombatAction("combat_backstep");
      baldur.addCombatAction("combat_block");
      baldur.addCombatAction("combat_dash");
      baldur.addCombatAction("combat_deflect_projectile");
      baldur.addCombatAction("combat_dodge");
      baldur.linkCombatActions();
      baldur.action_death = new WorldAction(baldurda);
      baldur.action_special_effect = (WorldAction)Delegate.Combine(baldur.action_special_effect, new WorldAction(baldurwa));
      baldur.action_attack_target = (AttackAction)Delegate.Combine(baldur.action_attack_target, new AttackAction(balduraa));
      baldur.action_get_hit = (GetHitAction)Delegate.Combine(baldur.action_get_hit, new GetHitAction(baldurgha));

      addTraitToGame(baldur);
      #endregion

      #region Heimdall
      ActorTrait heimdall = new ActorTrait
      {
        id = "Heimdall",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/heimdall",
        needs_to_be_explored = false,
        can_be_given = true,
        rate_birth = 0,
        rate_inherit = 0
      };
      heimdall.base_stats = new BaseStats();
      heimdall.base_stats.set("lifespan", 200f);
      heimdall.base_stats.set("attack_speed", 6);
      heimdall.base_stats.set("damage", 140);
      heimdall.base_stats.set("speed", 30f);
      heimdall.base_stats.set("health", 5000);
      heimdall.base_stats.set("accuracy", 100f);
      heimdall.base_stats.set("targets", 2.0f);
      heimdall.base_stats.set("critical_chance", 0.8f);
      heimdall.base_stats.set("knockback", 1.2f);
      heimdall.base_stats.set("intelligence", 100);
      heimdall.base_stats.set("warfare", 40);
      heimdall.base_stats.set("stewardship", 10);
      heimdall.base_stats.set("opinion", 10f);
      heimdall.base_stats.set("skill_combat", 10f);
      heimdall.addCombatAction("combat_deflect_projectile");
      heimdall.addCombatAction("combat_dodge");
      heimdall.linkCombatActions();
      heimdall.action_death = new WorldAction(heimdallda);
      heimdall.action_special_effect = (WorldAction)Delegate.Combine(heimdall.action_special_effect, new WorldAction(heimdallra));
      heimdall.action_get_hit = (GetHitAction)Delegate.Combine(heimdall.action_get_hit, new GetHitAction(heimdallaa));

      addTraitToGame(heimdall);
      #endregion

      #region Kratos
      ActorTrait kratos = new ActorTrait
      {
        id = "Kratos",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/kratos",
        needs_to_be_explored = false,
        can_be_given = true,
        rate_birth = 0,
        rate_inherit = 0
      };
      kratos.base_stats = new BaseStats();
      kratos.base_stats.set("lifespan", 200f);
      kratos.base_stats.set("attack_speed", 0.25f);
      kratos.base_stats.set("damage", 400f);
      kratos.base_stats.set("health", 8250f);
      kratos.base_stats.set("accuracy", 30f);
      kratos.base_stats.set("armor", 30f);
      kratos.base_stats.set("scale", 0.03f);
      kratos.base_stats.set("critical_chance", 0.25f);
      kratos.base_stats.set("knockback", 1.3f);
      kratos.base_stats.set("intelligence", 90);
      kratos.base_stats.set("warfare", 500);
      kratos.base_stats.set("diplomacy", 40);
      kratos.base_stats.set("stewardship", 40);
      kratos.base_stats.set("opinion", 80f);
      kratos.base_stats.set("skill_combat", 1f);
      kratos.addSpell("Throw leviathan axe");
      kratos.addSpell("Cast Draupnir rain");
      kratos.linkSpells();
      kratos.addCombatAction("combat_backstep");
      kratos.addCombatAction("combat_block");
      kratos.addCombatAction("combat_dash");
      kratos.addCombatAction("combat_deflect_projectile");
      kratos.addCombatAction("combat_dodge");
      kratos.addCombatAction("leviathan_axe_throw");
      kratos.linkCombatActions();
      kratos.special_effect_interval = 3;
      kratos.action_special_effect = new WorldAction(kratoswa);
      kratos.action_attack_target = (AttackAction)Delegate.Combine(kratos.action_attack_target, new AttackAction(kratosaa));
      kratos.action_get_hit = (GetHitAction)Delegate.Combine(kratos.action_get_hit, new GetHitAction(kratosgha));

      addTraitToGame(kratos);
      #endregion

      #region mistletoe_curse
      ActorTrait mistletoeCurse = new ActorTrait
      {
        id = "mistletoe_curse",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/mistletoe_curse",
        type = TraitType.Negative,
        can_be_cured = true,
        needs_to_be_explored = false,
        can_be_given = true,
      };
      mistletoeCurse.action_death = new WorldAction(mistletoeda);
      mistletoeCurse.action_special_effect = (WorldAction)Delegate.Combine(mistletoeCurse.action_special_effect, new WorldAction(mistletoewa));

      addTraitToGame(mistletoeCurse);
      #endregion

      #region Jormungandr
      ActorTrait jormungandr = new ActorTrait
      {
        id = "Jormungandr",
        group_id = GroupId_GOWR2,
        path_icon = "ui/Icons/jormungandr",
        needs_to_be_explored = false,
        can_be_given = false,
        rate_birth = 0,
        rate_inherit = 0
      };
      jormungandr.base_stats = new BaseStats();
      jormungandr.base_stats.set("lifespan", 200f);
      jormungandr.base_stats.set("attack_speed", 0.1f);
      jormungandr.base_stats.set("damage", 375f);
      jormungandr.base_stats.set("speed", 20f);
      jormungandr.base_stats.set("health", 25000);
      jormungandr.base_stats.set("range", 35);
      jormungandr.base_stats.set("armor", 600);
      jormungandr.base_stats.set("scale", 0.5f);
      jormungandr.base_stats.set("targets", 10f);
      jormungandr.base_stats.set("mass", 100f);

      jormungandr.action_special_effect = new WorldAction(jormungandrwa);
      jormungandr.action_attack_target = (AttackAction)Delegate.Combine(jormungandr.action_attack_target, new AttackAction(jormungandraa));
      jormungandr.action_get_hit = (GetHitAction)Delegate.Combine(jormungandr.action_get_hit, new GetHitAction(jormungandrgha));
      jormungandr.action_death = (WorldAction)Delegate.Combine(jormungandr.action_death, new WorldAction(jormungandrda));

      addTraitToGame(jormungandr);
      #endregion

      #region Atreus
      ActorTrait atreus = new ActorTrait
      {
        id = "Atreus",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/trait_atreus",
        needs_to_be_explored = false,
        can_be_given = true,
        rate_birth = 0,
        rate_inherit = 0
      };
      atreus.base_stats = new BaseStats();
      atreus.base_stats.set("lifespan", 200f);
      atreus.base_stats.set("attack_speed", 2f);
      atreus.base_stats.set("damage", 120);
      atreus.base_stats.set("speed", 20f);
      atreus.base_stats.set("health", 2500);
      atreus.base_stats.set("accuracy", 10f);
      atreus.base_stats.set("range", 10);
      atreus.base_stats.set("scale", -0.01f);
      atreus.base_stats.set("intelligence", 120);
      atreus.base_stats.set("warfare", 10);
      atreus.base_stats.set("diplomacy", 10);
      atreus.base_stats.set("stewardship", 30);
      atreus.base_stats.set("skill_spell", 0.5f);
      atreus.addSpell("Transform to Bjorn");
      atreus.linkSpells();

      atreus.action_death = new WorldAction(atreusda);
      atreus.action_get_hit = (GetHitAction)Delegate.Combine(atreus.action_get_hit, new GetHitAction(atreusgha));

      addTraitToGame(atreus);
      #endregion

      #region Bjorn
      ActorTrait bjorn = new ActorTrait
      {
        id = "Bjorn",
        group_id = GroupId_GOWR2,
        path_icon = "ui/Icons/bjorn",
        needs_to_be_explored = false,
        can_be_given = false,
        rate_birth = 0,
        rate_inherit = 0
      };
      bjorn.base_stats = new BaseStats();
      bjorn.base_stats.set("lifespan", 200f);
      bjorn.base_stats.set("attack_speed", 2);
      bjorn.base_stats.set("damage", 275);
      bjorn.base_stats.set("speed", 20f);
      bjorn.base_stats.set("health", 5000);
      bjorn.base_stats.set("range", 2);
      bjorn.base_stats.set("armor", 20);
      bjorn.base_stats.set("scale", 0.1f);
      bjorn.base_stats.set("targets", 5f);
      bjorn.base_stats.set("critical_chance", 0.30f);
      bjorn.base_stats.set("knockback", 2f);
      bjorn.base_stats.set("skill_spell", 0.5f);
      bjorn.addSpell("Transform to Bjorn");

      bjorn.action_death = new WorldAction(bjornda);
      bjorn.action_special_effect = (WorldAction)Delegate.Combine(bjorn.action_special_effect, new WorldAction(bjornwa));
      bjorn.action_attack_target = (AttackAction)Delegate.Combine(bjorn.action_attack_target, new AttackAction(bjornaa));
      bjorn.action_get_hit = (GetHitAction)Delegate.Combine(bjorn.action_get_hit, new GetHitAction(bjorngha));

      addTraitToGame(bjorn);
      #endregion

      #region Freya
      ActorTrait freya = new ActorTrait
      {
        id = "Freya",
        group_id = GroupId_GOWR,
        path_icon = "ui/Icons/freya",
        needs_to_be_explored = false,
        can_be_given = true
      };
      freya.base_stats = new BaseStats();
      freya.base_stats.set("lifespan", 200f);
      freya.base_stats.set("damage", 275f);
      freya.base_stats.set("speed", 20f);
      freya.base_stats.set("health", 6000f);
      freya.base_stats.set("range", 4);
      freya.base_stats.set("targets", 4f);
      freya.base_stats.set("knockback", 0.4f);
      freya.base_stats.set("intelligence", 150);
      freya.base_stats.set("warfare", 10);
      freya.base_stats.set("diplomacy", 30);
      freya.base_stats.set("opinion", 50f);
      freya.base_stats.set("skill_spell", 1f);
      freya.base_stats.set("skill_combat", 0.4f);
      freya.base_stats.set("mana", 1000f);
      freya.addSpell("Cast living plants");
      freya.linkSpells();
      freya.addCombatAction("combat_backstep");
      freya.linkCombatActions();
      freya.action_special_effect = new WorldAction(freyawa);
      freya.action_attack_target = (AttackAction)Delegate.Combine(freya.action_attack_target, new AttackAction(freyaaa));
      freya.action_get_hit = (GetHitAction)Delegate.Combine(freya.action_get_hit, new GetHitAction(freyagha));

      addTraitToGame(freya);
      #endregion

      populateListOppositeTraits(GOWConfig.EnableTraitOpposites);
    }

    public static bool Odinaa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
      if (UnityEngine.Random.value > 0.10f) return false;

      pTarget.addStatusEffect("poisoned", 10f);
      pSelf.addStatusEffect("invincible", 1f);

      MapAction.damageWorld(pTarget.current_tile, 5, AssetManager.terraform.get("grenade"), null);
      EffectsLibrary.spawnAtTileRandomScale("fx_explosion_small", pTarget.current_tile, 0.1f, 0.15f);

      return false;
    }

    public static bool OdinPhaseTwo(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
      if (pTarget.current_tile == null) return false;

      Actor a = pTarget.a;
      if (!a.hasTrait("mistletoe_curse"))
      {
        var act = World.world.units.createNewUnit(a.asset.id, pTarget.current_tile, false);
        a.removeTrait("Odin");
        act.kingdom = pTarget.kingdom;
        ActorTool.copyUnitToOtherUnit(a, act);
        act.removeTrait("Odin");
        act.addTrait("Odin Phase 2");
        act.restoreHealth(6000);

        //transfer Control to phase2 unit if possessed
        Actor possessedActor = ControllableUnit._unit_main;
        if (possessedActor != null && a == possessedActor)
        {
          ControllableUnit.setControllableCreature(act);
          ActionLibrary.removeUnit(a);
        }
        else
        {
          ActionLibrary.removeUnit(a);
        }

        float randomChance = UnityEngine.Random.Range(0f, 1f);
        if (randomChance < 0.25f)
        {
          act.addStatusEffect("Frost Shield", 60f);
          act.addStatusEffect("invincible", 1f);

          MusicBox.playSound("event:/SFX/UNIQUE/SpawnCloud", pTarget.current_tile, false, false);
          Vector3 tPos = a.cur_transform_position;
          Vector2 tHeadPos = a.getRenderedItemPosition();
          tPos.y += tHeadPos.y - 8f;
          EffectsLibrary.spawnAt("ice_Explosion_Effect", tPos, 0.3f);
          for (int i = 0; i < 40; ++i)
          {
            World.world.loopWithBrush(pTarget.current_tile, Brush.get(11, "circ_"), new PowerActionWithID(PowerLibrary.drawTemperatureMinus), "temperatureMinus");
          }
          EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 3f, 0.5f);
          World.world.applyForceOnTile(pTile: pTarget.current_tile, 10, 1.5f, true, 10, null);

        }
        if (randomChance < 0.5f && randomChance > 0.25f)
        {
          act.addStatusEffect("Fire Shield", 60f);
          act.addStatusEffect("invincible", 1f);

          EffectsLibrary.spawnAtTile("fx_napalm_flash", pTarget.current_tile, 0.5f);
          EffectsLibrary.spawnAtTileRandomScale("fx_explosion_tiny", pTarget.current_tile, 0.15f, 0.3f);

          EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 3f, 0.5f);
          World.world.applyForceOnTile(pTile: pTarget.current_tile, 10, 1.5f, true, 0, null);
        }
        if (randomChance < 1.0f && randomChance > 0.5f)
        {
          act.addStatusEffect("Bifrost Shield", 60f);
          act.addStatusEffect("invincible", 1f);

          EffectsLibrary.spawnAtTile("fx_antimatter_effect", pTarget.current_tile, 0.3f);

          EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 3f, 0.5f);
          World.world.applyForceOnTile(pTile: pTarget.current_tile, 10, 1.5f, true, 0, null, pByWho: pTarget);
        }
      }
      return true;
    }

    public static bool OdinPhaseTwoGetHit(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
      if (pSelf.current_tile == null) return false;

      Actor a = pSelf.a;
      if (a.hasStatus("frozen"))
      {
        a.finishStatusEffect("frozen");
      }

      if (Randy.randomChance(0.2f))
      {
        pSelf.addStatusEffect("enchanted", 10f);
        pSelf.addStatusEffect("invincible", 1f);
      }
      float randomChance = UnityEngine.Random.Range(0f, 1f);
      if (randomChance < 0.066f)
      {
        if (a.hasStatus("Bifrost Shield") || a.hasStatus("Fire Shield") || a.hasStatus("Frost Shield"))
        {
          return false;
        }
        else
        {
          pSelf.addStatusEffect("Frost Shield", 40f);
          pSelf.addStatusEffect("invincible", 1f);

          MusicBox.playSound("event:/SFX/UNIQUE/SpawnCloud", pSelf.current_tile, false, false);
          Vector3 tPos = a.cur_transform_position;
          Vector2 tHeadPos = a.getRenderedItemPosition();
          tPos.y += tHeadPos.y - 8f;
          EffectsLibrary.spawnAt("ice_Explosion_Effect", tPos, 0.3f);
          for (int i = 0; i < 80; ++i)
          {
            World.world.loopWithBrush(pSelf.current_tile, Brush.get(11, "circ_"), new PowerActionWithID(PowerLibrary.drawTemperatureMinus), "temperatureMinus");
          }
          EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
          World.world.applyForceOnTile(pTile: pAttackedBy.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);


          MapBox.instance.drop_manager.spawn(a.current_tile, "blood_rain", 15f, -1f);
        }
      }
      if (randomChance < 0.02f && randomChance > 0.015f)
      {

        if (a.hasStatus("Bifrost Shield") || a.hasStatus("Frost Shield") || a.hasStatus("Fire Shield"))
        {
          return false;
        }
        else
        {
          pSelf.addStatusEffect("Fire Shield", 40f);
          pSelf.addStatusEffect("invincible", 1f);

          EffectsLibrary.spawnAtTile("fx_napalm_flash", pTile, 0.5f);
          EffectsLibrary.spawnAtTileRandomScale("fx_explosion_tiny", pTile, 0.15f, 0.3f);

          EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
          World.world.applyForceOnTile(pTile: pAttackedBy.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);


          MapBox.instance.drop_manager.spawn(pTile, "blood_rain", 15f, -1f);
        }
      }
      if (randomChance < 0.04f && randomChance > 0.02f)
      {

        if (a.hasStatus("Fire Shield") || a.hasStatus("Frost Shield") || a.hasStatus("Bifrost Shield"))
        {
          return false;
        }
        else
        {
          pSelf.addStatusEffect("Bifrost Shield", 40f);
          pSelf.addStatusEffect("invincible", 1f);

          EffectsLibrary.spawnAtTile("fx_antimatter_effect", pTile, 0.3f);

          EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
          World.world.applyForceOnTile(pTile: pAttackedBy.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);

          MapBox.instance.drop_manager.spawn(pTile, "blood_rain", 15f, -1f);
        }

      }
      PowerLibrary pb = new PowerLibrary();
      if (UnityEngine.Random.value < 0.2f)
      {
        if (a.hasStatus("burning"))
        {
          pb.spawnCloudRain(a.current_tile, null);
          pb.spawnCloudRain(a.current_tile, null);
          MapBox.instance.drop_manager.spawn(pTile, "blood_rain", 5f, -1f);
        }
      }
      return false;
    }

    public static bool Odinwa(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      Actor a = pTarget.a;
      if (a.hasStatus("burning"))
      {
        a.finishStatusEffect("burning");
      }
      if (pTile.Type.liquid)
      {
        a.restoreStaminaPercent(5);
      }
      if (pTile.Type.lava)
      {
        LavaHelper.putOut(pTile);
      }
      return false;
    }

    public static bool thoraa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      if (pTarget != null)
      {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.15f))
        {
          EffectsLibrary.spawnExplosionWave(a.current_tile.posV3, 0.5f, 3f);

          World.world.applyForceOnTile(a.current_tile, 3, 0.3f, true, 0, null, pTarget);

          MapBox.spawnLightningMedium(a.current_tile, 0.25f);
        }
        if (Randy.randomChance(0.01f))
        {

          ActionLibrary.castTornado(null, pTarget, null);
        }
        if (Randy.randomChance(0.1f))
        {
          MapBox.spawnLightningBig(a.current_tile, 0.03f);
        }
        if (Randy.randomChance(0.01f))
        {
          PowerLibrary pb = new PowerLibrary();
          pb.spawnCloud(a.current_tile, "ThunderCloud");
        }
      }
      return false;
    }
    public static bool thorgha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
      if (pAttackedBy == null || pAttackedBy.a == null || !pAttackedBy.a.isAlive()) return false;

      Actor a = pSelf.a;

      if (a.hasStatus("frozen") || a.hasStatus("slowness"))
      {
        a.finishStatusEffect("frozen");
        a.finishStatusEffect("slowness");
      }

      if (Randy.randomChance(0.05f))
      {
        if (pAttackedBy != null && pAttackedBy.isActor())
        {
          MapBox.spawnLightningMedium(pAttackedBy.current_tile, 0.15f);
        }
      }
      if (Randy.randomChance(0.05f))
      {
        if (a.hasStatus("Thunder god's rage"))
        {
          return false;
        }
        else
        {
          pSelf.addStatusEffect("Thunder god's rage", 30f);
          pSelf.addStatusEffect("invincible", 1f);
          MapBox.spawnLightningMedium(pSelf.current_tile, 0.45f);
        }
      }
      if (Randy.randomChance(0.4f))
      {
        if (a.hasStatus("Vines Spell"))
        {
          a.finishStatusEffect("Vines Spell");
        }
      }
      return false;
    }
    public static bool Thorda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      Actor a = pTarget.a;

      MapBox.spawnLightningMedium(pTarget.current_tile, 0.15f);

      MapBox.spawnLightningBig(pTarget.current_tile, 0.45f);

      ActionLibrary.castTornado(null, pTarget, null);

      ActionLibrary.castTornado(null, pTarget, null);

      ActionLibrary.castTornado(null, pTarget, null);

      EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 10f, 0.5f);

      return false;

    }
    public static bool thorProof(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      a.addTrait("fire_proof");
      return false;
    }

    public static bool balduraa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
      if (pTarget != null)
      {
        Actor a = pTarget.a;

        // if (Randy.randomChance(0.030f))
        // {
        //   Earthquake.startQuake(a.current_tile, EarthquakeType.SmallDisaster);
        // }
      }
      return false;

    }
    public static bool baldurgha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      Actor a = pSelf.a;

      if (a.hasStatus("Vines Spell") || a.hasStatus("burning") || a.hasStatus("slowness") || a.hasStatus("stunned"))
      {
        a.finishStatusEffect("Vines Spell");
        a.finishStatusEffect("burning");
        a.finishStatusEffect("slowness");
        a.finishStatusEffect("stunned");
      }

      if (Randy.randomChance(0.1f))
      {
        pSelf.addStatusEffect("enchanted", 60f);
      }
      return false;
    }

    public static bool baldurwa(BaseSimObject pTarget, WorldTile pTile = null) //not in use
    {
      Actor a = pTarget.a;
      a.asset.spell_ids = List.Of<string>(new string[]
      {
        "Baldur punch"
      });
      return false;
    }

    public static bool baldurda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      PowerLibrary pb = new PowerLibrary();
      if (a.hasTrait("mistletoe_curse"))
      {
        return false;
      }
      else
      {
        MapBox.instance.era_manager.setCurrentAge(AssetManager.era_library.get("age_ice"));
      }
      return false;
    }

    public static bool heimdallaa(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

      Actor a = pSelf.a;
      WorldTile selfTile = pSelf.current_tile;
      if (selfTile == null) return false;

      if (a.hasStatus("frozen") || a.hasStatus("stunned"))
      {
        a.finishStatusEffect("frozen");
        a.finishStatusEffect("stunned");
      }
      if (Randy.randomChance(0.5f))
      {
        if (a.hasStatus("Vines Spell"))
        {
          a.finishStatusEffect("Vines Spell");
        }
      }

      if (!a.hasStatus("frozen") && !a.hasStatus("Binding Spell") && !a.hasStatus("Vines Spell") && !a.hasStatus("Bifrost Armor"))
      {
        if (pAttackedBy != null && pAttackedBy.isActor() && pAttackedBy.current_tile != null)
        {
          EffectsLibrary.spawnAt("fx_miss", pSelf.current_position, pSelf.a.actor_scale);
          WorldTile dodgeSourceTile = pAttackedBy.current_tile;
          MapRegion region = dodgeSourceTile.region;
          WorldTile worldTile = ((region != null) ? region.tiles.GetRandom<WorldTile>() : null);
          if (worldTile != null)
          {
            pSelf.a.spawnOn(worldTile, 1f);
          }
          pSelf.addStatusEffect("Dodge Effect", 3f);
          pSelf.addStatusEffect("Knockback Resistance", 5f);
        }
      }
      if (Randy.randomChance(0.025f))
      {
        if (!a.hasStatus("Bifrost Armor"))
        {
          if (a.hasStatus("Vines Spell"))
          {
            a.finishStatusEffect("Vines Spell");
          }
          pSelf.addStatusEffect("Bifrost Armor", 30f);
          WorldTile effectTile = pSelf.current_tile ?? selfTile;
          EffectsLibrary.spawnExplosionWave(effectTile.posV3, 0.01f, 3.0f);
        }
      }
      if (Randy.randomChance(0.2f))
      {
        if (a.hasStatus("burning"))
        {
          WorldTile effectTile = pSelf.current_tile ?? selfTile;
          MapBox.instance.drop_manager.spawn(effectTile, "blood_rain", 5f, -1f);
        }
      }
      if (Randy.randomChance(0.1f))
      {
        if (pAttackedBy != null && pAttackedBy.isActor() && pAttackedBy.current_tile != null)
        {
          Vector2Int pos = pAttackedBy.current_tile.pos; // Position of the tile as a Vector2Int
          float pDist = Vector2.Distance(pAttackedBy.current_position, pos); // Distance between target and tile
          Vector3 newPointa = Toolbox.getNewPoint(pSelf.current_position.x + 0f, pSelf.current_position.y + 1f, (float)pos.x + 1f, (float)pos.y + 1f, pDist, true); // Projectile launch point
          Vector3 newPoint2 = Toolbox.getNewPoint(pAttackedBy.current_position.x, pAttackedBy.current_position.y, (float)pos.x, (float)pos.y, pAttackedBy.a.actor_scale, true);
          World.world.projectiles.spawn(pSelf, pAttackedBy, "plasma_ball", newPointa, newPoint2);
        }
      }
      return false;
    }

    public static bool heimdallra(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;

      a.asset.die_in_lava = false;

      if (a.hasStatus("burning"))
      {
        a.finishStatusEffect("burning");
      }
      return false;

    }
    public static bool heimdallda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      if (Randy.randomChance(1.0f))
      {
        EffectsLibrary.spawnAtTile("fx_antimatter_effect", pTile, 0.10f);
      }
      return false;

    }
    public static bool kratosaa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null)
      {
        Actor a = pSelf.a;
        if (Randy.randomChance(0.40f))
        {
          World.world.applyForceOnTile(pTile: pTarget.current_tile, pByWho: pSelf);
        }
      }
      return false;

    }
    public static bool kratosgha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

      Actor a = pSelf.a;
      if (a.hasStatus("frozen"))
      {
        a.finishStatusEffect("frozen");
      }
      if (Randy.randomChance(0.1f))
      {
        if (!a.hasStatus("Spartan Rage"))
        {
          a.restoreHealth(500);
          ActionLibrary.castFire(null, pSelf, null);
          EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
          pSelf.addStatusEffect("invincible", 1f);
          pSelf.addStatusEffect("Spartan Rage", 30f);
          if (a.hasStatus("slowness") || a.hasStatus("Binding Spell"))
          {
            a.finishStatusEffect("slowness");
            a.finishStatusEffect("Binding Spell");
          }
        }
      }

      //Switch Code Starts here
      //Switch to blades
      if (pAttackedBy != null && pAttackedBy.isActor())
      {
        float randomChance = UnityEngine.Random.Range(0f, 1f);
        if (randomChance < 0.1f)
        {
          if (a.asset.id != "Kratosblades" && !a.hasStatus("Spartan Rage"))
          {
            if (a.hasStatus("possessed")) return false;
            var act = World.world.units.createNewUnit("Kratosblades", pTile);
            if (act == null) return false;
            if (pSelf.kingdom != null && pSelf.kingdom.isAlive())
              act.kingdom = pSelf.kingdom;
            ActorTool.copyUnitToOtherUnit(a, act);
            act.data.health = pSelf.a.data.health;
            act.data.level = pSelf.a.data.level;
            ActionLibrary.removeUnit(pSelf.a);
          }
        }

        //Switch to Spear
        if (randomChance < 0.25f && randomChance > 0.1f)
        {

          if (a.asset.id != "Kratosspear" && !a.hasStatus("Spartan Rage"))
          {
            if (a.hasStatus("possessed")) return false;
            var act = World.world.units.createNewUnit("Kratosspear", pTile);
            if (act == null) return false;
            if (pSelf.kingdom != null && pSelf.kingdom.isAlive())
              act.kingdom = pSelf.kingdom;
            ActorTool.copyUnitToOtherUnit(a, act);
            act.data.health = pSelf.a.data.health;
            act.data.level = pSelf.a.data.level;
            ActionLibrary.removeUnit(pSelf.a);
          }
        }

        //Switch to Axe
        if (randomChance < 0.5f && randomChance > 0.25f)
        {
          if (a.asset.id != "Kratos" && !a.hasStatus("Spartan Rage"))
          {
            if (a.hasStatus("possessed")) return false;
            var act = World.world.units.createNewUnit("Kratos", pTile);
            if (act == null) return false;
            if (pSelf.kingdom != null && pSelf.kingdom.isAlive())
              act.kingdom = pSelf.kingdom;
            ActorTool.copyUnitToOtherUnit(a, act);
            act.data.health = pSelf.a.data.health;
            act.data.level = pSelf.a.data.level;
            ActionLibrary.removeUnit(pSelf.a);
          }
        }
      }
      return false;
    }
    public static bool kratoswa(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      a.restoreHealth(20);

      return false;

    }
    public static bool mistletoewa(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      a.asset.has_soul = false;
      a.asset.has_skin = false;
      a.asset.immune_to_injuries = true;

      return false;

    }

    //original code by MasonScarbro
    public static bool mistletoeda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      a.finishStatusEffect("cursed");
      a.removeTrait("infected");
      a.removeTrait("mush_spores");
      a.removeTrait("tumor_infection");

      if (Randy.randomChance(1.0f))
      {
        var act = World.world.units.createNewUnit(a.asset.id, pTile);
        if (a.kingdom != null && a.kingdom.isAlive())
        {
          act.kingdom = a.kingdom;
          ActorTool.copyUnitToOtherUnit(a, act);
          act.data.level = a.data.level;
          if (a.isSapient())
          {
            if (a.hasCity())
            {
              act.setCity(a.city);
            }
            if (a.hasClan())
            {
              act.clan = a.clan;
            }
            if (a.hasHouse())
            {
              act.setHomeBuilding(a._home_building);
            }
            if (a.isKing())
            {
              a.kingdom.king = act;
            }
            if (a.isCityLeader())
            {
              a.city.leader = act;
            }
            if (a.isArmyGroupLeader())
            {
              a.army._captain = act;
            }
          }
          a.data.custom_data_bool = act.data.custom_data_bool;
          a.data.custom_data_float = act.data.custom_data_float;
          a.data.custom_data_int = act.data.custom_data_int;
          a.data.custom_data_string = act.data.custom_data_string;
          a.data.custom_data_flags = act.data.custom_data_flags;
          EffectsLibrary.spawnAtTile("fx_cast_top_blue", act.current_tile, act.a.actor_scale);
          EffectsLibrary.spawnAtTile("fx_cast_ground_blue", act.current_tile, act.a.actor_scale);
          act.addStatusEffect("stunned", 5f);

          Actor possessedActor = ControllableUnit._unit_main;
          if (possessedActor != null && a == possessedActor)
          {
            ControllableUnit.setControllableCreature(act);
            ActionLibrary.removeUnit(a);
          }
          else
          {
            ActionLibrary.removeUnit(a);
          }
        }
      }
      return true;
    }

    public static bool jormungandraa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget != null)
      {
        Actor a = pTarget.a;
        WorldTile tileme = pSelf.current_tile;

        MapAction.damageWorld(pTile, 5, AssetManager.terraform.get("crab_step"), null);
        if (Randy.randomChance(1.0f))
        {
          World.world.applyForceOnTile(pTile: pTarget.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);

          ActionLibrary.addPoisonedEffectOnTarget(null, pTarget, null);
        }
        if (Randy.randomChance(0.1f))
        {

          pTarget.addStatusEffect("frozen", 2f);
        }
        if (Randy.randomChance(0.2f))
        {
          pSelf.addStatusEffect("invincible", 1f);
        }
        if (Randy.randomChance(0.1f))
        {
          Earthquake.startQuake(pTarget.a.current_tile, EarthquakeType.SmallDisaster);
        }
      }
      return false;
    }

    public static bool jormungandrgha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      Actor a = pSelf.a;
      if (a.hasStatus("Vines Spell"))
      {
        a.finishStatusEffect("Vines Spell");
      }
      if (Randy.randomChance(0.01f))
      {
        MapBox.spawnLightningMedium(pTile, 0.15f);

        EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);

        MapBox.spawnLightningMedium(pTile, 0.15f);
      }
      return false;

    }
    public static bool jormungandrda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      Actor a = pTarget.a;
      if (Randy.randomChance(0.4f))
      {
        World.world.applyForceOnTile(pTile: pTarget.current_tile, 10, 1.5f, true, 10, null, pByWho: pTarget);

        ActionLibrary.addPoisonedEffectOnTarget(null, pTarget, null);
      }
      if (Randy.randomChance(0.1f))
      {
        pTarget.addStatusEffect("frozen", 5f);
      }
      return false;
    }

    public static bool jormungandrwa(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      Actor a = pTarget.a;
      if (a.hasStatus("burning"))
      {
        a.finishStatusEffect("burning");
      }
      return false;
    }
    public static bool atreusgha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      if (pAttackedBy == null || pAttackedBy.a == null || !pAttackedBy.a.isAlive()) return false;
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

      if (Randy.randomChance(0.5f))
      {
        if (pAttackedBy != null && pAttackedBy.isActor())
        {

          if (pAttackedBy.a.hasTrait("mistletoe_curse"))
          {
            pAttackedBy.a.removeTrait("mistletoe_curse");
            pAttackedBy.a.spawnParticle(Toolbox.color_heal);
            pAttackedBy.addStatusEffect("Curse Cured", 60f);
            pAttackedBy.a.restoreHealth(10000);
          }
        }
      }
      if (Randy.randomChance(0.1f) && !pSelf.a.hasStatus("recovery_spell"))
      {
        int enemiesCount = 0;
        if (pTile == null)
        {
          return false;
        }
        foreach (Actor actorEnemy in Finder.getUnitsFromChunk(pTile, 1, 0f, false))
        {
          if (pSelf.a.isOnSameIsland(actorEnemy) && actorEnemy.kingdom.isEnemy(pSelf.a.kingdom))
          {
            enemiesCount++;
          }
        }
        if (enemiesCount > 10 || pAttackedBy.a.data.health > 999)
        {
          Actor possessedActor = ControllableUnit._unit_main;
          if (possessedActor != null && pSelf.a == possessedActor) return false;
          pSelf.a.removeTrait("Atreus");
          GOWB.Spells.morphInto(pSelf.a, "bear", "fx_fireball_explosion", "Bjorn", "Bjorn", 0.3f);
        }
      }
      return false;

    }
    public static bool atreusda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
      if (pTarget.current_tile == null || pTile == null) return false;

      Actor a = pTarget.a;
      return true;
    }

    public static bool bjornaa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      if (pTarget != null)
      {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.1f))
        {
          EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 0.01f, 3.0f);

          World.world.applyForceOnTile(pTile: pTarget.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);

          pSelf.addStatusEffect("invincible", 1f);

        }
      }
      return false;
    }

    public static bool bjorngha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

      // if (Randy.randomChance(0.5f))
      // {
      //   // pSelf.a.data.get("oldself", out string oldself, "Bjorn");
      //   // morphInto(pSelf.a, oldself, "fx_fireball_explosion");
      //   // pSelf.a.removeTrait("Bjorn");

      // }
      return false;

    }
    public static bool bjornwa(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      Actor a = pTarget.a;
      a.addTrait("fire_proof");
      a.addTrait("freeze_proof");
      if (Randy.randomChance(0.1f))
      {
        int enemiesCount = 0;
        if (pTile == null)
        {
          return false;
        }
        foreach (Actor actorEnemy in Finder.getUnitsFromChunk(pTile, 1, 0f, false))
        {
          if (a.isOnSameIsland(actorEnemy) && actorEnemy.kingdom.isEnemy(a.kingdom))
          {
            enemiesCount++;
          }
        }
        if (enemiesCount < 1 && a != null && a.asset.id == "bear")
        {
          Actor possessedActor = ControllableUnit._unit_main;
          if (possessedActor != null && a == possessedActor) return false;
          a.removeTrait("Bjorn");
          a.removeTrait("freeze_proof");
          a.data.get("oldself", out string oldself, "bear");
          GOWB.Spells.morphInto(a, oldself, "fx_fireball_explosion", "Atreus", "Atreus", 0.3f, false);
        }
      }
      return false;

    }
    public static bool bjornda(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget.current_tile == null || pTile == null) return false;
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      Actor a = pTarget.a;

      if (Randy.randomChance(1.0f) && a != null && a.asset.id == "bear")
      {
        a.removeTrait("Bjorn");
        a.removeTrait("freeze_proof");
        a.data.get("oldself", out string oldself, "bear");
        GOWB.Spells.morphInto(a, oldself, "fx_fireball_explosion", "Atreus", "Atreus", 0.3f, false, false);
      }
      return true;


    }
    public static bool freyaaa(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

      Actor a = pSelf.a;
      if (Randy.randomChance(0.1f))
      {
        pSelf.addStatusEffect("invincible", 1f);
      }
      if (Randy.randomChance(0.5f))
      {
        pSelf.addStatusEffect("Knockback Resistance", 5f);

      }

      return false;
    }

    public static bool freyagha(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
      Actor a = pSelf.a;
      return false;
    }

    public static bool freyawa(BaseSimObject pTarget, WorldTile pTile = null)
    {
      if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

      Actor a = pTarget.a;
      if (pTile.Type.liquid)
      {
        a.restoreStaminaPercent(5);
      }
      if (a.kingdom != null && a.kingdom.isAlive())
      {
        foreach (Actor actor in Finder.findSpeciesAroundTileChunk(pTile, "living_plants"))
        {
          if (actor != null || actor.a != null || actor.a.isAlive())
          {
            actor.setKingdom(a.kingdom);
          }
        }
      }
      return false;

    }

    public static int ConfiguredBirthRate(bool enabled)
    {
      string rawValue = enabled ? BirthRateEnabled : BirthRateDisabled;
      if (int.TryParse(rawValue, out int parsedRate) && parsedRate >= 0)
      {
        return parsedRate;
      }

      return 0;
    }

    public static void populateListOppositeTraits(bool enabled)
    {
      if (myListTraits == null || myListTraits.Count == 0)
      {
        return;
      }

      string[] allTraitIds = myListTraits
        .Where(t => t != null && !string.IsNullOrEmpty(t.id))
        .Select(t => t.id)
        .Distinct()
        .ToArray();

      if (allTraitIds.Length == 0)
      {
        return;
      }

      clearGroupOpposites(allTraitIds);

      if (!enabled)
      {
        return;
      }

      setGroupOpposites(allTraitIds);
    }

    private static void setGroupOpposites(string[] ids)
    {
      foreach (ActorTrait trait in myListTraits)
      {
        if (trait != null && trait.opposite_traits == null)
        {
          trait.opposite_traits = new HashSet<ActorTrait>();
        }
      }

      foreach (string id in ids)
      {
        ActorTrait current = myListTraits.FirstOrDefault(t => t.id == id);
        if (current == null || current.opposite_traits == null) continue;

        foreach (string otherId in ids)
        {
          if (id == otherId) continue;
          ActorTrait other = myListTraits.FirstOrDefault(t => t.id == otherId);
          if (other != null)
          {
            current.opposite_traits.Add(other);
          }
        }
      }
    }

    private static void clearGroupOpposites(string[] ids)
    {
      HashSet<string> idsSet = new HashSet<string>(ids);

      foreach (string id in ids)
      {
        ActorTrait current = myListTraits.FirstOrDefault(t => t.id == id);
        if (current?.opposite_traits == null || current.opposite_traits.Count == 0) continue;

        current.opposite_traits.RemoveWhere(other => other != null && idsSet.Contains(other.id));
      }
    }

    #region addTraitToGame
    private static void addTraitToGame(ActorTrait trait)
    {
      if (AssetManager.traits.dict.ContainsKey(trait.id)) return;

      GOWConfig.ApplyConfigToTrait(trait);

      AssetManager.traits.add(trait);
      myListTraits.Add(trait);
      reAddToBirthTraitPool(trait);
      // reAddToCombatTraitPool();
    }

    private static void reAddToBirthTraitPool(ActorTrait trait)
    {
      if (trait == null || trait.rate_birth <= 0) return;
      if (AssetManager.traits?.pot_traits_birth == null) return;

      if (AssetManager.traits.pot_traits_birth.Count == 0) return;

      int existingCount = AssetManager.traits.pot_traits_birth.Count(t => t == trait || t.id == trait.id);
      for (int i = existingCount; i < trait.rate_birth; i++)
      {
        AssetManager.traits.pot_traits_birth.Add(trait);
      }
    }
    #endregion

  }
}
