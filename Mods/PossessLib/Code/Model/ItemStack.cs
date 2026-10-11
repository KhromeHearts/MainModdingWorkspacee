using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Core;
using UnityEngine;

namespace PossessLib.Model
{
    /// <summary>What an <see cref="ItemStack"/> holds.</summary>
    public enum StackKind
    {
        /// <summary>A WorldBox <see cref="global::Item"/> (weapon, armor, accessory). Never stacks.</summary>
        Item = 1,
        /// <summary>A WorldBox resource (food, minerals, gold...). The unit's ActorBag holds the totals.</summary>
        Resource = 2,
        /// <summary>A mod item registered through <see cref="PossessLibApi.RegisterCustomItem"/>.</summary>
        Custom = 3,
        /// <summary>A custom item whose mod is not loaded right now. Kept so it is not lost.</summary>
        Unresolved = 9
    }

    /// <summary>
    /// One inventory slot's content. Game items are referenced, never copied, so modifiers, quality,
    /// durability, kills and names are always the game's own values.
    /// </summary>
    public sealed class ItemStack
    {
        /// <summary>Kind of content.</summary>
        public StackKind Kind { get; private set; }

        /// <summary>The game item for <see cref="StackKind.Item"/>, null otherwise.</summary>
        public Item Item { get; private set; }

        /// <summary>Resource id, custom item id, or the EquipmentAsset id for items.</summary>
        public string Id { get; private set; }

        /// <summary>Amount in the stack (always 1 for items).</summary>
        public int Count { get; internal set; }

        /// <summary>Optional per-stack data for custom items (saved with the inventory). Null by default.</summary>
        public Dictionary<string, string> Data { get; internal set; }

        /// <summary>Raw saved JSON of an unresolved stack (written back unchanged).</summary>
        internal string UnresolvedJson;

        private ItemStack(StackKind pKind, Item pItem, string pId, int pCount, Dictionary<string, string> pData)
        {
            Kind = pKind;
            Item = pItem;
            Id = pId;
            Count = pCount;
            Data = pData;
        }

        /// <summary>Wraps a game item.</summary>
        public static ItemStack FromItem(Item pItem)
        {
            if (pItem == null) return null;
            return new ItemStack(StackKind.Item, pItem, pItem.data != null ? pItem.data.asset_id : null, 1, null);
        }

        /// <summary>Creates a resource stack.</summary>
        public static ItemStack FromResource(string pResourceId, int pCount)
        {
            if (string.IsNullOrEmpty(pResourceId) || pCount <= 0) return null;
            return new ItemStack(StackKind.Resource, null, pResourceId, pCount, null);
        }

        /// <summary>Creates a custom item stack.</summary>
        public static ItemStack FromCustom(string pCustomId, int pCount, Dictionary<string, string> pData = null)
        {
            if (string.IsNullOrEmpty(pCustomId) || pCount <= 0) return null;
            return new ItemStack(StackKind.Custom, null, pCustomId, pCount, pData);
        }

        internal static ItemStack Unresolved(string pId, int pCount, string pJson)
        {
            return new ItemStack(StackKind.Unresolved, null, pId, pCount, null) { UnresolvedJson = pJson };
        }

        /// <summary>True for game items.</summary>
        public bool IsItem => Kind == StackKind.Item;

        /// <summary>True for resources.</summary>
        public bool IsResource => Kind == StackKind.Resource;

        /// <summary>True for registered custom items.</summary>
        public bool IsCustom => Kind == StackKind.Custom;

        /// <summary>The equipment asset of a game item, null otherwise.</summary>
        public EquipmentAsset EquipmentAsset => Kind == StackKind.Item && Item != null ? Item.getAsset() : null;

        /// <summary>The resource asset of a resource stack, null otherwise.</summary>
        public ResourceAsset ResourceAsset => Kind == StackKind.Resource ? AssetManager.resources.get(Id) : null;

        /// <summary>The definition of a custom stack, null otherwise.</summary>
        public CustomItemDefinition CustomDefinition => Kind == StackKind.Custom ? CustomItemRegistry.Get(Id) : null;

        /// <summary>True when this is a weapon item.</summary>
        public bool IsWeapon
        {
            get
            {
                EquipmentAsset asset = EquipmentAsset;
                return asset != null && asset.equipment_type == EquipmentType.Weapon;
            }
        }

        /// <summary>True when this is armor or an accessory (helmet, armor, boots, ring, amulet).</summary>
        public bool IsWearable
        {
            get
            {
                EquipmentAsset asset = EquipmentAsset;
                return asset != null && asset.equipment_type != EquipmentType.Weapon;
            }
        }

        /// <summary>True when the game item carries the "cursed" modifier.</summary>
        public bool IsCursed => Kind == StackKind.Item && Item != null && Item.isCursed();

        /// <summary>False when the referenced game object is gone (destroyed item, removed asset).</summary>
        public bool IsValid
        {
            get
            {
                switch (Kind)
                {
                    case StackKind.Item:
                        return Item != null && Item.isAlive() && Item.getAsset() != null;
                    case StackKind.Resource:
                        return Count > 0 && AssetManager.resources.get(Id) != null;
                    case StackKind.Custom:
                        return Count > 0;
                    default:
                        return Count > 0;
                }
            }
        }

        /// <summary>Largest amount that fits in one slot.</summary>
        public int MaxStack
        {
            get
            {
                switch (Kind)
                {
                    case StackKind.Resource:
                        return Mathf.Max(1, PLConfig.StackSize);
                    case StackKind.Custom:
                        CustomItemDefinition def = CustomDefinition;
                        return def != null ? Mathf.Max(1, def.MaxStack) : 1;
                    case StackKind.Unresolved:
                        return int.MaxValue;
                    default:
                        return 1;
                }
            }
        }

        /// <summary>True when more than one fits in a slot.</summary>
        public bool IsStackable => MaxStack > 1 && Kind != StackKind.Item && Kind != StackKind.Unresolved;

        /// <summary>True when <paramref name="pOther"/> can be merged into this stack.</summary>
        public bool CanStackWith(ItemStack pOther)
        {
            if (pOther == null || ReferenceEquals(pOther, this)) return false;
            if (!IsStackable || pOther.Kind != Kind || pOther.Id != Id) return false;
            return DataEquals(Data, pOther.Data);
        }

        /// <summary>Copy of a resource or custom stack with another amount. Not valid for items.</summary>
        public ItemStack CloneWithCount(int pCount)
        {
            if (Kind == StackKind.Item) return null;
            Dictionary<string, string> data = Data != null ? new Dictionary<string, string>(Data) : null;
            return new ItemStack(Kind, null, Id, pCount, data) { UnresolvedJson = UnresolvedJson };
        }

        /// <summary>Icon used in slots (UI sprite).</summary>
        public Sprite GetIcon()
        {
            switch (Kind)
            {
                case StackKind.Item:
                    return Item != null && Item.getAsset() != null ? Item.getSprite() : null;
                case StackKind.Resource:
                    ResourceAsset res = ResourceAsset;
                    return res != null ? res.getSpriteIcon() : null;
                case StackKind.Custom:
                    CustomItemDefinition def = CustomDefinition;
                    return def != null ? def.GetIcon() : null;
                default:
                    return null;
            }
        }

        /// <summary>Name shown in tooltips and above the hotbar.</summary>
        public string GetDisplayName()
        {
            switch (Kind)
            {
                case StackKind.Item:
                    return Item != null && Item.getAsset() != null ? Item.getName(true) : "?";
                case StackKind.Resource:
                    ResourceAsset res = ResourceAsset;
                    return res != null ? res.getTranslatedName() : Id;
                case StackKind.Custom:
                    CustomItemDefinition def = CustomDefinition;
                    return def != null ? Loc.T(def.Name, def.Name ?? Id) : Id;
                default:
                    return Loc.F("possesslib_missing_item", "Missing item: $0", Id);
            }
        }

        /// <summary>Rarity colour as "#RRGGBB", or null for no tint.</summary>
        public string GetRarityColorHex()
        {
            switch (Kind)
            {
                case StackKind.Item:
                    if (Item == null || Item.getAsset() == null) return null;
                    return Item.getQuality() == Rarity.R0_Normal ? null : Item.getQualityColor();
                case StackKind.Custom:
                    CustomItemDefinition def = CustomDefinition;
                    return def != null && def.RarityColorHex != "#FFFFFF" ? def.RarityColorHex : null;
                default:
                    return null;
            }
        }

        private static bool DataEquals(Dictionary<string, string> pA, Dictionary<string, string> pB)
        {
            if (pA == null || pA.Count == 0) return pB == null || pB.Count == 0;
            if (pB == null || pA.Count != pB.Count) return false;
            foreach (KeyValuePair<string, string> pair in pA)
            {
                if (!pB.TryGetValue(pair.Key, out string other) || other != pair.Value) return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Kind == StackKind.Item ? "Item(" + Id + "#" + (Item != null ? Item.data.id : -1) + ")" : Kind + "(" + Id + " x" + Count + ")";
        }
    }
}
