using UnityEngine;
using VisibleArmors.Appearance;

namespace VisibleArmors.Render
{
    /// <summary>
    /// Paints armor pieces into a composite canvas (Color32 pixels, bottom-up rows) using the label grid and
    /// the anatomy of the frame. Order matters: torso, boots, cape, amulet, then the helmet on top.
    /// </summary>
    internal sealed class ArmorPainter
    {
        public Color32[] Canvas;
        public byte[] Labels;
        public int Width;
        public int Height;
        public ColorAsset Kingdom;
        public bool Swimming;

        private const byte LabelPainted = 12;

        private bool InBounds(int pX, int pY) => pX >= 0 && pY >= 0 && pX < Width && pY < Height;

        private Color32 Resolve(TemplateCell pCell, MaterialPalette pPalette)
        {
            switch (pCell.Role)
            {
                case TemplateRole.Accent: return pPalette.Accent;
                case TemplateRole.AccentDark: return pPalette.AccentDark;
                case TemplateRole.Kingdom: return Kingdom != null ? Kingdom.k_color_0 : pPalette.Accent;
                case TemplateRole.KingdomDark: return Kingdom != null ? Kingdom.k_color_3 : pPalette.AccentDark;
                case TemplateRole.Visor: return pPalette.Shade(0);
                default: return pPalette.Shade(pCell.Shade);
            }
        }

        private void Put(int pX, int pY, Color32 pColor)
        {
            int i = pX + pY * Width;
            pColor.a = 255;
            Canvas[i] = pColor;
            if (Labels[i] == PixelLabel.Empty) Labels[i] = LabelPainted;
        }

        private bool IsOpaque(int pX, int pY) => Canvas[pX + pY * Width].a > 0;

        // ---------------------------------------------------------------- torso

        public void PaintTorso(ArmorLook pLook, ref Anatomy pBody)
        {
            PieceTemplate t = pLook.Template;
            int wc = pBody.CoreWidth;
            int ht = pBody.TorsoHeight;
            for (int dy = 0; dy <= ht; dy++)
            {
                int y = pBody.TorsoBottom + dy;
                int row = dy == ht ? 3 : dy == ht - 1 ? 2 : dy == 0 ? 0 : 1;
                if (Swimming && row == 3) continue;
                for (int dx = -1; dx <= wc; dx++)
                {
                    int x = pBody.CoreLeft + dx;
                    if (!InBounds(x, y)) continue;
                    TemplateCell cell = t.Get(PieceTemplate.SliceColumn(dx, wc), row);
                    if (cell.Role == TemplateRole.None || cell.Role == TemplateRole.Keep) continue;
                    byte label = Labels[x + y * Width];
                    if (PixelLabel.IsHead(label)) continue;
                    bool opaque = IsOpaque(x, y);
                    if ((cell.Clip || Swimming) && !opaque) continue;
                    // In water only clothes and skin are covered, never the splash pixels.
                    if (Swimming && (label == PixelLabel.Other || label == PixelLabel.Empty)) continue;
                    Put(x, y, Resolve(cell, pLook.Palette));
                }
            }
        }

        // ---------------------------------------------------------------- boots

        public void PaintBoots(ArmorLook pLook, ref Anatomy pBody)
        {
            if (Swimming) return;
            BootsStyle style = pLook.Boots;
            int legsTop = pBody.TorsoBottom - 1;
            if (legsTop < pBody.BodyBottom) return;

            // Feet: bare skin in the leg area, plus the outer pixels of the lowest row.
            int rowMin = int.MaxValue, rowMax = int.MinValue;
            int bottom = pBody.BodyBottom;
            for (int x = 0; x < Width; x++)
            {
                if (!PixelLabel.IsBody(Labels[x + bottom * Width])) continue;
                if (x < rowMin) rowMin = x;
                if (x > rowMax) rowMax = x;
            }
            if (rowMin == int.MaxValue) return;

            for (int x = 0; x < Width; x++)
            {
                int footTop = -1;
                for (int y = bottom; y <= legsTop; y++)
                {
                    byte label = Labels[x + y * Width];
                    if (!PixelLabel.IsBody(label)) continue;
                    // Bare feet near the bottom (not a hand hanging down beside the legs), or the outer pixels of the lowest row.
                    bool bareFoot = label == PixelLabel.Skin && y <= bottom + 1 && x >= pBody.CoreLeft - 1 && x <= pBody.CoreRight + 1;
                    bool foot = bareFoot || (y == bottom && (x <= rowMin + 1 || x >= rowMax - 1));
                    if (!foot) continue;
                    Put(x, y, pLook.Palette.Shade(style.Shade));
                    footTop = y;
                    // Boot shaft: body pixels straight above the foot, up to the style height.
                    for (int k = 1; k < style.Height; k++)
                    {
                        int yy = y + k;
                        if (yy > legsTop || !PixelLabel.IsBody(Labels[x + yy * Width])) break;
                        Put(x, yy, pLook.Palette.Shade(style.Shade));
                        footTop = yy;
                    }
                    break;
                }
                if (footTop >= 0)
                {
                    Color32 cuff = style.AccentCuff ? pLook.Palette.Accent : pLook.Palette.Shade(style.TopShade);
                    if (footTop > bottom || style.Height == 1) Put(x, footTop, cuff);
                }
            }
        }

        // ---------------------------------------------------------------- cape

        public void PaintCape(ArmorLook pLook, ref Anatomy pBody)
        {
            if (Swimming || pLook.Cape == CapeMode.None) return;
            int x = pBody.CoreRight + 1;
            int top = pBody.TorsoTop;
            int bottom = pBody.BodyBottom + 1;
            if (bottom > top) bottom = top;
            for (int y = top; y >= bottom; y--)
            {
                int shade = y == top ? 2 : y <= bottom + 1 ? 1 : 0;
                Paint(x, y, shade);
                if (y <= bottom + 1) Paint(x + 1, y, 2);
            }

            void Paint(int pX, int pY, int pShade)
            {
                if (!InBounds(pX, pY) || IsOpaque(pX, pY)) return;
                Color32 c;
                if (pLook.Cape == CapeMode.Kingdom && Kingdom != null)
                    c = pShade == 0 ? Kingdom.k_color_1 : pShade == 1 ? Kingdom.k_color_2 : Kingdom.k_color_3;
                else
                    c = pLook.Palette.Shade(pShade == 0 ? 3 : pShade == 1 ? 2 : 1);
                Put(pX, pY, c);
            }
        }

        // ---------------------------------------------------------------- amulet

        public void PaintAmulet(ArmorLook pLook, ref Anatomy pBody)
        {
            if (Swimming) return;
            int x = pBody.CoreLeft + 1;
            int y = pBody.TorsoTop - 1;
            if (!InBounds(x, y + 1) || y < pBody.TorsoBottom) return;
            if (PixelLabel.IsHead(Labels[x + y * Width])) return;
            Put(x, y, pLook.Palette.Accent);
            if (!PixelLabel.IsHead(Labels[x + (y + 1) * Width]) && IsOpaque(x, y + 1)) Put(x, y + 1, pLook.Palette.Shade(4));
        }

        // ---------------------------------------------------------------- helmet

        public void PaintHelmet(ArmorLook pLook, ref Anatomy pBody)
        {
            PieceTemplate t = pLook.Template;
            int w = pBody.HeadW;
            int h = pBody.HeadH;
            // Furry heads (animal civs) use skin colours everywhere; there only the lower half counts as the face.
            int skin = 0, other = 0;
            for (int y = pBody.HeadY; y < pBody.HeadY + h; y++)
            {
                for (int x = pBody.HeadX; x < pBody.HeadX + w; x++)
                {
                    if (!InBounds(x, y)) continue;
                    byte l = Labels[x + y * Width];
                    if (l == PixelLabel.HeadSkin) skin++;
                    else if (l == PixelLabel.HeadOther) other++;
                }
            }
            int faceRows = skin > other ? (h + 1) / 2 : h;
            for (int dy = 0; dy <= h + 1; dy++)
            {
                int y = pBody.HeadY + dy;
                int row = dy == h + 1 ? 4 : dy == h ? 3 : dy == h - 1 ? 2 : dy == 0 ? 0 : 1;
                for (int dx = -1; dx <= w; dx++)
                {
                    int x = pBody.HeadX + dx;
                    if (!InBounds(x, y)) continue;
                    // Round the dome of wide (animal) heads: the corners above the head stay empty.
                    if (w >= 4 && dy >= h && (dx == 0 || dx == w - 1)) continue;
                    TemplateCell cell = t.Get(PieceTemplate.SliceColumn(dx, w), row);
                    if (cell.Role == TemplateRole.None || cell.Role == TemplateRole.Keep) continue;
                    byte label = Labels[x + y * Width];
                    // Never cover the body, and leave the face visible unless the template asks for a visor.
                    if (PixelLabel.IsBody(label)) continue;
                    if (label == PixelLabel.HeadSkin && cell.Role != TemplateRole.Visor && dy < faceRows) continue;
                    if (cell.Clip && !IsOpaque(x, y)) continue;
                    Put(x, y, Resolve(cell, pLook.Palette));
                }
            }
        }
    }
}
