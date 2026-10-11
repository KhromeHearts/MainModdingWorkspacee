using System;
using System.Threading;
using NCMS;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Linq;
using ai;
using NeoModLoader.services;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using NCMS.Utils;
using UnityEngine.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace GOWB
{
    internal static class Spells
    {
        public static void Init()
        {
            SpellAsset dynja = new SpellAsset();
            dynja.id = "Dynja eitr";
            dynja.chance = 0.1f;
            dynja.min_distance = 3.5f;
            dynja.can_be_used_in_combat = true;
            dynja.action = new AttackAction(Dynjaaction);
            AssetManager.spells.add(dynja);

            SpellAsset drifa = new SpellAsset();
            drifa.id = "Drifa";
            drifa.chance = 0.6f;
            drifa.min_distance = 3.5f;
            drifa.can_be_used_in_combat = true;
            drifa.action = new AttackAction(Drifaaction);
            AssetManager.spells.add(drifa);

            SpellAsset allfireball = new SpellAsset();
            allfireball.id = "All fireball";
            allfireball.chance = 0.1f;
            allfireball.min_distance = 5f;
            allfireball.can_be_used_in_combat = true;
            allfireball.action = new AttackAction(Allfireballaction);
            AssetManager.spells.add(allfireball);

            //This is only to be used via Unit Possession with kingbox installed, the ai does not use this
            SpellAsset allshield = new SpellAsset();
            allshield.id = "All shield";
            allshield.chance = 0.0f;
            allshield.cast_target = CastTarget.Himself;
            allshield.action = new AttackAction(Explosionshieldaction);
            AssetManager.spells.add(allshield);

            SpellAsset baldurpunch = new SpellAsset();
            baldurpunch.id = "Baldur punch";
            baldurpunch.chance = 1.0f;
            baldurpunch.min_distance = 0f;
            baldurpunch.cost_mana = 0;
            baldurpunch.can_be_used_in_combat = true;
            baldurpunch.action = new AttackAction(Baldurpunchaction);
            AssetManager.spells.add(baldurpunch);

            SpellAsset godlyuppercut = new SpellAsset();
            godlyuppercut.id = "Godly uppercut";
            godlyuppercut.chance = 0.1f;
            godlyuppercut.min_distance = 0f;
            godlyuppercut.cost_mana = 0;
            godlyuppercut.can_be_used_in_combat = true;
            godlyuppercut.action = new AttackAction(tryToDoGU);
            AssetManager.spells.add(godlyuppercut);

            //This is only to be used via Unit Possession with kingbox installed, the ai does not use this
            SpellAsset throwleviathanaxe = new SpellAsset();
            throwleviathanaxe.id = "Throw leviathan axe";
            throwleviathanaxe.chance = 0.0f;
            throwleviathanaxe.min_distance = 3.5f;
            throwleviathanaxe.action = new AttackAction(Throwleviathanaxeaction);
            AssetManager.spells.add(throwleviathanaxe);

            SpellAsset castlivingplants = new SpellAsset();
            castlivingplants.id = "Cast living plants";
            castlivingplants.chance = 1.0f;
            castlivingplants.action = new AttackAction(Castlivingplantsaction);
            AssetManager.spells.add(castlivingplants);

            //This is only to be used via Unit Possession with kingbox installed, the ai does not use this
            SpellAsset transformtobjorn = new SpellAsset();
            transformtobjorn.id = "Transform to Bjorn";
            transformtobjorn.chance = 0.0f;
            transformtobjorn.cast_target = CastTarget.Himself;
            transformtobjorn.action = new AttackAction(MorphintoBjorn);
            AssetManager.spells.add(transformtobjorn);

            //Just for testing
            SpellAsset draupnirain = new SpellAsset();
            draupnirain.id = "Cast Draupnir rain";
            draupnirain.chance = 0.0f;
            draupnirain.cast_target = CastTarget.Enemy;
            draupnirain.action = new AttackAction(CastDraupnirRain);
            AssetManager.spells.add(draupnirain);
        }

        public static bool Dynjaaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || pSelf.current_tile == null)
            {
                return false;
            }

            pSelf.a.addStatusEffect("No friendly fire", 15f);

            for (int i = 0; i < 60; ++i)
            {
                World.world.drop_manager.spawnParabolicDrop(pSelf.current_tile, "lava", 4f, 0.1f, 3.5f, 4.0f, 20f, Randy.randomFloat(0.025f, 0.2f));
                World.world.drop_manager.spawnParabolicDrop(pSelf.current_tile, "acid", 4f, 0.1f, 3.5f, 2.0f, 10f, Randy.randomFloat(0.025f, 0.2f));
                World.world.drop_manager.spawnParabolicDrop(pSelf.current_tile, "snow", 4f, 0.1f, 3.5f, 4.0f, 20f, Randy.randomFloat(0.025f, 0.2f));
            }

            return true;
        }

        public static bool Drifaaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || pTarget == null || pTarget.a == null)
            {
                return false;
            }

            pTile ??= pTarget.current_tile;
            if (pTile == null)
            {
                return false;
            }

            Vector2Int pos = pTile.pos;
            float pDist = Vector2.Distance(pTarget.current_position, pos);
            Vector3 newPointa = Toolbox.getNewPoint(pSelf.current_position.x, pSelf.current_position.y + 0.5f, pos.x + 1f, pos.y + 1f, pDist, true);
            Vector3 newPoint2 = Toolbox.getNewPoint(pTarget.current_position.x, pTarget.current_position.y, pos.x, pos.y, pTarget.a.stats["scale"], true);
            World.world.projectiles.spawn(pSelf, pTarget, "RavenSwarm", newPointa, newPoint2);

            return true;
        }

        public static bool Allfireballaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || pTarget == null || pTarget.a == null)
            {
                return false;
            }

            pTile ??= pTarget.current_tile;
            if (pTile == null)
            {
                return false;
            }

            Vector2Int pos = pTile.pos;
            float pDist = Vector2.Distance(pTarget.current_position, pos);
            Vector3 newPointa = Toolbox.getNewPoint(pSelf.current_position.x, pSelf.current_position.y + 3f, pos.x + 1f, pos.y + 1f, pDist, true);
            Vector3 newPointb = Toolbox.getNewPoint(pSelf.current_position.x + 5f, pSelf.current_position.y + 3f, pos.x + 1f, pos.y + 1f, pDist, true);
            Vector3 newPointc = Toolbox.getNewPoint(pSelf.current_position.x - 5f, pSelf.current_position.y + 3f, pos.x + 1f, pos.y + 1f, pDist, true);
            Vector3 newPoint2 = Toolbox.getNewPoint(pTarget.current_position.x, pTarget.current_position.y, pos.x, pos.y, pTarget.a.stats["scale"], true);

            if (pSelf.a.hasTrait("Odin"))
            {
                World.world.projectiles.spawn(pSelf, pTarget, "MagicFlameBall", newPointa, newPoint2);
            }

            else
            {
                World.world.projectiles.spawn(pSelf, pTarget, "MagicFlameBall", newPointa, newPoint2);
                World.world.projectiles.spawn(pSelf, pTarget, "MagicFlameBall", newPointb, newPoint2);
                World.world.projectiles.spawn(pSelf, pTarget, "MagicFlameBall", newPointc, newPoint2);
            }

            return true;
        }

        public static bool Baldurpunchaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            Actor pSelfActor = pSelf as Actor;
            Actor pTargetActor = pTarget as Actor;
            if (pTargetActor != null && pSelfActor != null)
            {
                pTile = pTargetActor.current_tile;
                pSelfActor.addStatusEffect("Knockback Resistance", 10f);
                EffectsLibrary.spawnExplosionWave(pTile.posV3, 0.001f, 3.0f);
                World.world.applyForceOnTile(pTile, 3, 0.3f, true, 0, null, pTargetActor);
                MapAction.damageWorld(pTile, 5, AssetManager.terraform.get("crab_step"), null);

                if (Randy.randomChance(0.030f))
                {
                    Earthquake.startQuake(pTargetActor.current_tile, EarthquakeType.SmallDisaster);
                }
            }

            return true;
        }

        public static bool tryToDoGU(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            Actor pSelfActor = pSelf as Actor;
            Actor pTargetActor = pTarget as Actor;
            Vector2 pSelfPosition = pSelf.current_position;
            if (pTargetActor != null && pSelfActor != null)
            {
                pTile = pTargetActor.current_tile;
                GodlyUppercut(pSelfActor, pTargetActor, pSelfPosition, null);
                EffectsLibrary.spawnExplosionWave(pTile.posV3, 0.001f, 3.0f);
                // MapAction.damageWorld(pTile, 5, AssetManager.terraform.get("crab_step"), null);
            }

            return true;
        }

        public static bool Throwleviathanaxeaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || pTarget == null || pTarget.a == null)
            {
                return false;
            }

            pTile ??= pTarget.current_tile;
            if (pTile == null)
            {
                return false;
            }

            Vector2Int pos = pTile.pos;
            float pDist = Vector2.Distance(pTarget.current_position, pos);
            Vector3 newPointa = Toolbox.getNewPoint(pSelf.current_position.x, pSelf.current_position.y, pos.x + 1f, pos.y + 1f, pDist, true);
            Vector3 newPoint2 = Toolbox.getNewPoint(pTarget.current_position.x, pTarget.current_position.y, pos.x, pos.y, pTarget.a.stats["scale"], true);
            World.world.projectiles.spawn(pSelf, pTarget, "LeviathanAxeThrow", newPointa, newPoint2);

            return true;
        }

        public static bool Castlivingplantsaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget == null || !pTarget.isActor())
            {
                return false;
            }

            pTile = pTarget.current_tile;
            if (pTile == null)
            {
                return false;
            }

            int livingPlantsCount = 0;
            foreach (Actor actor in Finder.findSpeciesAroundTileChunk(pTile, "living_plants"))
            {
                if (actor == null || actor.a == null || !actor.a.isAlive())
                {
                    continue;
                }

                livingPlantsCount++;
                if (livingPlantsCount > 20)
                {
                    return false;
                }
            }

            MapRegion region = pTile.region;
            WorldTile worldTile = region != null ? region.tiles.GetRandom<WorldTile>() : null;
            if (worldTile == null)
            {
                return false;
            }

            ActionLibrary.tryToGrowTree(null, worldTile);
            World.world.drop_manager.spawn(worldTile, "living_plants", 1f, -1f);
            return true;
        }

        public static bool Explosionshieldaction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pSelf == null || !pSelf.a.isAlive()) return false;
            // if (pSelf.current_tile == null) return false;
            pTile = pSelf.current_tile;
            pSelf.a.finishAllStatusEffects();
            float randomChance = UnityEngine.Random.Range(0f, 1f);
            if (randomChance < 0.3f)
            {
                CastFrostShield(pSelf, pTile);
            }
            if (randomChance < 0.6f && randomChance > 0.3f)
            {
                CastFireShield(pSelf, pTile);
            }
            if (randomChance < 1.0f && randomChance > 0.6f)
            {
                CastBifrostShield(pSelf, pTile);
            }
            return true;
        }

        public static bool MorphintoBjorn(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            Actor pSelfActor = pSelf as Actor;
            Actor pTargetActor = pTarget as Actor;
            if (pSelfActor != null)
            {
                pSelfActor.removeTrait("Atreus");
                morphInto(pSelfActor, "bear", "fx_fireball_explosion", "Bjorn", "Bjorn", 0.3f);
            }
            if (pSelfActor != null && pSelfActor.asset.id == "bear")
            {
                pSelfActor.removeTrait("Bjorn");
                pSelfActor.removeTrait("freeze_proof");
                pSelfActor.data.get("oldself", out string oldself, "bear");
                morphInto(pSelfActor, oldself, "fx_fireball_explosion", "Atreus", "Atreus", 0.3f);
            }

            return true;
        }

        public static bool CastDraupnirRain(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            Actor pSelfActor = pSelf as Actor;
            Actor pTargetActor = pTarget as Actor;
            pTile = pTarget.current_tile;
            if (pTargetActor == null) return false;
            Vector3 tPos = pSelfActor.cur_transform_position;
            Vector2 tHeadPos = pSelfActor.getRenderedItemPosition();
            tPos.y += tHeadPos.y * 9f * pSelfActor.current_scale.y + 0.7f;
            EffectsLibrary.spawnAt("fx_draupnir_cast", tPos, pSelfActor.actor_scale);
            GOWB.GOWCommands.instance.StartCoroutine(DraupnirRainAction(pTile));
            return true;
        }

        public static IEnumerator DraupnirRainAction(WorldTile pTile)
        {
            if (pTile != null)
            {
                WorldTile tSourceTile = pTile;
                MapRegion region = tSourceTile.region;
                for (int i = 0; i < 100; i++)
                {
                    WorldTile areaTile = ((region != null) ? region.tiles.GetRandom<WorldTile>() : null);
                    if (areaTile != null)
                    {
                        World.world.drop_manager.spawn(areaTile, "Draupdrop", 10f, -1f, -1L);
                    }
                    yield return new WaitForSeconds(0.1f); 
                }
            }
        }

        public static bool CastFrostShield(BaseSimObject pSelf, WorldTile pTile = null)
        {
            // Actor a = pSelf.a;
            // if (pSelf.a.hasStatus("Bifrost Shield") || pSelf.a.hasStatus("Fire Shield") || pSelf.a.hasStatus("Frost Shield"))
            // {
            //     // return false;
            // }
            pSelf.addStatusEffect("Frost Shield", 40f);
            pSelf.addStatusEffect("invincible", 1f);

            MusicBox.playSound("event:/SFX/UNIQUE/SpawnCloud", pSelf.current_tile, false, false);
            Vector3 tPos = pSelf.a.cur_transform_position;
            Vector2 tHeadPos = pSelf.a.getRenderedItemPosition();
            tPos.y += tHeadPos.y - 8f;
            EffectsLibrary.spawnAt("ice_Explosion_Effect", tPos, 0.3f);

            for (int i = 0; i < 80; ++i)
            {
                World.world.loopWithBrush(pSelf.current_tile, Brush.get(11, "circ_"), new PowerActionWithID(PowerLibrary.drawTemperatureMinus), "temperatureMinus");
            }

            EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
            World.world.applyForceOnTile(pTile: pSelf.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);


            MapBox.instance.drop_manager.spawn(pSelf.current_tile, "blood_rain", 15f, -1f);

            return true;
        }

        public static bool CastFireShield(BaseSimObject pSelf, WorldTile pTile = null)
        {
            // if (pSelf.a.hasStatus("Bifrost Shield") || pSelf.a.hasStatus("Frost Shield") || pSelf.a.hasStatus("Fire Shield"))
            // {
            //     // return false;
            // }
            pSelf.addStatusEffect("Fire Shield", 40f);
            pSelf.addStatusEffect("invincible", 1f);

            EffectsLibrary.spawnAtTile("fx_napalm_flash", pTile, 0.5f);
            EffectsLibrary.spawnAtTileRandomScale("fx_explosion_tiny", pTile, 0.15f, 0.3f);

            EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
            World.world.applyForceOnTile(pTile: pSelf.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);

            MapBox.instance.drop_manager.spawn(pTile, "blood_rain", 15f, -1f);

            return true;
        }

        public static bool CastBifrostShield(BaseSimObject pSelf, WorldTile pTile = null)
        {
            // Actor a = pSelf.a;
            // if (pSelf.a.hasStatus("Bifrost Shield") || pSelf.a.hasStatus("Frost Shield") || pSelf.a.hasStatus("Fire Shield"))
            // {
            //     // return false;
            // }
            pSelf.addStatusEffect("Bifrost Shield", 40f);
            pSelf.addStatusEffect("invincible", 1f);

            EffectsLibrary.spawnAtTile("fx_antimatter_effect", pTile, 0.3f);

            EffectsLibrary.spawnExplosionWave(pTile.posV3, 3f, 0.5f);
            World.world.applyForceOnTile(pTile: pSelf.current_tile, 10, 1.5f, true, 10, null, pByWho: pSelf);

            MapBox.instance.drop_manager.spawn(pTile, "blood_rain", 15f, -1f);

            return true;
        }

        //Uppercut knockback logic
        public static bool GodlyUppercut(Actor pSelfActor, Actor pTargetActor, Vector2 pSelfPosition, WorldTile pTile = null)
        {
            pSelfActor.punchTargetAnimation(pSelfPosition, true, false, 45f);
            pSelfActor.setActionTimeout(1f);
            pSelfActor.spawnSlashKick(pSelfPosition);
            pSelfActor.lookTowardsPosition(pSelfPosition);
            if (pTargetActor == null)
            {
                return false;
            }
            pTargetActor.makeStunned(5f);
            Vector2 tStartPos = pSelfActor.current_position;
            tStartPos.y += pSelfActor.current_scale.y;
            Vector2 tDirection = pTargetActor.current_position;
            float tForceDirection = Randy.randomFloat(3f, 4f);
            float tForceHeight = Randy.randomFloat(3f, 4f);
            if (pSelfActor.under_forces || pSelfActor.hasStatus("dash"))
            {
                tForceDirection *= 1.2f;
                tForceHeight *= 1.5f;
            }
            pTargetActor.calculateForce(tDirection.x, tDirection.y, tStartPos.x, tStartPos.y, tForceDirection, tForceHeight, true);
            pTargetActor.addAggro(pSelfActor);
            return true;
        }

        //original code by MasonScarbro
        public static bool morphInto(Actor pActor, string morphid, string morphEffect, string newName, string traitToAdd, float morphEffectSize = 0f, bool dontKnockOut = true, bool dontCoolDown = true)
        {
            if (pActor == null)
            {
                return false;
            }
            if (pActor.asset.id == morphid)
            {
                return false;
            }

            var pMorph = World.world.units.createNewUnit(morphid, pActor.current_tile);
            if (pMorph == null) return false;
            if (pActor.kingdom != null && pActor.kingdom.isAlive())
            {
                // pMorph.data.saved_traits.Clear();
                pMorph.kingdom = pActor.kingdom;
                if (pActor.hasCity())
                {
                    pMorph.setCity(pActor.city);
                }
                if (pActor.hasClan())
                {
                    pMorph.clan = pActor.clan;
                }
                if (pActor.hasHouse())
                {
                    pMorph.setHomeBuilding(pActor._home_building);
                }
                pMorph.data.level = pActor.data.level;
                pActor.data.custom_data_bool = pMorph.data.custom_data_bool;
                pActor.data.custom_data_float = pMorph.data.custom_data_float;
                pActor.data.custom_data_int = pMorph.data.custom_data_int;
                pActor.data.custom_data_string = pMorph.data.custom_data_string;
                pActor.data.custom_data_flags = pMorph.data.custom_data_flags;
                pMorph.data.set("morphedinto", morphid);
                pMorph.data.set("oldself", pActor.asset.id);
                copyUnitToNewUnit(pActor, pMorph);

                pMorph.data.name = newName;
                pMorph.addTrait(traitToAdd);
                EffectsLibrary.spawnAtTile(morphEffect, pMorph.current_tile, morphEffectSize);
                if (!dontKnockOut)
                {
                    pMorph.addStatusEffect("stunned", 5f);

                }
                if (!dontCoolDown)
                {
                    pMorph.addStatusEffect("recovery_spell", 30f);

                }

                Actor possessedActor = ControllableUnit._unit_main;
                if (possessedActor != null && pActor == possessedActor)
                {
                    ControllableUnit.setControllableCreature(pMorph);
                    ActionLibrary.removeUnit(pActor);
                }
                else
                {
                    ActionLibrary.removeUnit(pActor);
                }
            }
            return true;
        }

        public static void copyUnitToNewUnit(Actor pParent, Actor pCloneTarget, bool pCopyAge = true)
		{
			pCloneTarget.current_position = pParent.current_position;
			pCloneTarget.current_rotation = pParent.current_rotation;
			copySpecialData(pParent.data, pCloneTarget.data, pCopyAge);
			pCloneTarget.takeItems(pParent, 1f, 0);
			foreach (ActorTrait tTrait in pParent.getTraits())
			{
				pCloneTarget.addTrait(tTrait, false);
			}
			pCloneTarget.setStatsDirty();
			if (MoveCamera.inSpectatorMode() && pParent.isCameraFollowingUnit())
			{
				MoveCamera.setFocusUnit(pCloneTarget);
			}
		}

        public static void copySpecialData(ActorData pFrom, ActorData pCloneTo, bool pCopyAge)
		{
			pCloneTo.name = pFrom.name;
			pCloneTo.custom_name = pFrom.custom_name;
			if (pCopyAge)
			{
				pCloneTo.created_time = pFrom.created_time;
				pCloneTo.age_overgrowth = pFrom.age_overgrowth;
			}
			pCloneTo.asset_id = pFrom.asset_id;
			pCloneTo.kills = pFrom.kills;
			pCloneTo.births = pFrom.births;
			pCloneTo.favorite = pFrom.favorite;
			pCloneTo.food_consumed = pFrom.food_consumed;
			pCloneTo.favorite_food = pFrom.favorite_food;
			pCloneTo.head = pFrom.head;
			pCloneTo.generation = pFrom.generation;
			pCloneTo.parent_id_1 = pFrom.parent_id_1;
			pCloneTo.parent_id_2 = pFrom.parent_id_2;
			pCloneTo.ancestor_family = pFrom.ancestor_family;
			pCloneTo.best_friend_id = pFrom.best_friend_id;
			pCloneTo.lover = pFrom.lover;
			pCloneTo.experience = pFrom.experience;
			pCloneTo.renown = pFrom.renown;
			pCloneTo.loot = pFrom.loot;
			pCloneTo.money = pFrom.money;
			pCloneTo.level = pFrom.level;
			pCloneTo.sex = pFrom.sex;
            if (pFrom.phenotype_shade != null)
			{
				pCloneTo.phenotype_shade = pFrom.phenotype_shade;
			}
            // if (pFrom.phenotype_index != null)
			// {
			// 	pCloneTo.phenotype_index = pFrom.phenotype_index;
			// }
			pCloneTo["diplomacy"] = pFrom["diplomacy"];
			pCloneTo["intelligence"] = pFrom["intelligence"];
			pCloneTo["stewardship"] = pFrom["stewardship"];
			pCloneTo["warfare"] = pFrom["warfare"];
		}
    }
}