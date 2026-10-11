using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VisibleArmors.Core;

namespace VisibleArmors.Appearance
{
    /// <summary>
    /// Loads the editable art from GameResources/visible_armors: material palettes (materials/*.png),
    /// helmet templates (helmets/*.png, 5x5) and torso templates (armor/*.png, 5x4). Other mods can add
    /// more through <see cref="Api.VisibleArmorsApi"/>. Everything is read once at mod load.
    /// </summary>
    internal static class ArtLibrary
    {
        public const string Folder = "visible_armors";
        public const int HelmetRows = 5;
        public const int TorsoRows = 4;

        private static readonly Dictionary<string, MaterialPalette> _palettes = new Dictionary<string, MaterialPalette>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, PieceTemplate> _helmets = new Dictionary<string, PieceTemplate>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, PieceTemplate> _torsos = new Dictionary<string, PieceTemplate>(StringComparer.OrdinalIgnoreCase);

        public static string RootPath { get; private set; }

        public static int PaletteCount => _palettes.Count;
        public static int HelmetCount => _helmets.Count;
        public static int TorsoCount => _torsos.Count;

        public static void Load(string pModFolder)
        {
            RootPath = Path.Combine(pModFolder, "GameResources", Folder);
            LoadFolder(Path.Combine(RootPath, "materials"), (id, px, w, h) =>
            {
                if (w < MaterialPalette.Size)
                {
                    VALog.Warn("Palette " + id + " must be at least 8 pixels wide, skipped");
                    return;
                }
                // Read the top row of the image left to right (Unity rows start at the bottom).
                var colors = new Color32[MaterialPalette.Size];
                int top = (h - 1) * w;
                for (int i = 0; i < MaterialPalette.Size; i++)
                {
                    colors[i] = px[top + i];
                    colors[i].a = 255;
                }
                _palettes[id] = new MaterialPalette(id, colors);
            });
            LoadFolder(Path.Combine(RootPath, "helmets"), (id, px, w, h) => AddTemplate(_helmets, id, px, w, h, HelmetRows, "helmet"));
            LoadFolder(Path.Combine(RootPath, "armor"), (id, px, w, h) => AddTemplate(_torsos, id, px, w, h, TorsoRows, "armor"));
            VALog.Info("Art loaded: " + _palettes.Count + " palettes, " + _helmets.Count + " helmet templates, " + _torsos.Count + " armor templates");
        }

        private static void AddTemplate(Dictionary<string, PieceTemplate> pTarget, string pId, Color32[] pPixels, int pWidth, int pHeight, int pRows, string pKind)
        {
            PieceTemplate template = PieceTemplate.FromPixels(pId, pPixels, pWidth, pHeight, pRows);
            if (template == null)
            {
                VALog.Warn(pKind + " template " + pId + " must be " + PieceTemplate.Columns + "x" + pRows + " pixels (is " + pWidth + "x" + pHeight + "), skipped");
                return;
            }
            pTarget[pId] = template;
        }

        private static void LoadFolder(string pFolder, Action<string, Color32[], int, int> pOnImage)
        {
            if (!Directory.Exists(pFolder))
            {
                VALog.Warn("Art folder missing: " + pFolder);
                return;
            }
            foreach (string file in Directory.GetFiles(pFolder, "*.png"))
            {
                string id = Path.GetFileNameWithoutExtension(file);
                Texture2D tex = null;
                try
                {
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!tex.LoadImage(File.ReadAllBytes(file)))
                    {
                        VALog.Warn("Could not decode " + file);
                        continue;
                    }
                    pOnImage(id, tex.GetPixels32(), tex.width, tex.height);
                }
                catch (Exception e)
                {
                    VALog.Warn("Could not load " + file + ": " + e.Message);
                }
                finally
                {
                    if (tex != null) UnityEngine.Object.Destroy(tex);
                }
            }
        }

        public static MaterialPalette GetPalette(string pId)
        {
            if (string.IsNullOrEmpty(pId)) return null;
            _palettes.TryGetValue(pId, out MaterialPalette palette);
            return palette;
        }

        public static PieceTemplate GetHelmet(string pId)
        {
            if (string.IsNullOrEmpty(pId)) return null;
            _helmets.TryGetValue(pId, out PieceTemplate template);
            return template;
        }

        public static PieceTemplate GetTorso(string pId)
        {
            if (string.IsNullOrEmpty(pId)) return null;
            _torsos.TryGetValue(pId, out PieceTemplate template);
            return template;
        }

        public static void AddPalette(MaterialPalette pPalette) => _palettes[pPalette.Id] = pPalette;

        public static bool AddTemplate(ArmorSlot pSlot, string pId, Color32[] pPixels, int pWidth, int pHeight)
        {
            if (pSlot == ArmorSlot.Helmet)
            {
                PieceTemplate t = PieceTemplate.FromPixels(pId, pPixels, pWidth, pHeight, HelmetRows);
                if (t == null) return false;
                _helmets[pId] = t;
                return true;
            }
            if (pSlot == ArmorSlot.Armor)
            {
                PieceTemplate t = PieceTemplate.FromPixels(pId, pPixels, pWidth, pHeight, TorsoRows);
                if (t == null) return false;
                _torsos[pId] = t;
                return true;
            }
            return false;
        }
    }
}
