using UnityEngine;

namespace GOWB
{
    internal static class GOWProjectiles
    {
        public static void Init()
        {
            if (AssetManager.projectiles.get("FriggProjectile") == null)
            {
                ProjectileAsset friggProjectile = new ProjectileAsset();
                friggProjectile.id = "FriggProjectile";
                friggProjectile.texture = "FriggProjectile";
                friggProjectile.look_at_target = true;
                friggProjectile.speed = 30f;
                friggProjectile.draw_light_area = true;
                friggProjectile.draw_light_size = 0.1f;
                friggProjectile.scale_start = 0.04f;
                friggProjectile.scale_target = 0.04f;
                AssetManager.projectiles.add(friggProjectile);
            }

            if (AssetManager.projectiles.get("TalonArrow") == null)
            {
                ProjectileAsset talonArrow = new ProjectileAsset();
                talonArrow.id = "TalonArrow";
                talonArrow.texture = "TalonArrow";
                talonArrow.look_at_target = true;
                talonArrow.speed = 25f;
                talonArrow.draw_light_area = true;
                talonArrow.draw_light_size = 0.1f;
                talonArrow.scale_start = 0.1f;
                talonArrow.scale_target = 0.1f;
                AssetManager.projectiles.add(talonArrow);
            }

            if (AssetManager.projectiles.get("MagicFlameBall") == null)
            {
                ProjectileAsset magicFlameBall = new ProjectileAsset();
                magicFlameBall.id = "MagicFlameBall";
                magicFlameBall.texture = "fireball";
                magicFlameBall.trigger_on_collision = true;
                magicFlameBall.terraform_option = "Flameball impact";
                magicFlameBall.trail_effect_enabled = false;
                magicFlameBall.look_at_target = true;
                magicFlameBall.end_effect = "fx_explosion_small";
                magicFlameBall.terraform_range = 4;
                magicFlameBall.speed = 15f;
                magicFlameBall.draw_light_area = true;
                magicFlameBall.draw_light_size = 0.25f;
                magicFlameBall.scale_start = 0.22f;
                magicFlameBall.scale_target = 0.22f;
                AssetManager.projectiles.add(magicFlameBall);
            }

            if (AssetManager.projectiles.get("RavenSwarm") == null)
            {
                ProjectileAsset ravenSwarm = new ProjectileAsset();
                ravenSwarm.id = "RavenSwarm";
                ravenSwarm.texture = "RavenSwarm";
                ravenSwarm.can_be_collided = true;
                ravenSwarm.terraform_option = "Drifa curse";
                ravenSwarm.look_at_target = true;
                // ravenSwarm.end_effect = "fx_fire_smoke";
                ravenSwarm.terraform_range = 2;
                ravenSwarm.speed = 20f;
                ravenSwarm.scale_start = 0.1f;
                ravenSwarm.scale_target = 0.1f;
                ravenSwarm.mass = 1f;
                ravenSwarm.impact_actions = new AttackAction(IA_RavenSwarm);
                AssetManager.projectiles.add(ravenSwarm);
            }

            if (AssetManager.projectiles.get("DraupClone") == null)
            {
                ProjectileAsset draupClone = new ProjectileAsset();
                draupClone.id = "DraupClone";
                draupClone.texture = "DraupClone";
                draupClone.look_at_target = true;
                draupClone.texture_shadow = "shadows/projectiles/shadow_arrow";
                draupClone.speed = 20f;
                draupClone.scale_start = 0.1f;
                draupClone.scale_target = 0.1f;
                draupClone.impact_actions = new AttackAction(IA_DraupClone);
                AssetManager.projectiles.add(draupClone);
            }

            if (AssetManager.projectiles.get("LeviathanAxeThrow") == null)
            {
                ProjectileAsset leviathanAxeThrow = new ProjectileAsset();
                leviathanAxeThrow.id = "LeviathanAxeThrow";
                leviathanAxeThrow.texture = "LeviathanAxeThrow";
                leviathanAxeThrow.look_at_target = true;
                leviathanAxeThrow.texture_shadow = "shadows/projectiles/shadow_arrow";
                leviathanAxeThrow.speed = 20f;
                leviathanAxeThrow.scale_start = 0.1f;
                leviathanAxeThrow.scale_target = 0.1f;
                leviathanAxeThrow.impact_actions = new AttackAction(IA_LeviathanAxeThrow);
                AssetManager.projectiles.add(leviathanAxeThrow);
            }
        }

        public static bool IA_LeviathanAxeThrow(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTile == null)
            {
                return false;
            }

            for (int i = 0; i < 30; ++i)
            {
                World.world.loopWithBrush(pTile, Brush.get(2, "circ_"), new PowerActionWithID(PowerLibrary.drawTemperatureMinus), "temperatureMinus");
            }

            World.world.applyForceOnTile(pTile, 2, 0.1f, true, 0, null, pTarget);
            World.world.drop_manager.spawnParabolicDrop(pTile, "snow", 0f, 0.1f, 3.5f, 0.5f, 4f, Randy.randomFloat(0.025f, 0.2f));
            return true;
        }

        public static bool IA_DraupClone(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTile == null)
            {
                return false;
            }

            if (Randy.randomChance(0.3f))
            {
                EffectsLibrary.spawnAtTileRandomScale("fx_spawn", pTile, 0.1f, 0.15f);
                World.world.applyForceOnTile(pTile, 2, 0.01f, true, 0, null, pTarget);
            }

            return true;
        }

        public static bool IA_RavenSwarm(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTile == null)
            {
                return false;
            }
            EffectsLibrary.spawnAtTile("fx_fire_smoke", pTile, 0.3f);
            if (pTarget != null)
            {
                pTarget.a.addStatusEffect("ash_fever", 300f, true);
                // pTarget.a.addTrait("cursed");
            }

            return true;
        }
    }
}
