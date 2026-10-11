using System;
using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.api;

namespace avbsexpandedarmory{
    internal static class MetaLoverTraits{
        public static void init(){
            ActorAsset lemonman = AssetManager.actor_library.get("civ_lemon_man");
            lemonman.addCultureTrait("Helmet Lovers");
            ActorAsset bandit = AssetManager.actor_library.get("bandit");
            bandit.addCultureTrait("Headband Lovers");
            ActorAsset evilmage = AssetManager.actor_library.get("evil_mage");
            evilmage.addCultureTrait("Cloak Lovers");
            ActorAsset whitemage = AssetManager.actor_library.get("white_mage");
            whitemage.addCultureTrait("Cloak Lovers");
            ActorAsset necromancer = AssetManager.actor_library.get("necromancer");
            necromancer.addCultureTrait("Cloak Lovers");
            ActorAsset druid = AssetManager.actor_library.get("druid");
            druid.addCultureTrait("Cloak Lovers");
            ActorAsset plaguedoctor = AssetManager.actor_library.get("plague_doctor");
            plaguedoctor.addCultureTrait("Cloak Lovers");
            ActorAsset civdog = AssetManager.actor_library.get("civ_dog");
            civdog.addCultureTrait("Scythe Weapon");
            ActorAsset civrabbit = AssetManager.actor_library.get("civ_rabbit");
            civrabbit.addCultureTrait("Bludgeon Lovers");
            ActorAsset civfox = AssetManager.actor_library.get("civ_fox");
            civfox.addCultureTrait("Crossbow Lovers");
            ActorAsset civrhino = AssetManager.actor_library.get("civ_rhino");
            civrhino.addCultureTrait("Armor Lovers");
            ActorAsset civbuffalo = AssetManager.actor_library.get("civ_buffalo");
            civbuffalo.addCultureTrait("Bludgeon Lovers");
            ActorAsset civgoat = AssetManager.actor_library.get("civ_goat");
            civgoat.addCultureTrait("Highcleat Lovers");
            ActorAsset civscorpion = AssetManager.actor_library.get("civ_scorpion");
            civscorpion.addCultureTrait("Slingshot Lovers");
            ActorAsset civseal = AssetManager.actor_library.get("civ_seal");
            civseal.addCultureTrait("Boot Lovers");
            ActorAsset bee = AssetManager.actor_library.get("bee");
            bee.addCultureTrait("Dagger Lovers");
            ActorAsset civsheep = AssetManager.actor_library.get("civ_sheep");
            civsheep.addCultureTrait("Ring Lovers");
            ActorAsset civunicorn = AssetManager.actor_library.get("civ_unicorn");
            civunicorn.addCultureTrait("Bracelet Lovers");
            ActorAsset civmonkey = AssetManager.actor_library.get("civ_monkey");
            civmonkey.addCultureTrait("Amulet Lovers");
            ActorAsset civliliar = AssetManager.actor_library.get("civ_liliar");
            civliliar.addCultureTrait("Earring Lovers");

            BiomeAsset corrupted = AssetManager.biome_library.get("biome_corrupted");
            corrupted.addCultureTrait("Dagger Lovers");
            BiomeAsset candy = AssetManager.biome_library.get("biome_candy");
            candy.addCultureTrait("Bludgeon Lovers");
            BiomeAsset desert = AssetManager.biome_library.get("biome_desert");
            desert.addCultureTrait("Slingshot Lovers");
        }
    }
}
