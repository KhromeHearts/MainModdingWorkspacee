
using System;
using NeoModLoader.api.attributes;
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


namespace JinchurikiBox.Content;
internal static class JinchurikiTraitGroup {
    public static string jinchurikiTrait = "jinchurikiTraits";
    public static string jinchurikiLockedTrait = "jinchurikiLockedTraits";
    public static string TransformationTrait = "TransformationTraits";
    public static string JutsuTrait = "JutsuTraits";
    public static string JutsuTraits2 = "JutsuTraits2";
    public static string ClanTrait = "ClanTraits";
    public static string ClanAddonTrait = "ClanAddonTraits";
    public static string SoulStuff = "SoulStuff";
    public static string RankTrait = "RankTraits";
    public static string ChakraTrait = "ChakraTraits";
    public static string ChakraTrait2 = "ChakraTraits2";
    public static string ChakraNTrait = "ChakraNTraits";
    public static string ChakraNTrait2 = "ChakraNTraits2";
    
    public static void init()
    {
        ActorTraitGroupAsset jinchurikiTraits = new ActorTraitGroupAsset();
        jinchurikiTraits.id = "jinchurikiTraits";
        jinchurikiTraits.name = "trait_group_jinchurikiTraits";
        jinchurikiTraits.color = "#990000";
        AssetManager.trait_groups.add(jinchurikiTraits);
        // addTraitGroupToLocalizedLibrary(jinchurikiTraits.id, "JinchurikiBox Traits");

        ActorTraitGroupAsset jinchurikiLockedTraits = new ActorTraitGroupAsset();
        jinchurikiLockedTraits.id = "jinchurikiLockedTraits";
        jinchurikiLockedTraits.name = "trait_group_jinchurikiLockedTraits";
        jinchurikiLockedTraits.color = "#bb8800";
        AssetManager.trait_groups.add(jinchurikiLockedTraits);
        // addTraitGroupToLocalizedLibrary(jinchurikiLockedTraits.id, "JinchurikiBox Traits #2");

        ActorTraitGroupAsset TransformationTraits = new ActorTraitGroupAsset();
        TransformationTraits.id = "TransformationTraits";
        TransformationTraits.name = "trait_group_transformationTraits";
        TransformationTraits.color = "#998888";
        AssetManager.trait_groups.add(TransformationTraits);
        // addTraitGroupToLocalizedLibrary(TransformationTraits.id, "Addon Traits");
        
        ActorTraitGroupAsset JutsuTraits2 = new ActorTraitGroupAsset();
        JutsuTraits2.id = "JutsuTraits2";
        JutsuTraits2.name = "trait_group_jutsuTraits2";
        JutsuTraits2.color = "#00cccc";
        AssetManager.trait_groups.add(JutsuTraits2);
        // addTraitGroupToLocalizedLibrary(JutsuTraits2.id, "Senjutsu");

        ActorTraitGroupAsset JutsuTraits = new ActorTraitGroupAsset();
        JutsuTraits.id = "JutsuTraits";
        JutsuTraits.name = "trait_group_jutsuTraits";
        JutsuTraits.color = "#004499";
        AssetManager.trait_groups.add(JutsuTraits);
        // addTraitGroupToLocalizedLibrary(JutsuTraits.id, "Jutsu Traits");

        // ActorTraitGroupAsset JutsuTrait2 = new ActorTraitGroupAsset();
        // JutsuTrait2.id = "JutsuTrait2";
        // JutsuTrait2.name = "trait_group_JutsuTrait2";
        // JutsuTrait2.color = Toolbox.makeColor("#004499", -1f);
        // AssetManager.trait_groups.add(JutsuTrait2);
        // addTraitGroupToLocalizedLibrary(JutsuTraits.id, "Jutsu Traits");

        ActorTraitGroupAsset ChakraNTraits2 = new ActorTraitGroupAsset();
        ChakraNTraits2.id = "ChakraNTraits2";
        ChakraNTraits2.name = "trait_group_chakraNTraits2";
        ChakraNTraits2.color = "#ffff00";
        AssetManager.trait_groups.add(ChakraNTraits2);
        // addTraitGroupToLocalizedLibrary(ChakraNTraits2.id, "Special Chakra Natures");

        ActorTraitGroupAsset ChakraNTraits = new ActorTraitGroupAsset();
        ChakraNTraits.id = "ChakraNTraits";
        ChakraNTraits.name = "trait_group_chakraNTraits";
        ChakraNTraits.color = "#004499";
        AssetManager.trait_groups.add(ChakraNTraits);
        // addTraitGroupToLocalizedLibrary(ChakraNTraits.id, "Chakra Natures");

        ActorTraitGroupAsset ClanTraits = new ActorTraitGroupAsset();
        ClanTraits.id = "ClanTraits";
        ClanTraits.name = "trait_group_clanTraits";
        ClanTraits.color = "#aaaa00";
        AssetManager.trait_groups.add(ClanTraits);
        // addTraitGroupToLocalizedLibrary(ClanTraits.id, "Clan Traits");

        ActorTraitGroupAsset ClanAddonTraits = new ActorTraitGroupAsset();
        ClanAddonTraits.id = "ClanAddonTraits";
        ClanAddonTraits.name = "trait_group_clanAddonTraits";
        ClanAddonTraits.color = "#aaaa00";
        AssetManager.trait_groups.add(ClanAddonTraits);
        // addTraitGroupToLocalizedLibrary(ClanAddonTraits.id, "Clan Addons");

        ActorTraitGroupAsset SoulStuff = new ActorTraitGroupAsset();
        SoulStuff.id = "SoulStuff";
        SoulStuff.name = "trait_group_SoulStuff";
        SoulStuff.color = "#00dddd";
        AssetManager.trait_groups.add(SoulStuff);
        
        ActorTraitGroupAsset RankTraits = new ActorTraitGroupAsset();
        RankTraits.id = "RankTraits";
        RankTraits.name = "trait_group_rankTraits";
        RankTraits.color = "#ffff00";
        AssetManager.trait_groups.add(RankTraits);
        // addTraitGroupToLocalizedLibrary(RankTraits.id, "Rank Traits");

        ActorTraitGroupAsset ChakraTraits = new ActorTraitGroupAsset();
        ChakraTraits.id = "ChakraTraits";
        ChakraTraits.name = "trait_group_chakraTraits";
        ChakraTraits.color = "#00cccc";
        AssetManager.trait_groups.add(ChakraTraits);
        // addTraitGroupToLocalizedLibrary(ChakraTraits.id, "Chakra Attributes");

        ActorTraitGroupAsset ChakraTraits2 = new ActorTraitGroupAsset();
        ChakraTraits2.id = "ChakraTraits2";
        ChakraTraits2.name = "trait_group_chakraTraits2";
        ChakraTraits2.color = "#00dddd";
        AssetManager.trait_groups.add(ChakraTraits2);
        // addTraitGroupToLocalizedLibrary(ChakraTraits2.id, "Chakra Types");


    }
}