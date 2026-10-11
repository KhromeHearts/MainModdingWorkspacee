using System;
using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.api;

namespace avbsexpandedarmory{
    internal static class Creatures{
        public static void init(){
            ActorAsset demon = AssetManager.actor_library.get("demon");
            demon.default_weapons = AssetLibrary<ActorAsset>.a<string>(new string[]
		    {
			    "flame_sword",
			    "flame_spear",
                "flame_hammer",
                "flame_axe",
                "flame_bludgeon",
                "flame_halberd",
                "flame_dagger",
                "flame_bow",
                "flame_crossbow",
                "flame_slingshot"
		    });
            ActorAsset coldone = AssetManager.actor_library.get("cold_one");
            coldone.default_weapons = AssetLibrary<ActorAsset>.a<string>(new string[]
		    {
			    "ice_sword",
			    "ice_spear",
                "ice_hammer",
                "ice_axe",
                "ice_bludgeon",
                "ice_halberd",
                "ice_dagger",
                "ice_bow",
                "ice_crossbow",
                "ice_slingshot"
		    });
            ActorAsset angle = AssetManager.actor_library.get("angle");
            angle.default_weapons = AssetLibrary<ActorAsset>.a<string>(new string[]
		    {
			    "angle_ruler"
		    });
            angle.addCultureTrait("Angle Items");
            ActorAsset plaguedoctor = AssetManager.actor_library.get("plague_doctor");
            plaguedoctor.addCultureTrait("Scythe Weapon");
        }
    }
}
