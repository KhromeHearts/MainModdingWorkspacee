using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using VisibleArmors.Appearance;
using VisibleArmors.Core;

namespace VisibleArmors.Render
{
    /// <summary>
    /// Turns the game's coloured unit sprite into an armored one. The cache key is (vanilla sprite, signature):
    /// the vanilla sprite already encodes body frame, head, kingdom colour and skin, the signature encodes
    /// the visible equipment. Composites live in <see cref="ArmorAtlas"/>; a reverse map tells the renderer
    /// which signature a sprite was built for, which is how equipment changes are noticed. Main thread only.
    /// </summary>
    internal static class Compositor
    {
        private const int PadX = 2;
        private const int PadTop = 2;

        private struct Key : IEquatable<Key>
        {
            public int Vanilla;
            public long Signature;
            public bool Equals(Key pOther) => Vanilla == pOther.Vanilla && Signature == pOther.Signature;
            public override bool Equals(object pObj) => pObj is Key k && Equals(k);
            public override int GetHashCode() => unchecked(Vanilla * 397 ^ Signature.GetHashCode());
        }

        private struct Entry
        {
            public Key Key;
            public long Signature;
        }

        private static readonly ArmorAtlas _atlas = new ArmorAtlas();
        private static readonly Dictionary<Key, Sprite> _byKey = new Dictionary<Key, Sprite>();
        private static readonly Dictionary<int, Entry> _byComposite = new Dictionary<int, Entry>();
        private static readonly HashSet<Key> _failed = new HashSet<Key>();
        private static readonly ArmorPainter _painter = new ArmorPainter();
        private static readonly List<ArmorAtlas.Page> _evictList = new List<ArmorAtlas.Page>();
        private static readonly HashSet<int> _evictedIds = new HashSet<int>();
        private static Color32[] _canvas = new Color32[32 * 32];
        private static byte[] _labels = new byte[32 * 32];
        private static bool _flushRequested;
        private static readonly Stopwatch _watch = new Stopwatch();

        // ---- statistics (debug overlay and log)
        public static long Created;
        public static long Hits;
        public static long Failures;
        public static long Evicted;
        public static double ComposeMsTotal;
        public static double UploadMsTotal;
        public static long Uploads;
        private static readonly Stopwatch _uploadWatch = new Stopwatch();

        /// <summary>Debug only: when not null, every new composite is also copied here (name, pixels, width, height).</summary>
        public static List<KeyValuePair<string, Color32[]>> DebugCapture;
        public static List<Vector2Int> DebugCaptureSizes;

        public static ArmorAtlas Atlas => _atlas;
        public static int CachedCount => _byKey.Count;

        /// <summary>Signature the sprite was built for; 0 for vanilla sprites (or anything that is not ours).</summary>
        public static long SignatureOf(Sprite pSprite)
        {
            if (ReferenceEquals(pSprite, null)) return 0;
            return _byComposite.TryGetValue(pSprite.GetInstanceID(), out Entry e) ? e.Signature : 0;
        }

        /// <summary>True when composing this vanilla sprite with this signature failed before (so it is not retried every frame).</summary>
        public static bool HasFailed(Sprite pVanilla, long pSignature)
        {
            return !ReferenceEquals(pVanilla, null) && _failed.Contains(new Key { Vanilla = pVanilla.GetInstanceID(), Signature = pSignature });
        }

        /// <summary>Returns the armored version of a vanilla sprite, building it on a cache miss. Null means "use the vanilla sprite".</summary>
        public static Sprite GetOrCreate(Sprite pVanilla, Sprite pBody, Sprite pHead, AnimationFrameData pFrame, ColorAsset pKingdom, ActorAsset pAsset, string pSheetId, long pSignature)
        {
            var key = new Key { Vanilla = pVanilla.GetInstanceID(), Signature = pSignature };
            if (_byKey.TryGetValue(key, out Sprite cached))
            {
                Hits++;
                return cached;
            }
            if (_failed.Contains(key)) return null;
            _watch.Restart();
            Sprite result = null;
            try
            {
                result = Compose(pVanilla, pBody, pHead, pFrame, pKingdom, pAsset, pSheetId, pSignature);
            }
            catch (Exception e)
            {
                VALog.GuardedException("Composing armor for " + (pAsset != null ? pAsset.id : "?") + " " + pVanilla.name, e);
            }
            _watch.Stop();
            ComposeMsTotal += _watch.Elapsed.TotalMilliseconds;
            if (result == null)
            {
                Failures++;
                _failed.Add(key);
                return null;
            }
            Created++;
            _byKey[key] = result;
            _byComposite[result.GetInstanceID()] = new Entry { Key = key, Signature = pSignature };
            return result;
        }

        private static Sprite Compose(Sprite pVanilla, Sprite pBody, Sprite pHead, AnimationFrameData pFrame, ColorAsset pKingdom, ActorAsset pAsset, string pSheetId, long pSignature)
        {
            if (pBody == null) return null;
            Rect vr = pVanilla.rect;
            int vw = (int)vr.width;
            int vh = (int)vr.height;
            int w = vw + PadX * 2;
            int h = vh + PadTop;
            int size = w * h;
            if (_canvas.Length < size)
            {
                _canvas = new Color32[size];
                _labels = new byte[size];
            }
            Array.Clear(_canvas, 0, size);
            Array.Clear(_labels, 0, size);
            if (!ReadVanilla(pVanilla, vr, w)) return null;

            // Labels from the game's own colour keys.
            PixelBag bodyBag = PixelBagManager.getPixelBag(pBody, true, false);
            int bodyX = PadX + (vw - bodyBag.texture_rect_width);
            LabelBag(bodyBag, bodyX, 0, w, h, false);
            var anatomy = new Anatomy();
            if (pHead != null && pFrame != null && pFrame.show_head)
            {
                PixelBag headBag = PixelBagManager.getPixelBag(pHead, true, false);
                anatomy.HasHead = true;
                anatomy.HeadX = bodyX + (int)pFrame.pos_head_new.x - (int)pHead.pivot.x;
                anatomy.HeadY = (int)pFrame.pos_head_new.y - (int)pHead.pivot.y;
                anatomy.HeadW = headBag.texture_rect_width;
                anatomy.HeadH = headBag.texture_rect_height;
                LabelBag(headBag, anatomy.HeadX, anatomy.HeadY, w, h, true);
            }
            for (int i = 0; i < size; i++)
            {
                if (_labels[i] == PixelLabel.Empty && _canvas[i].a > 0) _labels[i] = PixelLabel.Other;
            }

            int neckX = pFrame != null ? bodyX + (int)pFrame.pos_head_new.x : bodyX + bodyBag.texture_rect_width / 2;
            int neckY = pFrame != null && pFrame.show_head ? (int)pFrame.pos_head_new.y : TopBodyRow(w, h);
            bool torso = anatomy.FindTorso(_labels, w, h, neckX, neckY);
            bool swimming = pFrame != null && pFrame.id != null && pFrame.id.StartsWith("swim", StringComparison.Ordinal);
            bool looksHumanoid = anatomy.LooksHumanoid();
            BodyProfile profile = SpeciesProfiles.For(pAsset);
            profile = SpeciesProfiles.Effective(profile, pSheetId, !swimming, () => looksHumanoid);
            bool humanoid = profile == BodyProfile.Humanoid && torso;

            _painter.Canvas = _canvas;
            _painter.Labels = _labels;
            _painter.Width = w;
            _painter.Height = h;
            _painter.Kingdom = pKingdom;
            _painter.Swimming = swimming;

            bool painted = false;
            if (humanoid)
            {
                ArmorLook armor = AppearanceRegistry.Get(Signature.Part(pSignature, ArmorSlot.Armor));
                if (armor != null && armor.Template != null)
                {
                    _painter.PaintTorso(armor, ref anatomy);
                    if (VAConfig.ShowCape) _painter.PaintCape(armor, ref anatomy);
                    painted = true;
                }
                ArmorLook boots = AppearanceRegistry.Get(Signature.Part(pSignature, ArmorSlot.Boots));
                if (boots != null && boots.Boots != null)
                {
                    _painter.PaintBoots(boots, ref anatomy);
                    painted = true;
                }
                ArmorLook amulet = AppearanceRegistry.Get(Signature.Part(pSignature, ArmorSlot.Amulet));
                if (amulet != null)
                {
                    _painter.PaintAmulet(amulet, ref anatomy);
                    painted = true;
                }
            }
            ArmorLook helmet = AppearanceRegistry.Get(Signature.Part(pSignature, ArmorSlot.Helmet));
            if (helmet != null && helmet.Template != null && anatomy.HasHead && profile != BodyProfile.None)
            {
                _painter.PaintHelmet(helmet, ref anatomy);
                painted = true;
            }
            if (!painted) return null;

            // Crop to the opaque pixels and keep the pivot where the game put it.
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    if (_canvas[row + x].a == 0) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            if (maxX < 0) return null;
            var pivot = new Vector2(pVanilla.pivot.x + PadX - minX, pVanilla.pivot.y - minY);
            if (DebugCapture != null) Capture(pVanilla.name + "_" + pSignature.ToString("X"), w, minX, minY, maxX - minX + 1, maxY - minY + 1);
            return _atlas.Add(_canvas, w, minX, minY, maxX - minX + 1, maxY - minY + 1, pivot, "va_" + pVanilla.name);
        }

        private static void Capture(string pName, int pWidth, int pX0, int pY0, int pW, int pH)
        {
            var copy = new Color32[pW * pH];
            for (int y = 0; y < pH; y++) Array.Copy(_canvas, pX0 + (pY0 + y) * pWidth, copy, y * pW, pW);
            DebugCapture.Add(new KeyValuePair<string, Color32[]>(pName, copy));
            DebugCaptureSizes.Add(new Vector2Int(pW, pH));
        }

        private static int TopBodyRow(int pWidth, int pHeight)
        {
            for (int y = pHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < pWidth; x++)
                {
                    if (PixelLabel.IsBody(_labels[x + y * pWidth])) return y;
                }
            }
            return 0;
        }

        /// <summary>Copies the vanilla composite into the canvas. Reads the game's atlas pixel buffer when the sprite is on the page still being filled (its texture is not uploaded yet).</summary>
        private static bool ReadVanilla(Sprite pVanilla, Rect pRect, int pCanvasWidth)
        {
            Texture2D tex = pVanilla.texture;
            if (tex == null) return false;
            int sx = (int)pRect.x;
            int sy = (int)pRect.y;
            int vw = (int)pRect.width;
            int vh = (int)pRect.height;
            UnitSpriteConstructorAtlas gameAtlas = DynamicSpritesLibrary.units != null ? DynamicSpritesLibrary.units.getAtlas() : null;
            if (gameAtlas != null && ReferenceEquals(gameAtlas.texture, tex) && gameAtlas.pixels != null)
            {
                Color32[] src = gameAtlas.pixels;
                int tw = tex.width;
                for (int y = 0; y < vh; y++)
                {
                    Array.Copy(src, sx + (sy + y) * tw, _canvas, PadX + y * pCanvasWidth, vw);
                }
                return true;
            }
            if (!tex.isReadable) return false;
            Color[] block = tex.GetPixels(sx, sy, vw, vh);
            for (int y = 0; y < vh; y++)
            {
                for (int x = 0; x < vw; x++) _canvas[PadX + x + y * pCanvasWidth] = block[x + y * vw];
            }
            return true;
        }

        private static void LabelBag(PixelBag pBag, int pOffsetX, int pOffsetY, int pWidth, int pHeight, bool pHead)
        {
            if (pHead)
            {
                Mark(pBag.arr_pixels_phenotype_shade_0, PixelLabel.HeadSkin);
                Mark(pBag.arr_pixels_phenotype_shade_1, PixelLabel.HeadSkin);
                Mark(pBag.arr_pixels_phenotype_shade_2, PixelLabel.HeadSkin);
                Mark(pBag.arr_pixels_phenotype_shade_3, PixelLabel.HeadSkin);
                Mark(pBag.arr_pixels_normal, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_light, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k1_0, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k1_1, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k1_2, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k1_3, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k1_4, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k2_0, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k2_1, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k2_2, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k2_3, PixelLabel.HeadOther);
                Mark(pBag.arr_pixels_k2_4, PixelLabel.HeadOther);
                return;
            }
            Mark(pBag.arr_pixels_k1_0, PixelLabel.Cloth);
            Mark(pBag.arr_pixels_k1_1, PixelLabel.Cloth);
            Mark(pBag.arr_pixels_k1_2, PixelLabel.Cloth);
            Mark(pBag.arr_pixels_k1_3, PixelLabel.ClothDark);
            Mark(pBag.arr_pixels_k1_4, PixelLabel.ClothDark);
            Mark(pBag.arr_pixels_k2_0, PixelLabel.Teal);
            Mark(pBag.arr_pixels_k2_1, PixelLabel.Teal);
            Mark(pBag.arr_pixels_k2_2, PixelLabel.Teal);
            Mark(pBag.arr_pixels_k2_3, PixelLabel.Teal);
            Mark(pBag.arr_pixels_k2_4, PixelLabel.Teal);
            Mark(pBag.arr_pixels_phenotype_shade_0, PixelLabel.Skin);
            Mark(pBag.arr_pixels_phenotype_shade_1, PixelLabel.Skin);
            Mark(pBag.arr_pixels_phenotype_shade_2, PixelLabel.Skin);
            Mark(pBag.arr_pixels_phenotype_shade_3, PixelLabel.Skin);
            Mark(pBag.arr_pixels_normal, PixelLabel.Other);
            Mark(pBag.arr_pixels_light, PixelLabel.Other);

            void Mark(Pixel[] pPixels, byte pLabel)
            {
                if (pPixels == null) return;
                for (int i = 0; i < pPixels.Length; i++)
                {
                    int x = pPixels[i].x + pOffsetX;
                    int y = pPixels[i].y + pOffsetY;
                    if (x < 0 || y < 0 || x >= pWidth || y >= pHeight) continue;
                    _labels[x + y * pWidth] = pLabel;
                }
            }
        }

        // ------------------------------------------------------------------ cache maintenance

        /// <summary>Drops every composite (config change, API registration). Done at the start of the next render pass.</summary>
        public static void RequestFlush() => _flushRequested = true;

        /// <summary>
        /// Called at the start of the game's main-thread render pass: performs a requested flush or evicts pages over
        /// the limit, then resets every unit that still points at a removed sprite. Returns the removed sprite ids
        /// (empty when nothing happened) so the caller can fix this frame's render list.
        /// </summary>
        public static HashSet<int> Maintain()
        {
            _evictedIds.Clear();
            _evictList.Clear();
            if (_flushRequested)
            {
                _flushRequested = false;
                _atlas.All(_evictList);
                _failed.Clear();
            }
            else
            {
                _atlas.CollectOverflow(VAConfig.CachePages, _evictList);
            }
            if (_evictList.Count == 0) return _evictedIds;

            foreach (ArmorAtlas.Page page in _evictList)
            {
                foreach (Sprite s in page.Sprites)
                {
                    if (ReferenceEquals(s, null)) continue;
                    int id = s.GetInstanceID();
                    if (_byComposite.TryGetValue(id, out Entry e))
                    {
                        _byKey.Remove(e.Key);
                        _byComposite.Remove(id);
                    }
                    _evictedIds.Add(id);
                    Evicted++;
                }
                _atlas.Destroy(page);
            }
            RenderSync.ResetUnitsUsing(_evictedIds);
            VALog.Debug("Evicted " + _evictList.Count + " atlas page(s), " + _evictedIds.Count + " sprites");
            return _evictedIds;
        }

        /// <summary>Uploads new pixels to the GPU. Called once per frame before rendering.</summary>
        public static void Upload()
        {
            _uploadWatch.Restart();
            bool uploaded = _atlas.Upload();
            _uploadWatch.Stop();
            if (!uploaded) return;
            Uploads++;
            UploadMsTotal += _uploadWatch.Elapsed.TotalMilliseconds;
        }
    }
}
