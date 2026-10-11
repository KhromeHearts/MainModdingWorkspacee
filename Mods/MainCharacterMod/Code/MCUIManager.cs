using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.api;
using UnityEngine.EventSystems;

namespace MC
{
    public class MCUIManager : MonoBehaviour
    {
        public static MCUIManager Instance;

        public Actor selectedActor;

        public GameObject mainUIPanel;
        private readonly List<Image> statusIcons = new List<Image>();
        public GameObject statusRow;
        private bool uiVisible;

        private void Awake()
        {
            Instance = this;
            CreateUIOverlay();
            SetUIVisible(false);
        }

        private void Update()
        {
            if (mainUIPanel == null)
            {
                CreateUIOverlay();
            }

            selectedActor = SelectedUnit.unit;

            if (selectedActor != null)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    ControllableUnit.setControllableCreature(selectedActor);
                }
            }

            UpdateStatusRow();
        }

        public void SetUIVisible(bool visible)
        {
            uiVisible = visible;

            if (visible && mainUIPanel == null)
            {
                CreateUIOverlay();
            }

            if (mainUIPanel != null)
            {
                mainUIPanel.SetActive(visible);
            }
        }

        private void CreateUIOverlay()
        {
            Canvas canvas = CanvasMain.instance != null
                ? CanvasMain.instance.canvas_ui
                : UnityEngine.Object.FindObjectOfType<Canvas>();

            if (canvas == null) return;

            mainUIPanel = new GameObject("MC_MainPanel", typeof(RectTransform));
            mainUIPanel.transform.SetParent(canvas.transform, false);

            // Anchor status row beneath the inventory panel (Shifted Y from -300f to -210f)
            RectTransform panelRT = mainUIPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 1f);
            panelRT.anchorMax = new Vector2(0f, 1f);
            panelRT.pivot = new Vector2(0f, 1f);
            panelRT.anchoredPosition = new Vector2(15f, -210f);
 
            /*
            if (MCInventory.GetInventoryVisible())
            {
                panelRT.anchoredPosition = new Vector2(15f, -210f);
            }
            else
            {
                panelRT.anchoredPosition = new Vector2(15f, -55f);
            }
            */
            panelRT.sizeDelta = new Vector2(250f, 30f);

            statusRow = new GameObject("MC_StatusRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            statusRow.transform.SetParent(mainUIPanel.transform, false);

            RectTransform statusRowRT = statusRow.GetComponent<RectTransform>();
            statusRowRT.anchorMin = Vector2.zero;
            statusRowRT.anchorMax = Vector2.one;
            statusRowRT.sizeDelta = Vector2.zero;
            statusRowRT.anchoredPosition = new Vector2(0f, 153f);

            HorizontalLayoutGroup statusLayout = statusRow.GetComponent<HorizontalLayoutGroup>();
            statusLayout.spacing = 3f;
            statusLayout.childAlignment = TextAnchor.MiddleLeft;
            statusLayout.childForceExpandWidth = false;
            statusLayout.childForceExpandHeight = false;

            for (int i = 0; i < 8; i++)
            {
                GameObject statusSlot = new GameObject($"Status_{i}", typeof(RectTransform), typeof(Image));
                statusSlot.transform.SetParent(statusRow.transform, false);

                RectTransform statusSlotRT = statusSlot.GetComponent<RectTransform>();
                statusSlotRT.sizeDelta = new Vector2(22f, 22f);

                Image statusImage = statusSlot.GetComponent<Image>();
                statusImage.color = Color.white;
                statusImage.preserveAspect = true;
                statusImage.enabled = false;
                statusSlot.AddComponent<MCStatusTooltip>();
                statusIcons.Add(statusImage);
            }

            mainUIPanel.SetActive(uiVisible);
        }

        private void UpdateStatusRow()
        {
            if (statusIcons.Count == 0) return;

            for (int i = 0; i < statusIcons.Count; i++)
            {
                statusIcons[i].enabled = false;
                statusIcons[i].sprite = null;
            }

            Actor actor = ControllableUnit.getControllableUnit();
            if (actor == null || !actor.isAlive()) return;

            int iconIndex = 0;
            foreach (Status status in actor.getStatuses())
            {
                if (status == null || status.is_finished || iconIndex >= statusIcons.Count) continue;

                Sprite statusSprite = status.asset.getSprite();
                if (statusSprite == null) continue;

                MCStatusTooltip tooltip = statusIcons[iconIndex].GetComponent<MCStatusTooltip>();
                tooltip.SetStatus(status);
                statusIcons[iconIndex].sprite = statusSprite;
                statusIcons[iconIndex].enabled = true;
                iconIndex++;
            }
        }
    }

    public class MCStatusTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Status status;

        public void SetStatus(Status newStatus) => status = newStatus;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (status == null || status.asset == null) return;

            Tooltip.show(
                this,
                "status_updatable",
                new TooltipData
                {
                    tip_name = status.asset.getLocaleID(),
                    tip_description = status.asset.getDescriptionID(),
                    status = status
                });
        }

        public void OnPointerExit(PointerEventData eventData) => Tooltip.hideTooltip();
        private void OnDisable() => Tooltip.hideTooltip();
    }
}