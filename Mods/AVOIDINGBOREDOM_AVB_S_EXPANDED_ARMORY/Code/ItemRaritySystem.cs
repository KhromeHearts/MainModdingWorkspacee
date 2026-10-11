using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.api;
using HarmonyLib;

namespace avbsexpandedarmory{
    internal static class ItemRaritySystem{
        public static void init(){
            //Masterwork
            RarityAsset mundane = AssetManager.rarity_library.add(new RarityAsset{//4
                id = "4",
                color_hex = "#fb4040",
                material_path = string.Empty
                //rarity_trait_string = "trait_common"
            });
            
            //Transcendent
            RarityAsset transcendent = AssetManager.rarity_library.add(new RarityAsset{//5
                id = "5",
                color_hex = "#c135f7",
                material_path = string.Empty
                //rarity_trait_string = "trait_common"
            });
            AssetManager.rarity_library.linkAssets();
        }

        [HarmonyPatch(typeof(Item), "getQuality")]
        public static class QualitySetIntercept1{
            static bool Prefix(Item __instance, ref Rarity __result){
                if (ConfigsAEA.changeRaritySystem == true){
                    EquipmentType itemType = __instance._asset.equipment_type;
                    Rarity selectedRarity = (Rarity)0;
                    int totalPlusAmount = 0;
                    int specialPlusAmount = 0;
                    int normalPlusAmount = 0;

                    foreach (string modId in __instance.data.modifiers){
                        ItemModAsset modAsset = AssetManager.items_modifiers.get(modId);
                        totalPlusAmount += modAsset.mod_rank;
                        if (modAsset.rarity == 0){ //if special modifier
                            specialPlusAmount += modAsset.mod_rank;
                        }
                    }
                    normalPlusAmount = totalPlusAmount - specialPlusAmount;

                    if (totalPlusAmount < 5){
                        selectedRarity = (Rarity)0; //Normal, less than 5 mod pluses
                    }
                    else if (totalPlusAmount < 10){
                        selectedRarity = (Rarity)1; //Rare, 5 to 9 mod pluses
                    }
                    else if (totalPlusAmount < 15){
                        selectedRarity = (Rarity)2; //Epic, 10 to 14 mod pluses
                    }
                    else if (totalPlusAmount >= 15){
                        selectedRarity = (Rarity)3; //Legendary, more than 15 mod pluses
                    }

                    if (itemType == EquipmentType.Weapon && normalPlusAmount == 25){
                        selectedRarity = (Rarity)4; //Masterwork, maxxed out in normal modifiers (weapon)
                    }
                    else if ((itemType == EquipmentType.Amulet || itemType == EquipmentType.Ring) && normalPlusAmount == 35){
                        selectedRarity = (Rarity)4; //Masterwork, maxxed out in normal modifiers (accessories)
                    }
                    else if (normalPlusAmount == 20){
                        selectedRarity = (Rarity)4; //Masterwork, maxxed out in normal modifiers (armor)
                    }

                    if (selectedRarity == (Rarity)4 && specialPlusAmount > 0){
                        selectedRarity = (Rarity)5; //Transcendent, maxxed out with special modifiers
                    }

                    __result = selectedRarity;
                    return false;
                }
                else {
                    return true;
                }
            }
        }
        //Weapon: Speed, Balance, Finesse, Truth, Power (25)
        //Armor: Speed, Balance, Health, Protection (20)
        //Accessories: Speed, Balance, Finesse, Health, Knowledge, Mastery, Sharpness (35)

        [HarmonyPatch(typeof(EquipmentBanner), "setupBanner")] //changes outline in the item editor
        public static class QualitySetIntercept2{
            static void Postfix(EquipmentBanner __instance){
                if (ConfigsAEA.changeRaritySystem == true){
                    if (__instance.meta_object.getQuality() != Rarity.R0_Normal){
                        if (__instance.meta_object.getQuality() == Rarity.R1_Rare){
                            __instance._outline.show(RarityLibrary.rare.color_container);
                        }
                        else if (__instance.meta_object.getQuality() == Rarity.R2_Epic){
                            __instance._outline.show(RarityLibrary.epic.color_container);
                        }
                        else if (__instance.meta_object.getQuality() == Rarity.R3_Legendary){
                            __instance._outline.show(RarityLibrary.legendary.color_container);
                        }
                        else if (__instance.meta_object.getQuality() == (Rarity)4){
                            __instance._outline.show(AssetManager.rarity_library.get("4").color_container);
                            __instance.part_frame.sprite = __instance._frame_sprite_legendary;
                            __instance.part_frame.gameObject.SetActive(true);
                        }
                        else if (__instance.meta_object.getQuality() == (Rarity)5){
                            __instance._outline.show(AssetManager.rarity_library.get("5").color_container);
                            __instance.part_frame.sprite = __instance._frame_sprite_legendary;
                            __instance.part_frame.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Item), "getQualityString")]
        public static class QualitySetIntercept3{
            static bool Prefix(Item __instance, ref string __result, Rarity pRarity){
                if (ConfigsAEA.changeRaritySystem == true){
                    string tQuality = "";
                    switch (pRarity)
                    {
                    case Rarity.R0_Normal:
                        tQuality = "";
                        break;
                    case Rarity.R1_Rare:
                        tQuality = "_rare";
                        break;
                    case Rarity.R2_Epic:
                        tQuality = "_epic";
                        break;
                    case Rarity.R3_Legendary:
                        tQuality = "_legendary";
                        break;
                    case (Rarity)4:
                        tQuality = "_masterwork";
                        break;
                    case (Rarity)5:
                        tQuality = "_transcendent";
                        break;
                    }
                    __result = tQuality;
                    return false;
                }
                else {
                    return true;
                }
            }
        }
        [HarmonyPatch(typeof(TooltipActorEquipmentsRow), "loadItems")] //changed the tooltip on actors to reflect rarity
        public static class QualitySetIntercept4{
            static bool Prefix(TooltipActorEquipmentsRow __instance){
                if (ConfigsAEA.changeRaritySystem == true){
                    __instance.items_pool.clear(true);
                    Actor tActor = __instance.tooltip_data.actor;
                    if (!tActor.canUseItems() || tActor.equipment == null || !tActor.equipment.hasItems()){
                        __instance.gameObject.SetActive(false);
                        return false;
                    }
                    bool tHasEquipment = false;
                    foreach (ActorEquipmentSlot actorEquipmentSlot in tActor.equipment){
                        Item tEquipment = actorEquipmentSlot.getItem();
                        if (tEquipment != null){
                            tHasEquipment = true;
                            TooltipOutlineItem tIcon = __instance.items_pool.getNext();
                            tIcon.image.sprite = tEquipment.getSprite();
                            if (tEquipment.getQuality() != Rarity.R0_Normal){
                                if (tEquipment.getQuality() == Rarity.R1_Rare){
                                    tIcon.outline.show(RarityLibrary.rare.color_container);
                                }
                                else if (tEquipment.getQuality() == Rarity.R2_Epic){
                                    tIcon.outline.show(RarityLibrary.epic.color_container);
                                }
                                else if (tEquipment.getQuality() == Rarity.R3_Legendary){
                                    tIcon.outline.show(RarityLibrary.legendary.color_container);
                                }
                                else if (tEquipment.getQuality() == (Rarity)4){
                                    tIcon.outline.show(AssetManager.rarity_library.get("4").color_container);
                                }
                                else if (tEquipment.getQuality() == (Rarity)5){
                                    tIcon.outline.show(AssetManager.rarity_library.get("5").color_container);
                                }
                            }
                            else{
                                tIcon.outline.gameObject.SetActive(false);
                            }
                        }
                    }
                    __instance.gameObject.SetActive(tHasEquipment);
                    return false;
                }
                else {
                    return true;
                }
            }
        }
        [HarmonyPatch(typeof(EquipmentButton), "load", new Type[] { typeof(Item) })]
        public static class QualitySetIntercept5{
            static bool Prefix(EquipmentButton __instance, Item pItem){
                if (ConfigsAEA.changeRaritySystem == true){
                    __instance._object_button = true;
                    __instance.create();
                    __instance._item = pItem;
                    __instance.augmentation_asset = __instance._item.getAsset();
                    if (__instance.augmentation_asset == null)
                    {
                        return false;
                    }
                    __instance.image.sprite = __instance._item.getSprite();

                    //base.loadLegendaryOutline();
                    __instance._shadow.enabled = true;
                    Item tEquipment = __instance._item;
                    if (tEquipment.getQuality() != Rarity.R0_Normal){
                        __instance._shadow.enabled = false;
                        if (tEquipment.getQuality() == Rarity.R1_Rare){
                            __instance._outline.show(RarityLibrary.rare.color_container);
                        }
                        else if (tEquipment.getQuality() == Rarity.R2_Epic){
                            __instance._outline.show(RarityLibrary.epic.color_container);
                        }
                        else if (tEquipment.getQuality() == Rarity.R3_Legendary){
                            __instance._outline.show(RarityLibrary.legendary.color_container);
                        }
                        else if (tEquipment.getQuality() == (Rarity)4){
                            __instance._outline.show(AssetManager.rarity_library.get("4").color_container);
                        }
                        else if (tEquipment.getQuality() == (Rarity)5){
                            __instance._outline.show(AssetManager.rarity_library.get("5").color_container);
                        }
                    }
                    else {
                        __instance._outline.gameObject.SetActive(false);
                    }

                    __instance.gameObject.name = __instance.getElementType() + "_" + __instance._item.data.asset_id;
                    bool tIsFavorite = __instance._item.isFavorite();
                    __instance._favorited_icon.gameObject.SetActive(tIsFavorite);
                    return false;
                }
                else {
                    return true;
                }
            }
        }
        // [HarmonyPatch(typeof(ItemTools), "mergeStats")]
        // public static class QualitySetIntercept_debugtests{
        //     static bool Prefix(BaseStats pStats, ItemAsset pAsset, bool pCalcGlobalValue = true, float pMultiplier = 1f){
        //         if (pAsset == null)
        //         {
        //             return false;
        //         }
        //         pStats.mergeStats(pAsset.base_stats, pMultiplier);
        //         if (pCalcGlobalValue)
        //         {
        //             if (pAsset.quality > ItemTools.s_quality)
        //             {
        //                 ItemTools.s_quality = pAsset.quality;
        //             }
        //             ItemTools.s_value += pAsset.equipment_value + pAsset.mod_rank * 2;
        //             Debug.Log($"ItemTools.s_value = {ItemTools.s_value}, the added is: {pAsset.equipment_value + pAsset.mod_rank * 2}");
        //         }
        //         return false;
        //     }
        // }
    }
}
