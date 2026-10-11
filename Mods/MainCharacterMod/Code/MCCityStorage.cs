using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MC
{
    public class MCCityStorage : MonoBehaviour
    {
        private Building selectedStorage;
        private GameObject storageUI;
        private RectTransform resourceList;
        private Text titleText;
        private float nextRefreshTime;
        private readonly List<StorageRow> rowPool = new List<StorageRow>();

        private sealed class StorageRow
        {
            public GameObject root;
            public Text label;
            public Button putButton;
            public Button takeButton;
        }

        private void Awake()
        {
            CreateStorageUI();
            SetStorageVisible(false);
        }

        private void Update()
        {
            Actor actor = ControllableUnit.getControllableUnit();
            if (actor == null || !actor.isAlive())
            {
                selectedStorage = null;
                SetStorageVisible(false);
                return;
            }

            if (Input.GetMouseButtonDown(0) &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                TrySelectStorage(actor);
            }

            if (selectedStorage != null &&
                (selectedStorage.isRemoved() || !selectedStorage.isUsable() ||
                 !IsInRange(actor, selectedStorage)))
            {
                selectedStorage = null;
                SetStorageVisible(false);
            }

            if (selectedStorage != null && Time.time >= nextRefreshTime)
            {
                RefreshStorageUI(actor);
                nextRefreshTime = Time.time + 0.25f;
            }
        }

        private void TrySelectStorage(Actor actor)
        {
            WorldTile tile = MapBox.instance == null ? null : MapBox.instance.getMouseTilePos();
            Building building = tile == null ? null : tile.building;
            if (building == null || building.asset == null || !building.asset.is_stockpile ||
                !building.isUsable() || !IsInRange(actor, building))
            {
                return;
            }

            selectedStorage = building;
            SetStorageVisible(true);
            RefreshStorageUI(actor);
        }

        private bool IsInRange(Actor actor, Building building)
        {
            return actor.current_tile != null && building.current_tile != null &&
                   actor.current_tile.distanceTo(building.current_tile) <= actor.getAttackRange() + 3;
        }

        private void CreateStorageUI()
        {
            Canvas canvas = CanvasMain.instance == null
                ? UnityEngine.Object.FindObjectOfType<Canvas>()
                : CanvasMain.instance.canvas_ui;
            if (canvas == null) return;

            storageUI = new GameObject("MC_CityStorage", typeof(RectTransform), typeof(Image));
            storageUI.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = storageUI.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-92f, -245f);
            panelRect.sizeDelta = new Vector2(270f, 210f);

            Image background = storageUI.GetComponent<Image>();
            background.color = new Color(0.04f, 0.05f, 0.07f, 0.94f);

            GameObject titleObject = CreateText(storageUI.transform, "StorageTitle", 15, TextAnchor.UpperLeft);
            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(10f, -8f);
            titleRect.sizeDelta = new Vector2(-20f, 24f);
            titleText = titleObject.GetComponent<Text>();
            titleText.text = "City Storage";

            GameObject listObject = new GameObject("ResourceList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listObject.transform.SetParent(storageUI.transform, false);
            resourceList = listObject.GetComponent<RectTransform>();
            resourceList.anchorMin = new Vector2(0f, 0f);
            resourceList.anchorMax = new Vector2(1f, 1f);
            resourceList.offsetMin = new Vector2(8f, 8f);
            resourceList.offsetMax = new Vector2(-8f, -38f);

            VerticalLayoutGroup layout = listObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 3f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private GameObject CreateText(Transform parent, string objectName, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return textObject;
        }

        private void RefreshStorageUI(Actor actor)
        {
            if (storageUI == null || resourceList == null || selectedStorage == null) return;

            titleText.text = "Storage";
            int activeRowIndex = 0;

            Dictionary<string, ResourceContainer> carried = actor.inventory == null
                ? null
                : actor.inventory.getResources();
            if (selectedStorage.resources != null)
            {
                foreach (CityStorageSlot slot in selectedStorage.resources.getSlots())
                {
                    if (slot == null || slot.amount <= 0) continue;
                    int amount = slot.amount;
                    RenderResourceRow(activeRowIndex++, slot.id, amount,
                        carried == null ? 0 : actor.inventory.getResource(slot.id));
                }
            }

            if (carried != null)
            {
                foreach (KeyValuePair<string, ResourceContainer> entry in carried)
                {
                    if (entry.Value.amount <= 0 || HasStorageResource(entry.Key)) continue;
                    RenderResourceRow(activeRowIndex++, entry.Key, 0, entry.Value.amount);
                }
            }

            for (int i = activeRowIndex; i < rowPool.Count; i++)
            {
                rowPool[i].root.SetActive(false);
            }
        }

        private bool HasStorageResource(string resourceId)
        {
            return selectedStorage != null && selectedStorage.getResourcesAmount(resourceId) > 0;
        }

        private void RenderResourceRow(int rowIndex, string resourceId, int storedAmount, int carriedAmount)
        {
            while (rowPool.Count <= rowIndex)
            {
                rowPool.Add(CreateStorageRow());
            }

            StorageRow row = rowPool[rowIndex];
            row.root.SetActive(true);
            row.label.text = resourceId + "  S:" + storedAmount + "  B:" + carriedAmount;

            row.putButton.gameObject.SetActive(carriedAmount > 0);
            row.putButton.onClick.RemoveAllListeners();
            row.putButton.onClick.AddListener(delegate { TransferToStorage(resourceId, 1); });

            row.takeButton.gameObject.SetActive(storedAmount > 0);
            row.takeButton.onClick.RemoveAllListeners();
            row.takeButton.onClick.AddListener(delegate { TransferFromStorage(resourceId, 1); });
        }

        private StorageRow CreateStorageRow()
        {
            GameObject rowObject = new GameObject("StorageResourceRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObject.transform.SetParent(resourceList, false);
            rowObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 24f);

            HorizontalLayoutGroup layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 3f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            Text label = CreateText(rowObject.transform, "Label", 11, TextAnchor.MiddleLeft).GetComponent<Text>();
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(145f, 24f);

            return new StorageRow
            {
                root = rowObject,
                label = label,
                putButton = CreateButton(rowObject.transform, "Put 1"),
                takeButton = CreateButton(rowObject.transform, "Take 1")
            };
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(48f, 24f);
            buttonObject.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.22f, 1f);

            Text buttonText = CreateText(buttonObject.transform, "Text", 9, TextAnchor.MiddleCenter).GetComponent<Text>();
            buttonText.text = label;
            buttonText.color = Color.white;
            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private void TransferToStorage(string resourceId, int amount)
        {
            Actor actor = ControllableUnit.getControllableUnit();
            if (actor == null || selectedStorage == null || actor.inventory == null) return;

            int carriedBefore = actor.inventory.getResource(resourceId);
            int requested = Mathf.Min(amount, carriedBefore);
            ResourceAsset asset = AssetManager.resources.get(resourceId);
            if (requested <= 0 || asset == null || !selectedStorage.hasSpaceForResource(asset)) return;

            int storedBefore = selectedStorage.getResourcesAmount(resourceId);
            selectedStorage.addResources(resourceId, requested);
            int transferred = selectedStorage.getResourcesAmount(resourceId) - storedBefore;
            if (transferred > 0)
            {
                actor.takeFromInventory(resourceId, transferred);
            }

            RefreshStorageUI(actor);
        }

        private void TransferFromStorage(string resourceId, int amount)
        {
            Actor actor = ControllableUnit.getControllableUnit();
            if (actor == null || selectedStorage == null || actor.inventory == null) return;

            int storedBefore = selectedStorage.getResourcesAmount(resourceId);
            int requested = Mathf.Min(amount, storedBefore);
            if (requested <= 0) return;

            selectedStorage.takeResource(resourceId, requested);
            int transferred = storedBefore - selectedStorage.getResourcesAmount(resourceId);
            if (transferred > 0)
            {
                actor.addToInventory(resourceId, transferred);
            }

            RefreshStorageUI(actor);
        }

        private void SetStorageVisible(bool visible)
        {
            if (storageUI != null) storageUI.SetActive(visible);
        }
    }

}
