using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using UnityEngine;
using VisibleArmors.Appearance;
using VisibleArmors.Core;

namespace VisibleArmors.Render
{
    /// <summary>
    /// Keeps the drawn sprites in step with the live equipment. Once per frame, before the game's main-thread
    /// render pass, every visible unit's current sprite is checked: if it was built for a different signature
    /// (equipment changed, unit died, zoom limit crossed, config changed) the unit's colour cache is reset so the
    /// game asks for a new sprite in the same frame. Cost: a few field reads and two dictionary lookups per
    /// visible unit, no allocations.
    /// </summary>
    internal static class RenderSync
    {
        private static readonly AccessTools.FieldRef<Actor, Sprite> _lastMainSprite = AccessTools.FieldRefAccess<Actor, Sprite>("_last_main_sprite");
        private static int _configVersion;
        private static int _lastFrame = -1;
        private static readonly Stopwatch _watch = new Stopwatch();

        public static double ScanMsTotal;
        public static long ScanFrames;
        public static long Invalidations;
        public static int LastVisible;
        public static int LastArmored;

        /// <summary>Main-thread hook, called before ActorManager.precalculateRenderDataNormal.</summary>
        public static void BeforeNormalPass(ActorManager pManager)
        {
            if (Time.frameCount == _lastFrame) return;
            _lastFrame = Time.frameCount;
            _watch.Restart();

            CheckConfig();
            Signature.UpdateLod();
            HashSet<int> removed = Compositor.Maintain();

            ActorRenderData data = pManager.render_data;
            Actor[] units = pManager.visible_units.array;
            int count = pManager.visible_units.count;
            bool[] normal = data.has_normal_render;
            Sprite[] colored = data.main_sprite_colored;
            int armored = 0;
            for (int i = 0; i < count; i++)
            {
                if (!normal[i]) continue;
                Sprite current = colored[i];
                if (ReferenceEquals(current, null)) continue;
                Actor actor = units[i];
                long want;
                try
                {
                    want = Signature.Compute(actor, true);
                }
                catch (Exception e)
                {
                    VALog.GuardedException("Signature for unit " + (actor != null && actor.asset != null ? actor.asset.id : "?"), e);
                    continue;
                }
                if (want != 0) armored++;
                bool gone = removed.Count > 0 && removed.Contains(current.GetInstanceID());
                long have = gone ? -1 : Compositor.SignatureOf(current);
                if (have == want) continue;
                if (want != 0 && have == 0 && Compositor.HasFailed(current, want)) continue;
                Invalidate(actor);
                colored[i] = null;
            }
            LastVisible = count;
            LastArmored = armored;
            _watch.Stop();
            ScanMsTotal += _watch.Elapsed.TotalMilliseconds;
            ScanFrames++;
        }

        /// <summary>Forgets the unit's cached coloured sprite so the game rebuilds it (through our getSpriteUnit postfix).</summary>
        public static void Invalidate(Actor pActor)
        {
            if (pActor == null) return;
            _lastMainSprite(pActor) = null;
            Invalidations++;
        }

        /// <summary>Resets every unit (also off-screen ones) whose cached sprite was evicted, so none keeps a destroyed sprite.</summary>
        public static void ResetUnitsUsing(HashSet<int> pRemovedIds)
        {
            if (pRemovedIds.Count == 0 || World.world == null || World.world.units == null) return;
            List<Actor> all = World.world.units.getSimpleList();
            for (int i = 0; i < all.Count; i++)
            {
                Actor actor = all[i];
                if (actor == null) continue;
                Sprite last = actor.getLastColoredSprite();
                if (!ReferenceEquals(last, null) && pRemovedIds.Contains(last.GetInstanceID())) Invalidate(actor);
            }
        }

        private static void CheckConfig()
        {
            if (_configVersion == VAConfig.Version) return;
            bool first = _configVersion == 0;
            _configVersion = VAConfig.Version;
            if (first) return;
            // Anything a composite depends on may have changed: rebuild lazily.
            AppearanceRegistry.ClearCache();
            SpeciesProfiles.Clear();
            Compositor.RequestFlush();
            VALog.Debug("Config changed, armor sprites will be rebuilt");
        }

        /// <summary>Forces every unit to rebuild its sprite on the next frame (API and debug use).</summary>
        public static void RefreshAll()
        {
            AppearanceRegistry.ClearCache();
            SpeciesProfiles.Clear();
            Compositor.RequestFlush();
        }
    }
}
