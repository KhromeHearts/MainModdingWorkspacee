using System;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;

namespace PossessLib.Persistence
{
    /// <summary>
    /// Writes PossessLib state into the game's own custom-data containers right before a save and
    /// rebuilds it while a save loads. No sidecar files: the data lives inside the world save it
    /// belongs to, so it cannot leak between worlds (DESIGN.md section 5).
    /// </summary>
    internal static class WorldPersistence
    {
        /// <summary>Called by the SavedMap.create prefix, before the game serializes actors and items.</summary>
        public static void BeforeSave()
        {
            try
            {
                InventoryRegistry.EnsureWorld();
                int written = 0;
                foreach (Inventory inv in InventoryRegistry.All)
                {
                    Actor owner = inv.Owner;
                    if (owner == null || owner.data == null) continue;
                    bool hasCursor = ReferenceEquals(CursorState.Origin, inv) && CursorState.Stack != null;
                    if (inv.IsEmpty && !hasCursor)
                    {
                        owner.data.removeString(InventorySerializer.ActorKey);
                        continue;
                    }
                    owner.data.set(InventorySerializer.ActorKey, InventorySerializer.Serialize(inv));
                    written++;
                }

                // Remove stale copies (clones inherit their parent's custom data; discarded inventories).
                int cleaned = 0;
                foreach (Actor actor in World.world.units)
                {
                    if (actor == null || actor.data == null || actor.data.custom_data_string == null) continue;
                    if (!actor.data.custom_data_string.TryGetValue(InventorySerializer.ActorKey, out string _)) continue;
                    if (InventoryRegistry.Has(actor.data.id)) continue;
                    actor.data.removeString(InventorySerializer.ActorKey);
                    cleaned++;
                }

                GroundItems.SaveToWorld();
                PLLog.Debug("Saved " + written + " inventories, cleaned " + cleaned + " stale keys, " + GroundItems.Count + " ground stacks");
            }
            catch (Exception e)
            {
                PLLog.Exception("Saving PossessLib data failed (the world itself still saves)", e);
            }
        }

        /// <summary>Called by the Actor.loadFromSave postfix: rebuild this unit's inventory if it has one.</summary>
        public static void AfterActorLoaded(Actor pActor)
        {
            try
            {
                InventoryRegistry.EnsureWorld();
                GroundItems.EnsureLoadedForWorld();
                if (pActor == null || pActor.data == null) return;
                pActor.data.get(InventorySerializer.ActorKey, out string json, null);
                if (string.IsNullOrEmpty(json)) return;

                Inventory inv = InventorySerializer.Deserialize(pActor, json, out StackDto cursorDto);
                if (inv == null) return;
                InventoryRegistry.Register(inv);

                ItemStack cursor = InventorySerializer.FromDto(cursorDto);
                if (cursor != null && inv.Insert(cursor) > 0 && cursor.Kind != StackKind.Resource)
                {
                    PendingDrops.Add(inv, new System.Collections.Generic.List<ItemStack> { cursor });
                }
                ActorSync.ReconcileResources(inv, pActor);
                PLLog.Debug("Loaded inventory of " + pActor.getName());
            }
            catch (Exception e)
            {
                PLLog.Exception("Loading the inventory of a unit failed (the unit is fine, its PossessLib slots are empty)", e);
            }
        }
    }
}
