using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.Sync;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>
    /// Cutting trees, harvesting bushes and mining with the possessed unit's real swings (refined from
    /// MainCharacterMod's ResourceMining). The rules live in Data/gathering.json. Tools make it faster
    /// rather than being required: WorldBox has no pickaxe items, so the old "needs a pickaxe" rule
    /// made mining impossible.
    /// </summary>
    internal static class Gathering
    {
        private struct Progress
        {
            public int Hits;
            public float LastHit;
        }

        private static readonly Dictionary<long, Progress> _progress = new Dictionary<long, Progress>();
        private static readonly List<long> _expired = new List<long>();

        /// <summary>Called once per left-click swing of the possessed unit.</summary>
        public static void OnPossessedSwing(Actor pActor, Inventory pInv)
        {
            if (!PLConfig.EnableGathering || pActor == null || pInv == null || pActor.current_tile == null) return;
            if (World.world == null || World.world.isOverUI()) return;

            Building building = FindTarget(World.world.getMouseTilePos());
            if (building == null) return;
            GatherRule rule = DataTables.GetGatherRule(building.asset);
            if (rule == null || !building.hasResourcesToCollect()) return;
            if (rule.RequiresSapient && !pActor.isSapient()) return;

            float reach = pActor.getAttackRange() + DataTables.Gathering.ReachBonus;
            if (building.current_tile == null || pActor.current_tile.distanceTo(building.current_tile) > reach) return;

            ExpireOld();
            long id = building.data.id;
            _progress.TryGetValue(id, out Progress progress);
            progress.Hits++;
            progress.LastHit = Time.unscaledTime;

            int needed = HasTool(pActor, rule) ? Mathf.Max(1, rule.HitsWithTool) : Mathf.Max(1, rule.Hits);
            building.startShake(0.3f, 0.1f, 0.1f);
            if (!string.IsNullOrEmpty(rule.HitSound)) PlaySound(rule.HitSound, building);

            if (progress.Hits < needed)
            {
                _progress[id] = progress;
                return;
            }
            _progress.Remove(id);
            Harvest(pActor, pInv, building, rule);
        }

        private static void Harvest(Actor pActor, Inventory pInv, Building pBuilding, GatherRule pRule)
        {
            BuildingAsset asset = pBuilding.asset;
            pBuilding.extractResources(pActor);
            if (asset.resources_given != null)
            {
                foreach (ResourceContainer container in asset.resources_given)
                {
                    int amount = container.amount;
                    if (amount <= 0) continue;
                    // same bonus the game gives in BehExtractResourcesFromBuilding
                    if (asset.building_type == BuildingType.Building_Mineral && pActor.hasTrait("miner") && Randy.randomBool()) amount++;
                    InventoryOps.AddResource(pActor, pInv, container.id, amount);
                    ResourceAsset res = AssetManager.resources.get(container.id);
                    Toasts.Show("+" + amount + " " + (res != null ? res.getTranslatedName() : container.id));
                    PossessLibEvents.RaiseResourceGathered(pActor, pBuilding, container.id, amount);
                }
            }
            if (pRule.Experience > 0) pActor.addExperience(pRule.Experience);
            if (!string.IsNullOrEmpty(pRule.FinishSound)) PlaySound(pRule.FinishSound, pBuilding);
        }

        /// <summary>The building on the tile, or on a neighbouring tile (forgiving aim, as in the old mod).</summary>
        private static Building FindTarget(WorldTile pTile)
        {
            if (pTile == null) return null;
            if (IsCandidate(pTile.building)) return pTile.building;
            WorldTile[] neighbours = pTile.neighboursAll;
            if (neighbours == null) return null;
            for (int i = 0; i < neighbours.Length; i++)
            {
                WorldTile n = neighbours[i];
                if (n != null && IsCandidate(n.building)) return n.building;
            }
            return null;
        }

        private static bool IsCandidate(Building pBuilding)
        {
            return pBuilding != null && pBuilding.isAlive() && pBuilding.asset != null
                   && DataTables.GetGatherRule(pBuilding.asset) != null && pBuilding.hasResourcesToCollect();
        }

        private static bool HasTool(Actor pActor, GatherRule pRule)
        {
            if (pRule.ToolGroups == null || pRule.ToolGroups.Count == 0 || !pActor.hasEquipment()) return false;
            Item weapon = pActor.equipment.weapon.getItem();
            EquipmentAsset asset = weapon != null ? weapon.getAsset() : null;
            return asset != null && pRule.ToolGroups.Contains(asset.group_id);
        }

        private static void ExpireOld()
        {
            float timeout = DataTables.Gathering.ProgressTimeout;
            float now = Time.unscaledTime;
            _expired.Clear();
            foreach (KeyValuePair<long, Progress> pair in _progress)
            {
                if (now - pair.Value.LastHit > timeout) _expired.Add(pair.Key);
            }
            for (int i = 0; i < _expired.Count; i++) _progress.Remove(_expired[i]);
        }

        private static void PlaySound(string pEvent, Building pBuilding)
        {
            try { MusicBox.playSound(pEvent, pBuilding.current_tile, false, false); }
            catch (System.Exception) { /* cosmetic */ }
        }
    }
}
