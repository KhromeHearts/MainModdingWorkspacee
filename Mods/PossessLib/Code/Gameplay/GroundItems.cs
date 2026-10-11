using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Model;
using PossessLib.Persistence;
using PossessLib.Sync;
using UnityEngine;

namespace PossessLib.Gameplay
{
    /// <summary>
    /// Items lying on the ground. WorldBox has no such entity, so this is a small one:
    /// a list of stacks with positions, drawn with the game's QuantumSprite system (the same way it
    /// draws thrown resources), picked up automatically by the possessed unit, despawning after a
    /// while, and saved in World.world.map_stats.custom_data.
    /// </summary>
    internal static class GroundItems
    {
        public const string WorldKey = "possesslib.ground";
        public const string QuantumAssetId = "possesslib_ground_items";
        private const float PickupRadius = 1.5f;
        private const float PickupDelaySelf = 1.5f;
        private const int MaxStacks = 300;
        private const float TargetWorldHeight = 0.9f;

        private sealed class GroundStack
        {
            public ItemStack Stack;
            public Vector2 Position;
            public double DroppedAtWorldTime;
            public float NoPickupUntil;
            public long DroppedBy = -1;
            public float Phase;
        }

        private sealed class GroundDto
        {
            [JsonProperty("v")] public int Version = 1;
            [JsonProperty("g")] public List<GroundEntryDto> Items = new List<GroundEntryDto>();
        }

        private sealed class GroundEntryDto
        {
            [JsonProperty("s")] public StackDto Stack;
            [JsonProperty("x")] public float X;
            [JsonProperty("y")] public float Y;
            [JsonProperty("t")] public double Time;
        }

        private static readonly List<GroundStack> _items = new List<GroundStack>();
        private static int _loadedSeed = int.MinValue;
        private static QuantumSpriteAsset _asset;

        public static int Count => _items.Count;

        // ------------------------------------------------------------------ world lifecycle

        private static void CheckWorld()
        {
            if (_loadedSeed == MapBox.current_world_seed_id) return;
            // A different world: forget the old stacks (their items belong to the old world).
            _items.Clear();
        }

        /// <summary>Loads this world's ground items once. Called while a save loads (after items are loaded).</summary>
        public static void EnsureLoadedForWorld()
        {
            int seed = MapBox.current_world_seed_id;
            if (_loadedSeed == seed) return;
            _items.Clear();
            _loadedSeed = seed;
            try
            {
                SaveCustomData data = World.world != null && World.world.map_stats != null ? World.world.map_stats.custom_data : null;
                if (data == null) return;
                data.get(WorldKey, out string json, null);
                if (string.IsNullOrEmpty(json)) return;
                GroundDto dto = JsonConvert.DeserializeObject<GroundDto>(json);
                if (dto == null || dto.Items == null) return;
                foreach (GroundEntryDto entry in dto.Items)
                {
                    ItemStack stack = InventorySerializer.FromDto(entry.Stack);
                    if (stack == null) continue;
                    Add(stack, new Vector2(entry.X, entry.Y), entry.Time, null);
                }
                PLLog.Debug("Loaded " + _items.Count + " ground stacks");
            }
            catch (Exception e)
            {
                PLLog.Exception("Loading ground items", e);
            }
        }

        /// <summary>Called on MapBox.on_world_loaded (covers new worlds and saves without units).</summary>
        public static void OnWorldLoaded() => EnsureLoadedForWorld();

        public static void SaveToWorld()
        {
            if (World.world == null || World.world.map_stats == null) return;
            SaveCustomData data = World.world.map_stats.custom_data;
            if (data == null) return;
            CheckWorld();
            if (_items.Count == 0)
            {
                data.removeString(WorldKey);
                return;
            }
            GroundDto dto = new GroundDto();
            foreach (GroundStack g in _items)
            {
                StackDto s = InventorySerializer.ToDto(g.Stack);
                if (s == null) continue;
                dto.Items.Add(new GroundEntryDto { Stack = s, X = g.Position.x, Y = g.Position.y, Time = g.DroppedAtWorldTime });
            }
            data.set(WorldKey, JsonConvert.SerializeObject(dto));
        }

        // ------------------------------------------------------------------ dropping and picking up

        /// <summary>Puts a stack on the ground (the caller has already detached it from its owner).</summary>
        public static void Drop(ItemStack pStack, Vector2 pPosition, Actor pDroppedBy)
        {
            if (pStack == null) return;
            CheckWorld();
            _loadedSeed = MapBox.current_world_seed_id;
            Vector2 pos = pPosition + new Vector2(UnityEngine.Random.Range(-0.4f, 0.4f), UnityEngine.Random.Range(-0.4f, 0.4f));
            GroundStack g = Add(pStack, pos, World.world.getCurWorldTime(), pDroppedBy);
            if (g != null && pDroppedBy != null) g.NoPickupUntil = Time.unscaledTime + PickupDelaySelf;
            if (pStack.Kind == StackKind.Item && pStack.Item != null && pStack.Item.getActor() != null) pStack.Item.clearUnit();
        }

        private static GroundStack Add(ItemStack pStack, Vector2 pPos, double pTime, Actor pDroppedBy)
        {
            if (pStack.Kind == StackKind.Item && pStack.Item != null) InventoryRegistry.Protect(pStack.Item);
            GroundStack g = new GroundStack
            {
                Stack = pStack,
                Position = pPos,
                DroppedAtWorldTime = pTime,
                DroppedBy = pDroppedBy != null && pDroppedBy.data != null ? pDroppedBy.data.id : -1,
                Phase = UnityEngine.Random.value * 6.28f
            };
            _items.Add(g);
            while (_items.Count > MaxStacks) RemoveAt(0, true);
            return g;
        }

        private static void RemoveAt(int pIndex, bool pDestroy)
        {
            GroundStack g = _items[pIndex];
            _items.RemoveAt(pIndex);
            if (g.Stack.Kind == StackKind.Item && g.Stack.Item != null)
            {
                InventoryRegistry.Unprotect(g.Stack.Item);
                if (pDestroy) InventoryOps.DestroyStack(g.Stack);
            }
        }

        /// <summary>Possessed unit: picks up stacks within reach and despawns old ones.</summary>
        public static void TickPickup(Actor pActor, Inventory pInv)
        {
            CheckWorld();
            if (_items.Count == 0 || pActor == null || pInv == null) return;
            float now = Time.unscaledTime;
            double worldNow = World.world.getCurWorldTime();
            double despawn = PLConfig.GroundDespawnMinutes * 60.0;
            Vector2 me = pActor.current_position;

            for (int i = _items.Count - 1; i >= 0; i--)
            {
                GroundStack g = _items[i];
                if (!g.Stack.IsValid && g.Stack.Kind != StackKind.Unresolved)
                {
                    RemoveAt(i, false);
                    continue;
                }
                if (worldNow - g.DroppedAtWorldTime > despawn && !(g.Stack.Item != null && g.Stack.Item.isFavorite()))
                {
                    RemoveAt(i, true);
                    continue;
                }
                if (now < g.NoPickupUntil) continue;
                if ((g.Position - me).sqrMagnitude > PickupRadius * PickupRadius) continue;
                TryPickup(pActor, pInv, i);
            }
        }

        private static void TryPickup(Actor pActor, Inventory pInv, int pIndex)
        {
            GroundStack g = _items[pIndex];
            ItemStack stack = g.Stack;
            int before = stack.Count;
            if (stack.Kind == StackKind.Item && stack.Item != null) InventoryRegistry.Unprotect(stack.Item);
            ItemStack rest = InventoryOps.GiveStack(pActor, pInv, stack);
            if (rest == null)
            {
                _items.RemoveAt(pIndex);
                Toasts.Show("+" + (before > 1 ? before + " " : "") + stack.GetDisplayName());
                UISound.Play(UISound.PickUp);
                PossessLibEvents.RaiseItemPickedUp(pActor, stack);
                return;
            }
            // did not (fully) fit: keep it on the ground, try again later
            if (stack.Kind == StackKind.Item && stack.Item != null) InventoryRegistry.Protect(stack.Item);
            g.NoPickupUntil = Time.unscaledTime + 2f;
            if (rest.Count < before) PossessLibEvents.RaiseItemPickedUp(pActor, stack);
        }

        // ------------------------------------------------------------------ rendering (game's QuantumSprite system)

        /// <summary>Registers the quantum sprite asset. Mirrors the game's "draw_parabolic_unload" asset.</summary>
        public static void RegisterRenderer()
        {
            try
            {
                if (AssetManager.quantum_sprites == null || AssetManager.quantum_sprites.has(QuantumAssetId)) return;
                _asset = new QuantumSpriteAsset
                {
                    id = QuantumAssetId,
                    id_prefab = "p_mapSprite",
                    add_camera_zoom_multiplier = false,
                    render_gameplay = true,
                    render_map = false,
                    draw_call = Draw
                };
                AssetManager.quantum_sprites.add(_asset);
                AssetManager.quantum_sprites._array = null; // the array is cached lazily; make sure it includes us
                if (QuantumSpriteManager._initiated && _asset.group_system == null)
                {
                    // Systems were already created (late registration): create ours like createSystems does.
                    QuantumSpriteGroupSystem group = new GameObject().AddComponent<QuantumSpriteGroupSystem>();
                    group.create(_asset);
                    _asset.group_system = group;
                }
            }
            catch (Exception e)
            {
                PLLog.Exception("Registering the ground item renderer (ground items will be invisible)", e);
            }
        }

        private static void Draw(QuantumSpriteAsset pAsset)
        {
            if (_items.Count == 0 || _loadedSeed != MapBox.current_world_seed_id || !PLConfig.EnableGroundItems) return;
            float t = Time.time;
            for (int i = 0; i < _items.Count; i++)
            {
                GroundStack g = _items[i];
                Sprite sprite = GetWorldSprite(g.Stack);
                if (sprite == null) continue;
                QuantumSprite q = pAsset.group_system.getNext();
                Vector3 pos = new Vector3(g.Position.x, g.Position.y + 0.15f * Mathf.Sin(t * 2f + g.Phase), 4f);
                // world size of a sprite = rect / pixelsPerUnit * scale; aim for a fixed on-map height
                float height = sprite.rect.height / Mathf.Max(0.01f, sprite.pixelsPerUnit);
                q.setSprite(sprite);
                q.setPosOnly(ref pos);
                q.setScale(height > 0f ? Mathf.Clamp(TargetWorldHeight / height, 0.02f, 2f) : 0.1f);
            }
        }

        /// <summary>Gameplay sprite when there is one (matches the world art), otherwise the UI icon.</summary>
        private static Sprite GetWorldSprite(ItemStack pStack)
        {
            switch (pStack.Kind)
            {
                case StackKind.Resource:
                    ResourceAsset res = pStack.ResourceAsset;
                    if (res != null && res.gameplay_sprites != null && res.gameplay_sprites.Length > 0) return res.getGameplaySprite();
                    return res != null ? res.getSpriteIcon() : null;
                case StackKind.Item:
                    EquipmentAsset asset = pStack.EquipmentAsset;
                    if (asset == null) return null;
                    Sprite[] sprites = asset.getSprites();
                    if (sprites != null && sprites.Length > 0 && sprites[0] != null) return sprites[0];
                    return asset.getSprite();
                default:
                    return pStack.GetIcon();
            }
        }
    }
}
