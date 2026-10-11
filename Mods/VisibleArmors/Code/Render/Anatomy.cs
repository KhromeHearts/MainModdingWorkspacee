namespace VisibleArmors.Render
{
    /// <summary>What the game's own colour keys say a pixel of the unit sprite is (from its PixelBags).</summary>
    internal static class PixelLabel
    {
        public const byte Empty = 0;
        public const byte Cloth = 1;      // kingdom colour 1, bright shades (shirt)
        public const byte ClothDark = 2;  // kingdom colour 1, dark shades (trousers, belts)
        public const byte Teal = 3;       // kingdom colour 2
        public const byte Skin = 4;       // phenotype (skin) shades
        public const byte Other = 5;      // fixed colours: hair, painted armor, water splashes
        public const byte HeadSkin = 8;   // face
        public const byte HeadOther = 9;  // hair, beard, vanilla helmet

        public static bool IsBody(byte pLabel) => pLabel >= Cloth && pLabel <= Other;
        public static bool IsHead(byte pLabel) => pLabel >= HeadSkin;
    }

    /// <summary>
    /// Landmarks of one animation frame, in canvas pixels (bottom-up). Found from the label grid:
    /// the neck point comes from the frame data, the torso core is the run of well-filled columns under
    /// the head, the torso is the 3 rows below its top, and everything under it is legs.
    /// Sprites face left, so "front" is -x and "back" is +x.
    /// </summary>
    internal struct Anatomy
    {
        public bool HasHead;
        public int HeadX, HeadY, HeadW, HeadH;

        public bool HasTorso;
        public int CoreLeft, CoreRight;
        public int TorsoTop, TorsoBottom;
        public int BodyBottom;

        public int CoreWidth => CoreRight - CoreLeft + 1;
        public int TorsoHeight => TorsoTop - TorsoBottom + 1;

        /// <summary>Finds torso and legs. Returns false when the frame does not look like an upright body.</summary>
        public bool FindTorso(byte[] pLabels, int pWidth, int pHeight, int pNeckX, int pNeckY)
        {
            HasTorso = false;
            BodyBottom = -1;
            for (int y = 0; y < pHeight && BodyBottom < 0; y++)
            {
                for (int x = 0; x < pWidth; x++)
                {
                    if (PixelLabel.IsBody(pLabels[x + y * pWidth]))
                    {
                        BodyBottom = y;
                        break;
                    }
                }
            }
            if (BodyBottom < 0) return false;

            int neckY = pNeckY;
            if (neckY < 0 || neckY >= pHeight) return false;
            int rowFrom = neckY - 3 < 0 ? 0 : neckY - 3;

            // Column fill counts in the rows just under the neck.
            int bestLeft = -1, bestRight = -1;
            int seed = pNeckX;
            for (int s = 0; s < 3 && bestLeft < 0; s++)
            {
                int sx = seed + (s == 0 ? 0 : s == 1 ? 1 : -1);
                if (sx < 0 || sx >= pWidth || ColumnCount(pLabels, pWidth, sx, rowFrom, neckY) < 2) continue;
                int left = sx, right = sx;
                while (left - 1 >= 0 && ColumnCount(pLabels, pWidth, left - 1, rowFrom, neckY) >= 2) left--;
                while (right + 1 < pWidth && ColumnCount(pLabels, pWidth, right + 1, rowFrom, neckY) >= 2) right++;
                bestLeft = left;
                bestRight = right;
            }
            if (bestLeft < 0) return false;
            CoreLeft = bestLeft;
            CoreRight = bestRight;

            // Torso top: the highest row at or below the neck with body pixels in the core.
            TorsoTop = -1;
            for (int y = neckY; y >= 0 && TorsoTop < 0; y--)
            {
                for (int x = CoreLeft; x <= CoreRight; x++)
                {
                    if (PixelLabel.IsBody(pLabels[x + y * pWidth]))
                    {
                        TorsoTop = y;
                        break;
                    }
                }
            }
            if (TorsoTop < 0) return false;
            TorsoBottom = TorsoTop - 2;
            if (TorsoBottom < BodyBottom) TorsoBottom = BodyBottom;
            HasTorso = true;
            return true;
        }

        /// <summary>True for an upright, roughly human-sized body with legs under the torso.</summary>
        public bool LooksHumanoid()
        {
            if (!HasTorso || !HasHead) return false;
            int core = CoreWidth;
            if (core < 3 || core > 6) return false;
            if (TorsoBottom - BodyBottom < 1) return false;
            // The head must sit over the torso core, not in front of it like a four-legged animal's.
            int headCenter2 = HeadX * 2 + HeadW - 1;
            return headCenter2 >= CoreLeft * 2 - 1 && headCenter2 <= CoreRight * 2 + 1;
        }

        private static int ColumnCount(byte[] pLabels, int pWidth, int pX, int pFromY, int pToY)
        {
            int n = 0;
            for (int y = pFromY; y <= pToY; y++)
            {
                if (PixelLabel.IsBody(pLabels[pX + y * pWidth])) n++;
            }
            return n;
        }
    }
}
