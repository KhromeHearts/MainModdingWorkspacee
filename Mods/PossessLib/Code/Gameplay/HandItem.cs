using System.Collections.Generic;
using PossessLib.Api;
using PossessLib.Model;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>
    /// Decides what the possessed unit holds in its hand. The game renders hand items inside a
    /// Parallel.For, so the Harmony postfix (Patches/HandRendererPatch) may run on worker threads:
    /// it only reads the two volatile fields below, which this class computes on the main thread.
    /// </summary>
    internal static class HandItem
    {
        internal static volatile Actor OverrideActor;
        internal static volatile IHandRenderer OverrideRenderer;

        private static readonly Dictionary<string, CustomHandRenderer> _customRenderers = new Dictionary<string, CustomHandRenderer>();

        /// <summary>Recomputes the override for the possessed unit (call after selection or equipment changes).</summary>
        public static void Refresh()
        {
            Actor actor = PossessionController.Current;
            Inventory inv = PossessionController.CurrentInventory;
            IHandRenderer renderer = null;
            if (actor != null && inv != null && inv.PinnedCursedWeapon == null)
            {
                ItemStack selected = inv.SelectedStack;
                // creatures that cannot use items do not get a weapon drawn in their hand
                if (selected != null && (!selected.IsItem || actor.canUseItems())) renderer = GetRenderer(selected);
            }

            bool changed = !ReferenceEquals(OverrideActor, actor) || !ReferenceEquals(OverrideRenderer, renderer);
            Actor previous = OverrideActor;
            OverrideActor = actor;
            OverrideRenderer = renderer;
            if (!changed) return;
            if (previous != null && !ReferenceEquals(previous, actor) && previous.isAlive()) previous.setItemSpriteRenderDirty();
            if (actor != null) actor.setItemSpriteRenderDirty();
        }

        public static void Clear()
        {
            Actor previous = OverrideActor;
            OverrideActor = null;
            OverrideRenderer = null;
            if (previous != null && previous.isAlive()) previous.setItemSpriteRenderDirty();
        }

        /// <summary>A renderer the game can draw for this stack, or null.</summary>
        public static IHandRenderer GetRenderer(ItemStack pStack)
        {
            if (pStack == null || !pStack.IsValid) return null;
            IHandRenderer renderer = null;
            switch (pStack.Kind)
            {
                case StackKind.Item:
                    renderer = pStack.EquipmentAsset;
                    break;
                case StackKind.Resource:
                    renderer = pStack.ResourceAsset;
                    break;
                case StackKind.Custom:
                    CustomItemDefinition def = pStack.CustomDefinition;
                    if (def != null && def.HandSprites != null && def.HandSprites.Length > 0)
                    {
                        if (!_customRenderers.TryGetValue(def.Id, out CustomHandRenderer custom))
                        {
                            custom = new CustomHandRenderer(def);
                            _customRenderers[def.Id] = custom;
                        }
                        renderer = custom;
                    }
                    break;
            }
            return IsDrawable(renderer) ? renderer : null;
        }

        private static bool IsDrawable(IHandRenderer pRenderer)
        {
            if (pRenderer == null) return false;
            Sprite[] sprites = pRenderer.getSprites();
            return sprites != null && sprites.Length > 0 && sprites[0] != null;
        }
    }

    /// <summary>Lets a custom item be drawn in hand. Extends Asset because IHandRenderer.getID() casts to Asset.</summary>
    internal sealed class CustomHandRenderer : Asset, IHandRenderer
    {
        private readonly CustomItemDefinition _definition;

        public CustomHandRenderer(CustomItemDefinition pDefinition)
        {
            _definition = pDefinition;
            id = "possesslib_hand_" + pDefinition.Id;
        }

        public Sprite[] getSprites() => _definition.HandSprites;

        public bool is_colored => false;

        public bool is_animated => _definition.HandSprites != null && _definition.HandSprites.Length > 1;
    }
}
