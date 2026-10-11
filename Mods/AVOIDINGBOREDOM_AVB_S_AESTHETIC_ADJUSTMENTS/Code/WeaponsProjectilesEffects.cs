//---Credit for code help: This page uses minorly modified WeaponsProjectilesEffects class from the Modernbox mod
//---This mod received no direct assistance from contributors of the Modernbox mod and was made by referencing its code
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using NeoModLoader;
using System.Linq;

namespace aaaMod{
    public class WeaponsProjectilesEffects : MonoBehaviour{
        public static void init(){
            int totalChecked = 0;
            int totalFixed = 0;
            int totalSkipped = 0;

            foreach (var itm in AssetManager.items.list)
            {
                string id = itm.id;
                EquipmentAsset asset = itm;

                if (asset == null)
                {
                    totalSkipped++;
                    continue;
                }

                totalChecked++;

                if (asset.gameplay_sprites == null || asset.gameplay_sprites.Length == 0)
                {
                    var sprites = FetchSprites(id);
                    asset.gameplay_sprites = sprites;

                    if (sprites != null && sprites.Length > 0)
                        totalFixed++;
                }
                else if (id.Equals("sword_iron")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "sword_iron") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("axe_iron")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "axe_iron") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("hammer_iron")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "hammer_iron") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("spear_iron")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "spear_iron") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("bow_iron")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "bow_iron") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("sword_steel")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "sword_steel") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("axe_steel")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "axe_steel") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("hammer_steel")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "hammer_steel") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("spear_steel")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "spear_steel") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else if (id.Equals("bow_steel")){Sprite[] sprite = { Resources.Load<Sprite>("weapons/" + "bow_steel") }; asset.gameplay_sprites = sprite; totalFixed++;}
                else
                {
                    totalSkipped++;
                }
            }
        }

        public static Sprite[] FetchSprites(string id)
        {
            EquipmentAsset item = AssetManager.items.get(id);
            if (item == null)
                return Array.Empty<Sprite>();

            if (item.animated)
            {
                List<Sprite> spriteList = new List<Sprite>();
                int frameIndex = 0;
                bool foundFrames = false;

                while (true)
                {
                    string[] paths = new[]
                    {
                        $"weapons/{id}_{frameIndex}",
                        $"weapons/{id}{frameIndex}",
                        $"weapons/{id}/main_0_{frameIndex}"
                    };

                    bool frameFound = false;
                    foreach (string path in paths)
                    {
                        Sprite sprite = Resources.Load<Sprite>(path);
                        if (sprite != null)
                        {
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

                if (!foundFrames)
                {
                    var fallback = Resources.LoadAll<Sprite>("weapons/" + id);
                    if (fallback != null && fallback.Length > 0)
                        return fallback;
                    else
                        return Array.Empty<Sprite>();
                }

                return spriteList.ToArray();
            }
            else
            {   
                var sprite = Resources.Load<Sprite>("weapons/" + id);;
                return sprite != null ? new Sprite[] { sprite } : Array.Empty<Sprite>();
            }
        }
    }
}

