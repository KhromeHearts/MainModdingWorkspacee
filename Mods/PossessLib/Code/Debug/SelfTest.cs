using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Persistence;
using PossessLib.Sync;
using PossessLib.UI;
using UnityEngine;

namespace PossessLib.DebugTools
{
    /// <summary>
    /// Automated in-game test of the main flows (possess, inventory, icons, weapon sync, cursed weapon,
    /// equip, split/merge, trash, save/load round trip, eating, ground items, crafting, unit window,
    /// release, death). Runs once per game session, only when the file "selftest.flag" exists in the mod
    /// folder. Results are written to Player.log as "[PossessLib] [SelfTest] PASS/FAIL ..." lines.
    /// If the flag file contains the word "quit", the game closes when the test is done.
    /// </summary>
    internal static class SelfTest
    {
        private static bool _started;
        private static int _passed;
        private static int _failed;

        private static string FlagPath => Path.Combine(PossessLibMod.FolderPath ?? string.Empty, "selftest.flag");

        private static int _worldLoads;

        private static int CountStacks(Inventory pInv)
        {
            if (pInv == null) return -1;
            int n = 0;
            for (int i = 0; i < pInv.Capacity; i++) if (pInv.Get(i) != null) n++;
            return n;
        }

        public static void OnWorldLoaded(MonoBehaviour pHost)
        {
            _worldLoads++;
            if (_started || PossessLibMod.FolderPath == null || !File.Exists(FlagPath)) return;
            _started = true;
            pHost.StartCoroutine(Run());
        }

        /// <summary>Logs a screenshot marker (the external test runner captures the window when it sees it).</summary>
        private static void ScreenshotMarker(string pName)
        {
            try
            {
                WorldBoxConsole.Console console = UnityEngine.Object.FindObjectOfType<WorldBoxConsole.Console>();
                if (console != null && console.isActive()) console.Hide();
            }
            catch (Exception)
            {
                // console is optional
            }
            PLLog.Info("[SelfTest] SCREENSHOT " + pName);
        }

        private static void Check(string pName, bool pOk, string pDetail = null)
        {
            if (pOk)
            {
                _passed++;
                PLLog.Info("[SelfTest] PASS " + pName + (pDetail != null ? " (" + pDetail + ")" : string.Empty));
            }
            else
            {
                _failed++;
                PLLog.Error("[SelfTest] FAIL " + pName + (pDetail != null ? " (" + pDetail + ")" : string.Empty));
            }
        }

        private static IEnumerator Wait(float pSeconds)
        {
            float until = Time.realtimeSinceStartup + pSeconds;
            while (Time.realtimeSinceStartup < until) yield return null;
        }

        private static IEnumerator Run()
        {
            PLLog.Info("[SelfTest] starting");
            PLConfig.SetDebug(true);
            Compat.Check();
            if (Compat.MainCharacterModActive)
            {
                // In memory only: pause MainCharacterMod's manager object so it does not fight over the hand.
                _oldModObject = GameObject.Find("MainCharacterModManager");
                if (_oldModObject != null && _oldModObject.activeSelf)
                {
                    _oldModObject.SetActive(false);
                    PLLog.Info("[SelfTest] Main Character Mod is enabled; its manager object is paused for the test and restored afterwards");
                }
            }
            yield return Wait(4f);
            Actor actor = null;
            try
            {
                actor = SpawnTestUnit();
            }
            catch (Exception e)
            {
                Check("spawn test unit", false, e.Message);
            }
            if (actor == null)
            {
                Finish();
                yield break;
            }
            Check("spawn test unit", true, actor.asset.id + " can use items: " + actor.canUseItems());
            yield return Wait(0.5f);

            // ---- possession
            ControllableUnit.setControllableCreature(actor);
            yield return Wait(0.5f);
            Check("possession detected", ReferenceEquals(PossessionController.Current, actor));
            Check("HUD built", Hud.IsBuilt);
            yield return Wait(0.5f);
            PLLog.Info("[SelfTest] layout: " + Hud.DescribeLayout());
            try
            {
                PossessionUnitInfo info = PossessionUI.instance != null ? PossessionUI.instance.GetComponentInChildren<PossessionUnitInfo>(true) : null;
                PLLog.Info("[SelfTest] native possession UI: inner active=" + (PossessionUI.instance != null && PossessionUI.instance._inner != null && PossessionUI.instance._inner.activeInHierarchy)
                           + " unitInfo active=" + (info != null && info.gameObject.activeInHierarchy)
                           + " mover visible=" + (PossessionUI.instance != null && PossessionUI.instance._mover != null && PossessionUI.instance._mover.visible));
            }
            catch (Exception e)
            {
                PLLog.Info("[SelfTest] native possession UI: " + e.Message);
            }
            Inventory inv = PossessionController.CurrentInventory;
            Check("inventory created", inv != null, inv != null ? inv.HotbarSize + "+" + inv.MainSize + " slots" : null);
            if (inv == null)
            {
                Finish();
                yield break;
            }

            // ---- resources follow the bag
            actor.addToInventory("wood", 30);
            yield return Wait(0.4f);
            Check("bag resources appear in slots", inv.CountResource("wood") == 30, "wood=" + inv.CountResource("wood"));

            // ---- items show with icons
            Item sword = MakeItem(actor, EquipmentType.Weapon, null);
            Item axe = MakeItem(actor, EquipmentType.Weapon, sword != null ? sword.getAsset().id : null);
            Item helmet = MakeItem(actor, EquipmentType.Helmet, null);
            Check("generate test items", sword != null && axe != null && helmet != null);
            if (sword == null || axe == null || helmet == null)
            {
                Finish();
                yield break;
            }
            bool added = PossessLibApi.AddItem(actor, sword) && PossessLibApi.AddItem(actor, axe) && PossessLibApi.AddItem(actor, helmet);
            Check("API AddItem", added);
            int swordSlot = inv.IndexOf(sword);
            int axeSlot = inv.IndexOf(axe);
            yield return null;
            yield return null;
            Check("item stack has an icon", inv.Get(swordSlot) != null && inv.Get(swordSlot).GetIcon() != null);
            SlotView view = Hud.GetSlotView(swordSlot);
            Check("hotbar slot shows the icon", view != null && view.IconVisible, view == null ? "no view" : null);

            // ---- weapon follows the selected slot
            WatchWeaponSlot(actor);
            PLLog.Info("[SelfTest] slots: sword=" + swordSlot + " axe=" + axeSlot + " selected=" + inv.Selected
                       + " hand=" + (actor.equipment.weapon.getItem() != null ? actor.equipment.weapon.getItem().getName(true) : "empty"));
            InventoryActions.SelectHotbar(inv, swordSlot);
            yield return null;
            yield return null;
            Item handNow = actor.equipment.weapon.getItem();
            Check("selected weapon is equipped", ReferenceEquals(handNow, sword),
                "selected=" + inv.Selected + " stack=" + inv.SelectedStack + " hand=" + (handNow != null ? handNow.getName(true) : "empty")
                + " sword cursed=" + sword.isCursed() + " pinned=" + (inv.PinnedCursedWeapon != null) + " sameInv=" + ReferenceEquals(inv, PossessionController.CurrentInventory));
            int woodSlot = FindSlot(inv, "wood");
            InventoryActions.SelectHotbar(inv, woodSlot >= 0 && woodSlot < inv.HotbarSize ? woodSlot : inv.FirstEmpty(true));
            yield return null;
            yield return null;
            Check("non-weapon selection empties the hand", actor.equipment.weapon.isEmpty());
            Check("weapon kept alive while unequipped", sword.isAlive() && InventoryRegistry.IsProtected(sword));

            // ---- cursed weapon (the old mod's bug)
            axe.addMod("cursed");
            InventoryActions.SelectHotbar(inv, axeSlot);
            yield return null;
            yield return null;
            Check("cursed weapon equips", ReferenceEquals(actor.equipment.weapon.getItem(), axe));
            InventoryActions.SelectHotbar(inv, swordSlot);
            yield return null;
            Check("cannot switch away from a cursed weapon to another weapon", inv.Selected == axeSlot && ReferenceEquals(actor.equipment.weapon.getItem(), axe));
            int emptySlot = FirstEmptyHotbar(inv);
            if (emptySlot >= 0)
            {
                InventoryActions.SelectHotbar(inv, emptySlot);
                yield return null;
                yield return null;
                Check("cursed weapon stays in hand and is reported", ReferenceEquals(actor.equipment.weapon.getItem(), axe) && ReferenceEquals(inv.PinnedCursedWeapon, axe));
            }
            axe.removeMod("cursed");
            yield return null;
            yield return null;
            Check("hand follows selection again once uncursed", actor.equipment.weapon.isEmpty() && inv.PinnedCursedWeapon == null);

            // ---- armor
            int helmetSlot = inv.IndexOf(helmet);
            bool equipped = InventoryOps.EquipFromSlot(inv, helmetSlot);
            Check("equip helmet from inventory", equipped && ReferenceEquals(actor.equipment.helmet.getItem(), helmet) && inv.IndexOf(helmet) < 0);
            bool unequipped = InventoryOps.Unequip(inv, EquipmentType.Helmet, false);
            Check("unequip helmet to inventory", unequipped && actor.equipment.helmet.isEmpty() && inv.IndexOf(helmet) >= 0);

            // ---- split / merge keep the bag total
            woodSlot = FindSlot(inv, "wood");
            InventoryActions.PickUp(inv, woodSlot, 15);
            Check("split half to cursor", CursorState.Stack != null && CursorState.Stack.Count == 15 && inv.Get(woodSlot).Count == 15);
            int target = inv.FirstEmpty(false);
            InventoryActions.PlaceHeld(inv, target);
            yield return Wait(0.3f);
            Check("place split stack", CursorState.IsEmpty && inv.CountResource("wood") == 30 && actor.data.inventory.getResource("wood") == 30);
            InventoryActions.PickUp(inv, target, 15);
            InventoryActions.PlaceHeld(inv, woodSlot);
            Check("merge back", CursorState.IsEmpty && inv.Get(woodSlot) != null && inv.Get(woodSlot).Count == 30);

            // ---- trash and take back
            InventoryActions.PickUp(inv, woodSlot, 5);
            ItemStack five = CursorState.Stack;
            CursorState.Clear();
            InventoryOps.Trash(inv, five, inv);
            yield return Wait(0.3f);
            Check("trash removes from the bag", actor.data.inventory.getResource("wood") == 25 && inv.CountResource("wood") == 25);
            ItemStack back = InventoryOps.TakeFromTrash(inv);
            inv.Insert(back);
            yield return Wait(0.3f);
            Check("take back from trash", actor.data.inventory.getResource("wood") == 30 && inv.CountResource("wood") == 30);

            // ---- save / load round trip (same world)
            string json = InventorySerializer.Serialize(inv);
            InventoryDto dto = JsonConvert.DeserializeObject<InventoryDto>(json);
            Check("serialize inventory", dto != null && dto.Slots.Count > 0 && json.Contains(sword.data.id.ToString()), json.Length + " chars");
            WorldPersistence.BeforeSave();
            actor.data.get(InventorySerializer.ActorKey, out string saved, null);
            Check("written to actor custom data", !string.IsNullOrEmpty(saved));
            int swordIndexBefore = inv.IndexOf(sword);
            InventoryRegistry.Unregister(inv);
            WorldPersistence.AfterActorLoaded(actor);
            Inventory reloaded = InventoryRegistry.Get(actor);
            Check("reload from custom data", reloaded != null && reloaded.IndexOf(sword) == swordIndexBefore && reloaded.CountResource("wood") == 30);
            yield return null;
            inv = PossessionController.CurrentInventory ?? reloaded;

            // ---- eating
            actor.addToInventory("bread", 3);
            yield return Wait(0.4f);
            int breadSlot = FindSlot(inv, "bread");
            if (breadSlot >= inv.HotbarSize)
            {
                int free = FirstEmptyHotbar(inv);
                if (free >= 0) inv.Swap(breadSlot, free);
                breadSlot = FindSlot(inv, "bread");
            }
            actor.setHealth(Mathf.Max(1, actor.getMaxHealth() / 3), true);
            int hpBefore = actor.getHealth();
            InventoryActions.SelectHotbar(inv, breadSlot);
            yield return Wait(1f);
            bool ate = Consumables.TryUse(actor, inv, breadSlot);
            yield return null;
            Check("eat bread", ate && actor.data.inventory.getResource("bread") == 2 && actor.getHealth() > hpBefore,
                "hp " + hpBefore + " -> " + actor.getHealth());

            // ---- ground items
            int groundBefore = GroundItems.Count;
            int woodBefore = actor.data.inventory.getResource("wood");
            woodSlot = FindSlot(inv, "wood");
            ItemStack dropStack = inv.Get(woodSlot);
            int dropped = dropStack != null ? dropStack.Count : 0;
            inv.SetSlot(woodSlot, null);
            InventoryOps.Drop(dropStack, inv, actor.current_position);
            Check("drop on ground", GroundItems.Count == groundBefore + 1 && actor.data.inventory.getResource("wood") == woodBefore - dropped,
                "dropped " + dropped);
            yield return Wait(2.5f);
            Check("pick up from ground", GroundItems.Count == groundBefore && actor.data.inventory.getResource("wood") == woodBefore,
                "ground=" + GroundItems.Count + " wood=" + actor.data.inventory.getResource("wood"));

            // ---- crafting
            Check("recipes available", Crafting.All.Count > 0, Crafting.All.Count + " recipes");
            RecipeDefinition stick = null;
            foreach (RecipeDefinition r in Crafting.All) if (r.Id == "possesslib:wooden_stick") stick = r;
            if (stick != null && Crafting.IsVisible(stick))
            {
                ItemStack crafted = Crafting.Craft(actor, inv, stick);
                Check("craft from data recipe", crafted != null && crafted.Item != null && inv.IndexOf(crafted.Item) >= 0 && actor.data.inventory.getResource("wood") == 27);
            }

            // ---- custom items through the API
            PossessLibApi.RegisterCustomItem(new CustomItemDefinition { Id = "possesslib:selftest_token", Name = "Self-test token", MaxStack = 10 });
            int left = PossessLibApi.AddCustomItem(actor, "possesslib:selftest_token", 3);
            Check("custom item via API", left == 0 && PossessLibApi.CountCustomItem(actor, "possesslib:selftest_token") == 3);
            PossessLibApi.RemoveCustomItem(actor, "possesslib:selftest_token", 3);

            // ---- inventory open / close (kept open a few seconds so a screenshot can be taken)
            PossessionController.SetInventoryOpen(true);
            yield return Wait(0.8f);
            Check("inventory opens", PossessionController.InventoryOpen);
            ScreenshotMarker("inventory_open");
            yield return Wait(4f);
            PossessionController.SetInventoryOpen(false);
            yield return Wait(0.5f);
            ScreenshotMarker("hud_closed");
            yield return Wait(3f);

            // ---- release
            ControllableUnit.clear(false);
            yield return Wait(0.5f);
            Check("release detected", PossessionController.Current == null && !PossessionController.InventoryOpen);
            Check("AI gets its best weapon back", !actor.equipment.weapon.isEmpty());

            // ---- unit window -> Inventory button -> PossessLib window (the real user flow, not possessed)
            SelectedUnit.select(actor, true);
            ScrollWindow.showWindow("unit");
            yield return Wait(1.5f);
            ScrollWindow unitWindow = ScrollWindow._all_windows.TryGetValue("unit", out ScrollWindow uw) ? uw : null;
            Transform button = unitWindow != null ? unitWindow.transform.Find("Background/PossessLib_InventoryButton") : null;
            Check("inventory button added to the unit window", button != null,
                "unit window active=" + (unitWindow != null && unitWindow.gameObject.activeInHierarchy));
            if (button != null)
            {
                UnityEngine.UI.Button b = button.GetComponent<UnityEngine.UI.Button>();
                if (b != null) b.onClick.Invoke();
                yield return Wait(2.5f);
                ScreenshotMarker("unit_window");
                yield return Wait(3f);
                UnitInventoryWindow w = UnitInventoryWindow.Instance;
                Check("unit inventory window opens from the button", w != null && w.gameObject.activeInHierarchy,
                    "instance=" + (w != null) + " activeSelf=" + (w != null && w.gameObject.activeSelf)
                    + " animating=" + ScrollWindow.isAnimationActive() + " windowActive=" + ScrollWindow.isWindowActive());
                SlotView first = w != null ? w.GetComponentInChildren<SlotView>(true) : null;
                Check("unit inventory window shows slots", first != null && first.Address.Inventory != null);
            }
            ScrollWindow.hideAllEvent(true);
            SelectedUnit.clear();
            yield return Wait(0.8f);

            // ---- real world save to disk + reload through the game's loader (temp folder, not the save slots)
            {
                long actorId = actor.data.id;
                long swordId = sword.data.id;
                Inventory before = InventoryRegistry.Get(actor);
                int diskWoodBefore = before != null ? before.CountResource("wood") : -1;
                int pouchesBefore = before != null ? before.CountCustom("possesslib_example:herb_pouch") : -1;
                int stacksBefore = CountStacks(before);
                string folder = Path.Combine(Application.temporaryCachePath, "possesslib_selftest_world");
                bool diskSaved = false;
                try
                {
                    SaveManager.saveWorldToDirectory(folder, true, true);
                    diskSaved = File.Exists(Path.Combine(folder, "map.wbox"));
                }
                catch (Exception e)
                {
                    PLLog.Info("[SelfTest] save failed: " + e.Message);
                }
                Check("world saved to disk", diskSaved, folder);
                if (diskSaved)
                {
                    int loadsBefore = _worldLoads;
                    World.world.save_manager.loadWorld(folder, false);
                    float until = Time.realtimeSinceStartup + 40f;
                    while (_worldLoads == loadsBefore && Time.realtimeSinceStartup < until) yield return null;
                    yield return Wait(1f);
                    Actor diskReloaded = World.world.units.get(actorId);
                    Inventory after = diskReloaded != null ? InventoryRegistry.Get(diskReloaded) : null;
                    Item swordAfter = World.world.items.get(swordId);
                    Check("world reloaded", diskReloaded != null && _worldLoads > loadsBefore);
                    Check("inventory survives a real save and load", after != null && CountStacks(after) == stacksBefore
                        && after.CountResource("wood") == diskWoodBefore && after.CountCustom("possesslib_example:herb_pouch") == pouchesBefore,
                        "stacks " + stacksBefore + " -> " + CountStacks(after) + ", wood " + diskWoodBefore + " -> " + (after != null ? after.CountResource("wood") : -1));
                    Check("carried items survive the reload", swordAfter != null && swordAfter.isAlive() && after != null
                        && (after.IndexOf(swordAfter) >= 0 || ItemOwnership.IsEquippedBy(diskReloaded, swordAfter)));
                    if (diskReloaded != null) actor = diskReloaded;
                    try { Directory.Delete(folder, true); }
                    catch (Exception) { /* temp folder */ }
                }
            }

            // ---- death
            int groundBeforeDeath = GroundItems.Count;
            ControllableUnit.setControllableCreature(actor);
            yield return Wait(0.5f);
            actor.getHitFullHealth(AttackType.Other);
            yield return Wait(1.5f);
            Check("death ends possession", PossessionController.Current == null);
            Check("dead unit's inventory released", InventoryRegistry.Get(actor) == null);
            Check("dead unit's items dropped", GroundItems.Count > groundBeforeDeath, "ground " + groundBeforeDeath + " -> " + GroundItems.Count);
            Finish();
        }

        // ---- diagnostics: who removes the weapon from the hand? (runtime patches, only during the self-test)
        private static ActorEquipmentSlot _watchedSlot;
        private static bool _watching;

        private static void WatchWeaponSlot(Actor pActor)
        {
            _watchedSlot = pActor.equipment.weapon;
            if (_watching) return;
            _watching = true;
            try
            {
                HarmonyLib.Harmony h = PossessLibMod.HarmonyInstance;
                h.Patch(HarmonyLib.AccessTools.Method(typeof(ActorEquipmentSlot), nameof(ActorEquipmentSlot.takeAwayItem)),
                    prefix: new HarmonyLib.HarmonyMethod(typeof(SelfTest), nameof(WatchTakeAway)) { priority = HarmonyLib.Priority.First });
                h.Patch(HarmonyLib.AccessTools.Method(typeof(ActorEquipmentSlot), nameof(ActorEquipmentSlot.setItem)),
                    prefix: new HarmonyLib.HarmonyMethod(typeof(SelfTest), nameof(WatchSetItem)) { priority = HarmonyLib.Priority.First });
                h.Patch(HarmonyLib.AccessTools.Method(typeof(ActorEquipmentSlot), nameof(ActorEquipmentSlot.setEmptyDebug)),
                    prefix: new HarmonyLib.HarmonyMethod(typeof(SelfTest), nameof(WatchTakeAway)) { priority = HarmonyLib.Priority.First });
            }
            catch (Exception e)
            {
                PLLog.Exception("[SelfTest] could not install weapon watch", e);
            }
        }

        private static void WatchTakeAway(ActorEquipmentSlot __instance)
        {
            if (!ReferenceEquals(__instance, _watchedSlot) || __instance.getItem() == null) return;
            PLLog.Info("[SelfTest] weapon slot cleared (guard=" + EquipmentGuard.Active + ") item=" + __instance.getItem().getName(true)
                       + "\n" + Environment.StackTrace);
        }

        private static void WatchSetItem(ActorEquipmentSlot __instance, Item pItem)
        {
            if (!ReferenceEquals(__instance, _watchedSlot)) return;
            PLLog.Info("[SelfTest] weapon slot set (guard=" + EquipmentGuard.Active + ") item=" + (pItem != null ? pItem.getName(true) : "null")
                       + (EquipmentGuard.Active ? string.Empty : "\n" + Environment.StackTrace));
        }

        private static GameObject _oldModObject;

        private static void Finish()
        {
            _watchedSlot = null;
            if (_oldModObject != null) _oldModObject.SetActive(true);
            PLLog.Info("[SelfTest] SUMMARY " + _passed + " passed, " + _failed + " failed");
            try
            {
                if (File.Exists(FlagPath) && File.ReadAllText(FlagPath).Contains("quit"))
                {
                    PLLog.Info("[SelfTest] quitting the game as requested by selftest.flag");
                    Application.Quit();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static Actor SpawnTestUnit()
        {
            WorldTile tile = FindLandTile();
            if (tile == null) throw new Exception("no land tile found");
            string[] ids = { "human", "elf", "dwarf", "orc" };
            foreach (string id in ids)
            {
                if (AssetManager.actor_library.get(id) == null) continue;
                Actor a = World.world.units.spawnNewUnit(id, tile, false, false, 0f, null, false, true);
                if (a != null && a.canUseItems()) return a;
            }
            throw new Exception("could not spawn an item-using unit");
        }

        private static WorldTile FindLandTile()
        {
            WorldTile[] tiles = World.world.tiles_list;
            if (tiles == null || tiles.Length == 0) return null;
            int start = tiles.Length / 2;
            for (int k = 0; k < tiles.Length; k++)
            {
                WorldTile t = tiles[(start + k * 7919) % tiles.Length];
                if (t != null && t.Type != null && t.Type.ground && !t.Type.liquid && !t.Type.block && t.building == null) return t;
            }
            return null;
        }

        private static Item MakeItem(Actor pActor, EquipmentType pType, string pExcludeId)
        {
            foreach (EquipmentAsset asset in AssetManager.items.list)
            {
                if (asset.equipment_type != pType || asset.id == pExcludeId || asset.getSprite() == null) continue;
                if (pType == EquipmentType.Weapon && asset.attack_type != WeaponType.Melee) continue;
                return World.world.items.generateItem(asset, pActor.kingdom, "SelfTest", 1, pActor, 0, true);
            }
            return null;
        }

        private static int FindSlot(Inventory pInv, string pResourceId)
        {
            for (int i = 0; i < pInv.Capacity; i++)
            {
                ItemStack s = pInv.Get(i);
                if (s != null && s.IsResource && s.Id == pResourceId) return i;
            }
            return -1;
        }

        private static int FirstEmptyHotbar(Inventory pInv) => ActorSync.FirstEmptyHotbar(pInv);
    }
}
