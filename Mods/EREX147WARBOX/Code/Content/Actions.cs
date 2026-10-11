using System;
using UnityEngine;

namespace WarBox.Content;

internal static class WarBoxActions
{
    public static void ToggleFactories(bool pCurrentValue)
    {
        WarBox.warbox_factories = pCurrentValue;
        WarBox.LogInfo("Factories toggled to " + pCurrentValue);
    }

    public static void ToggleForceCapture(bool pCurrentValue)
    {
        WarBox.warbox_force_capture = pCurrentValue;
        WarBox.LogInfo("Force Capture toggled to " + pCurrentValue);
    }

    public static void ToggleArmyLimit(bool pCurrentValue)
    {
        WarBox.warbox_army_limits = pCurrentValue;
        WarBox.LogInfo("Army Limits toggled to " + pCurrentValue);
    }

    public static void UpdateArmyPerPop(int pCurrentValue)
    {
        WarBox.warbox_army_per_pop = pCurrentValue;
        WarBox.LogInfo("Army Limits toggled to " + pCurrentValue);
    }

    public static bool LaunchATGM(Actor pSelf, Vector2 pTarget, WorldTile pTile = null)
    {
        Vector2Int pos = pTile.pos;
        Vector3 vector = pSelf.current_position;
        float pDist = Vector2.Distance(vector, pos);
        Vector3 newPoint = Toolbox.getNewPoint(vector.x, vector.y, pos.x, pos.y, pDist);
        Vector3 newPoint2 = Toolbox.getNewPoint(vector.x, vector.y, pos.x, pos.y, pSelf.a.stats["size"]);
        newPoint2.y += 0.5f;
        World.world.projectiles.spawn(pSelf, null, "rpg_projectile", newPoint2, newPoint);
        return true;
    }
}