using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VisibleArmors.Appearance;
using VisibleArmors.Core;
using VisibleArmors.Render;

namespace VisibleArmors.DebugTools
{
    /// <summary>
    /// Automated in-game test. Runs once per session, only when "selftest.flag" exists in the mod folder.
    /// Spawns a grid of armored units (every species x material), checks that the drawn sprites follow
    /// equipment changes, death, the zoom limit and config switches, opens the unit window, runs a stress test
    /// with many armored units (frame time with the mod on and off) and a cache-eviction test, and takes
    /// screenshots plus a PNG sheet of every composite into debug/selftest/. Results are logged as
    /// "[SelfTest] PASS/FAIL" lines. Test units are removed at the end; if the flag file contains "quit"
    /// the game closes afterwards. It does not save the world.
    /// </summary>
    internal static class SelfTest
    {
        private static bool _started;
        private static int _worldLoads;
        private static int _passed;
        private static int _failed;
        private static readonly List<Actor> _spawned = new List<Actor>();
        private static readonly List<Actor> _grid = new List<Actor>();

        private static string FlagPath => Path.Combine(VisibleArmorsMod.FolderPath ?? string.Empty, "selftest.flag");
        private static string OutDir => Path.Combine(VisibleArmorsMod.FolderPath ?? string.Empty, "debug", "selftest");

        private static readonly string[] Materials = { "leather", "copper", "bronze", "silver", "iron", "steel", "mythril", "adamantine" };
        private static readonly string[] Species = { "human", "elf", "orc", "dwarf" };

        public static void OnWorldLoaded(MonoBehaviour pHost)
        {
            _worldLoads++;
            if (_started || VisibleArmorsMod.FolderPath == null || !File.Exists(FlagPath)) return;
            _started = true;
            pHost.StartCoroutine(Run());
        }

        private static bool FlagSays(string pWord)
        {
            try
            {
                return File.Exists(FlagPath) && File.ReadAllText(FlagPath).Contains(pWord);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void Check(string pName, bool pOk, string pDetail = null)
        {
            if (pOk) _passed++;
            else _failed++;
            string line = "[SelfTest] " + (pOk ? "PASS " : "FAIL ") + pName + (pDetail != null ? " (" + pDetail + ")" : string.Empty);
            if (pOk) VALog.Info(line);
            else VALog.Error(line);
        }

        private static IEnumerator Wait(float pSeconds)
        {
            // Keep the autosave from capturing test units.
            AutoSaveManager.resetAutoSaveTimer();
            float until = Time.realtimeSinceStartup + pSeconds;
            while (Time.realtimeSinceStartup < until) yield return null;
        }

        private static IEnumerator Frames(int pCount)
        {
            for (int i = 0; i < pCount; i++) yield return null;
        }

        private static IEnumerator Shot(string pName)
        {
            Directory.CreateDirectory(OutDir);
            try
            {
                WorldBoxConsole.Console console = UnityEngine.Object.FindObjectOfType<WorldBoxConsole.Console>();
                if (console != null && console.isActive()) console.Hide();
            }
            catch (Exception)
            {
                // the console is optional
            }
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(OutDir, pName + ".png"));
            yield return null;
            yield return null;
            VALog.Info("[SelfTest] screenshot " + pName);
        }

        private static IEnumerator Run()
        {
            VALog.Info("[SelfTest] starting");
            VAConfig.SetDebug(true);
            Compositor.DebugCapture = new List<KeyValuePair<string, Color32[]>>();
            Compositor.DebugCaptureSizes = new List<Vector2Int>();
            yield return Wait(4f);

            // ---------------------------------------------------------------- units from the loaded save
            Actor existing = null;
            int existingArmored = 0;
            foreach (Actor a in World.world.units.getSimpleList())
            {
                if (a == null || !a.isAlive() || Signature.Compute(a, false) == 0) continue;
                existingArmored++;
                if (existing == null) existing = a;
            }
            VALog.Info("[SelfTest] loaded world has " + existingArmored + " units wearing visible gear");
            if (existing != null)
            {
                MoveCamera.instance.focusOn(existing.current_position);
                MoveCamera.instance.forceZoom(12f);
                yield return Wait(2f);
                long want = Signature.Compute(existing, true);
                long have = Compositor.SignatureOf(existing.getLastColoredSprite());
                Check("unit from the save shows its gear", want == have && want != 0, "want=" + want.ToString("X") + " have=" + have.ToString("X") + " " + existing.asset.id);
                yield return Shot("00_existing_unit");
            }

            // ---------------------------------------------------------------- grid of every species x material
            var sets = new List<string[]> { null };
            foreach (string m in Materials) sets.Add(new[] { "helmet_" + m, "armor_" + m, "boots_" + m, "amulet_" + m });
            if (AssetManager.items.get("gold_headband") != null) sets.Add(new[] { "gold_headband", "gold_cloak", "gold_highcleats", "amulet_silver" });
            if (AssetManager.items.get("ice_headband") != null) sets.Add(new[] { "ice_headband", "ice_cloak", "ice_highcleats", null });
            if (AssetManager.items.get("scale_cloak") != null) sets.Add(new[] { "helmet_mythril", "scale_cloak", "scale_highcleats", null });
            // Armor from other mods this mod knows nothing about (ShinobiBox): exercises the icon-colour fallback.
            foreach (string modItem in new[] { "armor_madara_sixpaths_cloak", "armor_shinobi_flak_jacket", "armor_warring_states" })
            {
                if (AssetManager.items.get(modItem) != null) sets.Add(new[] { "helmet_iron", modItem, "boots_leather", null });
            }

            WorldTile origin = FindOpenArea(sets.Count * 2 + 2, Species.Length * 3 + 2);
            Check("found an open area", origin != null);
            if (origin == null)
            {
                Finish();
                yield break;
            }
            Config.paused = true;
            for (int r = 0; r < Species.Length; r++)
            {
                for (int c = 0; c < sets.Count; c++)
                {
                    WorldTile tile = World.world.GetTile(origin.x + 1 + c * 2, origin.y + 1 + (Species.Length - 1 - r) * 3);
                    Actor a = Spawn(Species[r], tile);
                    if (a == null) continue;
                    if (sets[c] != null) foreach (string id in sets[c]) Equip(a, id);
                    _grid.Add(a);
                }
            }
            Check("spawned grid", _grid.Count == Species.Length * sets.Count, _grid.Count + " units");
            Vector3 center = World.world.GetTile(origin.x + sets.Count, origin.y + Species.Length * 3 / 2).posV3;
            MoveCamera.instance.focusOn(center);
            MoveCamera.instance.forceZoom(14f);
            yield return Wait(2f);
            CheckAllMatch("grid sprites match equipment", _grid);
            yield return Shot("01_grid");

            Vector3 left = World.world.GetTile(origin.x + sets.Count / 2, origin.y + Species.Length * 3 / 2).posV3;
            MoveCamera.instance.focusOn(left);
            MoveCamera.instance.forceZoom(10f);
            yield return Wait(1f);
            yield return Shot("02_grid_left_close");
            Vector3 right = World.world.GetTile(origin.x + sets.Count + sets.Count / 2, origin.y + Species.Length * 3 / 2).posV3;
            MoveCamera.instance.focusOn(right);
            MoveCamera.instance.forceZoom(10f);
            yield return Wait(1f);
            yield return Shot("03_grid_right_close");
            MoveCamera.instance.focusOn(center);
            foreach (float zoom in new[] { 35f, 60f })
            {
                MoveCamera.instance.forceZoom(zoom);
                yield return Wait(1f);
                yield return Shot("zoom_" + (int)zoom);
            }
            MoveCamera.instance.forceZoom(14f);
            yield return Wait(1f);

            // ---------------------------------------------------------------- other species (auto profile)
            string[] others = { "civ_cat", "civ_dog", "civ_bear", "civ_chicken", "civ_rhino", "civ_scorpion", "bandit", "necromancer", "alien", "skeleton", "demon", "white_mage" };
            WorldTile area2 = FindOpenArea(others.Length * 2 + 2, 8);
            var otherUnits = new List<Actor>();
            if (area2 != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        Actor a = Spawn(others[i], World.world.GetTile(area2.x + 1 + i * 2, area2.y + 2 + k * 4));
                        if (a == null || a.equipment == null) continue;
                        string m = k == 0 ? "steel" : "adamantine";
                        Equip(a, "helmet_" + m);
                        Equip(a, "armor_" + m);
                        Equip(a, "boots_" + m);
                        otherUnits.Add(a);
                    }
                }
                MoveCamera.instance.focusOn(World.world.GetTile(area2.x + others.Length, area2.y + 4).posV3);
                MoveCamera.instance.forceZoom(12f);
                yield return Wait(2f);
                CheckAllMatch("other species sprites match equipment", otherUnits);
                yield return Shot("03b_other_species");
                foreach (Actor a in otherUnits)
                {
                    if (a == null || !a.isAlive()) continue;
                    VALog.Info("[SelfTest] species " + a.asset.id + ": profile " + SpeciesProfiles.For(a.asset) + ", drawn sig " + SignatureNow(a).ToString("X"));
                }
                MoveCamera.instance.focusOn(center);
                MoveCamera.instance.forceZoom(14f);
                yield return Wait(1f);
            }

            // ---------------------------------------------------------------- equipment changes
            Actor target = FindGrid("human", "helmet_steel");
            if (target != null)
            {
                Item helmet = target.equipment.helmet.getItem();
                target.equipment.helmet.takeAwayItem();
                yield return Frames(3);
                long sig = SignatureNow(target);
                Check("unequipped helmet disappears", Signature.Part(sig, ArmorSlot.Helmet) == 0 && Signature.Part(sig, ArmorSlot.Armor) != 0, "sig=" + sig.ToString("X"));
                yield return Shot("04_helmet_removed");
                if (helmet != null && helmet.isAlive()) target.equipment.setItem(helmet, target);
                else Equip(target, "helmet_steel");
                yield return Frames(3);
                Check("re-equipped helmet appears", Signature.Part(SignatureNow(target), ArmorSlot.Helmet) != 0);
                // Swap material through the slot directly (what the unit window and the AI do).
                Equip(target, "armor_leather");
                yield return Frames(3);
                ArmorLook look = AppearanceRegistry.Get(Signature.Part(SignatureNow(target), ArmorSlot.Armor));
                Check("armor swap updates the look", look != null && look.Palette.Id == "leather", look != null ? look.Style + "/" + look.Palette.Id : "none");
            }
            else
            {
                Check("find steel human", false);
            }

            // ---------------------------------------------------------------- death
            Actor victim = FindGrid("orc", "helmet_iron");
            if (victim != null)
            {
                victim.dieSimpleNone();
                yield return Frames(3);
                Sprite s = victim.getLastColoredSprite();
                Check("dead unit loses its armor", Compositor.SignatureOf(s) == 0, "alive=" + victim.isAlive());
                yield return Shot("05_death");
            }

            // ---------------------------------------------------------------- zoom limit
            // Use a low temporary limit so units are still drawn (past ~80 the game hides units anyway).
            float savedLimit = VAConfig.MaxZoom;
            VAConfig.SetMaxZoom(16f);
            MoveCamera.instance.forceZoom(24f);
            yield return Wait(1.5f);
            Check("zoomed out past the limit", Signature.TooFarAway, "ortho=" + Camera.main.orthographicSize + " limit=16");
            int vanilla = 0;
            int checkedUnits = 0;
            foreach (Actor a in _grid)
            {
                if (a == null || !a.isAlive() || !a.is_visible || !a.current_tile.zone.visible) continue;
                checkedUnits++;
                if (Compositor.SignatureOf(a.getLastColoredSprite()) == 0) vanilla++;
            }
            Check("past the zoom limit units show vanilla sprites", checkedUnits > 0 && vanilla == checkedUnits, vanilla + "/" + checkedUnits + " visible units vanilla");
            yield return Shot("06_past_zoom_limit");
            VAConfig.SetMaxZoom(savedLimit);
            MoveCamera.instance.forceZoom(14f);
            yield return Wait(1.5f);
            CheckAllMatch("armor back after zooming in", _grid);

            // ---------------------------------------------------------------- config switches
            VAConfig.SetShowHelmet(false);
            yield return Frames(4);
            bool noHelmets = true;
            foreach (Actor a in _grid)
            {
                if (a == null || !a.isAlive()) continue;
                if (Signature.Part(Compositor.SignatureOf(a.getLastColoredSprite()), ArmorSlot.Helmet) != 0) noHelmets = false;
            }
            Check("helmet switch hides helmets", noHelmets);
            yield return Shot("07_no_helmets");
            VAConfig.SetShowHelmet(true);
            VAConfig.SetEnabled(false);
            yield return Frames(4);
            int ours = 0;
            foreach (Actor a in _grid) if (a != null && a.isAlive() && Compositor.SignatureOf(a.getLastColoredSprite()) != 0) ours++;
            Check("master switch restores vanilla", ours == 0, ours + " armored sprites left");
            VAConfig.SetEnabled(true);
            yield return Frames(4);
            CheckAllMatch("armor back after re-enabling", _grid);

            // ---------------------------------------------------------------- walking, flipping, attacking
            Config.paused = false;
            for (int i = 0; i < 4; i++)
            {
                yield return Wait(1.2f);
                CheckAllMatch("moving units stay in sync #" + i, _grid);
                if (i % 2 == 0) yield return Shot("08_moving_" + i);
            }
            Config.paused = true;

            // ---------------------------------------------------------------- unit window
            Actor windowUnit = FindGrid("elf", "helmet_adamantine") ?? FindGrid("human", "helmet_mythril");
            if (windowUnit != null && windowUnit.isAlive())
            {
                long before = Compositor.Created;
                bool opened = true;
                try
                {
                    ActionLibrary.openUnitWindow(windowUnit);
                }
                catch (Exception e)
                {
                    opened = false;
                    Check("open unit window", false, e.Message);
                }
                if (opened)
                {
                    yield return Wait(1.5f);
                    Check("unit window built armored avatar sprites", Compositor.Created > before || Compositor.Hits > 0, (Compositor.Created - before) + " new composites");
                    yield return Shot("09_unit_window");
                    try
                    {
                        ScrollWindow.hideAllEvent(false);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                    yield return Wait(0.5f);
                }
            }

            // ---------------------------------------------------------------- save and load round trip (into the mod's debug folder, never a save slot)
            yield return SaveLoad();

            // ---------------------------------------------------------------- export composites (before the stress test, so capture does not skew timings)
            ExportCaptured();

            // ---------------------------------------------------------------- stress test
            if (!FlagSays("nostress")) yield return Stress();

            Cleanup();
            Finish();
        }

        private static IEnumerator SaveLoad()
        {
            var expected = new Dictionary<long, long>();
            var gearBefore = new Dictionary<long, string>();
            foreach (Actor a in _grid)
            {
                if (a == null || !a.isAlive()) continue;
                long sig = Signature.Compute(a, false);
                if (sig == 0) continue;
                expected[a.data.id] = sig;
                gearBefore[a.data.id] = Gear(a);
            }
            string folder = Path.Combine(OutDir, "save_roundtrip");
            bool saved = true;
            try
            {
                SaveManager.saveWorldToDirectory(folder, true, false);
            }
            catch (Exception e)
            {
                saved = false;
                Check("save test world", false, e.Message);
            }
            if (!saved) yield break;
            int loadsBefore = _worldLoads;
            try
            {
                // The game's own load path (the same one the load-slot button uses).
                World.world.save_manager.loadWorld(folder, false);
            }
            catch (Exception e)
            {
                Check("load test world", false, e.Message);
                yield break;
            }
            float until = Time.realtimeSinceStartup + 90f;
            while (_worldLoads == loadsBefore && Time.realtimeSinceStartup < until) yield return null;
            Check("test world reloaded", _worldLoads > loadsBefore);
            yield return Wait(3f);
            Config.paused = true;

            // Actor objects are new after a load: find the grid again by id.
            _grid.Clear();
            _spawned.Clear();
            int matching = 0;
            int dead = 0;
            foreach (Actor a in World.world.units.getSimpleList())
            {
                if (a == null || !expected.TryGetValue(a.data.id, out long sig)) continue;
                _grid.Add(a);
                _spawned.Add(a);
                if (!a.isAlive())
                {
                    // Units that were dying when the save was taken load dead, and dead units correctly show no gear.
                    dead++;
                    continue;
                }
                if (Signature.Compute(a, false) == sig) matching++;
                else VALog.Warn("[SelfTest] after load " + a.asset.id + " #" + a.data.id + ": before [" + gearBefore[a.data.id] + "] after [" + Gear(a) + "] alive=" + a.isAlive());
            }
            if (_grid.Count < expected.Count) VALog.Warn("[SelfTest] " + (expected.Count - _grid.Count) + " armored test units were not found after the load");
            Check("gear survives save/load (derived, nothing stored)", matching == expected.Count - dead && matching > 0,
                matching + "/" + (expected.Count - dead) + " living units (" + dead + " loaded dead)");
            if (_grid.Count > 0)
            {
                MoveCamera.instance.focusOn(_grid[_grid.Count / 2].current_position);
                MoveCamera.instance.forceZoom(14f);
                yield return Wait(2f);
                CheckAllMatch("reloaded units draw their gear", _grid);
                yield return Shot("11_after_reload");
            }
            try
            {
                Directory.Delete(folder, true);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static IEnumerator Stress()
        {
            int target = 800;
            WorldTile area = FindOpenArea(44, 44, 0.6f);
            if (area == null)
            {
                Check("stress area", false);
                yield break;
            }
            var rnd = new System.Random(7);
            for (int i = 0, tries = 0; i < target && tries < target * 4; tries++)
            {
                WorldTile t = World.world.GetTile(area.x + rnd.Next(44), area.y + rnd.Next(44));
                if (t == null || t.Type == null || t.Type.liquid || t.Type.block) continue;
                i++;
                Actor a = Spawn("human", t);
                if (a == null) continue;
                string m = Materials[rnd.Next(Materials.Length)];
                Equip(a, "helmet_" + Materials[rnd.Next(Materials.Length)]);
                Equip(a, "armor_" + m);
                Equip(a, "boots_" + m);
            }
            MoveCamera.instance.focusOn(World.world.GetTile(area.x + 22, area.y + 22).posV3);
            MoveCamera.instance.forceZoom(30f);
            Config.paused = false;
            yield return Wait(10f);

            // Alternate short windows so both settings see the same crowd.
            float onSum = 0f, offSum = 0f;
            double scanSum = 0;
            for (int round = 0; round < 3; round++)
            {
                yield return Measure("mod on #" + round, true, 3f);
                onSum += _lastFrameMs;
                scanSum += _lastScanMs;
                if (round == 0) yield return Shot("10_stress");
                VAConfig.SetEnabled(false);
                yield return Frames(5);
                yield return Measure("mod off #" + round, false, 3f);
                offSum += _lastFrameMs;
                VAConfig.SetEnabled(true);
                yield return Wait(1.5f);
            }
            float on = onSum / 3f;
            float off = offSum / 3f;
            double scanOn = scanSum / 3;

            ArmorAtlas.UseMipmaps = true;
            RenderSync.RefreshAll();
            yield return Wait(4f);
            yield return Measure("mod on, with mipmaps", true, 3f);
            float onNoMips = _lastFrameMs;
            ArmorAtlas.UseMipmaps = false;
            RenderSync.RefreshAll();
            yield return Wait(1f);

            VALog.Info("[SelfTest] STRESS " + _spawned.Count + " units spawned, visible=" + RenderSync.LastVisible + " armored=" + RenderSync.LastArmored
                       + " | frame ms on=" + on.ToString("F2") + " off=" + off.ToString("F2") + " on(with mipmaps)=" + onNoMips.ToString("F2")
                       + " | scan ms/frame=" + scanOn.ToString("F3")
                       + " | composites=" + Compositor.CachedCount + " pages=" + Compositor.Atlas.PageCount + " (~" + (Compositor.Atlas.ApproxBytes / (1024 * 1024)) + " MB)"
                       + " | compose ms avg=" + (Compositor.Created > 0 ? (Compositor.ComposeMsTotal / Compositor.Created).ToString("F3") : "-")
                       + " | upload ms avg=" + (Compositor.Uploads > 0 ? (Compositor.UploadMsTotal / Compositor.Uploads).ToString("F3") : "-"));
            Check("stress: armored units in view", RenderSync.LastArmored > 50, RenderSync.LastArmored + " armored visible");

            // Eviction with a tiny cache: everything must keep rendering.
            int pages = VAConfig.CachePages;
            long evictedBefore = Compositor.Evicted;
            VAConfig.SetCachePages(2);
            MoveCamera.instance.forceZoom(18f);
            for (int i = 0; i < 6; i++)
            {
                MoveCamera.instance.focusOn(World.world.GetTile(area.x + 6 + (i % 3) * 16, area.y + 6 + (i / 3) * 30).posV3);
                yield return Wait(1f);
            }
            int mismatched = CountMismatched(_spawned);
            Check("eviction keeps sprites valid", mismatched == 0, "evicted " + (Compositor.Evicted - evictedBefore) + " sprites, pages=" + Compositor.Atlas.PageCount + ", mismatched=" + mismatched);
            VAConfig.SetCachePages(pages);
            Config.paused = true;
        }

        private static float _lastFrameMs;
        private static double _lastScanMs;

        private static IEnumerator Measure(string pLabel, bool pScan, float pSeconds = 5f)
        {
            int frames = 0;
            float total = 0f;
            double scan0 = RenderSync.ScanMsTotal;
            long scanFrames0 = RenderSync.ScanFrames;
            long created0 = Compositor.Created;
            double compose0 = Compositor.ComposeMsTotal;
            long uploads0 = Compositor.Uploads;
            double upload0 = Compositor.UploadMsTotal;
            long postTicks0 = Patches.SpriteUnitPatch.Ticks;
            long postCalls0 = Patches.SpriteUnitPatch.Calls;
            AutoSaveManager.resetAutoSaveTimer();
            float until = Time.realtimeSinceStartup + pSeconds;
            while (Time.realtimeSinceStartup < until)
            {
                yield return null;
                total += Time.unscaledDeltaTime;
                frames++;
            }
            _lastFrameMs = frames > 0 ? total / frames * 1000f : 0f;
            long sf = RenderSync.ScanFrames - scanFrames0;
            _lastScanMs = sf > 0 ? (RenderSync.ScanMsTotal - scan0) / sf : 0;
            VALog.Info("[SelfTest] measure " + pLabel + ": " + frames + " frames, " + _lastFrameMs.ToString("F2") + " ms/frame"
                       + (pScan ? ", scan " + _lastScanMs.ToString("F3") + " ms/frame" : string.Empty)
                       + ", composes " + (Compositor.Created - created0) + " (" + ((Compositor.ComposeMsTotal - compose0) / Math.Max(1, frames)).ToString("F3") + " ms/frame)"
                       + ", uploads " + (Compositor.Uploads - uploads0) + " (" + ((Compositor.UploadMsTotal - upload0) / Math.Max(1, frames)).ToString("F3") + " ms/frame)"
                       + ", postfix " + (Patches.SpriteUnitPatch.Calls - postCalls0) + " calls ("
                       + ((Patches.SpriteUnitPatch.Ticks - postTicks0) * 1000.0 / System.Diagnostics.Stopwatch.Frequency / Math.Max(1, frames)).ToString("F3") + " ms/frame)"
                       + ", visible " + RenderSync.LastVisible + ", armored " + RenderSync.LastArmored);
        }

        // ------------------------------------------------------------------------------------ helpers

        private static long SignatureNow(Actor pActor) => Compositor.SignatureOf(pActor.getLastColoredSprite());

        private static int CountMismatched(List<Actor> pUnits)
        {
            int bad = 0;
            foreach (Actor a in pUnits)
            {
                if (a == null || !a.isAlive() || !a.is_visible || a.current_tile == null || !a.current_tile.zone.visible) continue;
                long want = Signature.Compute(a, true);
                Sprite s = a.getLastColoredSprite();
                if (s == null) continue;
                long have = Compositor.SignatureOf(s);
                if (want != have && !(have == 0 && Compositor.HasFailed(s, want))) bad++;
            }
            return bad;
        }

        private static void CheckAllMatch(string pName, List<Actor> pUnits)
        {
            int armored = 0;
            foreach (Actor a in pUnits) if (a != null && a.isAlive() && Signature.Compute(a, true) != 0) armored++;
            int bad = CountMismatched(pUnits);
            Check(pName, bad == 0 && armored > 0, armored + " armored, " + bad + " mismatched");
        }

        private static string Gear(Actor pActor)
        {
            if (pActor.equipment == null) return "no equipment";
            var sb = new System.Text.StringBuilder();
            foreach (ActorEquipmentSlot slot in pActor.equipment)
            {
                Item item = slot.getItem();
                if (item != null) sb.Append(slot.type).Append('=').Append(item.getAsset() != null ? item.getAsset().id : "?").Append(' ');
            }
            return sb.ToString().Trim();
        }

        private static Actor FindGrid(string pSpecies, string pHelmet)
        {
            foreach (Actor a in _grid)
            {
                if (a == null || !a.isAlive() || a.asset.id != pSpecies) continue;
                Item h = a.equipment?.helmet.getItem();
                if (h != null && h.getAsset().id == pHelmet) return a;
            }
            return null;
        }

        private static Actor Spawn(string pId, WorldTile pTile)
        {
            if (pTile == null || AssetManager.actor_library.get(pId) == null) return null;
            try
            {
                Actor a = World.world.units.spawnNewUnit(pId, pTile, false, false, 0f, null, false, true);
                if (a != null) _spawned.Add(a);
                return a;
            }
            catch (Exception e)
            {
                VALog.Warn("[SelfTest] spawn " + pId + " failed: " + e.Message);
                return null;
            }
        }

        private static void Equip(Actor pActor, string pItemId)
        {
            if (pItemId == null || pActor.equipment == null) return;
            EquipmentAsset asset = AssetManager.items.get(pItemId);
            if (asset == null) return;
            Item item;
            try
            {
                item = World.world.items.generateItem(asset, pActor.kingdom, "VisibleArmors SelfTest", 1, pActor, 0, true);
            }
            catch (Exception e)
            {
                // Some mod items break the game's random-modifier naming; a plain item is enough for the test.
                VALog.Warn("[SelfTest] generateItem(" + pItemId + ") failed in the game (" + e.GetType().Name + "), using a plain item");
                item = World.world.items.newItem(asset);
                item.data.asset_id = asset.id;
                item.initItem();
            }
            if (item != null) pActor.equipment.setItem(item, pActor);
        }

        private static WorldTile FindOpenArea(int pWidth, int pHeight, float pMinLand = 1f)
        {
            int w = MapBox.width;
            int h = MapBox.height;
            Vector3 cam = Camera.main != null ? Camera.main.transform.position : new Vector3(w / 2f, h / 2f);
            int cx = Mathf.Clamp((int)cam.x, 0, w - 1);
            int cy = Mathf.Clamp((int)cam.y, 0, h - 1);
            for (int radius = 0; radius < Math.Max(w, h); radius += 4)
            {
                for (int k = 0; k < 16; k++)
                {
                    float angle = k * Mathf.PI / 8f;
                    int x0 = cx + (int)(Mathf.Cos(angle) * radius) - pWidth / 2;
                    int y0 = cy + (int)(Mathf.Sin(angle) * radius) - pHeight / 2;
                    if (IsOpen(x0, y0, pWidth, pHeight, pMinLand)) return World.world.GetTile(x0, y0);
                    if (radius == 0) break;
                }
            }
            return null;
        }

        private static bool IsOpen(int pX, int pY, int pWidth, int pHeight, float pMinLand)
        {
            if (pX < 0 || pY < 0 || pX + pWidth >= MapBox.width || pY + pHeight >= MapBox.height) return false;
            int land = 0;
            int allowedWater = (int)((1f - pMinLand) * pWidth * pHeight);
            for (int y = pY; y < pY + pHeight; y++)
            {
                for (int x = pX; x < pX + pWidth; x++)
                {
                    WorldTile t = World.world.GetTile(x, y);
                    if (t == null || t.Type == null || t.Type.liquid || t.Type.block)
                    {
                        if (--allowedWater < 0) return false;
                        continue;
                    }
                    land++;
                }
            }
            return land > 0;
        }

        private static void ExportCaptured()
        {
            List<KeyValuePair<string, Color32[]>> captured = Compositor.DebugCapture;
            List<Vector2Int> sizes = Compositor.DebugCaptureSizes;
            Compositor.DebugCapture = null;
            Compositor.DebugCaptureSizes = null;
            if (captured == null || captured.Count == 0) return;
            const int cell = 16;
            const int scale = 6;
            int count = Math.Min(captured.Count, 600);
            int columns = 30;
            int rows = (count + columns - 1) / columns;
            var tex = new Texture2D(columns * cell * scale, rows * cell * scale, TextureFormat.RGBA32, false);
            var fill = new Color32[tex.width * tex.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(48, 52, 60, 255);
            for (int i = 0; i < count; i++)
            {
                Color32[] px = captured[i].Value;
                Vector2Int size = sizes[i];
                int ox = (i % columns) * cell + 1;
                int oy = (rows - 1 - i / columns) * cell + 1;
                for (int y = 0; y < size.y && y < cell - 1; y++)
                {
                    for (int x = 0; x < size.x && x < cell - 1; x++)
                    {
                        Color32 c = px[x + y * size.x];
                        if (c.a == 0) continue;
                        for (int sy = 0; sy < scale; sy++)
                            for (int sx = 0; sx < scale; sx++)
                                fill[(ox + x) * scale + sx + ((oy + y) * scale + sy) * tex.width] = c;
                    }
                }
            }
            tex.SetPixels32(fill);
            tex.Apply();
            Directory.CreateDirectory(OutDir);
            File.WriteAllBytes(Path.Combine(OutDir, "composites.png"), tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            VALog.Info("[SelfTest] exported " + count + " of " + captured.Count + " composites to composites.png");
        }

        private static void Cleanup()
        {
            foreach (Actor a in _spawned)
            {
                try
                {
                    if (a != null && a.isAlive()) a.dieAndDestroy(AttackType.None);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            _spawned.Clear();
            _grid.Clear();
            Config.paused = false;
        }

        private static void Finish()
        {
            VALog.Info("[SelfTest] SUMMARY " + _passed + " passed, " + _failed + " failed | composites created=" + Compositor.Created
                       + " hits=" + Compositor.Hits + " failures=" + Compositor.Failures + " evicted=" + Compositor.Evicted
                       + " looks=" + AppearanceRegistry.LookCount + " invalidations=" + RenderSync.Invalidations);
            try
            {
                if (File.Exists(FlagPath) && File.ReadAllText(FlagPath).Contains("quit"))
                {
                    VALog.Info("[SelfTest] quitting the game as requested by selftest.flag");
                    Application.Quit();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
