using UnityEngine;

namespace VisibleArmors.Appearance
{
    /// <summary>
    /// The colours of one material. Loaded from an 8x1 PNG (materials/&lt;id&gt;.png):
    /// pixels 0-5 are the shade ramp (outline, shadow, dark, mid, light, highlight),
    /// pixel 6 is the accent (trim, gems, rivets) and pixel 7 the dark accent.
    /// </summary>
    internal sealed class MaterialPalette
    {
        public const int RampSize = 6;
        public const int Size = 8;

        public readonly string Id;
        public readonly Color32[] Ramp = new Color32[RampSize];
        public Color32 Accent;
        public Color32 AccentDark;

        public MaterialPalette(string pId, Color32[] pColors)
        {
            Id = pId;
            for (int i = 0; i < RampSize; i++) Ramp[i] = pColors[i];
            Accent = pColors[6];
            AccentDark = pColors[7];
        }

        public Color32 Shade(int pIndex)
        {
            if (pIndex < 0) pIndex = 0;
            if (pIndex >= RampSize) pIndex = RampSize - 1;
            return Ramp[pIndex];
        }

        /// <summary>Builds a palette by tinting a neutral metal ramp with a hue, used for materials nobody drew.</summary>
        public static MaterialPalette FromHue(string pId, float pHue, float pSaturation)
        {
            var colors = new Color32[Size];
            float[] values = { 0.14f, 0.28f, 0.42f, 0.58f, 0.74f, 0.92f };
            for (int i = 0; i < RampSize; i++)
            {
                float s = pSaturation * (i == RampSize - 1 ? 0.45f : 1f);
                colors[i] = Color.HSVToRGB(pHue, s, values[i]);
            }
            colors[6] = Color.HSVToRGB(Mathf.Repeat(pHue + 0.5f, 1f), 0.6f, 0.9f);
            colors[7] = Color.HSVToRGB(Mathf.Repeat(pHue + 0.5f, 1f), 0.6f, 0.5f);
            return new MaterialPalette(pId, colors);
        }

        /// <summary>
        /// Builds a palette from an arbitrary set of opaque pixels (an item icon): the pixels are sorted by
        /// brightness and spread over the ramp. Returns null when there are too few usable pixels.
        /// </summary>
        public static MaterialPalette FromPixels(string pId, Color32[] pPixels)
        {
            if (pPixels == null) return null;
            int count = 0;
            for (int i = 0; i < pPixels.Length; i++) if (pPixels[i].a > 200) count++;
            if (count < 6) return null;
            var opaque = new Color32[count];
            var keys = new float[count];
            int n = 0;
            for (int i = 0; i < pPixels.Length; i++)
            {
                Color32 c = pPixels[i];
                if (c.a <= 200) continue;
                opaque[n] = c;
                keys[n] = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                n++;
            }
            System.Array.Sort(keys, opaque);
            var colors = new Color32[Size];
            for (int i = 0; i < RampSize; i++)
            {
                int index = Mathf.Clamp(Mathf.RoundToInt(i / (float)(RampSize - 1) * (count - 1)), 0, count - 1);
                colors[i] = opaque[index];
                colors[i].a = 255;
            }
            // The most saturated colour of the icon becomes the accent.
            Color32 best = colors[3];
            float bestSat = -1f;
            for (int i = 0; i < count; i++)
            {
                Color.RGBToHSV(opaque[i], out _, out float s, out float v);
                if (s * v > bestSat)
                {
                    bestSat = s * v;
                    best = opaque[i];
                }
            }
            best.a = 255;
            colors[6] = best;
            colors[7] = Color32.Lerp(best, new Color32(0, 0, 0, 255), 0.45f);
            return new MaterialPalette(pId, colors);
        }
    }
}
