using ai.behaviours;
using tools;
using UnityEngine;

namespace WarBox.Behaviour;

public class BehArtilleryAttack : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        Actor caster = pActor;
        using (var enemies = caster.kingdom.getEnemiesKingdoms())
        {
            foreach (var enemyKingdom in enemies)
            {
                if (enemyKingdom.hasKing() && enemyKingdom.cities.Count > 0)
                {
                    var targetCity = enemyKingdom.cities.GetRandom();
                    if (targetCity == null)
                        continue;

                    Vector2? attackPos = null;

                    if (attackPos == null && targetCity.hasArmy() && targetCity.army.countUnits() >= 15)
                        attackPos = targetCity.army.getRandomUnit().current_position;
                    if (attackPos == null && targetCity.buildings.Count > 0)
                    {
                        var building = targetCity.buildings.GetRandom();
                        if (building != null && building.current_tile != null)
                            attackPos = building.current_tile.pos;
                    }
                    if (attackPos == null && targetCity.hasLeader() && targetCity.leader.isAlive()) 
                        attackPos = targetCity.leader.current_position;
                    if (attackPos == null && enemyKingdom.hasKing() && enemyKingdom.king.isAlive()) 
                        attackPos = enemyKingdom.king.current_position;
                    if (attackPos == null)
                    {
                        WorldTile tile = targetCity.getTile();
                        if (tile != null) attackPos = tile.pos;
                    }

                    if (attackPos == null) continue;

                    Vector3 selfPos = caster.current_position;
                    float dist = Vector2.Distance(selfPos, attackPos.Value);
                    if (dist < 100f || dist > 225f)
                        continue;

                    Vector3 attackVector = Toolbox.getNewPoint(
                        selfPos.x, selfPos.y,
                        attackPos.Value.x, attackPos.Value.y,
                        dist
                    );

                    Vector3 startProjectile = Toolbox.getNewPoint(
                        selfPos.x, selfPos.y,
                        attackPos.Value.x, attackPos.Value.y,
                        caster.stats["size"]
                    );
                    startProjectile.y += 0.5f;

                    World.world.projectiles.spawn(caster, null, "cannon_shell", startProjectile, attackVector);
                    caster.punchTargetAnimation(attackVector, true, false, 45f);
                    caster.spendStamina(100);
                    return BehResult.Continue;
                }
            }
        }

        return BehResult.Stop;
    }
}
