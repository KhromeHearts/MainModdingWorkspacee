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
  internal static class GOWCombat
  {
    public static void Init()
    {
      CombatActionAsset UltraInstinct = new CombatActionAsset();
      UltraInstinct.id = "ultra_instinct";
      UltraInstinct.cost_stamina = 0;
      UltraInstinct.chance = 1f;
      UltraInstinct.cooldown = 0f;
      UltraInstinct.action_actor = new CombatActionActor(doUltraDodge);
      UltraInstinct.pools = new CombatActionPool[]
      {
            CombatActionPool.BEFORE_HIT
      };
      Localization.addLocalization("ultra_instinct", UltraInstinct.id);
      AssetManager.combat_action_library.add(UltraInstinct);

      CombatActionAsset LeviathanAxeThrow = new CombatActionAsset();
      LeviathanAxeThrow.id = "leviathan_axe_throw";
      LeviathanAxeThrow.cost_stamina = 0;
      LeviathanAxeThrow.chance = 0.5f;
      LeviathanAxeThrow.cooldown = 4f;
      LeviathanAxeThrow.action_actor_target_position = new CombatActionActorTargetPosition(doThrowLeviathanAction);
      LeviathanAxeThrow.can_do_action = delegate (Actor pSelf, BaseSimObject pAttackTarget)
      {
        float tDist = Toolbox.SquaredDistVec2Float(pSelf.current_position, pAttackTarget.current_position);
        return tDist > 36f && tDist < 2500f;
      };
      LeviathanAxeThrow.pools = new CombatActionPool[]
      {
            CombatActionPool.BEFORE_ATTACK_MELEE,
            CombatActionPool.BEFORE_ATTACK_RANGE
      };
      AssetManager.combat_action_library.add(LeviathanAxeThrow);

    }

    public static bool doUltraDodge(Actor pActor, AttackData pData, float pTargetX = 0f, float pTargetY = 0f)
    {
      float tDodgePowerSide = 3f;
      float tDodgePowerHeight = 1.5f;
      Vector2 tStart = pActor.cur_transform_position;
      Vector2 tFrom = pData.initiator_position;
      Vector2 tAB = tStart - tFrom;
      Vector2 tTarget;
      if (Randy.randomBool())
      {
        tTarget = tStart + Toolbox.rotateVector(tAB, 90f) * tDodgePowerSide;
      }
      else
      {
        tTarget = tStart + Toolbox.rotateVector(tAB, -90f) * tDodgePowerSide;
      }
      pActor.calculateForce(tStart.x, tStart.y, tTarget.x, tTarget.y, tDodgePowerSide, tDodgePowerHeight, false);
      pActor.addStatusEffect("dodge", 0f, false);
      pActor.punchTargetAnimation(tStart, false, false, -60f);
      Vector2 tEffectPos = pActor.current_position;
      tEffectPos.y += pActor.getHeight();
      BaseEffect tEffect = EffectsLibrary.spawnAt("fx_dodge", tEffectPos, pActor.actor_scale);
      if (tEffect != null)
      {
        tEffect.transform.rotation = Toolbox.getEulerAngle(tStart, tTarget);
      }
      return true;
    }

    public static bool doThrowLeviathanAction(Actor pSelf, Vector2 pTarget, WorldTile pTile = null)
    {
      throwWeaponAtTile(pSelf, pTile, "LeviathanAxeThrow");
      pSelf.punchTargetAnimation(pTarget, true, false, 45f);
      return true;
    }

    // Weapon throw logic
    public static void throwWeaponAtTile(BaseSimObject pSelf, WorldTile pTile, string projectileID)
    {
      Vector2Int tAttackPosition = pTile.pos;
      Vector3 tSelfPosition = pSelf.current_position;
      float tDist = Vector2.Distance(tSelfPosition, tAttackPosition);
      Vector3 tAttackVector = Toolbox.getNewPoint(tSelfPosition.x, tSelfPosition.y, (float)tAttackPosition.x, (float)tAttackPosition.y, tDist, true);
      Vector3 tStartProjectile = Toolbox.getNewPoint(tSelfPosition.x, tSelfPosition.y, (float)tAttackPosition.x, (float)tAttackPosition.y, pSelf.a.stats["size"], true);
      tStartProjectile.y += 0.5f;
      World.world.projectiles.spawn(pSelf, null, projectileID, tStartProjectile, tAttackVector, 0f, 0.25f, null, null);
    }

    // Reserved for Later
    // public static bool doGodlyUppercutAction(Actor pSelf, Vector2 pSelfPosition, WorldTile pTile = null)
    // {
    //   pSelf.punchTargetAnimation(pSelfPosition, true, false, 45f);
    //   pSelf.setActionTimeout(1f);
    //   pSelf.spawnSlashKick(pSelfPosition);
    //   pSelf.lookTowardsPosition(pSelfPosition);
    //   Actor tActorToHit = pSelf.attack_target.a;
    //   if (tActorToHit == null)
    //   {
    //     return false;
    //   }
    //   // if (Vector2.Distance(pSelf.current_position, tActorToHit.current_position) <= pSelf.stats["range"])
    //   // if (pSelf.isInAttackRange(tActorToHit))
    //   // if (Vector2.Distance(pSelf.current_position, tActorToHit.current_position) < 10f)
    //   // {
    //     tActorToHit.makeStunned(5f);
    //     Vector2 tStartPos = pSelf.current_position;
    //     tStartPos.y += pSelf.current_scale.y;
    //     Vector2 tDirection = tActorToHit.current_position;
    //     float tForceDirection = Randy.randomFloat(3f, 5f);
    //     float tForceHeight = Randy.randomFloat(3f, 5f);
    //     if (pSelf.under_forces || pSelf.hasStatus("dash"))
    //     {
    //       tForceDirection *= 2f;
    //       tForceHeight *= 1.5f;
    //     }
    //     tActorToHit.calculateForce(tDirection.x, tDirection.y, tStartPos.x, tStartPos.y, tForceDirection, tForceHeight, true);
    //     tActorToHit.addAggro(pSelf);
    //   // }
    //   return true;
    // }
  }
}