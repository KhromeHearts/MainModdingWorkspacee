using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Config;
using Random=UnityEngine.Random;
using ReflectionUtility;
using System.Reflection;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using System.IO;
using NCMS;
using ai;
using NeoModLoader.api.attributes;
using JinchurikiBox.Core;
using JinchurikiBox.Content;
using JinchurikiBox.Patches;


namespace JinchurikiBox.Actions;
internal static class JinchurikiActions {

    public static int getLvl(string jutsu, Actor a) {
        return JutsuActions.getLvl(jutsu, a);
    }

    public static bool clanHybridsPossible = false;

    public static void ClanHybridBoolChange(bool pCurrentValue) {
        clanHybridsPossible = pCurrentValue;
    }

    public static bool tryRemoveChakra(Actor a, float amount) {
        return ChakraSystem.tryRemoveChakra(a, amount);
    }

    public static bool SixPathsTechnique(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        /*pTile = pTarget.current_tile;

        if (Randy.randomChance(0.4f)) {
            MapBox.spawnLightningSmall(pTile, 0.125f);
        } else if (Randy.randomChance(0.15f)) {
            MapBox.spawnLightningBig(pTile, 0.3f);
        }

        if (Randy.randomChance(0.3f)) {
            ActionLibrary.castTornado(null, pTarget, pTile);
        }

        if (Randy.randomChance(0.4f)) {
            pTarget.addStatusEffect("frozen");
        } else if (Randy.randomChance(0.6f)) {
            pTarget.addStatusEffect("burning");
        } else {
            pTarget.addStatusEffect("burning");
        }

        return true;*/
        if (pTarget.a.has_attack_target && tryRemoveChakra(a, 15)) {
            if (Randy.randomChance(0.3f)) {
                return false;
            }
            Debug.Log(pTarget.a.attack_target);
            BaseSimObject attackTarget = pTarget.a.attack_target;
            pTile = pTarget.current_tile;
            if (Randy.randomChance(0.3f) && tryRemoveChakra(a, 15)) {
                //deva path
                if (Randy.randomChance(0.4f)) {
                    if (pTarget?.a != null && a != null) {
                        Vector2 aPos = a.current_position;
                        Vector2 tPos = pTarget.current_position;
                        Vector2 dir = (tPos - aPos).normalized;
                        pTarget.a.addForce(dir.x * 8f, dir.y * 8f, 3f, true, false);
                    }
                    EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
                } else {
                    if (pTarget?.a != null && a != null) {
                        Vector2 aPos = a.current_position;
                        Vector2 tPos = pTarget.current_position;
                        Vector2 dir = (tPos - aPos).normalized;
                        pTarget.a.addForce(dir.x * 4f, dir.y * 4f, 2f, true, false);
                    }
                }
            }
            if (Randy.randomChance(0.1f)) {
                //asura path
            }
            /*if (Randy.randomChance(0.2f)) {
                //human path
                for (int i = 0; i < World.world.temp_map_objects.Count; i++)
                {
                    if (World.world.temp_map_objects[i] == pTarget.a.attack_target)
                    {
                        pTarget.a.attack_target.a.getHit(500f);
                        if (Randy.randomChance(0.3f) && a.hasTrait("strong_minded")) {
                            pTarget.a.attack_target.a.getHit(pTarget.a.attack_target.a.getMaxHealth() * 0.1f);
                        }
                        if (Randy.randomChance(0.4f)) {
                            a.stats["intelligence"] += 1;
                        } else if (Randy.randomChance(0.02f)) {
                            a.stats["intelligence"] += 3;
                        }
                    }
                }
            }*/
            if (Randy.randomChance(0.4f)) {
                //animal path
                Actor act = null;
                if (Randy.randomChance(0.33f)) {
                    act = World.world.units.createNewUnit("dragon", pTile);
                } else if (Randy.randomChance(0.5f)) {
                    act = World.world.units.createNewUnit("snake", pTile);
                } else {
                    act = World.world.units.createNewUnit("candy_bear_red", pTile);
                }
                if (act != null)
                {
                    act.data.health = a.getMaxHealth();
                    act.addTrait("largeBeast");
                    act.addStatusEffect("Rinnegan Controlled");
                    act.addStatusEffect("Rinnegan Created");
                    act.setAttackTarget(pTarget.a.attack_target);
                    act.setKingdom(a.kingdom);
                    act.data.health = a.getMaxHealth();
                }
            }
            if (Randy.randomChance(0.25f)) {
                if (Randy.randomChance(0.5f)) {
                    //StartCoroutine(removeChakra(pTarget, 3, 60f));
                }
            }
            if (Randy.randomChance(0.25f)) {
                // if (Randy.randomChance(0.25f)) {
                //     Debug.Log("Outer Path Skeleton");
                //     for (int i = 0; i < 4; i++) {
                //         var act = World.world.units.createNewUnit(SA.skeleton, pTile);
                //         act.addStatusEffect("rinnegan Controlled");
                //         act.setAttackTarget(pTarget.a.attack_target);
                //         act.setKingdom(a.kingdom);
                //     }
                // } else if (Randy.randomChance(0.25f)) {
                //     Debug.Log("Outer Path Ghost");
                //     for (int i = 0; i < 5; i++) {
                //         var act = World.world.units.createNewUnit(SA.ghost, pTile);
                //         act.addStatusEffect("rinnegan Controlled");
                //         act.setAttackTarget(pTarget.a.attack_target);
                //         act.setKingdom(a.kingdom);
                //     }
                // } else {
                //     Debug.Log("Outer Path");
                //     for (int i = 0; i < 6; i++) {
                //         var act = World.world.units.createNewUnit(SA.zombie, pTile);
                //         act.addStatusEffect("rinnegan Controlled");
                //         act.addStatusEffect("Checker");
                //         act.removeTrait("zombie");
                //         act.setAttackTarget(pTarget.a.attack_target);
                //         act.setKingdom(a.kingdom);
                //     }
                // }
            }
            return true;
        }
        return false;
    }
    public static bool ToadSageMode(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (a.has_attack_target) {
            float chakra = 0;
            a.data.get("chakra", out chakra);
            if (hasChakraPercent(a, 0.6f)) {
                if (!a.hasStatus("Toad Sage Mode")) {
                    addStatus(a, "Toad Sage Mode", 100);
                }
                return true;
            } else if (chakra <= 15) {
                if (a.hasStatus("Toad Sage Mode")) {
                    if (Randy.randomChance(1.0f - (a.data.level / 10))) {
                        var act = World.world.units.createNewUnit("frog", pTile);
                        ActorTool.copyUnitToOtherUnit(a, act);
                    } else {
                        a.finishStatusEffect("Toad Sage Mode");
                    }
                }
            } else if (a.hasStatus("Toad Sage Mode")) {
                removeChakra(a, 5);
            }
        }
        return false;
    }

    public static bool sealK2(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null){
        Actor a = pSelf.a;
        bool hasIncompletedKarmaSealTrait = a.hasTrait("incompletedKarmaSeal");

        if (hasIncompletedKarmaSealTrait) {
            addStatus(a, "Ikarma1", 425);
            return true;
        }

        return false;
    }

    public static bool sealK(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
        Actor a = pSelf.a;
        bool hasCompletedKarmaSealTrait = a.hasTrait("completedKarmaSeal");
        bool hasDNA20 = a.hasTrait("otsutsuki20");
        bool hasDNA60 = a.hasTrait("otsutsuki60");
        bool hasDNA80 = a.hasTrait("otsutsuki80");

        if (!a.hasStatus("karma1") && !(hasDNA60 || hasDNA80)) {
            addStatus(a, "karma1");
            addKarmaSealUsage(a, 1);
        }

        if (hasDNA60 || hasDNA80) {
            addStatus(a, "karma2");
            addKarmaSealUsage(a, 2);
        }

        return false;
    }

    public static bool RinneganActivation(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
        if (pSelf == null || pSelf.a == null || pSelf.a.data == null) {
            return false;
        }

        Actor actor = pSelf.a;

        if (!hasChakraPercent(actor, 0.6f)) return false;

        bool hasOtsutsukiClanTrait = actor.hasTrait("otsutsukiClan");

        if (actor.hasStatus("Active GoldRinnegan"))
            return false;

        if (actor.data.level >= 19 && Randy.randomChance(0.3f) && hasOtsutsukiClanTrait) {
            actor.finishStatusEffect("Active Rinnegan");
            actor.finishStatusEffect("StolenRinnegan");
            addStatus(actor, "Active GoldRinnegan");
            return true;
        }

        if (!actor.hasStatus("Active GoldRinnegan") && !actor.hasStatus("Active Rinnegan") && Randy.randomChance(0.5f)) {
            actor.finishStatusEffect("StolenRinnegan");
            addStatus(actor, "Active Rinnegan");
            return true;
        }

        return false;
    }
    
    public static bool Checker2 (BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;

        if (!a.hasTrait("indraChakra") && !a.hasTrait("hamuraChakra") && !a.hasTrait("asuraChakra") && !a.hasTrait("hagoromoChakra")){
            a.addTrait("sealedfate");
            return true;
        }

        if (a.hasTrait("indraChakra") || a.hasTrait("hamuraChakra") || a.hasTrait("asuraChakra") || a.hasTrait("hagoromoChakra")){
            a.removeTrait("sealedfate");
            return true;
        }
        return false;
    }

    public static bool ShinobiRankControl(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;

        if (a.hasTrait("kage") && 
            (a.data.profession == UnitProfession.Leader ||
                a.data.profession == UnitProfession.Warrior ||
                a.data.profession == UnitProfession.Unit
                )) {
            a.removeTrait("kage");
            a.addTrait("formerKage");
            return true;
        }

        // Other rank control logic remains unchanged
        if (a.data.profession == UnitProfession.King) {
            a.removeTrait("academyStudent");
            a.removeTrait("genin");
            a.removeTrait("chunin");
            a.removeTrait("jonin");
            a.removeTrait("formerKage");
            a.addTrait("kage");
            return true;
        }
        if (a.data.level >= 7 && a.hasTrait("chunin")) {
            a.removeTrait("chunin");
            a.addTrait("jonin");
            return true;
        }
        if (a.data.level >= 5 && a.hasTrait("genin")) {
            a.removeTrait("genin");
            a.addTrait("chunin");
            return true;
        }
        if (a.data.level >= 3 && a.hasTrait("academyStudent")) {
            a.removeTrait("academyStudent");
            a.addTrait("genin");
            return true;
        }

        return false;
    }

    public static bool ChakraRegeneration(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        a.data.get("_chakra_last_regen", out float lastRegen);
        if (Time.time - lastRegen < 1.0f) return false;

        float amount = 0f;
        string[] chakraRegenIds = { "immeasurableReserves", "chakraC5", "chakraC4", "chakraC3", "chakraC2", "chakraC1", "chakraC" };
        foreach (var id in chakraRegenIds)
        {
            if (a.hasTrait(id))
            {
                ActorTrait t = AssetManager.traits.get(id);
                if (t != null) amount += t.base_stats["chakra_regen"];
            }
        }
        string[] rankRegenIds = { "academyStudent", "genin", "chunin", "jonin", "prodigy", "SLegend", "kage", "formerKage" };
        foreach (var id in rankRegenIds)
        {
            if (a.hasTrait(id))
            {
                ActorTrait t = AssetManager.traits.get(id);
                if (t != null) amount += t.base_stats["chakra_regen"];
            }
        }
        if (a.data.health <= a.getMaxHealth() * 0.45f) amount *= 0.5f;
        addChakra(a, amount);
        a.data.set("_chakra_last_regen", Time.time);

        float tbMax = a.stats["tailed_beast_chakra"];
        if (tbMax > 0f)
        {
            a.data.get("tailed_beast_chakra", out float tbCur);
            float newTB = Mathf.Min(tbCur + 100f, tbMax);
            a.data.set("tailed_beast_chakra", newTB);
        }

        float sageMax = a.stats["sage_chakra"];
        if (sageMax > 0f)
        {
            float sageRegen = 0f;
            string[] sageTraitIds = { "senjuSage", "snakeSage", "100heals", "toadSage" };
            foreach (var id in sageTraitIds)
            {
                if (a.hasTrait(id))
                {
                    ActorTrait t = AssetManager.traits.get(id);
                    if (t != null) sageRegen += t.base_stats["sage_chakra_regen"];
                }
            }
            string[] sageStatusIds = { "SenjuS", "SnakeSage", "OneHundredSeal", "Toad Sage Mode", "Kyuubi Sage", "Six Path Sage" };
            foreach (var id in sageStatusIds)
            {
                if (a.hasStatus(id))
                {
                    StatusAsset s = AssetManager.status.get(id);
                    if (s != null) sageRegen += s.base_stats["sage_chakra_regen"];
                }
            }
            if (sageRegen <= 0f) sageRegen = 6f;
            a.data.get("sage_chakra", out float sageCur);
            float newSage = Mathf.Min(sageCur + sageRegen, sageMax);
            a.data.set("sage_chakra", newSage);
        }
        return false;
    }

    public static bool SnakeActivation(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf != null && pSelf.isActor())
            {
                Actor a = pSelf.a;

                if (!hasChakraPercent(a, 0.6f)) return false;

                if (Randy.randomChance(0.5f))
                {
                    addStatus(a, "SnakeSage", 350);
                    return true;
                }
            }

            return false;
        }

    public static bool SenjuActivation(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
        if (pSelf != null && pSelf.isActor())
        {
            Actor a = pSelf.a;

            if (!hasChakraPercent(a, 0.6f)) return false;

            if (Randy.randomChance(0.5f))
            {
                addStatus(a, "SenjuS", 500);
                return true;
            }
        }

        return false;
    }

    public static bool SlugActivation(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
        if (pSelf != null && pSelf.isActor())
        {
            Actor a = pSelf.a;

            if (!hasChakraPercent(a, 0.6f)) return false;

            if (Randy.randomChance(0.5f))
            {
                addStatus(a, "OneHundredSeal", 500);
                return true;
            }
        }

        return false;
    }

    public static bool HashCells(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(1f)) {
         a.restoreHealth((int)(a.getMaxHealth() * 0.05f));
            if (Randy.randomChance(1f)) {
                a.removeTrait("crippled");
                a.removeTrait("eyepatch");
                a.removeTrait("skin_burns");
            }
            return true;
        } else {
            return false;
        }
    }

    public static bool OnehunHealings(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.8f)) {
            a.restoreHealth(10 + a.data.level * 2);
            if (Randy.randomChance(0.6f)) {
                a.removeTrait("crippled");
                a.removeTrait("eyepatch");
                a.removeTrait("skin_burns");
            }
            return true;
        } else {
            return false;
        }
    }

public static bool RotationTechnique(BaseSimObject pTarget, WorldTile pTile = null){
    Actor caster = pTarget?.a;
    if (caster == null || !caster.isAlive()) return false;

    WorldTile centerTile = pTile ?? caster.current_tile;
    if (centerTile == null) return false;

    caster.makeWait(0.6f);
    caster.current_position = centerTile.posV3;
    EffectsLibrary.spawnExplosionWave(centerTile.posV3, 3f, 0.5f);

    float casterChakra = 0;
    caster.data.get("chakra", out casterChakra);

    List<Actor> allUnits = World.world.units.getSimpleList();
    foreach (Actor unit in allUnits)
    {
        if (unit == caster || !unit.isAlive()) continue;

        float dist = Vector3.Distance(centerTile.posV3, unit.current_tile.posV3);
        if (dist > 4f) continue;

        float targetChakra = 0;
        unit.data.get("chakra", out targetChakra);

        if (targetChakra < casterChakra)
        {
            if (unit != null && centerTile != null) {
                Vector2 uPos = unit.current_position;
                Vector2 dir = new Vector2(uPos.x - centerTile.posV3.x, uPos.y - centerTile.posV3.y).normalized;
                unit.addForce(dir.x * 4f, dir.y * 4f, 2f, true, false);
            }
            unit.makeWait(1f);
        }
    }

    return true;
}

public static bool RotationDeflect(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
{
    if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
    if (pAttackedBy == null || pAttackedBy.a == null) return false;

    float dist = Vector3.Distance(pSelf.a.current_tile.posV3, pAttackedBy.a.current_tile.posV3);
    if (dist <= 2f) return false;

    float defenderChakra = pSelf.a.data["chakra"];
    float attackerChakra = pAttackedBy.a.data["chakra"];
    if (attackerChakra >= defenderChakra) return false;

    pSelf.a.data.get("_j_health_snapshot", out float restore);
    if (restore > 0f)
    {
        float current = pSelf.a.getHealth();
        if (current < restore)
            pSelf.a.restoreHealth((int)(restore - current));
    }

    string projId = "arrow";
    try
    {
        var weaponSlot = pAttackedBy.a.equipment?.weapon;
        if (weaponSlot != null)
        {
            var itemField = weaponSlot.GetType().GetField("item", BindingFlags.Public | BindingFlags.Instance);
            if (itemField != null)
            {
                var eq = itemField.GetValue(weaponSlot);
                if (eq != null)
                {
                    var projField = eq.GetType().GetField("projectile", BindingFlags.Public | BindingFlags.Instance);
                    if (projField != null)
                    {
                        string pid = projField.GetValue(eq) as string;
                        if (!string.IsNullOrEmpty(pid))
                            projId = pid;
                    }
                }
            }
        }
    }
    catch { }
    World.world.projectiles.spawn(pSelf, pAttackedBy, projId, pSelf.a.current_tile.posV3, pAttackedBy.a.current_tile.posV3);
    pAttackedBy.a.getHit(15, true, AttackType.Other, pSelf.a, false, false, true);
    EffectsLibrary.spawnExplosionWave(pSelf.a.current_tile.posV3, 2f, 0.5f);
    return true;
}

public static bool FocusPointDeflect(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
{
    if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
    if (pAttackedBy == null || pAttackedBy.a == null) return false;
    Actor a = pSelf.a;
    Actor attacker = pAttackedBy.a;
    float defenderChakra = a.data["chakra"];
    float attackerChakra = attacker.data["chakra"];
    if (attackerChakra >= defenderChakra) return false;
    if (!Randy.randomChance(0.4f)) return false;
    a.data.get("_j_fp_health_snapshot", out float restore);
    if (restore > 0f)
    {
        float current = a.getHealth();
        if (current < restore)
            a.restoreHealth((int)(restore - current));
    }
    if (Randy.randomChance(0.5f))
    {
        Vector3 dir = (a.current_tile.posV3 - attacker.current_tile.posV3).normalized;
        Vector2 pushPos = new Vector2(a.current_position.x + dir.x * 3f, a.current_position.y + dir.y * 3f);
        a.current_position = pushPos;
    }
    else
    {
        string projId = "arrow";
        try
        {
            var weaponSlot = attacker.equipment?.weapon;
            if (weaponSlot != null)
            {
                var itemField = weaponSlot.GetType().GetField("item", BindingFlags.Public | BindingFlags.Instance);
                if (itemField != null)
                {
                    var eq = itemField.GetValue(weaponSlot);
                    if (eq != null)
                    {
                        var projField = eq.GetType().GetField("projectile", BindingFlags.Public | BindingFlags.Instance);
                        if (projField != null)
                        {
                            string pid = projField.GetValue(eq) as string;
                            if (!string.IsNullOrEmpty(pid))
                                projId = pid;
                        }
                    }
                }
            }
        }
        catch { }
        World.world.projectiles.spawn(pSelf, pAttackedBy, projId, a.current_tile.posV3, attacker.current_tile.posV3);
    }
    return true;
}

public static bool Morestuff1(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)

{

    bool actionTaken = false;

    if (Randy.randomChance(0.45f))

    {

        if (pTarget?.a != null) tryRemoveChakra(pTarget.a, 3);

        actionTaken = true; 

    }

    if (Randy.randomChance(0.10f))

    {

        pTarget.a.addStatusEffect("Pain1");

        actionTaken = true; 

    }

    return actionTaken;

}

public static bool BHit(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor a = pSelf.a;
        bool hasAnyByakugan = a.hasStatus("ByakuG") || a.hasStatus("ByakuP") || a.hasStatus("StolenByakup");
        if (!hasAnyByakugan && a._active_status_dict != null) {
            foreach (string sid in a._active_status_dict.Keys) {
                if (sid.StartsWith("StolenByakup_")) { hasAnyByakugan = true; break; }
            }
        }

        if (hasAnyByakugan && !a.hasStatus("Palmtech1"))
        {
         if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 10))
            {
                a.addStatusEffect("Palmtech1");
                return true;  
            }             
        }

        if (hasAnyByakugan && !a.hasStatus("Palmtech2"))
        {
         if (Randy.randomChance(0.2f) && tryRemoveChakra(a, 20))
            {
                a.addStatusEffect("Palmtech2");
                return true;  
            }             
        }
        return false;        
    }

    public static bool ByakuganMethod(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;
    
        bool hasTarget = pTarget.a.has_attack_target;
        bool hasIndrasChakraTrait = a.hasTrait("hamuraChakra");
        bool hasOtsutsukiClanTrait = a.hasTrait("otsutsukiClan");

        if (!hasChakraPercent(a, 0.6f)) return false;

        if (Randy.randomChance(100f) && hasTarget)
        {

            if (hasOtsutsukiClanTrait && a.data.level >= 10 && !a.hasStatus("ByakuG"))
            {
                a.finishStatusEffect("Byaku3");
                a.finishStatusEffect("ByakuP");
                addStatus(a, "ByakuG");
                return true;
            }
            else if (!a.hasStatus("ByakuG") && hasIndrasChakraTrait && !a.hasStatus("ByakuP"))
            {
                a.finishStatusEffect("Byaku3");
                addStatus(a, "ByakuP");
                return true;
            }
            else if (!a.hasStatus("ByakuG") && !a.hasStatus("ByakuP") && !a.hasStatus("Byaku3"))
            {
                addStatus(a, "Byaku3");
                return true;
            }
        }

        return false;
    }

    //public static bool Hyugastuff (BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
      //    Actor a = pSelf.a;
        
  //            float chakra = 0;
       //       a.data.get("chakra", out chakra);
  //
      //    if (a.data.level >= 19 && chakra >=100 && a.hasTrait("otsutsukiClan")) {

      //        if (a.hasStatus("StolenByakup")) {
     //             JinchurikiActions.addStatus(a, "Tenseigan", 450);
       //           return true;
      //        }
      //    }

   //     return false;
   // }

    public static bool OneTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (!ChakraSystem.chakraDuel(a, 40)) {
                switch(a.data.level) {
                    case 2: case 3: case 4:
                        addStatus(a, "Innate Jinchuriki", 25);
                        break;
                    case 5: case 6: case 7: case 8: case 9: case 10:
                        addStatus(a, "One Tailed", 30);
                        break;
                    default:
                        break;
                }
            }
        }
        return false;
    }

    public static bool TwoTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 2: case 3: case 4:
                        addStatus(a, "One Tailed", 25);
                        break;
                    case 5: case 6: case 7: case 8: case 9: case 10:
                        addStatus(a, "Two Tailed", 30);
                        break;
                    default:
                        break;
                }
            }
        }
        return false;
    }

    public static bool ThreeTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 2:
                        addStatus(a, "One Tailed", 25);
                        break;
                    case 3: case 4: 
                        addStatus(a, "Two Tailed", 30);
                        break;
                    case 5: case 6: case 7: case 8: case 9: case 10:
                        addStatus(a, "Three Tailed", 40);
                        break;
                    default:
                        break;
                }
            }
        }
        return false;
    }
    public static bool FourTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 2:
                        addStatus(a, "One Tailed", 25);
                        break;
                    case 3:
                        addStatus(a, "Two Tailed", 30);
                        break;
                    case 4:
                        addStatus(a, "Three Tailed", 40);
                        break;
                    case 5: case 6: case 7: case 8: case 9: case 10:
                        addStatus(a, "Four Tailed", 50);
                        break;
                    default:
                        break;
                }
            }
        }
        return false;
    }

    public static bool FiveTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 2: case 3:
                        addStatus(a, "One Tailed", 25);
                        break;
                    case 4: case 5:
                        addStatus(a, "Two Tailed", 30);
                        break;
                    case 6:
                        addStatus(a, "Three Tailed", 40);
                        break;
                    case 7:
                        addStatus(a, "Four Tailed", 50);
                        break;
                    case 8: case 9: case 10:
                        addStatus(a, "Five Tailed", 60);
                        break;
                    default:
                        break;
                }
            }
        }
        return false;
    }

    public static bool SixTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 2:
                        addStatus(a, "One Tailed", 25);
                        break;
                    case 3:
                        addStatus(a, "Two Tailed", 30);
                        break;
                    case 4:
                        addStatus(a, "Three Tailed", 40);
                        break;
                    case 5: case 6:
                        addStatus(a, "Four Tailed", 50);
                        break;
                    case 7:
                        addStatus(a, "Five Tailed", 60);
                        break;
                    case 8:
                        addStatus(a, "Six Tailed", 60);
                        break;
                    default:
                        break;
                }
            }
        }
        return false;
    }

    public static bool SevenTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 25)) {
                switch(a.data.level) {
                    case 2: case 3: case 4:
                        addStatus(a, "One Tailed", 25);
                        break;
                    case 5:
                        addStatus(a, "Two Tailed", 40);
                        break;
                    case 6: case 7: case 8:
                        addStatus(a, "Three Tailed", 50);
                        break;
                    default:
                        break;
                }
            }

            if (a.data.health <= a.getMaxHealth() / 2 && a.has_attack_target) {
                if (a.hasStatus("Three Tailed") && ChakraSystem.chakraDuel(a, 25)) {
                    a.finishStatusEffect("Three Tailed");
                    addStatus(a, "Four Tailed");
                } else if (a.hasStatus("Four Tailed") && ChakraSystem.chakraDuel(a, 30)) {
                    a.finishStatusEffect("Four Tailed");
                    addStatus(a, "Five Tailed");
                } else if (a.hasStatus("Five Tailed") && ChakraSystem.chakraDuel(a, 35)) {
                    a.finishStatusEffect("Five Tailed");
                    addStatus(a, "Six Tailed");
                } else if (a.hasStatus("Six Tailed") && ChakraSystem.chakraDuel(a, 50)) {
                    a.finishStatusEffect("Six Tailed");
                    addStatus(a, "Seven Tailed");
                }
            }
        }
        return false;
    }

    public static bool EightTailedBeast(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.2f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 1.5f) {
            if (ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 3: case 4: case 5:
                        addStatus(a, "One Tailed", 30);
                        break;
                    case 6:
                        addStatus(a, "Two Tailed", 40);
                        break;
                    case 7: case 8: case 9:
                        addStatus(a, "Three Tailed", 50);
                        break;
                    default:
                        break;
                }
            }

            if (a.data.health <= a.getMaxHealth() / 3) {
                if (a.hasStatus("Three Tailed") && ChakraSystem.chakraDuel(a, 35)) {
                    a.finishStatusEffect("Three Tailed");
                    addStatus(a, "Four Tailed");
                } else if (a.hasStatus("Four Tailed") && ChakraSystem.chakraDuel(a, 40)) {
                    a.finishStatusEffect("Four Tailed");
                    addStatus(a, "Five Tailed");
                } else if (a.hasStatus("Five Tailed") && ChakraSystem.chakraDuel(a, 45)) {
                    a.finishStatusEffect("Five Tailed");
                    addStatus(a, "Six Tailed");
                } else if (a.hasStatus("Six Tailed") && ChakraSystem.chakraDuel(a, 60)) {
                    a.finishStatusEffect("Six Tailed");
                    addStatus(a, "Seven Tailed");
                } else if (a.hasStatus("Seven Tailed") && ChakraSystem.chakraDuel(a, 75)) {
                    a.finishStatusEffect("Seven Tailed");
                    addStatus(a, "Eight Tailed");
                }
            }
        }
        return false;
    }

    public static bool NineTailedBeast(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;
        if (Randy.randomChance(1f) && a.has_attack_target && a.data.health <= a.getMaxHealth() / 2) {
            if (a.hasTrait("nineTailedPartner")) {
                return false;
            }
            if (!ChakraSystem.chakraDuel(a, 30)) {
                switch(a.data.level) {
                    case 4:
                        addStatus(a, "Innate Jinchuriki", 20);
                        break;
                    case 5:
                        addStatus(a, "One Tailed", 30);
                        break;
                    case 6:
                        addStatus(a, "Two Tailed", 40);
                        break;
                    case 7: case 8: case 9:
                        addStatus(a, "Three Tailed", 50);
                        break;
                    default:
                        break;
                }
            }

            if (a.data.health <= a.getMaxHealth() / 3) {
                if (a.hasStatus("Three Tailed")) {
                    a.finishStatusEffect("Three Tailed");
                    addStatus(a, "Four Tailed");
                    if (!ChakraSystem.chakraDuel(a, 50)) {
                        a.addTrait("madness");
                    }
                } else if (a.hasStatus("Four Tailed")) {
                    a.finishStatusEffect("Four Tailed");
                    addStatus(a, "Five Tailed");
                    if (!ChakraSystem.chakraDuel(a, 60)) {
                        a.addTrait("madness");
                    }
                } else if (a.hasStatus("Five Tailed")) {
                    a.finishStatusEffect("Five Tailed");
                    addStatus(a, "Six Tailed");
                    if (!ChakraSystem.chakraDuel(a, 90)) {
                        a.addTrait("madness");
                    }
                } else if (a.hasStatus("Six Tailed")) {
                    a.finishStatusEffect("Six Tailed");
                    addStatus(a, "Seven Tailed");
                    if (!ChakraSystem.chakraDuel(a, 120)) {
                        a.addTrait("madness");
                    }
                } else if (a.hasStatus("Seven Tailed")) {
                    a.finishStatusEffect("Seven Tailed");
                    addStatus(a, "Eight Tailed");
                    if (!ChakraSystem.chakraDuel(a, 150)) {
                        a.addTrait("madness");
                    }
                } else if (a.hasStatus("Eight Tailed")) {
                    a.finishStatusEffect("Eight Tailed");
                    addStatus(a, "Nine Tailed");
                    if (!ChakraSystem.chakraDuel(a, 180)) {
                        a.addTrait("madness");
                    }
                }
            } else if (a.hasTrait("madness")) {
                a.removeTrait("madness");
            }

            if (a.data.level >= 11 && ChakraSystem.chakraDuel(a, 80)) {
                a.addTrait("nineTailedPartner");
                return true;
            }
        }
        return false;
        
    }

    public static bool RTB(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
        Actor a = pSelf.a;
        if (Randy.randomChance(0.1f)) {
            if (a.data.kills >= 3 && a.data.kills <= 10) {
                addStatus(a, "One Tailed", 15);
            } else if (a.data.kills <= 20) {
                addStatus(a, "Two Tailed", 30);
            } else if (a.data.kills <= 30) {
                addStatus(a, "Three Tailed", 50);
            } else if (a.data.kills <= 45) {
                addStatus(a, "Four Tailed", 60);
            } else if (a.data.kills <= 60) {
                addStatus(a, "Five Tailed", 70);
            } else if (a.data.kills <= 75) {
                addStatus(a, "Six Tailed", 90);
            } else if (a.data.kills <= 90) {
                addStatus(a, "Seven Tailed", 100);
            } else if (a.data.kills <= 125) {
                addStatus(a, "Eight Tailed", 100);
            } else {
                addStatus(a, "Nine Tailed", 100);
            }
            return true;
        }
        return false;
    }

    public static bool WoodStyle_Damage(BaseSimObject pTarget, WorldTile pTile = null)
    {
            Actor a = pTarget.a;
        if (Randy.randomChance(0.5f)) {
            a.getHit(a.getMaxHealth() * 0.10f, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
            return true;
        }
        else {
            return false;
        }
    }

    public static bool Amaterasu(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;
        a.getHit(a.getMaxHealth() * 0.01f * (3f + getLvl("amaterasu", a)), pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
        if (Randy.randomChance(0.5f * getLvl("amaterasu", a)))
        {
            a.addTrait("skin_burns");
        }
        return true;
    }

    public static bool DontMove(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
    {
        Actor a = pSelf.a;
        if (a != null)
        {
            a.makeWait(5.00f);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ChainsUzumaki(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor a = pSelf.a;

        if ((!a.hasTrait("yangRelease") || !a.hasTrait("yinYangRelease")) && !a.hasTrait("uzumakiClan") && !a.hasTrait("adamantineSealingChains"))
        {
            return false;
        }

        float chakra = 0;
        a.data.get("chakra", out chakra);

        if (Randy.randomChance(100f) && tryRemoveChakra(a, 35) && (a.hasTrait("yangRelease") || a.hasTrait("yinYangRelease")) && a.hasTrait("adamantineSealingChains") && !a.hasStatus("ChainedAttacker"))
        {
            a.addStatusEffect("ChainedAttacker");
            return true;
        }

        return false;
        
    }

    public static bool WoodStyle(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        
        Actor a = pSelf.a;

        if (!a.hasTrait("waterRelease") && !a.hasTrait("earthRelease"))
        {
            return false;
        }

        float chakra = 0;
        a.data.get("chakra", out chakra);

        if (Randy.randomChance(0.25f) && tryRemoveChakra(a, 35))
        {
            pTarget.a.addStatusEffect("WoodSheild");
            pTarget.a.makeWait(10f);
            a.getHit(a.getMaxHealth() * 0.05f, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
            return true;
        }

        return false;
        
    }

    public static bool ChainsAttack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor a = pSelf.a;
        float chakra = 0;
        a.data.get("chakra", out chakra);
        
        if (a.hasStatus("ChainedAttacker"))
        {
            if (Randy.randomChance(100f) && tryRemoveChakra(a, 35) && !pTarget.hasStatus("ChainedGuy"))
            {
                pTarget.a.addStatusEffect("ChainedGuy");
                pTarget.a.makeWait(25f);
                return true;
            }
        }
        return false;
    }

    public static bool SharinganGen(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor a = pSelf.a;
        float chakra = 0;
        a.data.get("chakra", out chakra);
        
        bool hasCMSlike = false;
        if (a._active_status_dict != null) {
            foreach (string sid in a._active_status_dict.Keys) {
                if (sid.StartsWith("CMS_") || sid.StartsWith("CMSImplant_")) { hasCMSlike = true; break; }
            }
        }
        if (a.hasStatus("MSharingan") || a.hasStatus("Sharingan3") || a.hasStatus("3 Tomoe-S eye") || a.hasStatus("Uchiha's MS eye") || a.hasStatus("ItachiMS") || a.hasStatus("Itachi's eye") || a.hasStatus("IndraMS") || a.hasStatus("Indra's eye") || a.hasStatus("madaraMS") || a.hasStatus("Madara's eye") || hasCMSlike)
        {
            if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 25) && !pTarget.hasStatus("sharinganGenjutsu"))
            {
                JutsuLibrary.SharinganGenjutsu.Cast(pSelf, pTarget);
                return true;
            }
        }
        return false;
    }

    public static bool LightningActiv (BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor a = pSelf.a;
            
        if (Randy.randomChance(0.2f) && tryRemoveChakra(a, 30) && a.hasTrait("raiton") && !a.hasStatus("LightingCl"))
        {
            a.addStatusEffect("LightingCl");
            a.getHit(a.getMaxHealth() * 0.05f, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
            return true;
        } 
        return false;
    }

    public static bool MSHit(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor a = pSelf.a;

        if (a.hasStatus("ItachiMS") || a.hasStatus("Itachi's eye") )
        
        {
            
        if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 10) && !pTarget.hasStatus("sharinganGenjutsu"))
            {
                JutsuLibrary.SharinganGenjutsu.Cast(pSelf, pTarget);
                return true;
            } 
            
            else if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 20) && !pTarget.hasStatus("amaterasu"))
            {
                JutsuLibrary.Amaterasu.Cast(pSelf,  pTarget);
                addSharinganUsage(a, 0.5f);
                return true;
            }

        }  

        if (a.hasStatus("IndraMS") || a.hasStatus("Indra's eye")){
            
         if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 20) && !a.hasStatus("madarasusanoo1"))
            {
                JutsuLibrary.SusanooRib.Cast(pSelf, pSelf, pTile);
                a.getHit(a.getMaxHealth() * 0.05f, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
                addSharinganUsage(a, 1.5f);
                return true;  
            } 
            
            else if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 30))
            {
                JutsuLibrary.SharinganGenjutsu.Cast(pSelf, pTarget);
                return true;
            } 

        }   

        if (a.hasStatus("madaraMS") || a.hasStatus("Madara's eye"))
        {

            if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 55))
            {
                JutsuLibrary.SusanooRib.Cast(pSelf, pSelf, pTile);
                {
                    EffectsLibrary.spawn("fx_meteorite", pTarget.current_tile, "meteorite_disaster", null, 0f, -1f, -5f);
                }
            }
            else if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 20) && !a.hasStatus("madarasusanoo1"))
            {
                JutsuLibrary.SusanooRib.Cast(pSelf, pSelf, pTile);
                a.getHit(a.getMaxHealth() * 0.05f, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
                addSharinganUsage(a, 1.5f);
                return true;  
            }
            else if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 25))
            {
                JutsuLibrary.SharinganGenjutsu.Cast(pSelf, pTarget);
                return true;
            }

        }        
        a.data.get("hasStolenCMS", out float hasSCMS);
        bool hasCMSImplant = false;
        if (a._active_status_dict != null) {
            foreach (string sid in a._active_status_dict.Keys) {
                if (sid.StartsWith("CMSImplant_")) { hasCMSImplant = true; break; }
            }
        }
        if (hasSCMS >= 0.5f || a.hasStatus("CMS_" + a.data.id) || hasCMSImplant)
        {
            if (Randy.randomChance(0.4f) && tryRemoveChakra(a, 25))
            {
                JutsuLibrary.SharinganGenjutsu.Cast(pSelf, pTarget);
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }


    public static bool SharinganMethod(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;

        if (a.hasTrait("sharinganBlindness"))
        {
            return false;
        }

        if (!hasChakraPercent(a, 0.6f)) return false;
    
        bool hasTarget = pTarget.a.has_attack_target;
        int kills = a.data.kills;

        if (Randy.randomChance(100f) && hasTarget)
        {

            if(a.hasTrait("itachiReincarnate") && kills >= 30)
            {
                a.addStatusEffect("ItachiMS");
                a.finishStatusEffect("Sharingan3");
                addSharinganUsage(a, 1);

            }

            if(a.hasTrait("indrasReincarnate") && kills >= 30)
            {
                a.addStatusEffect("IndraMS");
                a.finishStatusEffect("Sharingan3");
                addSharinganUsage(a, 1);

            }

            if(a.hasTrait("madarasReincarnate") && kills >= 30)
            {
                a.addStatusEffect("madaraMS");
                a.finishStatusEffect("Sharingan3");
                addSharinganUsage(a, 1);

            }

            if (kills >= 30)
            {
                if (a.hasTrait("madarasReincarnate") || a.hasTrait("indrasReincarnate") || a.hasTrait("itachiReincarnate"))
                {
                    return false;
                }
                CustomSharinganActions.applyCMS(a);
                a.finishStatusEffect("Sharingan3");


            } else if (kills >= 21 && kills < 29) {
                if (!a.hasStatus("Sharingan3"))
                {
                    a.addStatusEffect("Sharingan3");
                }
                a.addStatusEffect("Sharingan3");
                if (a.hasStatus("Sharingan2"))
                {
                    a.finishStatusEffect("Sharingan2");
                }
                if (a.hasStatus("Sharingan"))
                {
                    a.finishStatusEffect("Sharingan");
                }
            } else if (kills >= 10 && kills < 20) {
                if (!a.hasStatus("Sharingan2"))
                {
                    a.addStatusEffect("Sharingan2");
                }
                a.addStatusEffect("Sharingan2");
                if (a.hasStatus("Sharingan"))
                {
                    a.finishStatusEffect("Sharingan");
                }
            } else if (kills < 10) {
                a.addStatusEffect("Sharingan");
            }


            return true;
        }

        return false;
    }
    
    // public static bool vampireEffect(BaseSimObject pTarget, WorldTile pTile = null)
    // {
    //     if(pTarget == null)
    //     {
    //         return false;
    //     }
    //     Actor pActor = (Actor)pTarget;
    //     pActor.spawnParticle(Toolbox.color_red);
    //     if(World.world.eraManager.current_era != AssetManager.era_library.age_dark && World.world.eraManager.current_era != S.age_moon)
    //     {
    //         pTarget.addStatusEffect("burning");
    //     }
    //     return true;
    // }

    public static bool Checker1(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;

        bool infected = a.hasTrait("zombie");

        if (infected) {
            a.removeTrait("zombie");
            return true;
        }
        return false;
    }

    public static bool setOnlyFightUnit(BaseSimObject pTarget, WorldTile pTile = null) {

        if (pTarget.a.hasStatus("Rinnegan Created")) {
            pTarget.a.removeTrait("zombie");
        }

        pTarget.a.getHit(10000000, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
        pTarget.a.addTrait("death_mark");
        return true;
    }

    public static bool RasenganAttack(BaseSimObject pTarget, WorldTile pTile = null) {
        if (!pTarget.a.has_attack_target) {
            return false;
        }
        return true;
    }

    public static bool Kirin(BaseSimObject pTarget, WorldTile pTile = null)
    {
        Actor a = pTarget.a;
        if (Randy.randomChance(0.25f) && a.has_attack_target) {
            JutsuLibrary.Kirin.Cast(pTarget, pTarget.a.attack_target, pTile);
            return true;
        }
        return false;
    }

    public static bool ApplyLightningJutsus(BaseSimObject pTarget, WorldTile pTile = null) {
        return false;
    }

    public static bool Rasengan(BaseSimObject pTarget, WorldTile pTile = null) {
        if (Randy.randomChance(0.3f) && pTarget.a.has_attack_target) {
            JutsuLibrary.Rasengan.Cast(pTarget, pTarget, pTile);
        }
        return false;
    }

    public static bool RaenganMovement(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (a.has_attack_target) {
            a.goTo(pTarget.a.attack_target.current_tile);
        } else {
            a.finishStatusEffect("Active Rasengan");
        }
        return true;
    }

    public static bool RasenganAttack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile) {
        Actor a = pSelf.a;
        a.finishStatusEffect("Active Rasengan");
        float bonusDamage = a.data.level * 120;
        pTarget.getHit(250 + bonusDamage, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
        EffectsLibrary.spawnExplosionWave(new Vector3(pTarget.current_position.x, pTarget.current_position.y, 0), 0.2f, 1.5f);
        EffectsLibrary.spawnAt("fx_explosion_small", pTarget.current_position, 0.06f);
        return true;
    }

    public static bool Chidori(BaseSimObject pTarget, WorldTile pTile = null) {
        if (Randy.randomChance(0.3f) && pTarget.a.has_attack_target) {
            JutsuLibrary.Chidori.Cast(pTarget, pTarget, pTile);
        }
        return false;
    }

    public static bool ChidoriMovement(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        if (a.has_attack_target) {
            a.goTo(pTarget.a.attack_target.current_tile);
        } else {
            a.finishStatusEffect("Chidori Attack");
        }
        return true;
    }

    public static bool ChidoriAttack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile) {
        Actor a = pSelf.a;
        a.finishStatusEffect("Chidori Attack");
        float bonusDamage = a.data.level * 60;
        pTarget.getHit(250 + bonusDamage, pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
        // EffectsLibrary.spawnExplosionWave(new Vector3(pTarget.current_position.x, pTarget.current_position.y, 0), 0.2f, 1.5f);
        // EffectsLibrary.spawnAt("fx_explosion_small", pTarget.current_position, 0.06f);
        EffectsLibrary.spawnAt("fx_cast_top_blue", pTarget.current_position, 0.15f);
        return true;
    }

    public static bool CurseMark(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
        Actor a = pSelf.a;


        
        if (Randy.randomChance(0.3f)) {


            if (a.data.level <= 3) {
                addStatus(a, "Curse1", 150);

            }
            else if (a.data.level >= 4 && a.data.level <= 6) {
                addStatus(a, "Curse2", 275);
            } else {
                addStatus(a, "Curse3", 425);
            }

            return true;
        }
        return false;
    }


    public static bool CloneDestroy(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.getHit(10000000, pFlash: false, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
        pTarget.a.addTrait("death_mark");
        return true;
    }

    public static bool ShadowCloneJutsu(BaseSimObject pTarget, WorldTile pTile = null) {
        JutsuLibrary.ShadowClone.Cast(pTarget, pTarget, pTile);
        return true;
    }

    public static bool jogan(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
        Actor a = pSelf.a;

        bool hasHamuraChakra = a.hasTrait("hamuraChakra");
        bool hasHagoromoChakra = a.hasTrait("hagoromoChakra");
        bool hasOtsutsukiDNA = a.hasTrait("otsutsukiClan");
        bool hasPureDNA = a.hasTrait("otsutsuki20");
        bool hasPureDNA2 = a.hasTrait("otsutsuki40");
        bool hasPureDNA3 = a.hasTrait("otsutsuki60");
        bool hasPureDNA4 = a.hasTrait("otsutsuki80");

        if (hasHamuraChakra && hasHagoromoChakra && (hasOtsutsukiDNA || hasPureDNA || hasPureDNA2 || hasPureDNA3 || hasPureDNA4) && Randy.randomChance(0.1f)) {
            a.addStatusEffect("Jog");
            return true;
        } else {
            return false;
        }
    }

    public static bool KyuubiSageModeChakraDrain(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        float chakra = 0;
        a.data.get("chakra", out chakra);
        if (chakra >= 50) {
            removeChakra(a, 2);
            return false;
        } else {
            a.finishStatusEffect("Kyuubi Sage");
            a.finishStatusEffect("Six Path Sage");
            addStatus(a, "Link Mode");
            return true;
        }
    }

    public static bool LinkModeChakraDrain(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget.a;
        float chakra = 0;
        a.data.get("chakra", out chakra);
        if (chakra >= 15) {
            removeChakra(a, 1);
            return false;
        } else {
            a.finishStatusEffect("Link Mode");
            return true;
        }
    }

    public static void addStatus(Actor a, string status, int chakra = 0, int heal = -1) {
        if (a == null) {
            return;
        }
        if (heal == -1) {
            var stats = AssetManager.status.get(status);
            heal = (int)(stats.base_stats["health"]);
        }
        if (!a.hasStatus(status)) {
            float healthPercent = ((float)(a.data.health) / (float)(a.getMaxHealth()));
            a.addStatusEffect(status);
            a.updateStats();
            a.data.health = Mathf.Clamp((int)((float)(a.getMaxHealth()) * healthPercent), 1, a.getMaxHealth());
            addChakra(a, chakra);
        } else {
            a.addStatusEffect(status);
        }
    }

        public static bool Bigguy(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
                if (Randy.randomChance(0.3f)) {
                        JutsuLibrary.Multisize.Cast(pSelf, pSelf, pTile);
                        return true;
                }

                return false; 
        }

    public static bool Eight(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null) {
        JutsuLibrary.EightGates.Cast(pSelf, pSelf, pTile);
        return true;
    }

public static bool Transfer(BaseSimObject pTarget, WorldTile pTile = null)
{
        if (pTarget == null || !pTarget.isActor())
        {
                return false;
        }

        Actor a = pTarget.a;

        if (!a.isAlive())
        {
                return false;
        }

        BaseSimObject attackedBy = a.attackedBy;

        if (attackedBy != null && attackedBy.isActor() && attackedBy.isAlive())
        {
                HandleStatusTransfer(a, attackedBy);
        }

        if (attackedBy != null && attackedBy.isActor() && attackedBy.isAlive())
        {
                if (a.data.level >= 20 && a.hasTrait("otsutsukiClan"))
                {
                        HandleKarmaSealTransfer(a, attackedBy);
                }
        }

        return false;
}

private static void HandleStatusTransfer(Actor a, BaseSimObject attackedBy)
{
        if (a.hasStatus("ItachiMS") || a.hasStatus("Itachi's eye"))
        {
                if (!attackedBy.a.hasTrait("itachiReincarnate") && Randy.randomChance(0.56f))
                {
                        attackedBy.a.addStatusEffect("Itachi's eye");
                        attackedBy.a.data.set("has_stolen_itachi_eye", 1f);
                }
        }

        if (a.hasStatus("IndraMS") || a.hasStatus("Indra's eye"))
        {
                if (!attackedBy.a.hasTrait("indrasReincarnate") && Randy.randomChance(0.56f))
                {
                        attackedBy.a.addStatusEffect("Indra's eye");
                        attackedBy.a.data.set("has_stolen_indra_eye", 1f);
                }
        }

        if (a.hasStatus("madaraMS") || a.hasStatus("Madara's eye"))
        {
                if (!attackedBy.a.hasTrait("madarasReincarnate") && Randy.randomChance(0.56f))
                {
                        attackedBy.a.addStatusEffect("Madara's eye");
                        attackedBy.a.data.set("has_stolen_madara_eye", 1f);
                }
        }

        a.data.get("hasCMS", out float hasCMS);
        a.data.get("hasStolenCMS", out float hasSCMS);
        if (a.hasStatus("CMS_" + a.data.id) || hasCMS > 0.5f || hasSCMS > 0.5f)
        {
                if (Randy.randomChance(1f))
                {
                        CustomSharinganActions.tryStealCMS(a, attackedBy.a);
                }
        }

        if (a.hasStatus("ByakuP"))
        {
                if (Randy.randomChance(0.56f))
                {
                        CustomSharinganActions.tryStealByakugan(a, attackedBy.a);
                }
        }

        if (a.hasStatus("Active Sharingan3") || a.hasStatus("3 Tomoe-S eye"))
        {
                if (Randy.randomChance(0.56f))
                {
                        attackedBy.a.addStatusEffect("3 Tomoe-S eye");
                        attackedBy.a.data.set("has_stolen_3tomoe", 1f);
                }
        }

        if (a.hasStatus("Active Rinnegan") || a.hasStatus("StolenRinnegan"))
        {
                if (Randy.randomChance(0.56f))
                {
                        attackedBy.a.addStatusEffect("StolenRinnegan");
                        attackedBy.a.data.set("has_stolen_rinnegan", 1f);
                        JBPatches.EnsureLogAssets();
                        WorldLogLibrary lib = AssetManager.world_log_library;
                        WorldLogAsset logAsset = lib.get("jb_log_rinnegan_stolen");
                        if (logAsset != null)
                        {
                            var logMsg = new WorldLogMessage(logAsset, attackedBy.a.getName() + " has stolen " + a.getName() + "'s Rinnegan!");
                            logMsg.unit = attackedBy.a;
                            if (attackedBy.a.current_tile != null)
                                logMsg.location = attackedBy.a.current_tile.posV3;
                            logMsg.add();
                        }
                }
        }

}

private static void HandleKarmaSealTransfer(Actor a, BaseSimObject attackedBy)
{
        if (attackedBy.a.hasTrait("completedKarmaSeal") || attackedBy.a.hasTrait("incompletedKarmaSeal"))
        {
                return;
        }

        if (Randy.randomChance(0.3f))
        {
                attackedBy.a.addTrait("completedKarmaSeal", false);
                attackedBy.a.data.set("karmaSealName", a.getName());
                a.data.favorite = true;
        }
        else
        {
                attackedBy.a.addTrait("incompletedKarmaSeal", false);
                attackedBy.a.data.set("karmaSealName", a.getName());
                a.data.favorite = true;
        }
}

    public static void removeChakra(Actor a, float amount) {
        ChakraSystem.removeChakra(a, amount);
    }

    // public static void chakraRain(WorldTile pTile = null, string pdrop_id = null)
    // {
    //     World.world.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
    //     for (int i = 0; i < World.world.temp_map_objects.Count; i++)
    //     {
    //         Actor actor = (Actor)World.world.temp_map_objects[i];
    //         if (actor.isAlive())
    //         {
    //             float chakra = 0;
    //             actor.data.get("chakra", out chakra);
    //             addChakra(actor, (int)(chakra * 0.2f));
    //             actor.startShake();
    //             actor.startColorEffect();
    //         }
    //     }
    // }

    public static void addSharinganUsage(Actor a, float amount) {
    float usage = 0;

    a.data.get("SharinganUsage", out usage);
    
    usage += amount;
    
    a.data.set("SharinganUsage", usage);

    string sharinganName = "";
    a.data.get("sharinganName", out sharinganName);
    
    if (sharinganName == null || sharinganName == "") {
        sharinganName = a.data.name; 
        a.data.set("sharinganName", sharinganName);
    }

    if (usage >= 55) {
        if (!a.hasTrait("Hashc")) { // Hashirama cells = No blindness :shrug:
            a.addTrait("sharinganBlindness");
        }
        
        a.finishStatusEffect("CMS_" + a.data.id);
        a.finishStatusEffect("madaraMS");
        a.finishStatusEffect("IndraMS");
        a.finishStatusEffect("ItachiMS");
    }
}

    public static void addKarmaSealUsage(Actor a, float amount) {
        a.data.get("karmaSealUsage", out float usage);
        usage += amount;
        a.data.set("karmaSealUsage", usage);
        a.data.get("karmaSealName", out string karmaSealName);
        if (karmaSealName == null) {
            karmaSealName = a.data.name;
        }
        if (usage >= 20) {
            a.addTrait("otsutsukiClan");
            a.addTrait("immortal");
            a.data.name = karmaSealName;
            a.removeTrait("otsutsuki80");
        } else if (usage >= 16) {
            a.addTrait("otsutsuki80");
            a.removeTrait("otsutsuki60");
        } else if (usage >= 12) {
            a.addTrait("otsutsuki60");
            a.removeTrait("otsutsuki40");
        } else if (usage >= 8) {
            a.addTrait("otsutsuki40");
            a.removeTrait("otsutsuki20");
        } else if (usage >= 4) {
            a.addTrait("otsutsuki20");
        }
    }

    public static void addChakra(Actor a, float amount) {
        ChakraSystem.addChakra(a, amount);
    }

    public static bool effectGate8(BaseSimObject pTarget, WorldTile pTile = null) {
            Actor a = pTarget.a;
            a.data.health = (int)((float)(a.data.health) * 0.01f);

            JBPatches.SendWorldLog(a, "jb_log_eighth_gate");
            if (WorldTip.instance != null)
                WorldTip.instance.show("<color=red>The Eighth Gate has been opened!</color>", false, "top", 3f);

            if (Randy.randomChance(0.20f)) {
                    a.addTrait("EGH");
                    a.removeTrait("eightGatesTechnique");
                    return false;
            } 
            else {
                    a.addTrait("death_mark");
            }
            
            return true;
    }
   
    public static bool effectGate7(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.1f);
        return true;
    }

    public static bool effectGate6(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.2f);
        return true;
    }

    public static bool effectGate5(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.3f);
        return true;
    }

    public static bool effectGate4(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.4f);
        return true;
    }

    public static bool effectGate3(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.5f);
        return true;
    }

    public static bool effectGate2(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.6f);
        return true;
    }

    public static bool effectGate1(BaseSimObject pTarget, WorldTile pTile = null) {
        pTarget.a.data.health = (int)((float)(pTarget.a.data.health) * 0.75f);
        return true;
    }

    public static bool EightGatesDrain(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget?.a;
        if (a == null || !a.isAlive()) return false;

        int highestGate = 0;
        for (int i = 1; i <= 8; i++) {
            if (a.hasStatus("gate" + i)) highestGate = i;
        }
        if (highestGate == 0) {
            a.data.set("_gate_active_elapsed", 0f);
            return false;
        }

        if (highestGate >= 8) {
            a.data.get("_gate8_healed", out float g8Healed);
            if (g8Healed < 0.5f) {
                a.data.health = a.getMaxHealth();
                a.data.set("chakra", a.stats["chakra"]);
                a.data.set("_gate8_healed", 1f);
            }
        } else {
            tryRemoveChakra(a, 3f);

            a.data.get("chakra", out float chakra);
            if (chakra <= a.stats["chakra"] * 0.3f) {
                int dmg = Mathf.Max(1, (int)(a.getMaxHealth() * 0.03f));
                a.getHit(dmg, pFlash: false, AttackType.Other, null, pSkipIfShake: true);
            }
        }

        a.data.get("_gate_active_elapsed", out float elapsed);
        elapsed += 1f;
        a.data.set("_gate_active_elapsed", elapsed);

        if (elapsed >= 85f) {
            a.data.set("_gate_active_elapsed", 0f);
            switch (highestGate) {
                case 8: effectGate8(pTarget, pTile); break;
                case 7: effectGate7(pTarget, pTile); break;
                case 6: effectGate6(pTarget, pTile); break;
                case 5: effectGate5(pTarget, pTile); break;
                case 4: effectGate4(pTarget, pTile); break;
                case 3: effectGate3(pTarget, pTile); break;
                case 2: effectGate2(pTarget, pTile); break;
                case 1: effectGate1(pTarget, pTile); break;
            }
        }
        return true;
    }

    public static bool ImmeasurableAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        Actor attacker = pSelf?.a;
        Actor target = pTarget?.a;
        if (attacker == null || target == null) return false;
        if (attacker.hasStatus("Immeasurable")) return false;
        float threshold = 30f + 0.1f * attacker.stats["chakra"];
        if (target.stats["chakra"] >= attacker.stats["chakra"] - threshold) return false;
        if (!Randy.randomChance(0.1f)) return false;
        if (!tryRemoveChakra(attacker, 200f)) return false;
        attacker.data.set("_j_rt_anchor_x", attacker.current_position.x);
        attacker.data.set("_j_rt_anchor_y", attacker.current_position.y);
        attacker.data.set("_j_rt_triggered", 0f);
        attacker.data.set("_j_imeasurable_shocked", 0f);
        attacker.addStatusEffect("Immeasurable");
        return true;
    }

    public static bool ImmeasurableStatusAction(BaseSimObject pSelf, WorldTile pTile)
    {
        Actor a = pSelf?.a;
        if (a == null) return false;
        a.data.set("_j_rt_triggered", 1f);
        a.current_position = new Vector2((float)a.data["_j_rt_anchor_x"], (float)a.data["_j_rt_anchor_y"]);
        a.setAttackTarget(null);
        a.data.get("_j_imeasurable_shocked", out float shocked);
        if (shocked > 0.5f)
            return true;
        a.data.set("_j_imeasurable_shocked", 1f);
        float threshold = 30f + 0.1f * a.stats["chakra"];
        List<Actor> allAlive = World.world.units.getSimpleList();
        for (int i = 0; i < allAlive.Count; i++)
        {
            Actor unit = allAlive[i];
            if (unit == a || !unit.isAlive()) continue;
            if (unit.stats["chakra"] >= a.stats["chakra"] - threshold) continue;
            float dist = Vector2.Distance(a.current_position, unit.current_position);
            if (dist > 17f) continue;
            unit.addStatusEffect("stunned");
            unit.data.set("_j_imeasurable_stunned_until", Time.time + 7f);
        }
        return true;
    }

    public static bool hasChakraPercent(Actor a, float pct) {
        a.data.get("chakra", out float chakra);
        return chakra >= a.stats["chakra"] * pct;
    }

    public static void finishAllEyeModes(Actor a) {
        a.finishStatusEffect("Active GoldRinnegan");
        a.finishStatusEffect("Active Rinnegan");
        a.finishStatusEffect("ByakuG");
        a.finishStatusEffect("ByakuP");
        a.finishStatusEffect("Byaku3");
        a.finishStatusEffect("madaraMS");
        a.finishStatusEffect("IndraMS");
        a.finishStatusEffect("ItachiMS");
        a.finishStatusEffect("MSharingan");
        a.finishStatusEffect("Sharingan3");
        a.finishStatusEffect("Sharingan2");
        a.finishStatusEffect("Sharingan");
        if (a._active_status_dict != null) {
            foreach (string sid in a._active_status_dict.Keys) {
                if (sid.StartsWith("CMS_")) {
                    a.finishStatusEffect(sid);
                    break;
                }
            }
        }
    }

    public static void finishAllSageModes(Actor a) {
        a.finishStatusEffect("Six Path Sage");
        a.finishStatusEffect("Kyuubi Sage");
        a.finishStatusEffect("SenjuS");
        a.finishStatusEffect("OneHundredSeal");
        a.finishStatusEffect("SnakeSage");
        a.finishStatusEffect("Toad Sage Mode");
    }

    public static bool EyeModeDrain(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget?.a;
        if (a == null || !a.isAlive()) return false;

        // Implant drain: permanent, never deactivates, flat 3 per second
        bool hasImplant = false;
        if (a._active_status_dict != null) {
            foreach (string sid in a._active_status_dict.Keys) {
                if (sid.StartsWith("CMSImplant_")) { hasImplant = true; break; }
            }
        }
        if (hasImplant) tryRemoveChakra(a, 3f);

        a.data.get("chakra", out float eyeChakra);
        if (eyeChakra <= a.stats["chakra"] * 0.2f) {
            finishAllEyeModes(a);
            finishAllSageModes(a);
            a.data.set("_eye_active_elapsed", 0f);
            return true;
        }

        bool hasEye = false;
        float baseDrain = 0f;

        if (a.hasStatus("Active GoldRinnegan")) { hasEye = true; baseDrain = 10f; }
        else if (a.hasStatus("Active Rinnegan")) { hasEye = true; baseDrain = 6f; }
        else if (a.hasStatus("ByakuG")) { hasEye = true; baseDrain = 5f; }
        else if (a.hasStatus("ByakuP")) { hasEye = true; baseDrain = 3f; }
        else if (a.hasStatus("Byaku3")) { hasEye = true; baseDrain = 2f; }
        else if (a.hasStatus("madaraMS") || a.hasStatus("IndraMS") || a.hasStatus("ItachiMS") || a.hasStatus("MSharingan")) { hasEye = true; baseDrain = 4f; }
        else if (a.hasStatus("Sharingan3")) { hasEye = true; baseDrain = 3f; }
        else if (a.hasStatus("Sharingan2")) { hasEye = true; baseDrain = 2f; }
        else if (a.hasStatus("Sharingan")) { hasEye = true; baseDrain = 1f; }
        else if (a._active_status_dict != null) {
            foreach (string sid in a._active_status_dict.Keys) {
                if (sid.StartsWith("CMS_")) { hasEye = true; baseDrain = 5f; break; }
            }
        }

        if (!hasEye) {
            a.data.set("_eye_active_elapsed", 0f);
            return false;
        }

        a.data.get("_eye_active_elapsed", out float elapsed);
        elapsed += 1f;
        a.data.set("_eye_active_elapsed", elapsed);

        float multiplier = Mathf.Pow(1.10f, Mathf.Floor(elapsed / 10f));
        float healthPct = (float)a.data.health / (float)a.getMaxHealth();
        if (healthPct <= 0.5f) multiplier *= 1.4f;
        float drain = baseDrain * multiplier;

        if (!tryRemoveChakra(a, drain)) {
            finishAllEyeModes(a);
            a.data.set("_eye_active_elapsed", 0f);
        }
        return true;
    }

    public static bool RinneganDrain(BaseSimObject pTarget, WorldTile pTile = null) {
        EyeModeDrain(pTarget, pTile);
        Actor a = pTarget?.a;
        if (a == null || !a.isAlive()) return false;
        if (!(a.hasStatus("Active Rinnegan") || a.hasStatus("Active GoldRinnegan"))) return false;
        a.data.get("_rinnegan_last_6p", out float last6p);
        if (Time.time - last6p >= 3f) {
            SixPathsTechnique(pTarget, pTile);
            a.data.set("_rinnegan_last_6p", Time.time);
        }
        return true;
    }

    public static bool SageModeDrain(BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pTarget?.a;
        if (a == null || !a.isAlive()) return false;

        a.data.get("chakra", out float chakra);
        if (chakra <= a.stats["chakra"] * 0.2f) {
            finishAllEyeModes(a);
            finishAllSageModes(a);
            return true;
        }

        float natureDrain = 0f;
        bool hasSage = false;

        if (a.hasStatus("Six Path Sage")) { hasSage = true; natureDrain = 8f; }
        else if (a.hasStatus("Kyuubi Sage")) { hasSage = true; natureDrain = 6f; }
        else if (a.hasStatus("SenjuS")) { hasSage = true; natureDrain = 4f; }
        else if (a.hasStatus("OneHundredSeal")) { hasSage = true; natureDrain = 5f; }
        else if (a.hasStatus("SnakeSage")) { hasSage = true; natureDrain = 3f; }
        else if (a.hasStatus("Toad Sage Mode")) { hasSage = true; natureDrain = 3f; }

        if (!hasSage) return false;

        float healthPct = (float)a.data.health / (float)a.getMaxHealth();
        if (healthPct <= 0.5f) natureDrain *= 1.4f;

        a.data.get("sage_chakra", out float sageCur);
        sageCur -= natureDrain;

        if (sageCur <= 0f) {
            a.data.set("sage_chakra", 0f);
            finishAllSageModes(a);
        } else {
            a.data.set("sage_chakra", sageCur);
        }

        if (a.hasStatus("SenjuS") || a.hasStatus("OneHundredSeal")) {
            a.data.get("_sage_heal_timer", out float healTimer);
            if (Time.time - healTimer >= 3f) {
                OnehunHealings(pTarget, pTile);
                a.data.set("_sage_heal_timer", Time.time);
            }
        }
        return true;
    }
}
