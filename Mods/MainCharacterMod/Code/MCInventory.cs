using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HarmonyLib;

namespace MC
{
    public class MCInventory : MonoBehaviour
    {
        public static MCInventory Instance;
        public static GameObject inventoryUI;
        public static GameObject hotbarUI;

        public MCInventoryData data = new MCInventoryData();
        private readonly Dictionary<Actor, MCInventoryData> actorInventories =
            new Dictionary<Actor, MCInventoryData>();

        private readonly Image[] inventorySlotImages = new Image[MCInventoryData.MainInventorySlotCount];
        private readonly Text[] inventoryQuantityTexts = new Text[MCInventoryData.MainInventorySlotCount];
        private readonly Image[] hotbarSlotImages = new Image[MCInventoryData.HotbarSlotCount];
        private readonly Text[] hotbarQuantityTexts = new Text[MCInventoryData.HotbarSlotCount];
        private readonly Image[] equipmentSlotImages = new Image[5]; // Helmet, Armor, Boots, Ring, Amulet
        private Image levelStatImage;
        private Image armorStatImage;
        private Text levelStatText;
        private Text armorStatText;

        private bool inventoryVisible;
        private bool hotbarVisible;

        private bool statusRowVisible;
        private Canvas canvas;
        private MCInventorySlot heldSlot;
        private Image heldItemGhost;
        private Actor boundActor;

        private static readonly Vector2 SlotSize = new Vector2(26f, 26f);
        private const float SlotSpacing = 3f;

        private static readonly EquipmentType[] EquipmentSlots =
        {
            EquipmentType.Helmet, EquipmentType.Armor,
            EquipmentType.Boots,  EquipmentType.Ring,
            EquipmentType.Amulet
        };

        private void Awake()
        {
            Instance = this;
            CreateHotbarUI();
            CreateInventoryUI();
            SetInventoryVisible(false);
            SetHotbarVisible(false);
        }

        private void Update()
        {
            if (!ControllableUnit.isControllingUnit())
            {
                if (inventoryVisible) SetInventoryVisible(false);
                if (hotbarVisible) SetHotbarVisible(false);
                ClearHeldSlot();
                boundActor = null;
                return;
            }

            Actor controlledActor = ControllableUnit.getControllableUnit();
            if (boundActor != controlledActor)
            {
                BindToActor(controlledActor);
            }

            if (ControllableUnit.isControllingUnit() && !hotbarVisible)
            {
                SetHotbarVisible(true);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                SetInventoryVisible(!inventoryVisible);
            }

            for (int i = 0; i < MCInventoryData.HotbarSlotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectHotbarSlot(i);
                }
            }

            RefreshInventoryUI();
            UpdateHeldItemGhost();
        }

        private void BindToActor(Actor actor)
        {
            if (boundActor != null && boundActor != actor)
            {
                ClearHeldSlot();
                actorInventories[boundActor] = data;
            }

            // Clean up dead units
            List<Actor> deadActors = new List<Actor>();
            foreach (var kvp in actorInventories)
            {
                if (kvp.Key == null || !kvp.Key.isAlive())
                {
                    deadActors.Add(kvp.Key);
                }
            }
            foreach (Actor dead in deadActors)
            {
                actorInventories.Remove(dead);
            }

            boundActor = actor;
            if (actor == null)
            {
                data = new MCInventoryData();
                return;
            }

            if (!actorInventories.TryGetValue(actor, out MCInventoryData actorData))
            {
                actorData = new MCInventoryData();
                actorInventories[actor] = actorData;
            }

            data = actorData;

            // Import any newly acquired native weapon on repossession
            if (actor.equipment != null)
            {
                ActorEquipmentSlot nativeWeaponSlot = actor.equipment.getSlot(EquipmentType.Weapon);
                if (nativeWeaponSlot != null)
                {
                    Item nativeWeapon = nativeWeaponSlot.getItem();
                    if (nativeWeapon != null && nativeWeapon.getAsset() != null)
                    {
                        bool existsInInventory = false;
                        for (int i = 0; i < data.slots.Length; i++)
                        {
                            if (data.slots[i].item == nativeWeapon)
                            {
                                existsInInventory = true;
                                break;
                            }
                        }

                        if (!existsInInventory)
                        {
                            nativeWeapon.setUnitHasIt(actor);
                            int activeHotbarIndex = data.hotbarSlotIndices[data.selectedHotbarSlot];

                            if (!data.slots[activeHotbarIndex].IsEmpty)
                            {
                                data.TryAdd(data.slots[activeHotbarIndex].item, actor);
                            }

                            data.slots[activeHotbarIndex] = new MCInventorySlot { item = nativeWeapon };
                        }
                    }
                }
            }
        }

        public void SetInventoryVisible(bool visible)
        {
            inventoryVisible = visible;
            if (inventoryUI == null) CreateInventoryUI();
            if (inventoryUI != null) inventoryUI.SetActive(visible);
            SetStatusRowVisible(!visible);
        }

        public void SetHotbarVisible(bool visible)
        {
            hotbarVisible = visible;
            if (hotbarUI == null) CreateHotbarUI();
            if (hotbarUI != null) hotbarUI.SetActive(ControllableUnit.isControllingUnit());
        }

        public void SetStatusRowVisible(bool visible)
        {
            statusRowVisible = visible;
            if (MCUIManager.Instance != null && MCUIManager.Instance.statusRow != null)
            {
                MCUIManager.Instance.statusRow.SetActive(visible);
            }
        }

        private void CreateHotbarUI()
        {
            canvas = CanvasMain.instance == null ? UnityEngine.Object.FindObjectOfType<Canvas>() : CanvasMain.instance.canvas_ui;
            if (canvas == null) return;

            hotbarUI = new GameObject("MC_Hotbar", typeof(RectTransform));
            hotbarUI.transform.SetParent(canvas.transform, false);

            RectTransform hotbarRect = hotbarUI.GetComponent<RectTransform>();
            hotbarRect.anchorMin = new Vector2(0f, 1f);
            hotbarRect.anchorMax = new Vector2(0f, 1f);
            hotbarRect.pivot = new Vector2(0f, 1f);
            hotbarRect.anchoredPosition = new Vector2(15f, -15f);

            float width = (MCInventoryData.HotbarSlotCount * SlotSize.x) + ((MCInventoryData.HotbarSlotCount - 1) * SlotSpacing);
            hotbarRect.sizeDelta = new Vector2(width, SlotSize.y);

            HorizontalLayoutGroup layout = hotbarUI.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = SlotSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            for (int i = 0; i < hotbarSlotImages.Length; i++)
            {
                hotbarSlotImages[i] = CreateSlot(hotbarUI.transform, $"HotbarSlot_{i}");
                hotbarQuantityTexts[i] = hotbarSlotImages[i].transform.parent.GetComponentInChildren<Text>();
                hotbarSlotImages[i].transform.parent.GetComponent<MCInventorySlotButton>().ConfigureStorage(this, i, true);
            }

            hotbarUI.SetActive(ControllableUnit.isControllingUnit());

            heldItemGhost = new GameObject("MC_HeldItemGhost", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            heldItemGhost.transform.SetParent(canvas.transform, false);
            heldItemGhost.raycastTarget = false;
            heldItemGhost.preserveAspect = true;
            heldItemGhost.enabled = false;
        }

        private void CreateInventoryUI()
        {
            if (canvas == null) return;

            inventoryUI = new GameObject("MC_Inventory", typeof(RectTransform));
            inventoryUI.transform.SetParent(canvas.transform, false);

            RectTransform inventoryRect = inventoryUI.GetComponent<RectTransform>();
            inventoryRect.anchorMin = Vector2.zero;
            inventoryRect.anchorMax = Vector2.one;
            inventoryRect.offsetMin = Vector2.zero;
            inventoryRect.offsetMax = Vector2.zero;

            CreateSlotGrid(inventoryUI.transform);
            CreateEquipmentPanel(inventoryUI.transform);
            CreateEquipmentStats(inventoryUI.transform);
            inventoryUI.SetActive(inventoryVisible);
        }

        private void CreateSlotGrid(Transform parent)
        {
            GameObject gridObject = new GameObject("MC_InventoryGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(parent, false);

            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0f, 1f);
            gridRect.anchorMax = new Vector2(0f, 1f);
            gridRect.pivot = new Vector2(0f, 1f);
            gridRect.anchoredPosition = new Vector2(15f, -55f);

            float gridWidth = (6 * SlotSize.x) + (5 * SlotSpacing);
            float gridHeight = (3 * SlotSize.y) + (2 * SlotSpacing);
            gridRect.sizeDelta = new Vector2(gridWidth, gridHeight);

            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = SlotSize;
            grid.spacing = new Vector2(SlotSpacing, SlotSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;

            for (int i = 0; i < inventorySlotImages.Length; i++)
            {
                inventorySlotImages[i] = CreateSlot(gridObject.transform, $"InventorySlot_{i + MCInventoryData.HotbarSlotCount}");
                inventoryQuantityTexts[i] = inventorySlotImages[i].transform.parent.GetComponentInChildren<Text>();
                inventorySlotImages[i].transform.parent.GetComponent<MCInventorySlotButton>().ConfigureStorage(this, i + MCInventoryData.HotbarSlotCount, false);
            }
        }

        private void CreateEquipmentPanel(Transform parent)
        {
            GameObject equipmentObject = new GameObject("MC_EquipmentPanel", typeof(RectTransform));
            equipmentObject.transform.SetParent(parent, false);

            RectTransform equipmentRect = equipmentObject.GetComponent<RectTransform>();
            equipmentRect.anchorMin = new Vector2(1f, 1f);
            equipmentRect.anchorMax = new Vector2(1f, 1f);
            equipmentRect.pivot = new Vector2(1f, 1f);
            equipmentRect.anchoredPosition = new Vector2(-14f, -78f);

            float panelWidth = SlotSize.x;
            float panelHeight = (EquipmentSlots.Length * SlotSize.y) + ((EquipmentSlots.Length - 1) * SlotSpacing);
            equipmentRect.sizeDelta = new Vector2(panelWidth, panelHeight);

            GridLayoutGroup layout = equipmentObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = SlotSize;
            layout.spacing = new Vector2(SlotSpacing, SlotSpacing);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 1;

            for (int i = 0; i < EquipmentSlots.Length; i++)
            {
                equipmentSlotImages[i] = CreateSlot(equipmentObject.transform, $"EquipmentSlot_{EquipmentSlots[i]}");
                equipmentSlotImages[i].transform.parent.GetComponent<MCInventorySlotButton>().ConfigureEquipment(this, EquipmentSlots[i]);
            }
        }

        private void CreateEquipmentStats(Transform parent)
        {
            GameObject statsObject = new GameObject("MC_EquipmentStats", typeof(RectTransform));
            statsObject.transform.SetParent(parent, false);

            RectTransform statsRect = statsObject.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(1f, 1f);
            statsRect.anchorMax = new Vector2(1f, 1f);
            statsRect.pivot = new Vector2(1f, 1f);
            statsRect.anchoredPosition = new Vector2(-47f, -99f);
            statsRect.sizeDelta = new Vector2(42f, 50f);

            VerticalLayoutGroup layout = statsObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.UpperRight;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            CreateStatRow(statsObject.transform, "LevelStat", "ui/Icons/iconLevels", out levelStatImage, out levelStatText);
            CreateStatRow(statsObject.transform, "ArmorStat", "ui/Icons/iconArmor", out armorStatImage, out armorStatText);
        }

        private void CreateStatRow(Transform parent, string objectName, string iconPath, out Image iconImage, out Text valueText)
        {
            GameObject row = new GameObject(objectName, typeof(RectTransform));
            row.transform.SetParent(parent, false);

            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(42f, 22f);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(row.transform, false);
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
            iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = SpriteTextureLoader.getSprite(iconPath);
            iconImage.preserveAspect = true;

            GameObject textObject = new GameObject("Value", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(row.transform, false);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
            valueText = textObject.GetComponent<Text>();
            valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            valueText.fontSize = 13;
            valueText.fontStyle = FontStyle.Bold;
            valueText.alignment = TextAnchor.MiddleLeft;
            valueText.color = Color.white;
            valueText.horizontalOverflow = HorizontalWrapMode.Overflow;
            valueText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private Image CreateSlot(Transform parent, string objectName)
        {
            GameObject slot = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(parent, false);
            slot.AddComponent<MCInventorySlotButton>();

            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.sizeDelta = SlotSize;

            Image bgImage = slot.GetComponent<Image>();
            bgImage.sprite = Resources.Load<Sprite>("ui/icons/equipment_slot");
            bgImage.color = new Color(1f, 1f, 1f, 0.75f);

            GameObject iconObj = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(slot.transform, false);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(3f, 3f);
            iconRect.offsetMax = new Vector2(-3f, -3f);

            Image iconImage = iconObj.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.enabled = false;

            GameObject quantityObj = new GameObject("Quantity", typeof(RectTransform), typeof(Text));
            quantityObj.transform.SetParent(slot.transform, false);

            RectTransform quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1f, 0f);
            quantityRect.anchorMax = new Vector2(1f, 0f);
            quantityRect.pivot = new Vector2(1f, 0f);
            quantityRect.anchoredPosition = new Vector2(-2f, 2f);
            quantityRect.sizeDelta = new Vector2(24f, 16f);

            Text quantityText = quantityObj.GetComponent<Text>();
            quantityText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            quantityText.fontSize = 10;
            quantityText.fontStyle = FontStyle.Bold;
            quantityText.alignment = TextAnchor.LowerRight;
            quantityText.color = Color.white;
            quantityText.horizontalOverflow = HorizontalWrapMode.Overflow;
            quantityText.verticalOverflow = VerticalWrapMode.Overflow;
            quantityText.text = string.Empty;

            return iconImage;
        }

        private void RefreshInventoryUI()
        {
            if (inventoryUI == null) return;

            Actor controlledActor = ControllableUnit.getControllableUnit();
            if (controlledActor != null)
            {
                if (heldSlot == null)
                {
                    data.SyncActorBag(controlledActor);
                }
                SyncSelectedWeaponToActor(controlledActor);
            }
            RefreshEquipmentStats(controlledActor);

            RefreshMainInventorySlots();
            RefreshHotbarSlots();
            RefreshEquipmentSlots();
            RefreshHotbarSelection();
        }

        private void RefreshEquipmentStats(Actor actor)
        {
            if (levelStatText == null || armorStatText == null) return;

            if (actor == null)
            {
                levelStatText.text = string.Empty;
                armorStatText.text = string.Empty;
                return;
            }

            levelStatText.text = actor.level.ToString();
            armorStatText.text = Mathf.RoundToInt(actor.stats["armor"]).ToString();
        }

        private void SyncSelectedWeaponToActor(Actor actor)
        {
            if (actor == null || actor.equipment == null) return;

            ActorEquipmentSlot nativeWeaponSlot = actor.equipment.getSlot(EquipmentType.Weapon);
            if (nativeWeaponSlot == null || !nativeWeaponSlot.canChangeSlot()) return;

            MCInventorySlot selectedSlot = data.GetSelectedHotbarSlot();
            Item currentWeapon = nativeWeaponSlot.getItem();

            Item targetWeapon = (selectedSlot != null && selectedSlot.HasValidItem &&
                                 selectedSlot.item.getAsset() != null &&
                                 selectedSlot.item.getAsset().equipment_type == EquipmentType.Weapon)
                ? selectedSlot.item
                : null;

            if (currentWeapon != targetWeapon)
            {
                // CRITICAL FIX: Explicitly detach current weapon BEFORE attaching target weapon
                if (currentWeapon != null)
                {
                    nativeWeaponSlot.takeAwayItem();
                    currentWeapon.setUnitHasIt(actor);
                }

                if (targetWeapon != null)
                {
                    targetWeapon.setUnitHasIt(actor);
                    nativeWeaponSlot.setItem(targetWeapon, actor);
                }
                actor.setStatsDirty();
            }
        }

        private void RefreshMainInventorySlots()
        {
            int offset = MCInventoryData.HotbarSlotCount;
            for (int i = 0; i < inventorySlotImages.Length; i++)
            {
                MCInventorySlot slot = data.slots[i + offset];
                Sprite slotSprite = GetSlotSprite(slot);

                if (!slot.IsEmpty && slotSprite != null)
                {
                    inventorySlotImages[i].enabled = true;
                    inventorySlotImages[i].sprite = slotSprite;
                    inventorySlotImages[i].color = Color.white;
                    inventoryQuantityTexts[i].text = slot.IsResource ? slot.resourceAmount.ToString() : string.Empty;
                }
                else
                {
                    inventorySlotImages[i].enabled = false;
                    inventorySlotImages[i].sprite = null;
                    inventoryQuantityTexts[i].text = string.Empty;
                }
            }
        }

        private void RefreshEquipmentSlots()
        {
            Actor actor = ControllableUnit.getControllableUnit();

            for (int i = 0; i < equipmentSlotImages.Length; i++)
            {
                Item item = null;
                if (actor != null && actor.equipment != null)
                {
                    item = actor.equipment.getSlot(EquipmentSlots[i])?.getItem();
                }

                if (item != null && item.getAsset() != null)
                {
                    Sprite equipmentSprite = GetSlotSprite(new MCInventorySlot { item = item });
                    if (equipmentSprite != null)
                    {
                        equipmentSlotImages[i].sprite = equipmentSprite;
                        equipmentSlotImages[i].enabled = true;
                        equipmentSlotImages[i].color = Color.white;
                    }
                    else
                    {
                        equipmentSlotImages[i].enabled = false;
                        equipmentSlotImages[i].sprite = null;
                    }
                }
                else
                {
                    equipmentSlotImages[i].enabled = false;
                    equipmentSlotImages[i].sprite = null;
                }
            }
        }

        private void RefreshHotbarSlots()
        {
            for (int i = 0; i < hotbarSlotImages.Length; i++)
            {
                MCInventorySlot slot = data.GetHotbarSlot(i);
                Sprite slotSprite = GetSlotSprite(slot);

                if (!slot.IsEmpty && slotSprite != null)
                {
                    hotbarSlotImages[i].enabled = true;
                    hotbarSlotImages[i].sprite = slotSprite;
                    hotbarSlotImages[i].color = Color.white;
                    hotbarQuantityTexts[i].text = slot.IsResource ? slot.resourceAmount.ToString() : string.Empty;
                }
                else
                {
                    hotbarSlotImages[i].enabled = false;
                    hotbarSlotImages[i].sprite = null;
                    hotbarQuantityTexts[i].text = string.Empty;
                }
            }
        }

        private void RefreshHotbarSelection()
        {
            for (int i = 0; i < hotbarSlotImages.Length; i++)
            {
                Image bgImage = hotbarSlotImages[i].transform.parent.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = i == data.selectedHotbarSlot
                        ? new Color(1f, 0.85f, 0.3f, 0.75f)
                        : new Color(1f, 1f, 1f, 0.75f);
                }
            }
        }

        public bool AddItem(Item item)
        {
            Actor actor = ControllableUnit.getControllableUnit();
            return actor != null && data.TryAdd(item, actor);
        }

        public bool StoreExtraEquipment(Item item) => AddItem(item);

        private Sprite GetSlotSprite(MCInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return null;

            if (slot.IsResource)
            {
                ResourceAsset resource = AssetManager.resources.get(slot.resourceId);
                return resource == null ? null : resource.getSpriteIcon();
            }

            if (!slot.HasValidItem) return null;

            try
            {
                return slot.item.getSprite();
            }
            catch
            {
                return null;
            }
        }

        public void HandleStorageSlotClick(int slotIndex)
        {
            if (heldSlot == null || heldSlot.IsEmpty)
            {
                if (data.slots[slotIndex].IsEmpty)
                {
                    return;
                }

                heldSlot = data.slots[slotIndex];
                data.slots[slotIndex] = new MCInventorySlot();
                return;
            }

            MCInventorySlot targetSlot = data.slots[slotIndex];
            data.slots[slotIndex] = heldSlot;
            heldSlot = targetSlot.IsEmpty ? null : targetSlot;
        }

        public void HandleHotbarSlotClick(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= MCInventoryData.HotbarSlotCount) return;

            int targetIndex = data.hotbarSlotIndices[hotbarIndex];

            if (heldSlot == null || heldSlot.IsEmpty)
            {
                if (!data.slots[targetIndex].IsEmpty)
                {
                    heldSlot = data.slots[targetIndex];
                    data.slots[targetIndex] = new MCInventorySlot();
                }
                SelectHotbarSlot(hotbarIndex);
                return;
            }

            MCInventorySlot targetSlot = data.slots[targetIndex];
            data.slots[targetIndex] = heldSlot;
            heldSlot = targetSlot.IsEmpty ? null : targetSlot;
            SelectHotbarSlot(hotbarIndex);
        }

        private void UpdateHeldItemGhost()
        {
            if (heldItemGhost == null) return;

            if (heldSlot == null || heldSlot.IsEmpty)
            {
                heldItemGhost.enabled = false;
                return;
            }

            Sprite ghostSprite = GetSlotSprite(heldSlot);
            if (ghostSprite != null)
            {
                heldItemGhost.sprite = ghostSprite;
                heldItemGhost.enabled = true;
                heldItemGhost.transform.position = Input.mousePosition;
                heldItemGhost.transform.SetAsLastSibling();
            }
            else
            {
                heldItemGhost.enabled = false;
            }
        }

        public void ClearHeldSlot()
        {
            heldSlot = null;
            if (heldItemGhost != null) heldItemGhost.enabled = false;
        }

        public bool TryEquipFromSlot(int slotIndex)
        {
            Actor actor = ControllableUnit.getControllableUnit();
            return actor != null && data.EquipReplacingFromInventory(slotIndex, actor);
        }

        public int AddResource(string resourceId, int amount) => data.AddResource(resourceId, amount);
        public bool RemoveResource(string resourceId, int amount) => data.RemoveResource(resourceId, amount);

        public void SelectHotbarSlot(int index)
        {
            if (index >= 0 && index < MCInventoryData.HotbarSlotCount)
            {
                data.selectedHotbarSlot = index;
            }
        }

        public bool HandleEquipmentSlotClick(EquipmentType equipmentType)
        {
            Actor actor = ControllableUnit.getControllableUnit();
            if (actor == null || actor.equipment == null) return false;

            ActorEquipmentSlot equipmentSlot = actor.equipment.getSlot(equipmentType);

            if (heldSlot == null)
            {
                Item equippedItem = equipmentSlot.getItem();

                if (equippedItem == null || equippedItem.getAsset() == null)
                {
                    return false;
                }

                equipmentSlot.takeAwayItem();
                actor.setStatsDirty();
                equippedItem.setUnitHasIt(actor);
                heldSlot = new MCInventorySlot { item = equippedItem };
                return true;
            }

            if (heldSlot.IsResource || heldSlot.item == null ||
                heldSlot.item.getAsset() == null ||
                heldSlot.item.getAsset().equipment_type != equipmentType ||
                !equipmentSlot.canChangeSlot())
            {
                return false;
            }

            Item displacedItem = equipmentSlot.getItem();
            if (displacedItem != null)
            {
                equipmentSlot.takeAwayItem();
                displacedItem.setUnitHasIt(actor);
            }

            equipmentSlot.setItem(heldSlot.item, actor);
            actor.setStatsDirty();

            heldSlot = (displacedItem != null && displacedItem.getAsset() != null)
                ? ClaimHeldItem(displacedItem, actor)
                : null;

            return true;
        }

        private MCInventorySlot ClaimHeldItem(Item item, Actor actor)
        {
            item.setUnitHasIt(actor);
            return new MCInventorySlot { item = item };
        }

        public bool GetInventoryVisible() => inventoryVisible;
        public bool GetHotbarVisible() => hotbarVisible;
    }

    // Integrated Native Equipment Acquisition Patch
    [HarmonyPatch(typeof(Actor), "takeItems")]
    public static class MCEquipmentAcquisitionPatch
    {
        private static readonly Dictionary<Actor, Dictionary<EquipmentType, Item>> snapshots =
            new Dictionary<Actor, Dictionary<EquipmentType, Item>>();

        private static readonly EquipmentType[] trackedSlots =
        {
            EquipmentType.Weapon,
            EquipmentType.Helmet,
            EquipmentType.Armor,
            EquipmentType.Boots,
            EquipmentType.Ring,
            EquipmentType.Amulet
        };

        public static void Prefix(Actor __instance)
        {
            if (__instance == null ||
                __instance != ControllableUnit.getControllableUnit() ||
                __instance.equipment == null)
            {
                return;
            }

            Dictionary<EquipmentType, Item> snapshot = new Dictionary<EquipmentType, Item>();
            foreach (EquipmentType type in trackedSlots)
            {
                ActorEquipmentSlot slot = __instance.equipment.getSlot(type);
                snapshot[type] = slot == null ? null : slot.getItem();
            }

            snapshots[__instance] = snapshot;
        }

        public static void Postfix(Actor __instance)
        {
            if (!snapshots.TryGetValue(__instance, out Dictionary<EquipmentType, Item> snapshot))
            {
                return;
            }

            snapshots.Remove(__instance);

            if (__instance == null ||
                __instance != ControllableUnit.getControllableUnit() ||
                __instance.equipment == null ||
                MCInventory.Instance == null)
            {
                return;
            }

            foreach (EquipmentType type in trackedSlots)
            {
                ActorEquipmentSlot slot = __instance.equipment.getSlot(type);
                Item previousItem = snapshot[type];
                Item currentItem = slot == null ? null : slot.getItem();

                if (currentItem != null && currentItem != previousItem && currentItem.getAsset() != null)
                {
                    if (MCInventory.Instance.StoreExtraEquipment(currentItem))
                    {
                        slot.takeAwayItem();
                        __instance.setStatsDirty();
                        currentItem = null;
                    }
                }

                if (previousItem == null || previousItem == currentItem || previousItem.getAsset() == null)
                {
                    continue;
                }

                previousItem.setUnitHasIt(__instance);

                if (type == EquipmentType.Weapon && currentItem != null)
                {
                    MCInventorySlot activeHotbarSlot = MCInventory.Instance.data.GetSelectedHotbarSlot();
                    if (activeHotbarSlot != null)
                    {
                        activeHotbarSlot.item = currentItem;
                    }
                }

                MCInventory.Instance.StoreExtraEquipment(previousItem);
            }
        }
    }
}