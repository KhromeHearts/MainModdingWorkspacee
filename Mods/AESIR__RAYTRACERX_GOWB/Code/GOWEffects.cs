using UnityEngine;

namespace GOWB
{
    internal static class GOWEffects
    {
        public static void Init()
        {
            if (AssetManager.effects_library.get("fx_odin_cast_top") == null)
            {
                EffectAsset fxOdinCastTop = new EffectAsset();
                fxOdinCastTop.id = "fx_odin_cast_top";
                fxOdinCastTop.use_basic_prefab = true;
                fxOdinCastTop.sorting_layer_id = "EffectsTop";
                fxOdinCastTop.sprite_path = "effects/fx_odin_cast_top";
                fxOdinCastTop.time_between_frames = 0.2f;
                fxOdinCastTop.draw_light_area = true;
                fxOdinCastTop.draw_light_size = 0.25f;
                AssetManager.effects_library.add(fxOdinCastTop);
            }

            if (AssetManager.effects_library.get("fx_odin_cast_ground") == null)
            {
                EffectAsset fxOdinCastGround = new EffectAsset();
                fxOdinCastGround.id = "fx_odin_cast_ground";
                fxOdinCastGround.use_basic_prefab = true;
                fxOdinCastGround.sorting_layer_id = "EffectsBack";
                fxOdinCastGround.sprite_path = "effects/fx_odin_cast_ground";
                fxOdinCastGround.time_between_frames = 0.2f;
                fxOdinCastGround.draw_light_area = true;
                fxOdinCastGround.draw_light_size = 0.25f;
                AssetManager.effects_library.add(fxOdinCastGround);
            }

            if (AssetManager.effects_library.get("fx_odin_teleport") == null)
            {
                EffectAsset fxOdinTeleport = new EffectAsset();
                fxOdinTeleport.id = "fx_odin_teleport";
                fxOdinTeleport.use_basic_prefab = true;
                fxOdinTeleport.sorting_layer_id = "EffectsTop";
                fxOdinTeleport.sprite_path = "effects/fx_odin_teleport";
                fxOdinTeleport.time_between_frames = 0.08f;
                AssetManager.effects_library.add(fxOdinTeleport);
            }

            if (AssetManager.effects_library.get("fx_draupnir_cast") == null)
            {
                EffectAsset fxDraupnirCast = new EffectAsset();
                fxDraupnirCast.id = "fx_draupnir_cast";
                fxDraupnirCast.use_basic_prefab = true;
                fxDraupnirCast.sorting_layer_id = "EffectsTop";
                fxDraupnirCast.sprite_path = "effects/fx_draupnir_cast";
                fxDraupnirCast.time_between_frames = 0.2f;
                fxDraupnirCast.draw_light_area = true;
                fxDraupnirCast.draw_light_size = 0.45f;
                AssetManager.effects_library.add(fxDraupnirCast);
            }

            if (AssetManager.effects_library.get("ice_Explosion_Effect") == null)
            {
                EffectAsset iceExplosionEffect = new EffectAsset();
                iceExplosionEffect.id = "ice_Explosion_Effect";
                iceExplosionEffect.use_basic_prefab = true;
                iceExplosionEffect.show_on_mini_map = true;
                iceExplosionEffect.sorting_layer_id = "EffectsTop";
                iceExplosionEffect.sprite_path = "effects/iceExplosionEffect";
                iceExplosionEffect.time_between_frames = 0.03f;
                iceExplosionEffect.draw_light_area = true;
                iceExplosionEffect.draw_light_size = 1.15f;
                iceExplosionEffect.sound_launch = "event:/SFX/EXPLOSIONS/ExplosionMiddle";
                AssetManager.effects_library.add(iceExplosionEffect);
            }

            if (AssetManager.drops.get("Draupdrop") == null)
            {
                DropAsset draupdrop = new DropAsset();
                draupdrop.id = "Draupdrop";
                draupdrop.path_texture = "drops/DraupDrop";
                draupdrop.default_scale = 0.1f;
                draupdrop.falling_height = new Vector2(30f, 45f);
                draupdrop.action_landed = new DropsAction(action_draupdrop);
                draupdrop.sound_drop = "event:/SFX/DROPS/DropAcid";
                draupdrop.type = DropType.DropGeneric;
                draupdrop.material = "mat_world_object_lit";
                draupdrop.surprises_units = false;
                AssetManager.drops.add(draupdrop);
            }
        }

        public static void action_draupdrop(WorldTile pTile = null, string pDropID = null)
        {
            if (pTile == null)
            {
                return;
            }

            EffectsLibrary.spawnAtTileRandomScale("fx_spawn", pTile, 0.1f, 0.15f);

            if (pTile.building != null && pTile.building.isAlive())
            {
                pTile.building.getHit(20f, true, AttackType.Other, null, true, false);
            }

            foreach (Actor actor in Finder.getUnitsFromChunk(pTile, 1, 3f, true))
            {
                if (actor == null || !actor.isAlive())
                {
                    continue;
                }

                if (!Randy.randomChance(0.6f) && !actor.hasTrait("Kratos") && !actor.hasStatus("No friendly fire"))
                {
                    actor.getHit(20f, true, AttackType.Other, null, true, false);
                }
            }

            World.world.conway_layer.checkKillRange(pTile.pos, 2);
        }
    }
}
