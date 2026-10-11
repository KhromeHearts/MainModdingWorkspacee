namespace VisibleArmors.Appearance
{
    /// <summary>The equipment slots that Visible Armors draws on the body. Weapons are already drawn by the game; rings are too small to show.</summary>
    public enum ArmorSlot
    {
        /// <summary>The helmet slot (drawn over the head sprite).</summary>
        Helmet = 0,
        /// <summary>The body armor slot (torso, sleeves and, for cloaks, a cape).</summary>
        Armor = 1,
        /// <summary>The boots slot (feet).</summary>
        Boots = 2,
        /// <summary>The amulet slot (one pendant pixel on the chest).</summary>
        Amulet = 3
    }
}
