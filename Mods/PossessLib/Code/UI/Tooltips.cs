using System;
using PossessLib.Api;
using PossessLib.Core;
using PossessLib.Gameplay;
using PossessLib.Model;
using PossessLib.Sync;

namespace PossessLib.UI
{
    /// <summary>
    /// Slot tooltips. Game items use the game's own "equipment" tooltip (stats, modifiers, durability,
    /// rarity colour, history). Resources and custom items use tooltip assets registered here in
    /// AssetManager.tooltips, the same way the game defines its own.
    /// </summary>
    internal static class SlotTooltips
    {
        public const string ResourceTip = "possesslib_resource";
        public const string CustomTip = "possesslib_custom";

        public static void Register()
        {
            try
            {
                if (!AssetManager.tooltips.has(ResourceTip))
                {
                    AssetManager.tooltips.add(new TooltipAsset { id = ResourceTip, callback = ShowResource });
                }
                if (!AssetManager.tooltips.has(CustomTip))
                {
                    AssetManager.tooltips.add(new TooltipAsset { id = CustomTip, callback = ShowCustom });
                }
            }
            catch (Exception e)
            {
                PLLog.Exception("Registering tooltips", e);
            }
        }

        public static void Show(SlotView pView)
        {
            if (pView == null || !CursorState.IsEmpty) return;
            try
            {
                ItemStack stack = pView.Shown;
                if (stack == null)
                {
                    ShowEmpty(pView);
                    return;
                }
                switch (stack.Kind)
                {
                    case StackKind.Item:
                        Tooltip.show(pView, "equipment", new TooltipData { item = stack.Item });
                        break;
                    case StackKind.Resource:
                        Tooltip.show(pView, ResourceTip, new TooltipData
                        {
                            resource = stack.ResourceAsset,
                            tip_name = stack.Id,
                            custom_data_int = new CustomDataContainer<int> { ["count"] = stack.Count }
                        });
                        break;
                    default:
                        Tooltip.show(pView, CustomTip, new TooltipData
                        {
                            tip_name = stack.Id,
                            custom_data_int = new CustomDataContainer<int> { ["count"] = stack.Count }
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                PLLog.Debug("Tooltip failed: " + e.Message);
            }
        }

        public static void Hide()
        {
            Tooltip.hideTooltip();
        }

        private static void ShowEmpty(SlotView pView)
        {
            string name = null;
            string desc = null;
            switch (pView.Address.Kind)
            {
                case SlotKind.Trash:
                    name = Loc.T("possesslib_trash", "Trash");
                    desc = Loc.T("possesslib_trash_tip", "Drop an item here to delete it. You can take back the last one.");
                    break;
                case SlotKind.Equipment:
                    name = EquipSlotRegistry.Slots.Count > 0 ? Loc.T("possesslib_slot_" + EquipSlotRegistry.VanillaId(pView.Address.EquipType),
                        pView.Address.EquipType.ToString()) : pView.Address.EquipType.ToString();
                    desc = Loc.T("possesslib_equip_tip", "Drag gear here, or right-click it in the inventory.");
                    break;
                case SlotKind.CustomEquipment:
                    name = pView.Address.Custom != null ? Loc.T(pView.Address.Custom.Name, pView.Address.Custom.Name) : null;
                    break;
            }
            if (name == null) return;
            Tooltip.show(pView, "tip", new TooltipData { tip_name = name, tip_description = desc });
        }

        // ---------------------------------------------------------------- tooltip callbacks

        private static int GetCount(TooltipData pData)
        {
            int count = 0;
            if (pData.custom_data_int != null) pData.custom_data_int.TryGetValue("count", out count);
            return count;
        }

        private static void ShowResource(Tooltip pTooltip, string pType, TooltipData pData)
        {
            ResourceAsset res = pData.resource;
            if (res == null) return;
            pTooltip.name.text = res.getTranslatedName();
            pTooltip.clearTextRows();
            pTooltip.addLineText(Loc.T("possesslib_tip_stack", "Stack"), GetCount(pData).ToString(), null, false, false, 21);
            Actor owner = PossessionController.Current;
            if (owner != null && owner.data != null)
            {
                pTooltip.addLineText(Loc.T("possesslib_tip_carried", "Carried"), owner.data.inventory.getResource(res.id).ToString(), null, false, false, 21);
            }
            if (DataTables.IsEdible(res))
            {
                pTooltip.addLineBreak();
                if (res.restore_nutrition != 0) pTooltip.addLineIntText("nutrition", res.restore_nutrition, null, true);
                DataTables.GetFood(res.id, out float _, out int flat, out float percent);
                float pct = res.restore_health + percent;
                if (pct > 0f) pTooltip.addLineText("health", "+" + Math.Round(pct * 100f) + "%", "#7CFC00", false, true, 21);
                if (flat > 0) pTooltip.addLineText(Loc.T("possesslib_tip_bonus_heal", "Bonus heal"), "+" + flat, "#7CFC00", false, false, 21);
                pTooltip.setDescription(Loc.T("possesslib_tip_eat", "Select it and left-click to eat."), "#FFFFFF");
            }
            else
            {
                pTooltip.setDescription(Loc.T("possesslib_tip_resource", "Resource. Used for crafting and trade."), "#CCCCCC");
            }
        }

        private static void ShowCustom(Tooltip pTooltip, string pType, TooltipData pData)
        {
            CustomItemDefinition def = CustomItemRegistry.Get(pData.tip_name);
            pTooltip.clearTextRows();
            if (def == null)
            {
                pTooltip.name.text = Loc.F("possesslib_missing_item", "Missing item: $0", pData.tip_name);
                pTooltip.setDescription(Loc.T("possesslib_missing_item_tip", "The mod that adds this item is not loaded. It is kept safe until it is."), "#FF8080");
                return;
            }
            pTooltip.name.text = Toolbox.coloredText(Loc.T(def.Name, def.Name ?? def.Id), def.RarityColorHex ?? "#FFFFFF", false);
            int count = GetCount(pData);
            if (count > 1) pTooltip.addLineText(Loc.T("possesslib_tip_stack", "Stack"), count.ToString(), null, false, false, 21);
            string desc = string.IsNullOrEmpty(def.Description) ? string.Empty : Loc.T(def.Description, def.Description);
            if (def.ExtraTooltip != null)
            {
                try
                {
                    string extra = def.ExtraTooltip(ItemStack.FromCustom(def.Id, Math.Max(1, count)));
                    if (!string.IsNullOrEmpty(extra)) desc = string.IsNullOrEmpty(desc) ? extra : desc + "\n" + extra;
                }
                catch (Exception e)
                {
                    PLLog.Exception("ExtraTooltip of " + def.Id, e);
                }
            }
            if (def.OnUse != null) desc += (desc.Length > 0 ? "\n" : string.Empty) + Loc.T("possesslib_tip_use", "Select it and left-click to use.");
            if (!string.IsNullOrEmpty(desc)) pTooltip.setDescription(desc, "#FFFFFF");
        }
    }
}
