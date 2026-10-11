//---Credit for code help: This page uses a modified version of the WeaponsProjectilesEffects class from the Modernbox mod
//---This mod received no direct assistance from the contributors of Modernbox mod and was made by referencing its code
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using NeoModLoader;
using System.Linq;

namespace avbsexpandedarmory{
    public class LoadWeaponSprites : MonoBehaviour{
        public static void init(){
            foreach (var itm in AssetManager.items.list){
                string id = itm.id;
                EquipmentAsset asset = itm;

                if (asset == null){
                    continue;
                }
                //Mini-Sprite Overwrites
                bool isIce = id.Contains("ice");
                bool isFlame = id.Contains("flame");
                Sprite[] spriteOVR = null;
                if (ConfigsAEA.changeItemSprites && isIce){
                    if (ConfigsAEA.scciVal == -1){
                        spriteOVR = new Sprite[] { Resources.Load<Sprite>("weapons/avbchanges/" + id + "_darkC") };
                    }
                    else {
                        spriteOVR = new Sprite[] { Resources.Load<Sprite>("weapons/avbchanges/" + id) };
                    }
                }
                else if (!ConfigsAEA.changeItemSprites && isIce){
                    if (ConfigsAEA.scciVal == -1){
                        spriteOVR = new Sprite[] { Resources.Load<Sprite>("weapons/" + id + "_darkC") };
                    }
                    else {
                        spriteOVR = new Sprite[] { Resources.Load<Sprite>("weapons/" + id ) };
                    }
                }
                else if (ConfigsAEA.changeItemSprites && !isIce && !isFlame){
                    spriteOVR = new Sprite[] { Resources.Load<Sprite>("weapons/avbchanges/" + id) };
                }
                else if (ConfigsAEA.changeItemSprites && isFlame){
                    spriteOVR = new Sprite[] { Resources.Load<Sprite>("weapons/avbchanges/" + id + "_0"), Resources.Load<Sprite>("weapons/avbchanges/" + id + "_1"), 
                    Resources.Load<Sprite>("weapons/avbchanges/" + id + "_2"), Resources.Load<Sprite>("weapons/avbchanges/" + id + "_3") };
                }

                if (spriteOVR != null){
                    if (spriteOVR[0] != null){
                        asset.gameplay_sprites = spriteOVR;
                    }
                }

                if (asset.gameplay_sprites == null || asset.gameplay_sprites.Length == 0){

                    var sprites = FetchSprites(id);
                    asset.gameplay_sprites = sprites;
                }
            }
        }

        public static Sprite[] FetchSprites(string id){
            EquipmentAsset item = AssetManager.items.get(id);
            if (item == null)
                return Array.Empty<Sprite>();

            if (item.animated){
                List<Sprite> spriteList = new List<Sprite>();
                int frameIndex = 0;
                bool foundFrames = false;

                while (true){
                    string[] paths = new[]{
                        $"weapons/{id}_{frameIndex}",
                        $"weapons/{id}{frameIndex}",
                        $"weapons/{id}/main_0_{frameIndex}"
                    };

                    bool frameFound = false;
                    foreach (string path in paths){
                        Sprite sprite = Resources.Load<Sprite>(path);
                        if (sprite != null){
                            spriteList.Add(sprite);
                            foundFrames = true;
                            frameIndex++;
                            frameFound = true;
                            break;
                        }
                    }

                    if (!frameFound) break;
                    if (frameIndex > 20) break;
                }

                if (!foundFrames){
                    var fallback = Resources.LoadAll<Sprite>("weapons/" + id);
                    if (fallback != null && fallback.Length > 0)
                        return fallback;
                    else
                        return Array.Empty<Sprite>();
                }

                return spriteList.ToArray();
            }
            else{   
                var sprite = Resources.Load<Sprite>("weapons/" + id);
                return sprite != null ? new Sprite[] { sprite } : Array.Empty<Sprite>();
            }
        }

        public static void fixItemModifiersAndSpells(){ //used in Main.cs
            foreach (EquipmentAsset tAsset in AssetManager.items.list){
                if (tAsset.item_modifier_ids != null){
                    tAsset.item_modifiers = new ItemModAsset[tAsset.item_modifier_ids.Length];
                    for (int i = 0; i < tAsset.item_modifier_ids.Length; i++){
                        string tModID = tAsset.item_modifier_ids[i];
                        ItemModAsset tModData = AssetManager.items_modifiers.get(tModID);
                        if (tModData == null){
                            BaseAssetLibrary.logAssetError("ItemLibrary: Item Modifier Asset <e>not found</e>", tModID);
                        }
                        else{
                            tAsset.item_modifiers[i] = tModData;
                        }
                    }
                }
            }
            // AssetManager.items.fillSubtypesAndGroups();
            // AssetManager.items.fillUnlockedPools();
            foreach (EquipmentAsset equipmentAsset in AssetManager.items.list){
                equipmentAsset.linkSpells();
            }

            //from ItemLibrary.post_init() for adding gold costs to custom items
            //right now all item gold costs are 2! so this basically just adds 2 or 4 to gold costs
            foreach (EquipmentAsset tEquipmentAsset in AssetManager.items.list){
                if (!string.IsNullOrEmpty(tEquipmentAsset.path_icon)){
                    int tResourcesGoldCostResources = 0;
                    if (tEquipmentAsset.cost_resource_id_1 != "none"){
                        ResourceAsset tResource = AssetManager.resources.get(tEquipmentAsset.cost_resource_id_1);
                        tResourcesGoldCostResources += tResource.money_cost;
                    }
                    if (tEquipmentAsset.cost_resource_id_2 != "none"){
                        ResourceAsset tResource2 = AssetManager.resources.get(tEquipmentAsset.cost_resource_id_2);
                        tResourcesGoldCostResources += tResource2.money_cost;
                    }
                    tEquipmentAsset.cost_coins_resources = tResourcesGoldCostResources;
                }
            }
        }
    }
}

