using System;
using System.Collections.Generic;
using VisibleArmors.Appearance;
using VisibleArmors.Core;

namespace VisibleArmors.Render
{
    /// <summary>
    /// Decides which armor pieces a species can show. Explicit entries come from appearances.json ("species")
    /// and from the API; everything else uses the "default" entry when the "Other species" switch is on.
    /// "auto" is resolved per sprite sheet by <see cref="Anatomy.LooksHumanoid"/>.
    /// </summary>
    internal static class SpeciesProfiles
    {
        private static readonly Dictionary<ActorAsset, BodyProfile> _byAsset = new Dictionary<ActorAsset, BodyProfile>();
        private static readonly Dictionary<string, BodyProfile> _api = new Dictionary<string, BodyProfile>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> _autoBySheet = new Dictionary<string, bool>();

        public static BodyProfile For(ActorAsset pAsset)
        {
            if (pAsset == null) return BodyProfile.None;
            if (_byAsset.TryGetValue(pAsset, out BodyProfile profile)) return profile;
            profile = Resolve(pAsset);
            _byAsset[pAsset] = profile;
            return profile;
        }

        private static BodyProfile Resolve(ActorAsset pAsset)
        {
            if (!pAsset.use_items || pAsset.is_boat || pAsset.ignore_generic_render || pAsset.has_avatar_prefab) return BodyProfile.None;
            if (!pAsset.need_colored_sprite || pAsset.texture_atlas != UnitTextureAtlasID.Units) return BodyProfile.None;
            if (_api.TryGetValue(pAsset.id, out BodyProfile api)) return api;
            if (AppearanceRules.Species.TryGetValue(pAsset.id, out BodyProfile listed)) return listed;
            // Zombies and other clones of a listed species inherit its profile.
            if (!string.IsNullOrEmpty(pAsset.base_asset_id) && AppearanceRules.Species.TryGetValue(pAsset.base_asset_id, out BodyProfile parent)) return parent;
            return VAConfig.OtherSpecies ? AppearanceRules.DefaultProfile : BodyProfile.None;
        }

        /// <summary>
        /// Resolves <see cref="BodyProfile.Auto"/> for one sprite sheet (cached per sheet path). Only frames that show
        /// the whole body may decide (pCanDecide); until one has, the sheet is treated as head-only.
        /// </summary>
        public static BodyProfile Effective(BodyProfile pProfile, string pSheetId, bool pCanDecide, Func<bool> pCheck)
        {
            if (pProfile != BodyProfile.Auto) return pProfile;
            string key = pSheetId ?? string.Empty;
            if (!_autoBySheet.TryGetValue(key, out bool humanoid))
            {
                if (!pCanDecide) return BodyProfile.HeadOnly;
                humanoid = pCheck();
                _autoBySheet[key] = humanoid;
                VALog.Debug("Sheet " + key + " auto profile: " + (humanoid ? "humanoid" : "head only"));
            }
            return humanoid ? BodyProfile.Humanoid : BodyProfile.HeadOnly;
        }

        public static void SetApi(string pActorAssetId, BodyProfile pProfile)
        {
            _api[pActorAssetId] = pProfile;
            Clear();
        }

        public static void Clear()
        {
            _byAsset.Clear();
            _autoBySheet.Clear();
        }
    }
}
