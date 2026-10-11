using System;
using System.Collections.Generic;
using PossessLib.Model;
using UnityEngine;

namespace PossessLib.Api
{
    /// <summary>
    /// Describes a mod-defined item type (anything that is not a WorldBox Item or resource).
    /// Register it with <see cref="PossessLibApi.RegisterCustomItem"/>.
    /// </summary>
    public sealed class CustomItemDefinition
    {
        /// <summary>Unique id. Prefix it with your mod name, e.g. "survivalbox:boss_trophy".</summary>
        public string Id;

        /// <summary>Display name, or a locale key (looked up first, used as-is if missing).</summary>
        public string Name;

        /// <summary>Tooltip description, or a locale key.</summary>
        public string Description;

        /// <summary>Slot icon. If null, <see cref="IconPath"/> is loaded through SpriteTextureLoader.</summary>
        public Sprite Icon;

        /// <summary>Resources path of the icon, e.g. "ui/icons/mymod/apple" (a file in your GameResources).</summary>
        public string IconPath;

        /// <summary>How many fit in one slot. 1 makes it unstackable.</summary>
        public int MaxStack = 99;

        /// <summary>Name colour in tooltips, as a hex string ("#FFD700").</summary>
        public string RarityColorHex = "#FFFFFF";

        /// <summary>Seconds between two uses with left click.</summary>
        public float UseCooldown = 0.5f;

        /// <summary>When true, one item is removed after <see cref="OnUse"/> returns true.</summary>
        public bool ConsumeOnUse = true;

        /// <summary>
        /// Called when the possessed unit uses the item (left click while selected).
        /// Return true if the item was used. Leave null for items that cannot be used.
        /// </summary>
        public Func<Actor, ItemStack, bool> OnUse;

        /// <summary>Optional extra tooltip text (already formatted, may contain rich-text colour tags).</summary>
        public Func<ItemStack, string> ExtraTooltip;

        /// <summary>Optional custom equipment slot id (see <see cref="EquipmentSlotDefinition"/>) this item can be worn in.</summary>
        public string EquipSlotId;

        /// <summary>Optional stats added to the wearer while equipped in a custom slot.</summary>
        public BaseStats EquippedStats;

        /// <summary>Optional sprites drawn in the unit's hand when selected in the hotbar (first frame used if not animated).</summary>
        public Sprite[] HandSprites;

        private Sprite _cachedIcon;

        internal Sprite GetIcon()
        {
            if (Icon != null) return Icon;
            if (_cachedIcon == null && !string.IsNullOrEmpty(IconPath))
            {
                _cachedIcon = SpriteTextureLoader.getSprite(IconPath) ?? Resources.Load<Sprite>(IconPath);
            }
            return _cachedIcon;
        }
    }

    /// <summary>What a recipe produces.</summary>
    public enum RecipeResultKind
    {
        /// <summary>A WorldBox equipment item (weapon, armor, accessory). ResultId is an EquipmentAsset id.</summary>
        Item,
        /// <summary>A resource added to the unit's bag. ResultId is a ResourceAsset id.</summary>
        Resource,
        /// <summary>A registered custom item. ResultId is a <see cref="CustomItemDefinition.Id"/>.</summary>
        Custom
    }

    /// <summary>A crafting recipe. Register it with <see cref="PossessLibApi.RegisterRecipe"/>.</summary>
    public sealed class RecipeDefinition
    {
        /// <summary>Unique recipe id. Re-registering an id replaces the old recipe.</summary>
        public string Id;

        /// <summary>Kind of result.</summary>
        public RecipeResultKind ResultKind = RecipeResultKind.Item;

        /// <summary>Id of the result (EquipmentAsset id, resource id or custom item id).</summary>
        public string ResultId;

        /// <summary>How many results are produced (ignored for items, which are always 1).</summary>
        public int ResultCount = 1;

        /// <summary>Resource ingredients: resource id -> amount. Taken from the unit's bag.</summary>
        public Dictionary<string, int> Resources = new Dictionary<string, int>();

        /// <summary>Custom item ingredients: custom id -> amount.</summary>
        public Dictionary<string, int> CustomItems = new Dictionary<string, int>();

        /// <summary>Coins taken from the unit's personal money (actor.data.money).</summary>
        public int Money;

        /// <summary>Optional crafting station: building asset ids (or id prefixes ending with '*') that must be nearby.</summary>
        public List<string> NearBuildings;

        /// <summary>Radius in tiles used for <see cref="NearBuildings"/>.</summary>
        public float StationRadius = 6f;

        /// <summary>Category shown in the crafting list ("armor", "weapon", "accessory", "misc").</summary>
        public string Category = "misc";

        /// <summary>Optional extra condition (for example a required trait or level).</summary>
        public Func<Actor, bool> Condition;

        /// <summary>True for recipes PossessLib derived from the game's own item costs.</summary>
        public bool AutoGenerated { get; internal set; }
    }

    /// <summary>
    /// An extra equipment slot (for example a backpack or a trinket) shown in the equipment column.
    /// The game only knows its six vanilla slots, so extra slots hold custom items and apply their
    /// effects through <see cref="CustomItemDefinition.EquippedStats"/> and the callbacks below.
    /// </summary>
    public sealed class EquipmentSlotDefinition
    {
        /// <summary>Unique slot id, e.g. "mymod:back".</summary>
        public string Id;

        /// <summary>Display name, or a locale key.</summary>
        public string Name;

        /// <summary>Faded icon shown while the slot is empty.</summary>
        public Sprite Icon;

        /// <summary>Resources path used when <see cref="Icon"/> is null.</summary>
        public string IconPath;

        /// <summary>Sort order in the column. Vanilla slots use 0-50.</summary>
        public int Order = 100;

        /// <summary>Which stacks fit. Default: custom items whose EquipSlotId equals this slot id.</summary>
        public Func<ItemStack, bool> Accepts;

        /// <summary>Called after a stack is placed in the slot.</summary>
        public Action<Actor, ItemStack> OnEquipped;

        /// <summary>Called after a stack is removed from the slot.</summary>
        public Action<Actor, ItemStack> OnUnequipped;

        internal bool AcceptsStack(ItemStack pStack)
        {
            if (pStack == null) return false;
            if (Accepts != null) return Accepts(pStack);
            CustomItemDefinition def = pStack.CustomDefinition;
            return def != null && def.EquipSlotId == Id;
        }

        private Sprite _cachedIcon;

        internal Sprite GetIcon()
        {
            if (Icon != null) return Icon;
            if (_cachedIcon == null && !string.IsNullOrEmpty(IconPath))
            {
                _cachedIcon = SpriteTextureLoader.getSprite(IconPath) ?? Resources.Load<Sprite>(IconPath);
            }
            return _cachedIcon;
        }
    }

    /// <summary>Screen corner a registered HUD element is attached to.</summary>
    public enum HudAnchor
    {
        /// <summary>Below the hotbar and status row.</summary>
        TopLeft,
        /// <summary>Below the health, XP and armor readouts.</summary>
        TopRight,
        /// <summary>Bottom-left corner.</summary>
        BottomLeft,
        /// <summary>Bottom-right corner.</summary>
        BottomRight,
        /// <summary>Top centre (where toasts appear).</summary>
        TopCenter
    }

    /// <summary>A HUD element shown while a unit is possessed. Register it with <see cref="PossessLibApi.RegisterHudElement"/>.</summary>
    public sealed class HudElementDefinition
    {
        /// <summary>Unique id.</summary>
        public string Id;

        /// <summary>Where it is attached.</summary>
        public HudAnchor Anchor = HudAnchor.TopLeft;

        /// <summary>Size of the container PossessLib creates for you (canvas units, before UI scale).</summary>
        public Vector2 Size = new Vector2(80f, 20f);

        /// <summary>Called once with your container. Build your UI inside it.</summary>
        public Action<RectTransform> Build;

        /// <summary>Called every frame while possessing (the possessed actor is passed). Keep it cheap.</summary>
        public Action<RectTransform, Actor> OnUpdate;

        /// <summary>Lower values are placed first inside the same anchor.</summary>
        public int Order = 100;
    }

    /// <summary>
    /// An extra panel shown under the crafting list while the inventory is open.
    /// Register it with <see cref="PossessLibApi.RegisterInventoryPanel"/>.
    /// </summary>
    public sealed class InventoryPanelDefinition
    {
        /// <summary>Unique id.</summary>
        public string Id;

        /// <summary>Title drawn above the panel (text or locale key). Null for none.</summary>
        public string Title;

        /// <summary>Panel size in canvas units.</summary>
        public Vector2 Size = new Vector2(150f, 60f);

        /// <summary>Called once with the panel content rect.</summary>
        public Action<RectTransform> Build;

        /// <summary>Called whenever the inventory opens and when the inventory changes while open.</summary>
        public Action<RectTransform, Actor, Inventory> OnRefresh;

        /// <summary>Lower values are placed first.</summary>
        public int Order = 100;
    }
}
