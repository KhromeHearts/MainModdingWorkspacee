using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Core;
using UnityEngine;

namespace PossessLib.Model
{
    /// <summary>Custom item types registered by other mods.</summary>
    internal static class CustomItemRegistry
    {
        private static readonly Dictionary<string, CustomItemDefinition> _items = new Dictionary<string, CustomItemDefinition>();

        public static int Count => _items.Count;

        public static bool Register(CustomItemDefinition pDefinition)
        {
            if (pDefinition == null || string.IsNullOrEmpty(pDefinition.Id))
            {
                PLLog.Warn("RegisterCustomItem: definition or Id is null");
                return false;
            }
            if (_items.ContainsKey(pDefinition.Id)) PLLog.Warn("RegisterCustomItem: replacing '" + pDefinition.Id + "'");
            _items[pDefinition.Id] = pDefinition;
            PLLog.Debug("Registered custom item " + pDefinition.Id);
            return true;
        }

        public static CustomItemDefinition Get(string pId)
        {
            return pId != null && _items.TryGetValue(pId, out CustomItemDefinition def) ? def : null;
        }

        public static bool Has(string pId) => pId != null && _items.ContainsKey(pId);
    }

    /// <summary>One entry of the equipment column (a vanilla slot or a registered custom slot).</summary>
    internal sealed class EquipSlotInfo
    {
        public string Id;
        public bool IsVanilla;
        public EquipmentType Type;
        public EquipmentSlotDefinition Custom;
        public int Order;

        public string DisplayName
        {
            get
            {
                if (IsVanilla) return Loc.T("possesslib_slot_" + Id, char.ToUpper(Id[0]) + Id.Substring(1));
                return Loc.T(Custom.Name, Custom.Name ?? Id);
            }
        }

        private Sprite _placeholder;
        private bool _placeholderSearched;

        /// <summary>Faded icon for the empty slot: the first unlocked item of that type, or the custom slot icon.</summary>
        public Sprite Placeholder
        {
            get
            {
                if (!IsVanilla) return Custom.GetIcon();
                if (_placeholderSearched) return _placeholder;
                _placeholderSearched = true;
                foreach (EquipmentAsset asset in AssetManager.items.list)
                {
                    if (asset.equipment_type != Type || asset.isTemplateAsset()) continue;
                    Sprite sprite = asset.getSprite();
                    if (sprite == null) continue;
                    _placeholder = sprite;
                    break;
                }
                return _placeholder;
            }
        }
    }

    /// <summary>The equipment column: the five wearable vanilla slots plus slots registered by mods.</summary>
    internal static class EquipSlotRegistry
    {
        private static readonly List<EquipSlotInfo> _slots = new List<EquipSlotInfo>();

        /// <summary>Bumped when a custom slot is registered so the UI can rebuild.</summary>
        public static int Version { get; private set; }

        static EquipSlotRegistry()
        {
            AddVanilla(EquipmentType.Helmet, 0);
            AddVanilla(EquipmentType.Armor, 10);
            AddVanilla(EquipmentType.Boots, 20);
            AddVanilla(EquipmentType.Ring, 30);
            AddVanilla(EquipmentType.Amulet, 40);
        }

        public static IReadOnlyList<EquipSlotInfo> Slots => _slots;

        public static string VanillaId(EquipmentType pType) => pType.ToString().ToLowerInvariant();

        private static void AddVanilla(EquipmentType pType, int pOrder)
        {
            _slots.Add(new EquipSlotInfo { Id = VanillaId(pType), IsVanilla = true, Type = pType, Order = pOrder });
        }

        public static bool Register(EquipmentSlotDefinition pDefinition)
        {
            if (pDefinition == null || string.IsNullOrEmpty(pDefinition.Id))
            {
                PLLog.Warn("RegisterEquipmentSlot: definition or Id is null");
                return false;
            }
            _slots.RemoveAll(s => !s.IsVanilla && s.Id == pDefinition.Id);
            _slots.Add(new EquipSlotInfo { Id = pDefinition.Id, Custom = pDefinition, Order = pDefinition.Order });
            _slots.Sort((a, b) => a.Order.CompareTo(b.Order));
            Version++;
            if (pDefinition != null) RecomputeStatFlag();
            return true;
        }

        public static EquipmentSlotDefinition GetCustom(string pId)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsVanilla && _slots[i].Id == pId) return _slots[i].Custom;
            }
            return null;
        }

        /// <summary>True once any registered custom item can add stats (enables the updateStats patch).</summary>
        public static bool AnyStatSlots { get; private set; }

        internal static void RecomputeStatFlag()
        {
            bool any = false;
            for (int i = 0; i < _slots.Count; i++) if (!_slots[i].IsVanilla) any = true;
            AnyStatSlots = any;
        }
    }
}
