using System.Text;
using UnityEngine;
using NeoModLoader.api;

namespace aaaMod{
    internal static class Actors{
        public static void init(){
        //Actor Icon to Phenotype Changes---------------------------------------------------------------------------------------------------------
            //Human
                // ActorAsset editHumanActor = AssetManager.actor_library.get("human");
                // editHumanActor.icon = "iconHuman_PhenoA";


        //Miscellaneous Actor Changes---------------------------------------------------------------------------------------------------------
            //Sand Spider
                ActorAsset sandspider = AssetManager.actor_library.get("sand_spider");
                sandspider.animation_walk = new string[] { "walk_0", "walk_1"};
                sandspider.texture_asset = new ActorTextureSubAsset("actors/a_sandSpiderNew/", false);
                sandspider._cached_sprite = Resources.Load<Sprite>("GameResources/actors/a_sandSpiderNew/Main");
                sandspider.texture_id = "a_sandSpiderNew";
                shadowFix(sandspider);
                AssetManager.actor_library.loadTexturesAndSprites(sandspider);

        //Langton Ants---------------------------------------------------------------------------------------------------------
            //Blue Ant
                ActorAsset blueant = AssetManager.actor_library.get("ant_blue");
                blueant.animation_walk = new string[] { "walk_0", "walk_1"};
                blueant.texture_asset = new ActorTextureSubAsset("actors/a_antBlueNew/", false);
                blueant._cached_sprite = Resources.Load<Sprite>("GameResources/actors/a_antBlueNew/Main");
                blueant.texture_id = "a_antBlueNew";
                shadowFix(blueant);
                AssetManager.actor_library.loadTexturesAndSprites(blueant);
            //Green Ant
                ActorAsset greenant = AssetManager.actor_library.get("ant_green");
                greenant.animation_walk = new string[] { "walk_0", "walk_1"};
                greenant.texture_asset = new ActorTextureSubAsset("actors/a_antGreenNew/", false);
                greenant._cached_sprite = Resources.Load<Sprite>("GameResources/actors/a_antGreenNew/Main");
                greenant.texture_id = "a_antGreenNew";
                shadowFix(greenant);
                AssetManager.actor_library.loadTexturesAndSprites(greenant);
            //Black Ant
                ActorAsset blackant = AssetManager.actor_library.get("ant_black");
                blackant.animation_walk = new string[] { "walk_0", "walk_1"};
                blackant.texture_asset = new ActorTextureSubAsset("actors/a_antBlackNew/", false);
                blackant._cached_sprite = Resources.Load<Sprite>("GameResources/actors/a_antBlackNew/Main");
                blackant.texture_id = "a_antBlackNew";
                shadowFix(blackant);
                AssetManager.actor_library.loadTexturesAndSprites(blackant);
            //Red Ant
                ActorAsset redant = AssetManager.actor_library.get("ant_red");
                redant.animation_walk = new string[] { "walk_0", "walk_1"};
                redant.texture_asset = new ActorTextureSubAsset("actors/a_antRedNew/", false);
                redant._cached_sprite = Resources.Load<Sprite>("GameResources/actors/a_antRedNew/Main");
                redant.texture_id = "a_antRedNew";
                shadowFix(redant);
                AssetManager.actor_library.loadTexturesAndSprites(redant);
            }

            public static void shadowFix(ActorAsset fixing){
                fixing.texture_asset.shadow_size.x = 1f;
                fixing.texture_asset.shadow_size.y = 1f;
                fixing.texture_asset.shadow_size_egg.x = 1f;
                fixing.texture_asset.shadow_size_egg.y = 1f;
                fixing.texture_asset.shadow_size_baby.x = 1f;
                fixing.texture_asset.shadow_size_baby.y = 1f;  
            }
        }
    }
