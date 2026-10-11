using UnityEngine;

namespace VisibleArmors.Appearance
{
    /// <summary>What one template pixel does to the unit sprite.</summary>
    internal enum TemplateRole : byte
    {
        None = 0,
        Shade = 1,
        Accent = 2,
        AccentDark = 3,
        Kingdom = 4,
        KingdomDark = 5,
        Keep = 6,
        Visor = 7
    }

    /// <summary>One decoded template pixel.</summary>
    internal struct TemplateCell
    {
        public TemplateRole Role;
        public byte Shade;
        /// <summary>True when the pixel only paints over pixels the unit already has (semi-transparent in the PNG).</summary>
        public bool Clip;
    }

    /// <summary>
    /// A 9-slice pixel-art template for a helmet (5x5) or a torso piece (5x4), loaded from a PNG.
    /// The middle column and the middle row stretch to the size of the head or torso of each frame, so one
    /// template fits 2x2, 3x3 and 2x3 heads and every animation frame. Colour roles (see README):
    /// grey = material shade (black = outline ... white = highlight), #FF0000 = accent, #800000 = dark accent,
    /// #FF00FF = kingdom colour, #800080 = dark kingdom colour, #00FF00 = keep the unit's pixel,
    /// #00FFFF = visor (darkest shade, also paints over the face). Opaque pixels may add new pixels
    /// (they change the silhouette); semi-transparent pixels only recolour pixels the unit already has.
    /// </summary>
    internal sealed class PieceTemplate
    {
        public const int Columns = 5;

        public readonly string Id;
        public readonly int Rows;
        /// <summary>Cells stored bottom-up: index = column + row * Columns, row 0 = bottom row of the PNG.</summary>
        private readonly TemplateCell[] _cells;

        private PieceTemplate(string pId, int pRows, TemplateCell[] pCells)
        {
            Id = pId;
            Rows = pRows;
            _cells = pCells;
        }

        public TemplateCell Get(int pColumn, int pRowFromBottom)
        {
            return _cells[pColumn + pRowFromBottom * Columns];
        }

        /// <summary>Decodes Unity-ordered pixels (bottom row first). Returns null if the size is wrong.</summary>
        public static PieceTemplate FromPixels(string pId, Color32[] pPixels, int pWidth, int pHeight, int pExpectedRows)
        {
            if (pPixels == null || pWidth != Columns || pHeight != pExpectedRows) return null;
            var cells = new TemplateCell[Columns * pHeight];
            for (int i = 0; i < cells.Length; i++) cells[i] = Decode(pPixels[i]);
            return new PieceTemplate(pId, pHeight, cells);
        }

        public static TemplateCell Decode(Color32 pColor)
        {
            var cell = new TemplateCell();
            if (pColor.a == 0) return cell;
            cell.Clip = pColor.a < 250;
            int r = pColor.r, g = pColor.g, b = pColor.b;
            if (r == 0 && g == 255 && b == 0) cell.Role = TemplateRole.Keep;
            else if (r == 0 && g == 255 && b == 255) cell.Role = TemplateRole.Visor;
            else if (r == 255 && g == 0 && b == 255) cell.Role = TemplateRole.Kingdom;
            else if (r == 128 && g == 0 && b == 128) cell.Role = TemplateRole.KingdomDark;
            else if (r == 255 && g == 0 && b == 0) cell.Role = TemplateRole.Accent;
            else if (r == 128 && g == 0 && b == 0) cell.Role = TemplateRole.AccentDark;
            else
            {
                cell.Role = TemplateRole.Shade;
                int grey = (r + g + b) / 3;
                cell.Shade = (byte)Mathf.Clamp(Mathf.RoundToInt(grey / 51f), 0, MaterialPalette.RampSize - 1);
            }
            return cell;
        }

        /// <summary>
        /// Maps an offset inside a stretched box to a template column or row.
        /// pOffset runs from -1 (front margin) to pSize (back margin); the first and last inner cells keep
        /// their own template column and everything between them uses the stretchable middle column.
        /// </summary>
        public static int SliceColumn(int pOffset, int pSize)
        {
            if (pOffset < 0) return 0;
            if (pOffset >= pSize) return 4;
            if (pSize == 1) return 2;
            if (pOffset == 0) return 1;
            if (pOffset == pSize - 1) return 3;
            return 2;
        }
    }
}
