using System.Collections.Generic;
using UnityEngine;

namespace VisibleArmors.Render
{
    /// <summary>
    /// The mod's own sprite atlas for armored composites. Pages are 512x512 RGBA textures packed in rows
    /// (like the game's unit atlas). Unlike the game's atlas it is bounded: when more pages than the
    /// configured limit exist, the oldest page is scheduled for eviction (FIFO). Evicted sprites are
    /// rebuilt on demand, so a busy page simply moves to a fresh one.
    /// </summary>
    internal sealed class ArmorAtlas
    {
        public const int PageSize = 512;

        /// <summary>
        /// Whether new pages get mipmaps. Off: regenerating mipmaps on every upload tripled the cost of the atlas in
        /// the stress test, and at the zoom levels where armor is drawn the difference is not visible.
        /// </summary>
        public static bool UseMipmaps;

        internal sealed class Page
        {
            public int Serial;
            public Texture2D Texture;
            public Color32[] Pixels;
            public int NextX;
            public int NextY;
            public int RowHeight;
            public bool Dirty;
            public bool Final;
            public readonly List<Sprite> Sprites = new List<Sprite>();
        }

        private readonly List<Page> _pages = new List<Page>();
        private Page _current;
        private int _serial;

        public int PageCount => _pages.Count;
        public int PagesCreated => _serial;
        public long SpriteCount
        {
            get
            {
                long n = 0;
                foreach (Page p in _pages) n += p.Sprites.Count;
                return n;
            }
        }

        /// <summary>Copies a block of pixels into the atlas and returns the sprite. The page is uploaded later by <see cref="Upload"/>.</summary>
        public Sprite Add(Color32[] pSource, int pSourceWidth, int pX0, int pY0, int pWidth, int pHeight, Vector2 pPivotPixels, string pName)
        {
            if (pWidth + 2 > PageSize || pHeight + 2 > PageSize) return null;
            Page page = Reserve(pWidth, pHeight, out int px, out int py);
            for (int y = 0; y < pHeight; y++)
            {
                int src = pX0 + (pY0 + y) * pSourceWidth;
                int dst = px + (py + y) * PageSize;
                System.Array.Copy(pSource, src, page.Pixels, dst, pWidth);
            }
            page.Dirty = true;
            var pivot = new Vector2(pPivotPixels.x / pWidth, pPivotPixels.y / pHeight);
            Sprite sprite = Sprite.Create(page.Texture, new Rect(px, py, pWidth, pHeight), pivot, 1f, 0, SpriteMeshType.FullRect);
            sprite.name = pName;
            page.Sprites.Add(sprite);
            return sprite;
        }

        private Page Reserve(int pWidth, int pHeight, out int pX, out int pY)
        {
            Page page = _current;
            if (page != null)
            {
                if (page.NextX + pWidth + 1 > PageSize)
                {
                    page.NextX = 1;
                    page.NextY += page.RowHeight + 1;
                    page.RowHeight = 0;
                }
                if (page.NextY + pHeight + 1 > PageSize) page = null;
            }
            if (page == null) page = NewPage();
            pX = page.NextX;
            pY = page.NextY;
            page.NextX += pWidth + 1;
            if (pHeight > page.RowHeight) page.RowHeight = pHeight;
            return page;
        }

        private Page NewPage()
        {
            if (_current != null) FinishPage(_current);
            var tex = new Texture2D(PageSize, PageSize, TextureFormat.RGBA32, UseMipmaps)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "VisibleArmors_page_" + _serial
            };
            var page = new Page
            {
                Serial = _serial++,
                Texture = tex,
                Pixels = new Color32[PageSize * PageSize],
                NextX = 1,
                NextY = 1,
                Dirty = true
            };
            _pages.Add(page);
            _current = page;
            return page;
        }

        /// <summary>Uploads the last pixels of a full page and drops its CPU copy (it is never written again).</summary>
        private static void FinishPage(Page pPage)
        {
            if (pPage.Final) return;
            pPage.Texture.SetPixels32(pPage.Pixels);
            pPage.Texture.Apply(pPage.Texture.mipmapCount > 1, true);
            pPage.Pixels = null;
            pPage.Dirty = false;
            pPage.Final = true;
        }

        /// <summary>Uploads the current page if it changed. Called once per frame before rendering.</summary>
        public bool Upload()
        {
            Page page = _current;
            if (page == null || !page.Dirty || page.Final) return false;
            page.Texture.SetPixels32(page.Pixels);
            page.Texture.Apply(page.Texture.mipmapCount > 1, false);
            page.Dirty = false;
            return true;
        }

        /// <summary>Pages beyond the limit, oldest first (never the page being filled).</summary>
        public void CollectOverflow(int pLimit, List<Page> pOut)
        {
            int extra = _pages.Count - pLimit;
            for (int i = 0; i < _pages.Count && extra > 0; i++)
            {
                if (_pages[i] == _current) continue;
                pOut.Add(_pages[i]);
                extra--;
            }
        }

        /// <summary>Destroys a page and its sprites. The caller must forget the sprites first.</summary>
        public void Destroy(Page pPage)
        {
            foreach (Sprite s in pPage.Sprites)
            {
                if (s != null) Object.Destroy(s);
            }
            pPage.Sprites.Clear();
            if (pPage.Texture != null) Object.Destroy(pPage.Texture);
            pPage.Texture = null;
            pPage.Pixels = null;
            _pages.Remove(pPage);
            if (_current == pPage) _current = null;
        }

        public void All(List<Page> pOut)
        {
            pOut.AddRange(_pages);
        }

        /// <summary>Approximate GPU memory in bytes (RGBA32 plus mipmaps).</summary>
        public long ApproxBytes => (long)_pages.Count * PageSize * PageSize * 4 * 4 / 3;
    }
}
