using System;
using NeoModLoader;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using ReflectionUtility;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using ai;
using ai.behaviours;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using Newtonsoft.Json;

namespace JinchurikiBox.Core;
internal static class ChakraSystem
{
    public static void init()
    {
        BaseStatAsset chakra = new BaseStatAsset();
        chakra.id = "chakra";
        chakra.normalize = true;
        chakra.normalize_max = 9999999;
        AssetManager.base_stats_library.add(chakra);

        BaseStatAsset chakraRegen = new BaseStatAsset();
        chakraRegen.id = "chakra_regen";
        chakraRegen.normalize = false;
        AssetManager.base_stats_library.add(chakraRegen);
        addStatToLocalizedLibrary(chakraRegen.id, "Chakra Regen");

        BaseStatAsset tbChakra = new BaseStatAsset();
        tbChakra.id = "tailed_beast_chakra";
        tbChakra.normalize = true;
        tbChakra.normalize_max = 9999999;
        AssetManager.base_stats_library.add(tbChakra);
        addStatToLocalizedLibrary(tbChakra.id, "Tailed Beasts Chakra");

        BaseStatAsset sageChakra = new BaseStatAsset();
        sageChakra.id = "sage_chakra";
        sageChakra.normalize = true;
        sageChakra.normalize_max = 9999999;
        AssetManager.base_stats_library.add(sageChakra);
        addStatToLocalizedLibrary(sageChakra.id, "Nature energy");

        BaseStatAsset sageChakraRegen = new BaseStatAsset();
        sageChakraRegen.id = "sage_chakra_regen";
        sageChakraRegen.normalize = false;
        AssetManager.base_stats_library.add(sageChakraRegen);
        addStatToLocalizedLibrary(sageChakraRegen.id, "Nature energy regen");
    }
    
    public static void addStatToLocalizedLibrary(string id, string name)
    {
        if (!LocalizedTextManager.instance.contains(id))
            LocalizedTextManager.add(id, name);
    }

    public static void removeChakra(Actor a, float amount, bool forceRemove = false) {
        a.data.get("chakra", out float chakra);
        if (chakra != null && chakra >= amount) {
            a.data.set("chakra", chakra - amount);
        } else {
            if (forceRemove) {
                amount -= chakra;
                a.getHit(20 * amount, true, AttackType.Other, null, true, false, true);
            }
            a.data.set("chakra", 0);
        }
    }

    public static bool tryRemoveChakra(Actor a, float amount) {
        a.data.get("chakra", out float chakra);
        if (chakra != null && chakra >= amount) {
            a.data.set("chakra", chakra - amount);
            return true;
        }
        return false;
    }

    public static void addChakra(Actor a, float amount) {
        a.data.get("chakra", out float chakra);
        float newChakra = chakra + amount;
        float maxChakra = a.stats["chakra"];
        a.data.set("chakra", Mathf.Min(newChakra, maxChakra));
    }

    public static float currentChakra(Actor a) {
        a.data.get("chakra", out float chakra);
        return chakra;
    }

    public static bool chakraDuel(Actor a1, Actor a2, bool drainChakra = false) {
        a1.data.get("chakra", out float chakra1);
        a2.data.get("chakra", out float chakra2);
        int randomness = (int)((chakra1 + chakra2) * 0.1f);
        chakra1 += Randy.randomInt(0, randomness);
        chakra2 += Randy.randomInt(0, randomness);
        if (a1.hasTrait("strong_minded")) {
            chakra1 += 10;
        }
        if (a2.hasTrait("strong_minded")) {
            chakra2 += 10;
        }
        if (drainChakra) {
            removeChakra(a1, chakra2 / 2, false);
            removeChakra(a2, chakra1 / 2, false);
        }
        return chakra1 > chakra2;
    }

    public static bool chakraDuel(float chakra1, float chakra2) {
        int randomness = (int)((chakra1 + chakra2) * 0.2f);
        chakra1 += Randy.randomInt(0, randomness);
        chakra2 += Randy.randomInt(0, randomness);
        return chakra1 > chakra2;
    }

    public static bool chakraDuel(Actor a, float chakra, bool drainChakra = false) {
        a.data.get("chakra", out float actorChakra);
        int randomness = (int)((actorChakra + chakra) * 0.1f);
        actorChakra += Randy.randomInt(0, randomness);
        chakra += Randy.randomInt(0, randomness);
        if (a.hasTrait("strong_minded")) {
            actorChakra += 10;
        }
        if (drainChakra) {
            removeChakra(a, chakra / 2, false);
        }
        return actorChakra > chakra;
    }
}