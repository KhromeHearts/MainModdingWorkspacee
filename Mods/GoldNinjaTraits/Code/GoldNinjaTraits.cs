using System;
using System.Threading;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ai;
using NeoModLoader.api.attributes;
using NeoModLoader.api;
using NeoModLoader.General;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using NCMS.Utils;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GoldNinjaTraits;

public static class Traits
{
    public static void Init()
    {
        // Add localization text (prevents warning)
        Localization.Add("trait_divine_damage", "Divine Damage");
        Localization.Add("trait_divine_damage_desc", "Deals massively increased damage.");

        ActorTraitGroupAsset group = new ActorTraitGroupAsset();
        group.id = "GoldNinjaTraitsGroup";
        group.name = "GoldNinjaTraits_group";
        group.color = "#FFD700";
        AssetManager.trait_groups.add(group);

        ActorTrait divineDamage = new ActorTrait
        {
            id = "divine_damage",
            group_id = "GoldNinjaTraitsGroup",
            path_icon = "ui/Icons/iconDivineDamage",
            rarity = Rarity.R2_Epic,
            needs_to_be_explored = false,
        };

        // Add base stats so it shows in unit stats
        divineDamage.base_stats = new BaseStats();
        divineDamage.base_stats.set("damage", 5000f);
        divineDamage.action_birth = new WorldAction(birth);

        // Register the trait
        AssetManager.traits.add(divineDamage);
    }

    public static bool birth(BaseSimObject pTarget, WorldTile pTile)
    {
        Debug.Log("yo");
        return true;
    }
}