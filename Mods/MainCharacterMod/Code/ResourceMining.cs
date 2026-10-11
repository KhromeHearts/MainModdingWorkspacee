using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MC
{
    public class ResourceMining : MonoBehaviour
    {
        public enum ToolKind
        {
            None,
            Axe,
            Hammer,
            Pickaxe
        }

        private const float GatherCooldown = 0.45f;
        private float nextGatherTime;

        private void Update()
        {
            Actor actor = ControllableUnit.getControllableUnit();
            if (actor == null || !actor.isAlive() || !Input.GetMouseButton(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (Time.time < nextGatherTime) return;

            WorldTile tile = MapBox.instance == null ? null : MapBox.instance.getMouseTilePos();
            Building building = GetTargetBuilding(tile, actor);
            if (building == null) return;

            ToolKind requiredTool = GetRequiredTool(building);
            if (requiredTool != ToolKind.None && !HasRequiredTool(actor, requiredTool)) return;
            if (!building.hasResourcesToCollect()) return;
            if (!IsInGatherRange(actor, building)) return;

            if (Gather(actor, building))
            {
                nextGatherTime = Time.time + GatherCooldown;
                actor.startAttackCooldown();
                actor.punchTargetAnimation(building.current_position, true, actor.hasRangeAttack(), 40f);
            }
        }

        public static Building GetTargetBuilding(WorldTile tile, Actor actor)
        {
            if (tile == null || actor == null || actor.current_tile == null) return null;

            Building building = tile.building;
            if (building == null && tile.neighboursAll != null)
            {
                foreach (WorldTile neighbour in tile.neighboursAll)
                {
                    if (neighbour != null && neighbour.building != null)
                    {
                        building = neighbour.building;
                        break;
                    }
                }
            }

            return building;
        }

        public static bool IsInGatherRange(Actor actor, Building building)
        {
            if (actor == null || building == null || actor.current_tile == null || building.current_tile == null)
                return false;

            return actor.current_tile.distanceTo(building.current_tile) <= actor.getAttackRange() + 2f;
        }

        public static ToolKind GetRequiredTool(Building building)
        {
            if (building == null || building.asset == null) return ToolKind.None;

            if (building.asset.building_type == BuildingType.Building_Tree)
                return ToolKind.Axe;

            if (building.asset.building_type == BuildingType.Building_Mineral)
                return ToolKind.Pickaxe;

            return ToolKind.None;
        }

        public static bool HasRequiredTool(Actor actor, ToolKind requiredTool)
        {
            if (requiredTool == ToolKind.None) return true;
            if (actor == null || actor.equipment == null) return false;

            ActorEquipmentSlot weaponSlot = actor.equipment.getSlot(EquipmentType.Weapon);
            Item weapon = weaponSlot == null ? null : weaponSlot.getItem();
            if (weapon == null || weapon.getAsset() == null) return false;

            ItemAsset asset = weapon.getAsset();
            string itemId = string.IsNullOrEmpty(asset.id) ? string.Empty : asset.id.ToLowerInvariant();
            string groupId = string.IsNullOrEmpty(asset.group_id) ? string.Empty : asset.group_id.ToLowerInvariant();

            if (requiredTool == ToolKind.Axe)
            {
                return (itemId.Contains("axe") && !itemId.Contains("pickaxe")) || groupId == "axe";
            }

            if (requiredTool == ToolKind.Pickaxe)
            {
                return itemId.Contains("pickaxe") || groupId == "pickaxe";
            }

            if (requiredTool == ToolKind.Hammer)
            {
                return itemId.Contains("hammer") || groupId == "hammer";
            }

            return false;
        }

        private bool Gather(Actor actor, Building building)
        {
            if (building.asset.resources_given == null) return false;

            foreach (ResourceContainer resource in building.asset.resources_given)
            {
                if (resource.amount <= 0) continue;
                actor.addToInventory(resource.id, resource.amount);
            }

            building.extractResources(actor);
            return true;
        }
    }
}
