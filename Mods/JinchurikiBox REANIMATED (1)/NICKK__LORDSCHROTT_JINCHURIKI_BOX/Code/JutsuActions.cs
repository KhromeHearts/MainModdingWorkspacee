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
using NeoModLoader;
using NeoModLoader.General;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using ai;
using JinchurikiBox.Core;
using JinchurikiBox.Content;


namespace JinchurikiBox.Actions;
internal static class JutsuActions {
    public static void addXP(string jutsu, Actor a, int amount = 1) {
        a.data.get("xp_" + jutsu, out int xp);
        a.data.get("level_" + jutsu, out int level);
        xp++;
        if (xp >= (level^2)) {
            level++;
        }
        a.data.set("xp_" + jutsu, xp);
        if (level <= 10) {
            a.data.set("level_" + jutsu, level);
        }
    }

    public static int getLvl(string jutsu, Actor a) {
        a.data.get("level_" + jutsu, out int num);
        return num;
    }

    public static bool amaterasuActivation(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {
        pTarget.a.addStatusEffect("amaterasu");
        addXP("amaterasu", pSelf.a);
        return true;
    }

    public static bool MakeShadowClone(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {
        Actor a = pSelf.a;

        if (a.hasStatus("Shadow Clone")) {
            a.removeTrait("shadowCloneJutsu");
        }

        if (a.has_attack_target && a.data.health <= a.getMaxHealth() / 2) {
            addXP("shadow_clone", a);
            for (int i = 0; i < getLvl("shadow_clone", a); i++) {
                if (i == 0 || Randy.randomChance(0.33f)) {
                    var act = World.world.units.createNewUnit(a.asset.id, pTile);
                    for (int j = 0; j < a.data.saved_traits.Count; j++)
                    {
                        string text = a.data.saved_traits[j];
                        act.addTrait(text);
                    }
                    Vector3 offset = new Vector2(UnityEngine.Random.Range(-4, 4), UnityEngine.Random.Range(-4, 4));
                    act.setName(a.data.name + " Shadow Clone");
                    act.current_position = a.current_position;
                    act.setKingdom(a.kingdom);
                    act.data.head = a.data.head;
                    // act.data.skin_id = a.data.skin_id;
                    act.data.sex = a.data.sex;
                    act.data.favorite = false;
                    act.removeTrait("shadowCloneJutsu");
                    act.stats["health"] = 1;
                    addStatus(act, "Shadow Clone");
                    act.setStatsDirty();
                    // act.setPosDirty();
                }
            }

            return true;
        }
        return false;
    }

    public static void addSharinganUsage(Actor a, float amount) {
        JinchurikiActions.addSharinganUsage(a, amount);
    }
public static bool Susanoo(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {

    int lvl = getLvl("susanoorib", pSelf.a); 
  
    if (lvl < 5) {

        addStatus(pSelf.a, "madarasusanoo1");

    }
    
    if (lvl >= 5) {
        addStatus(pSelf.a, "madarasusanoo2");
    }
    addXP("susanoorib", pSelf.a);
    return true;

}

    public static bool SharinganGenjutsu(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {
        pTarget.a.addStatusEffect("sharinganGenjutsu");
        pTarget.a.makeWait(3f + getLvl("sharingan_genjutsu", pSelf.a) / 2);
        addXP("sharingan_genjutsu", pSelf.a);
        return true;
    }

    public static bool Kirin(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {
        pTile = pTarget.current_tile;
        for (int i = 0; i < 3  + getLvl("kirin", pSelf.a); i++) {
            MapBox.spawnLightningBig(pTile, 0.2f);
            addXP("kirin", pSelf.a);
        }
        return true;
    }

public static bool Multisize(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {

    Actor a = pSelf.a;  
    addStatus(a, "MultiSize");
     addXP("multis", a);


    int level = getLvl("multis", a);

 //   float scalingFactor = 0.015f; 

 ///   a.base_stats["scale"] = scalingFactor * getLvl; 

    return true;

}

    public static bool Rasengan(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {
        addStatus(pSelf.a, "Active Rasengan");
        addXP("rasengan", pSelf.a);
        return true;
    }

    public static bool Chidori(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {
        addStatus(pSelf.a, "Chidori Attack");
        addXP("chidori", pSelf.a);
        return true;
    }

public static bool InnerGates(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null) {       

    Actor a = pTarget.a;       

    if (Randy.randomChance(0.8f)) {       

        addXP("gates", a);       

        int lvl = getLvl("gates", a);       

        if (!a.hasStatus("gate1")) {       

            a.addStatusEffect("gate1");       

        }       

        if (a.hasStatus("gate1") && lvl > 1 && !a.hasStatus("gate2") && Randy.randomChance(0.7f)) {       

            a.addStatusEffect("gate2");       

        }       

        if (a.hasStatus("gate2") && lvl > 2 && !a.hasStatus("gate3") && Randy.randomChance(0.6f)) {       

            a.addStatusEffect("gate3");       

        }       

        if (a.hasStatus("gate3") && lvl > 3 && !a.hasStatus("gate4") && Randy.randomChance(0.5f)) {       

            a.addStatusEffect("gate4");       

        }       

        if (a.hasStatus("gate4") && lvl > 4 && !a.hasStatus("gate5") && Randy.randomChance(0.4f)) {       

            a.addStatusEffect("gate5");       

        }       

        if (a.hasStatus("gate5") && lvl > 5 && !a.hasStatus("gate6") && Randy.randomChance(0.3f)) {       

            a.addStatusEffect("gate6");       

        }       

        if (a.hasStatus("gate6") && lvl > 6 && !a.hasStatus("gate7") && Randy.randomChance(0.2f)) {       

            a.addStatusEffect("gate7");       

        }       

        if (a.hasStatus("gate7") && lvl > 7 && !a.hasStatus("gate8") && Randy.randomChance(0.1f)) {       

            a.addStatusEffect("gate8");       

        }       

        return true;       

    }       

    return false;       

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
            a.addStatusEffect(status);
            a.restoreHealth(heal);
            ChakraSystem.addChakra(a, chakra);
        } else {
            a.addStatusEffect(status);
        }
    }
}