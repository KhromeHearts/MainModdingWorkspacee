using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.api;
using HarmonyLib;

// using DG.Tweening;
// using DG.Tweening.Core;
// using DG.Tweening.Plugins.Options;
// using UnityEngine.Events;

namespace avbsexpandedarmory{
    internal static class ItemEditorMenuAdditions{
        [HarmonyPatch(typeof(ScrollWindow), "showWindow", typeof(string), typeof(bool), typeof(bool))]
        public class Patch_ScrollWindow_ShowWindow{

            private static bool firstLoad = true;

            public static void Postfix(string pWindowID){
                if (pWindowID != "item" || firstLoad == false){
                    return;
                }

                if (ScrollWindow._all_windows.TryGetValue("item", out ScrollWindow baseWindow) && baseWindow != null){
                    ItemWindow itemWindowInstance = baseWindow.GetComponent<ItemWindow>();
                    if (itemWindowInstance != null){
                        itemWindowInstance.updateStates();
                        firstLoad = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ItemWindow), "updateStates")]
        public class Patch_ItemWindow_AddSquareButton{
            
            private static Button _button_transmutation;

            public static void Postfix(ItemWindow __instance){

                //Stop duplicates
                if (_button_transmutation != null){
                    return;
                }

                //Find and copy transmutation button
                if (_button_transmutation == null){
                    foreach (Button btn in __instance.GetComponentsInChildren<Button>(true)){
                        int persistentEventCount = btn.onClick.GetPersistentEventCount();
                        for (int i = 0; i < persistentEventCount; i++){
                            if (btn.onClick.GetPersistentMethodName(i) == "clickTransmutation"){
                                _button_transmutation = btn;
                                break;
                            }
                        }
                        if (_button_transmutation != null){
                            break;
                        }
                    }
                }
                if (_button_transmutation == null){
                    Debug.LogError("_button_transmutation is still null (AEA mod)");
                    return;
                }

                //0 --- Transmutation
                RectTransform originalRect = _button_transmutation.GetComponent<RectTransform>();
                if (originalRect != null){
                    originalRect.anchoredPosition = new Vector2(-20f, 15f);
                }

                //1 --- Destroy Item
                CreateCustomButton(__instance, "DestroyItemButton", new Vector2(0f, -30f), "ui/Icons/iconDestroyItem", false, "DestroyItemButton_name", "DestroyItemButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        Actor hActor = currentItem.getActor();
                        if (hActor != null){
                            ActorEquipmentSlot tSlot = hActor.equipment.getSlot(currentItem._asset.equipment_type);
                            tSlot.takeAwayItem();
                            hActor.setStatsDirty();
                        }
                        City hCity = currentItem.getCity();
                        if (hCity != null){
                            currentItem.clearCity();
                            List<long> tCityItemList = hCity.data.equipment.getEquipmentList(currentItem._asset.equipment_type);
                            tCityItemList.Remove(currentItem.id);
                        }

                        World.world.items.removeObject(currentItem);
                        CloseActiveWindow();
                    }
                });

                //2 --- Absolute Reforge
                CreateCustomButton(__instance, "AbsoluteReforgeButton", new Vector2(-30f, 0f), "ui/Icons/iconReforgeAbsolute", false, "AbsoluteReforgeButton_name", "AbsoluteReforgeButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        currentItem.clearForReforge();
                        EquipmentType itemType = currentItem._asset.equipment_type;
                        List<ItemModAsset> itemModifiers = new List<ItemModAsset>();
                        if (itemType == EquipmentType.Weapon){
                            itemModifiers = AssetManager.items_modifiers.pools["weapon"];
                        }
                        else if (itemType == EquipmentType.Ring || itemType == EquipmentType.Amulet){
                            itemModifiers = AssetManager.items_modifiers.pools["accessory"];
                        }
                        else{
                            itemModifiers = AssetManager.items_modifiers.pools["armor"];
                        }

                        foreach (ItemModAsset modifier in itemModifiers){
                            if (modifier.mod_can_be_given && modifier.mod_rank == 5){
                                currentItem.addMod(modifier);
                            }
                        }
                        currentItem.addMod("divine_rune");

                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        __instance.updateStates();
                    }
                });

                //3 --- Purify
                CreateCustomButton(__instance, "PurifyButton", new Vector2(-30f, -30f), "ui/Icons/iconBucketFilled", false, "PurifyButton_name", "PurifyButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        currentItem.clearForReforge();
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //4 --- Flame Modifier
                CreateCustomButton(__instance, "FlameModifierButton", new Vector2(-175f, 5f), "ui/Icons/flameModButtonM", true, "FlameModifierButton_name", "FlameModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("flame")){
                            currentItem.addMod("flame");
                        }
                        else{
                            currentItem.removeMod("flame");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //5 --- Ice Modifier
                CreateCustomButton(__instance, "IceModifierButton", new Vector2(-155f, 5f), "ui/Icons/iceModButtonM", true, "IceModifierButton_name", "IceModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("ice")){
                            currentItem.addMod("ice");
                        }
                        else{
                            currentItem.removeMod("ice");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //6 --- Slowness Modifier
                CreateCustomButton(__instance, "SlownessModifierButton", new Vector2(-115f, 5f), "ui/Icons/slownessModButtonM", true, "SlownessModifierButton_name", "SlownessModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("slowness")){
                            currentItem.addMod("slowness");
                        }
                        else{
                            currentItem.removeMod("slowness");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //7 --- Poison Modifier
                CreateCustomButton(__instance, "PoisonModifierButton", new Vector2(-135f, 5f), "ui/Icons/poisonModButtonM", true, "PoisonModifierButton_name", "PoisonModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("poison")){
                            currentItem.addMod("poison");
                        }
                        else{
                            currentItem.removeMod("poison");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //8 --- Stun Modifier
                CreateCustomButton(__instance, "StunModifierButton", new Vector2(-175f, -15f), "ui/Icons/stunModButtonM", true, "StunModifierButton_name", "StunModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("stun")){
                            currentItem.addMod("stun");
                        }
                        else{
                            currentItem.removeMod("stun");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //9 --- Measurement Modifier
                CreateCustomButton(__instance, "MeasurementModifierButton", new Vector2(-155f, -15f), "ui/Icons/measurementModButtonM", true, "MeasurementModifierButton_name", "MeasurementModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("measurement")){
                            currentItem.addMod("measurement");
                        }
                        else{
                            currentItem.removeMod("measurement");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //10 --- Silence Modifier
                CreateCustomButton(__instance, "SilenceModifierButton", new Vector2(-135f, -15f), "ui/Icons/silenceModButtonM", true, "SilenceModifierButton_name", "SilenceModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("silence")){
                            currentItem.addMod("silence");
                        }
                        else{
                            currentItem.removeMod("silence");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //11 --- Initiative Modifier
                CreateCustomButton(__instance, "InitiativeModifierButton", new Vector2(-115f, -15f), "ui/Icons/initiativeModButtonM", true, "InitiativeModifierButton_name", "InitiativeModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("initiative")){
                            currentItem.addMod("initiative");
                        }
                        else{
                            currentItem.removeMod("initiative");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });

                //12 --- Reaper's Curse Modifier
                CreateCustomButton(__instance, "ReapersCurseModifierButton", new Vector2(-175f, -35f), "ui/Icons/reaperscurseModButtonM", true, "ReapersCurseModifierButton_name", "ReapersCurseModifierButton_description", () => {
                    Item currentItem = __instance.getMetaObject();
                    if (currentItem != null){
                        if (!currentItem.hasMod("reapers_curse")){
                            currentItem.addMod("reapers_curse");
                        }
                        else{
                            currentItem.removeMod("reapers_curse");
                        }
                        currentItem.calculateValues();
                        if (currentItem._actor != null){
                            currentItem._actor.addTrait("scar_of_divinity", false);
                        }
                        currentItem.addMod("divine_rune");
                        __instance.updateStates();
                    }
                });
            }

            private static void CreateCustomButton(ItemWindow window, string buttonName, Vector2 positionOffset, string iconPath, bool resizeButton, string name, string description, UnityEngine.Events.UnityAction onClickAction){
                GameObject newButtonObj = UnityEngine.Object.Instantiate(_button_transmutation.gameObject, _button_transmutation.transform.parent);
                newButtonObj.name = buttonName;

                Component tooltipDisplay = newButtonObj.GetComponent("EventTrigger");
                if (tooltipDisplay != null){UnityEngine.Object.Destroy(tooltipDisplay);}
                // Component worldBoxTooltip = newButtonObj.GetComponent("TipButton");
                // if (worldBoxTooltip != null){UnityEngine.Object.Destroy(worldBoxTooltip);}

                TipButton newTooltip = newButtonObj.GetComponent<TipButton>();

                // If for some reason it doesn't have one, add it safely
                if (newTooltip == null){
                    newTooltip = newButtonObj.AddComponent<TipButton>();
                }

                // Now assign your custom text fields safely
                newTooltip.textOnClick = name;
                newTooltip.textOnClickDescription = description;
                newTooltip.type = "tip";


                // A --- Dimensions
                RectTransform rect = newButtonObj.GetComponent<RectTransform>();
                if (rect != null){
                    rect.anchoredPosition += positionOffset;
                    rect.localScale = Vector3.one;

                    if (resizeButton){
                        rect.sizeDelta = new Vector2(20f, 20f);
                    }
                }

                // B --- Icon size and alignment
                Sprite customSprite = Resources.Load<Sprite>(iconPath);
                if (newButtonObj.transform.childCount > 0 && customSprite != null){
                    Transform iconChild = newButtonObj.transform.GetChild(0);
                    Image buttonIcon = iconChild.GetComponent<Image>();
                    
                    if (buttonIcon != null){
                        buttonIcon.sprite = customSprite;
                        buttonIcon.color = Color.white;

                        RectTransform iconRect = iconChild.GetComponent<RectTransform>();
                        if (iconRect != null){
                            if (resizeButton){
                                iconRect.anchorMin = Vector2.zero;
                                iconRect.anchorMax = Vector2.one;
                                iconRect.pivot = new Vector2(0.5f, 0.5f);
                                
                                float padding = 4f;
                                iconRect.offsetMin = new Vector2(padding, padding);
                                iconRect.offsetMax = new Vector2(-padding, -padding);
                                
                                iconRect.anchoredPosition = Vector2.zero;
                                iconRect.localScale = Vector3.one;
                            }
                        }
                    }
                }

                Button unityButton = newButtonObj.GetComponent<Button>();
                if (unityButton != null){
                    unityButton.onClick.RemoveAllListeners();
                    int persistentEvents = unityButton.onClick.GetPersistentEventCount();
                    for (int i = 0; i < persistentEvents; i++){
                        unityButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                    }
                    unityButton.onClick.AddListener(onClickAction);
                }
            }

            private static void CloseActiveWindow(){
                if (ScrollWindow.isWindowActive()){
                    if (ScrollWindow.isAnimationActive()){
                        ScrollWindow.finishAnimations();
                    }
                }
                WindowHistory.clickBack();
            }
        }
    }
}
