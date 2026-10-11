using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NeoModLoader.General;
using static Config;
using Random=UnityEngine.Random;
using ReflectionUtility;
using System.Reflection;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using System.IO;
using NCMS;
using ai;
using JinchurikiBox.Core;
using JinchurikiBox.Content;
using JinchurikiBox.Patches;


namespace JinchurikiBox.Actions;
internal static class CustomSharinganActions {
    public static bool tryRemoveChakra(Actor a, float amount) {
        return ChakraSystem.tryRemoveChakra(a, amount);
    }

    public static void removeChakra(Actor a, float amount) {
        ChakraSystem.removeChakra(a, amount);
    }

    public static void addChakra(Actor a, float amount) {
        ChakraSystem.addChakra(a, amount);
    }

    public static void addSharinganUsage(Actor a, float amount) {
        JinchurikiActions.addSharinganUsage(a, amount);
    }

    public static bool SharinganGen(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
    {
        return JinchurikiActions.SharinganGen(pSelf, pTarget, pTile);
    }

    public static void applyCMS(Actor a) {
        a.data.get("hasCMS", out float hasCustomSharingan);
        if (hasCustomSharingan < 0.5f) {
            StatusAsset MSharingan = new StatusAsset
            {
            base_stats = new BaseStats(),
                id = "CMS_" + a.data.id,
                locale_id = "status_customSharingan_" + a.data.id + a.data.name,
                locale_description = "status_customSharingan_description"
            };
            MSharingan.base_stats["speed"] = 31 + Randy.randomInt(-5, 5);
            MSharingan.base_stats["attack_speed"] = 75 + Randy.randomInt(-15, 15);
            MSharingan.base_stats["range"] = 0.45f + ((float)(Randy.randomInt(-10, 10)) / 100f);
            MSharingan.base_stats["damage"] = 53 + Randy.randomInt(-11, 12);
            MSharingan.base_stats["armor"] = 6 + Randy.randomInt(-2, 2);
            MSharingan.duration = 30f;
            MSharingan.action = JinchurikiActions.EyeModeDrain;
            MSharingan.action_interval = 1f;
            MSharingan.animated = false;
        string[] availableIcons = { "ui/icons/SwirlMS", "ui/icons/SwirlMS2", "ui/icons/SecretMS", "ui/icons/TomoeMS", "ui/icons/BulkMS", "ui/icons/BibsMS" };
        int iconIdx = Randy.randomInt(0, availableIcons.Length);
        MSharingan.path_icon = availableIcons[iconIdx];
        a.data.set("CMS_icon_idx", (float)iconIdx);

        if (AssetManager.status.get("CMS_" + a.data.id) == null)
            AssetManager.status.add(MSharingan);
        AddLocaleEntry("status_customSharingan_" + a.data.id + a.data.name, a.getName() + "'s Mangekyo Sharingan");
        AddLocaleEntry("status_customSharingan_description", "After seeing the result of Many fallen comrades and foes, they've unlocked the Uchiha Clan's prized possession, The Mangekyo Sharingan, though its power can be quite taxing.");
 
            a.data.set("hasCMS", 1f);
            a.data.set("CMS_speed", MSharingan.base_stats["speed"]);
            a.data.set("CMS_attack_speed", MSharingan.base_stats["attack_speed"]);
            a.data.set("CMS_range", MSharingan.base_stats["range"]);
            a.data.set("CMS_damage", MSharingan.base_stats["damage"]);
            a.data.set("CMS_armor", MSharingan.base_stats["armor"]);

            JBPatches.EnsureLogAssets();
            WorldLogLibrary lib = AssetManager.world_log_library;
            WorldLogAsset logAsset = lib.get("jb_log_cms_awakened");
            if (logAsset != null)
            {
                var logMsg = new WorldLogMessage(logAsset, a.getName() ?? string.Empty);
                logMsg.unit = a;
                if (a.current_tile != null)
                    logMsg.location = a.current_tile.posV3;
                logMsg.add();
            }

            if (WorldTip.instance != null)
                WorldTip.instance.show("<color=red>" + a.getName() + " has awakened the Mangekyo Sharingan!</color>", false, "top", 3f);
        }
        
        if (!a.hasStatus("CMS_" + a.data.id)) {
            float healthPercent = ((float)(a.data.health) / (float)(a.getMaxHealth()));
            a.addStatusEffect("CMS_" + a.data.id);
            a.updateStats();
            a.data.health = Mathf.Clamp((int)((float)(a.getMaxHealth()) * healthPercent), 1, a.getMaxHealth());
            addSharinganUsage(a, 1);
        } else {
            a.addStatusEffect("CMS_" + a.data.id);
        }
        JBPatches.AddCMSHitAction(a);
    }

    public static void tryStealCMS(Actor a1, Actor a2) {
        a1.data.get("hasCMS", out float hasCustomSharingan);
        a1.data.get("hasStolenCMS", out float hasStolenCMS);
        if (hasCustomSharingan < 0.5f && hasStolenCMS < 0.5f) {
            return;
        }

        StatusAsset implant = new StatusAsset();
        implant.id = "CMSImplant_" + a1.data.id;
        implant.locale_id = "status_customSharinganImplant_" + a1.data.id;
        implant.locale_description = $"status_customSharinganImplant_{a1.data.id}_description";

        if (hasCustomSharingan > 0.5f) {
            implant.base_stats = AssetManager.status.get("CMS_" + a1.data.id).base_stats;
        } else if (hasStolenCMS > 0.5f) {
            implant.base_stats = AssetManager.status.get("CMSImplant_" + a1.data.id).base_stats;
        }
        implant.duration = 86400f;
        implant.animated = false;
        implant.path_icon = "ui/icons/PulledShar";
        if (AssetManager.status.get("CMSImplant_" + a1.data.id) == null)
            AssetManager.status.add(implant);

        AddLocaleEntry("status_customSharinganImplant_" + a1.data.id, a1.getName() + "'s Stolen Mangekyo");
        AddLocaleEntry($"status_customSharinganImplant_{a1.data.id}_description", "After defeating " + a1.getName() + " in combat, they've claimed their Mangekyo Sharingan as their own.");
            
                a2.addStatusEffect("CMSImplant_" + a1.data.id);
                a2.data.set("hasStolenCMS", 1f);
                a2.data.set("originalSharinganUserID", a1.data.id);
                JBPatches.attackActions["CMSImplant_" + a1.data.id] = JinchurikiActions.MSHit;

                JBPatches.EnsureLogAssets();
                WorldLogLibrary lib = AssetManager.world_log_library;
                WorldLogAsset logAsset = lib.get("jb_log_cms_stolen");
                if (logAsset != null)
                {
                    var logMsg = new WorldLogMessage(logAsset, a2.getName() + " has stolen " + a1.getName() + "'s Mangekyo Sharingan!");
                    logMsg.unit = a2;
                    if (a2.current_tile != null)
                        logMsg.location = a2.current_tile.posV3;
                    logMsg.add();
                }
            }

    public static void removeCMS(string id) {
        AssetManager.status.dict.Remove(id);
    }

    public static void createSusanoo(Actor a) {

    }

    public static void tryStealByakugan(Actor victim, Actor attacker)
    {
        string sourceId = null;
        string displayName = null;

        if (victim.hasStatus("ByakuG")) {
            sourceId = "ByakuG";
            displayName = "Golden Byakugan";
        } else if (victim.hasStatus("ByakuP")) {
            sourceId = "ByakuP";
            displayName = "Pure Byakugan";
        } else if (victim.hasStatus("Byaku3")) {
            sourceId = "Byaku3";
            displayName = "Byakugan";
        } else if (victim.hasStatus("StolenByakup")) {
            sourceId = "StolenByakup";
            displayName = "Stolen Byakugan";
        } else if (victim._active_status_dict != null) {
            foreach (string sid in victim._active_status_dict.Keys) {
                if (sid.StartsWith("StolenByakup_")) {
                    sourceId = sid;
                    displayName = "Stolen Byakugan";
                    break;
                }
            }
        }
        if (sourceId == null) return;

        StatusAsset source = AssetManager.status.get(sourceId);
        if (source == null) return;

        string newId = "StolenByakup_" + victim.data.id;

        if (AssetManager.status.get(newId) == null)
        {
            StatusAsset stolen = new StatusAsset
            {
                base_stats = new BaseStats(),
                id = newId,
                locale_id = "stolenByakugan_" + victim.data.id,
                locale_description = "stolenByakugan_" + victim.data.id + "_description"
            };
            stolen.base_stats["speed"] = source.base_stats["speed"];
            stolen.base_stats["attack_speed"] = source.base_stats["attack_speed"];
            stolen.base_stats["armor"] = source.base_stats["armor"];
            stolen.base_stats["damage"] = source.base_stats["damage"];
            stolen.duration = 86400f;
            stolen.animated = false;
            stolen.path_icon = "ui/icons/PulledByak";
            AssetManager.status.add(stolen);

            AddLocaleEntry("stolenByakugan_" + victim.data.id, victim.getName() + "'s Stolen " + displayName);
            AddLocaleEntry("stolenByakugan_" + victim.data.id + "_description", "After defeating " + victim.getName() + " in combat, they've claimed their " + displayName + " as their own.");

            JBPatches.attackActions[newId] = JinchurikiActions.BHit;
        }

        attacker.addStatusEffect(newId);
        attacker.data.set("has_stolen_byakugan", victim.data.id);
    }

    private static void AddLocaleEntry(string key, string text)
    {
        var dict = GetLocaleDictionary();
        if (dict != null && !dict.ContainsKey(key))
            dict[key] = text;
    }

    private static Dictionary<string, string> GetLocaleDictionary()
    {
        if (LocalizedTextManager.instance == null) return null;
        var type = LocalizedTextManager.instance.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var field in type.GetFields(flags))
        {
            if (field.FieldType == typeof(Dictionary<string, string>))
                return field.GetValue(LocalizedTextManager.instance) as Dictionary<string, string>;
        }

        foreach (var prop in type.GetProperties(flags))
        {
            if (prop.PropertyType == typeof(Dictionary<string, string>) && prop.CanRead)
                return prop.GetValue(LocalizedTextManager.instance) as Dictionary<string, string>;
        }

        return null;
    }
}