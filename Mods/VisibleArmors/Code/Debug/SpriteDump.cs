using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using VisibleArmors.Core;

namespace VisibleArmors.DebugTools
{
    /// <summary>
    /// Developer tool: exports the game's own unit body and head sprites (plus animation frame data) as PNG files,
    /// so armor art can be drawn against the real pixels. Runs once per session, only when the file
    /// "dump_sprites.flag" exists in the mod folder. Output goes to "debug/dump/" inside the mod folder.
    /// </summary>
    internal static class SpriteDump
    {
        private static bool _done;

        private static string FlagPath => Path.Combine(VisibleArmorsMod.FolderPath ?? string.Empty, "dump_sprites.flag");

        public static void OnWorldLoaded(MonoBehaviour pHost)
        {
            if (_done || VisibleArmorsMod.FolderPath == null || !File.Exists(FlagPath)) return;
            _done = true;
            pHost.StartCoroutine(Run());
        }

        private static IEnumerator Run()
        {
            yield return null;
            string root = Path.Combine(VisibleArmorsMod.FolderPath, "debug", "dump");
            Directory.CreateDirectory(root);
            var info = new StringBuilder();
            try
            {
                info.AppendLine("transparencySortMode=" + GraphicsSettings.transparencySortMode + " axis=" + GraphicsSettings.transparencySortAxis);
                Camera cam = Camera.main;
                if (cam != null) info.AppendLine("camera pos=" + cam.transform.position + " ortho=" + cam.orthographic + " size=" + cam.orthographicSize + " sortMode=" + cam.transparencySortMode);
                Material m = LibraryMaterials.instance.mat_world_object;
                if (m != null) info.AppendLine("mat_world_object shader=" + m.shader.name + " queue=" + m.renderQueue);
                info.AppendLine("atlas texture_size=" + DynamicSpritesConfig.texture_size);
            }
            catch (Exception e)
            {
                info.AppendLine("env error " + e.Message);
            }

            string[] species = { "human", "elf", "orc", "dwarf", "zombie", "civ_cat", "civ_dog", "civ_bear", "civ_chicken", "civ_scorpion",
                "civ_rhino", "bandit", "skeleton", "necromancer", "demon", "alien", "plague_doctor", "fox" };
            foreach (string id in species)
            {
                try
                {
                    DumpSpecies(id, root, info);
                }
                catch (Exception e)
                {
                    info.AppendLine(id + " failed: " + e);
                }
                yield return null;
            }

            // Armor item icons, to match the palettes to the game's own material colours.
            try
            {
                string icons = Path.Combine(root, "_item_icons");
                Directory.CreateDirectory(icons);
                foreach (EquipmentAsset item in AssetManager.items.list)
                {
                    if (item == null || item.id.StartsWith("$") || item.equipment_type == EquipmentType.Weapon) continue;
                    Sprite icon = item.getSprite();
                    info.AppendLine("item " + item.id + " type=" + item.equipment_type + " material=" + item.material + " icon=" + item.path_icon + " readable=" + (icon != null && icon.texture.isReadable));
                    if (icon != null && icon.texture.isReadable) WriteSprite(icon, Path.Combine(icons, item.id + ".png"));
                }
            }
            catch (Exception e)
            {
                info.AppendLine("icons failed: " + e);
            }

            // Other item users, so the species policy can be checked.
            foreach (ActorAsset asset in AssetManager.actor_library.list)
            {
                if (asset == null || !asset.use_items || asset.id.StartsWith("$")) continue;
                info.AppendLine("use_items: " + asset.id + " civ=" + asset.civ + " advanced=" + asset.has_advanced_textures + " atlas=" + asset.texture_atlas
                                + " path=" + (asset.texture_asset != null ? asset.texture_asset.texture_path_main : "-")
                                + " heads=" + (asset.texture_asset != null ? asset.texture_asset.texture_heads : "-"));
            }

            File.WriteAllText(Path.Combine(root, "info.txt"), info.ToString());
            VALog.Info("Sprite dump written to " + root);
        }

        private static void DumpSpecies(string pId, string pRoot, StringBuilder pInfo)
        {
            ActorAsset asset = AssetManager.actor_library.get(pId);
            if (asset == null || asset.texture_asset == null)
            {
                pInfo.AppendLine(pId + ": no asset");
                return;
            }
            ActorTextureSubAsset ta = asset.texture_asset;
            string dir = Path.Combine(pRoot, pId);
            Directory.CreateDirectory(dir);
            pInfo.AppendLine("== " + pId + " base=" + ta.texture_path_base + " main=" + ta.texture_path_main + " advanced=" + ta.has_advanced_textures);
            var paths = new List<string>
            {
                ta.texture_path_main, ta.texture_path_base_male, ta.texture_path_base_female, ta.texture_path_warrior,
                ta.texture_path_king, ta.texture_path_leader, ta.texture_path_baby, ta.texture_path_zombie,
                ta.texture_heads, ta.texture_heads_male, ta.texture_heads_female, ta.texture_head_warrior,
                ta.texture_head_king, ta.texture_heads_old_male, ta.texture_heads_old_female
            };
            bool classic = pId == "human" || pId == "elf" || pId == "orc" || pId == "dwarf";
            if (classic && asset.skin_warrior != null) foreach (string s in asset.skin_warrior) paths.Add(ta.texture_path_base + s);
            if (classic && asset.skin_citizen_male != null) foreach (string s in asset.skin_citizen_male) paths.Add(ta.texture_path_base + s);
            if (classic && asset.skin_citizen_female != null) foreach (string s in asset.skin_citizen_female) paths.Add(ta.texture_path_base + s);
            var seen = new HashSet<string>();
            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path) || !seen.Add(path)) continue;
                Sprite[] sprites = SpriteTextureLoader.getSpriteList(path, true);
                if (sprites == null || sprites.Length == 0)
                {
                    pInfo.AppendLine("  " + path + ": (none)");
                    continue;
                }
                string sub = Path.Combine(dir, path.Replace('/', '_'));
                Directory.CreateDirectory(sub);
                pInfo.AppendLine("  " + path + ": " + sprites.Length + " sprites");
                foreach (Sprite sprite in sprites)
                {
                    pInfo.AppendLine("    " + sprite.name + " rect=" + sprite.rect + " pivot=" + sprite.pivot);
                    WriteSprite(sprite, Path.Combine(sub, sprite.name + ".png"));
                }
                if (!path.Contains("head"))
                {
                    AnimationContainerUnit container = ActorAnimationLoader.getAnimationContainer(path, asset, null, null);
                    foreach (var pair in container.dict_frame_data)
                    {
                        AnimationFrameData fd = pair.Value;
                        pInfo.AppendLine("    frame " + pair.Key + " head=" + fd.pos_head + " head_new=" + fd.pos_head_new + " item=" + fd.pos_item
                                         + " size=" + fd.size_unit + " show_head=" + fd.show_head + " show_item=" + fd.show_item);
                    }
                    pInfo.AppendLine("    walk=" + Names(container.walking) + " idle=" + Names(container.idle) + " swim=" + Names(container.swimming));
                }
            }
        }

        private static string Names(ActorAnimation pAnim)
        {
            if (pAnim == null || pAnim.frames == null) return "-";
            var sb = new StringBuilder();
            foreach (Sprite s in pAnim.frames) sb.Append(s.name).Append(',');
            return sb.ToString();
        }

        private static void WriteSprite(Sprite pSprite, string pFile)
        {
            Rect r = pSprite.rect;
            int w = (int)r.width;
            int h = (int)r.height;
            Color[] px = pSprite.texture.GetPixels((int)r.x, (int)r.y, w, h);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(pFile, tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
        }
    }
}
