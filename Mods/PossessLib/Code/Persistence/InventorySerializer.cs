using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.Sync;

namespace PossessLib.Persistence
{
    /// <summary>
    /// Versioned JSON format for one inventory (stored in ActorData custom data, see DESIGN.md section 5).
    /// Keep field names short: this string lives inside every save that uses PossessLib.
    /// </summary>
    internal sealed class InventoryDto
    {
        public const int CurrentVersion = 1;

        [JsonProperty("v")] public int Version = CurrentVersion;
        [JsonProperty("h")] public int Hotbar;
        [JsonProperty("m")] public int Main;
        [JsonProperty("sel")] public int Selected;
        [JsonProperty("s")] public List<StackDto> Slots = new List<StackDto>();
        [JsonProperty("c", NullValueHandling = NullValueHandling.Ignore)] public StackDto Cursor;
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)] public StackDto Trash;
        [JsonProperty("eq", NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, StackDto> Equipment;
    }

    /// <summary>One stack. "k" is "item", "res" or "custom"; unknown kinds are preserved as-is.</summary>
    internal sealed class StackDto
    {
        [JsonProperty("i", DefaultValueHandling = DefaultValueHandling.Include)] public int Index = -1;
        [JsonProperty("k")] public string Kind;
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)] public string Id;
        [JsonProperty("n", DefaultValueHandling = DefaultValueHandling.Ignore)] public int Count;
        [JsonProperty("item", DefaultValueHandling = DefaultValueHandling.Ignore)] public long ItemId;
        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, string> Data;
    }

    internal static class InventorySerializer
    {
        public const string ActorKey = "possesslib.inventory";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Formatting = Formatting.None
        };

        // ------------------------------------------------------------------ write

        public static string Serialize(Inventory pInv)
        {
            InventoryDto dto = new InventoryDto
            {
                Hotbar = pInv.HotbarSize,
                Main = pInv.MainSize,
                Selected = pInv.Selected
            };
            for (int i = 0; i < pInv.Capacity; i++)
            {
                StackDto s = ToDto(pInv.Get(i));
                if (s == null) continue;
                s.Index = i;
                dto.Slots.Add(s);
            }
            if (ReferenceEquals(CursorState.Origin, pInv)) dto.Cursor = ToDto(CursorState.Stack);
            dto.Trash = ToDto(pInv.Trash);
            if (pInv.CustomEquipmentCount > 0)
            {
                dto.Equipment = new Dictionary<string, StackDto>();
                foreach (KeyValuePair<string, ItemStack> pair in pInv.CustomEquipment)
                {
                    StackDto s = ToDto(pair.Value);
                    if (s != null) dto.Equipment[pair.Key] = s;
                }
            }
            return JsonConvert.SerializeObject(dto, Settings);
        }

        public static StackDto ToDto(ItemStack pStack)
        {
            if (pStack == null) return null;
            switch (pStack.Kind)
            {
                case StackKind.Item:
                    if (pStack.Item == null || !pStack.Item.isAlive()) return null;
                    return new StackDto { Kind = "item", ItemId = pStack.Item.data.id };
                case StackKind.Resource:
                    return new StackDto { Kind = "res", Id = pStack.Id, Count = pStack.Count };
                case StackKind.Custom:
                    return new StackDto { Kind = "custom", Id = pStack.Id, Count = pStack.Count, Data = pStack.Data };
                default:
                    // Unresolved: write the original back untouched.
                    try
                    {
                        return JsonConvert.DeserializeObject<StackDto>(pStack.UnresolvedJson, Settings);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
            }
        }

        // ------------------------------------------------------------------ read

        /// <summary>
        /// Rebuilds an inventory for <paramref name="pActor"/>. Items already claimed by another
        /// inventory (a clone copied the parent's custom data) are skipped: first claim wins.
        /// </summary>
        public static Inventory Deserialize(Actor pActor, string pJson, out StackDto pCursor)
        {
            pCursor = null;
            InventoryDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<InventoryDto>(pJson, Settings);
            }
            catch (Exception e)
            {
                PLLog.Warn("Bad inventory data on " + pActor.getName() + ": " + e.Message);
                return null;
            }
            if (dto == null) return null;
            if (dto.Version > InventoryDto.CurrentVersion)
            {
                PLLog.Warn("Inventory data on " + pActor.getName() + " was saved by a newer PossessLib (v" + dto.Version + "); loading what this version understands.");
            }

            int hotbar = dto.Hotbar > 0 ? dto.Hotbar : PLConfig.HotbarSlots;
            int main = dto.Main >= 0 ? dto.Main : PLConfig.MainSlots;
            Inventory inv = new Inventory(pActor, hotbar, main);
            List<ItemStack> homeless = new List<ItemStack>();

            if (dto.Slots != null)
            {
                foreach (StackDto s in dto.Slots)
                {
                    ItemStack stack = FromDto(s);
                    if (stack == null) continue;
                    if (s.Index >= 0 && s.Index < inv.Capacity && inv.Get(s.Index) == null) inv.SetSlot(s.Index, stack);
                    else homeless.Add(stack);
                }
            }
            foreach (ItemStack stack in homeless) inv.Insert(stack);

            ItemStack trash = FromDto(dto.Trash);
            if (trash != null) inv.SetTrash(trash);

            if (dto.Equipment != null)
            {
                foreach (KeyValuePair<string, StackDto> pair in dto.Equipment)
                {
                    ItemStack stack = FromDto(pair.Value);
                    if (stack != null) inv.SetCustomEquipment(pair.Key, stack);
                }
            }
            if (dto.Selected > 0) inv.Select(dto.Selected);

            // Apply the configured size (moves overflow to free slots; leftovers go to the cursor list).
            List<ItemStack> overflow = inv.Resize(PLConfig.HotbarSlots, PLConfig.MainSlots);
            if (overflow.Count > 0) PendingDrops.Add(inv, overflow);
            pCursor = dto.Cursor;
            return inv;
        }

        public static ItemStack FromDto(StackDto pDto)
        {
            if (pDto == null || string.IsNullOrEmpty(pDto.Kind)) return null;
            switch (pDto.Kind)
            {
                case "item":
                {
                    Item item = World.world.items.get(pDto.ItemId);
                    if (item == null || !item.isAlive() || item.getAsset() == null)
                    {
                        PLLog.Debug("Saved item " + pDto.ItemId + " no longer exists");
                        return null;
                    }
                    if (InventoryRegistry.IsProtected(item))
                    {
                        PLLog.Debug("Saved item " + pDto.ItemId + " is already claimed (clone?), skipping");
                        return null;
                    }
                    return ItemStack.FromItem(item);
                }
                case "res":
                    if (pDto.Count <= 0 || AssetManager.resources.get(pDto.Id) == null) return null;
                    return ItemStack.FromResource(pDto.Id, pDto.Count);
                case "custom":
                    if (pDto.Count <= 0) return null;
                    if (CustomItemRegistry.Has(pDto.Id)) return ItemStack.FromCustom(pDto.Id, pDto.Count, pDto.Data);
                    return ItemStack.Unresolved(pDto.Id, pDto.Count, JsonConvert.SerializeObject(pDto, Settings));
                default:
                    return ItemStack.Unresolved(pDto.Id ?? pDto.Kind, Math.Max(1, pDto.Count), JsonConvert.SerializeObject(pDto, Settings));
            }
        }
    }

    /// <summary>Stacks that did not fit after loading (smaller config); dropped at the owner's feet on the next tick.</summary>
    internal static class PendingDrops
    {
        private static readonly List<KeyValuePair<Inventory, List<ItemStack>>> _pending = new List<KeyValuePair<Inventory, List<ItemStack>>>();

        public static void Add(Inventory pInv, List<ItemStack> pStacks)
        {
            _pending.Add(new KeyValuePair<Inventory, List<ItemStack>>(pInv, pStacks));
        }

        public static void Flush()
        {
            if (_pending.Count == 0) return;
            foreach (KeyValuePair<Inventory, List<ItemStack>> pair in _pending)
            {
                Actor owner = pair.Key.Owner;
                if (owner == null) continue;
                foreach (ItemStack stack in pair.Value)
                {
                    // Resources stay in the bag (shown as overflow until there is room); the rest is dropped.
                    if (stack.Kind == StackKind.Resource) continue;
                    InventoryOps.Drop(stack, pair.Key, owner.current_position);
                }
            }
            _pending.Clear();
        }
    }
}
