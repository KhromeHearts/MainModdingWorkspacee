using System.Collections.Generic;
using NeoModLoader.api;
using PossessLib.Api;
using PossessLib.Model;
using UnityEngine;
using UnityEngine.UI;

namespace PossessLibExample
{
    /// <summary>
    /// Minimal mod built on PossessLib. Shows the API surface a mod like SurvivalBox would use:
    /// a custom usable item, a crafting recipe, an extra equipment slot with stats, a HUD element and
    /// event hooks. Delete the PossessLibExample folder if you don't want it.
    /// </summary>
    public class ExampleMod : BasicMod<ExampleMod>
    {
        private const string HerbPouch = "possesslib_example:herb_pouch";
        private const string LuckyCharm = "possesslib_example:lucky_charm";
        private const string CharmSlot = "possesslib_example:charm";

        protected override void OnModLoad()
        {
            if (!PossessLibApi.IsApiAtLeast(1))
            {
                LogWarning("PossessLib is too old for this example.");
                return;
            }

            // 1. A usable custom item: heals 25 HP, consumed on use.
            PossessLibApi.RegisterCustomItem(new CustomItemDefinition
            {
                Id = HerbPouch,
                Name = "Herb Pouch",
                Description = "A pouch of crushed herbs. Heals 25 health.",
                IconPath = "ui/Icons/iconHealth",
                MaxStack = 10,
                RarityColorHex = "#7CFC00",
                UseCooldown = 1f,
                OnUse = (actor, stack) =>
                {
                    if (actor.getHealth() >= actor.getMaxHealth()) return false;
                    actor.restoreHealth(25);
                    PossessLibApi.ShowToast("<color=#7CFC00>+25</color> Herb Pouch");
                    return true;
                }
            });

            // 2. A recipe for it (3 herbs -> 1 pouch). Recipes can also live in PossessLib/Data/recipes.json.
            PossessLibApi.RegisterRecipe(new RecipeDefinition
            {
                Id = "possesslib_example:herb_pouch",
                ResultKind = RecipeResultKind.Custom,
                ResultId = HerbPouch,
                Resources = new Dictionary<string, int> { { "herbs", 3 } },
                Category = "misc"
            });

            // 3. An extra equipment slot and an item that fits it (+5 armor while worn).
            PossessLibApi.RegisterEquipmentSlot(new EquipmentSlotDefinition
            {
                Id = CharmSlot,
                Name = "Charm",
                IconPath = "ui/Icons/iconFavoriteStar",
                Order = 60
            });
            BaseStats charmStats = new BaseStats();
            charmStats["armor"] = 5f;
            PossessLibApi.RegisterCustomItem(new CustomItemDefinition
            {
                Id = LuckyCharm,
                Name = "Lucky Charm",
                Description = "+5 armor while worn in the Charm slot.",
                IconPath = "ui/Icons/iconFavoriteStar",
                MaxStack = 1,
                RarityColorHex = "#FFD700",
                EquipSlotId = CharmSlot,
                EquippedStats = charmStats
            });
            PossessLibApi.RegisterRecipe(new RecipeDefinition
            {
                Id = "possesslib_example:lucky_charm",
                ResultKind = RecipeResultKind.Custom,
                ResultId = LuckyCharm,
                Resources = new Dictionary<string, int> { { "gold", 2 } },
                Money = 5,
                Category = "accessory"
            });

            // 4. A HUD element: kill counter under the health bar.
            PossessLibApi.RegisterHudElement(new HudElementDefinition
            {
                Id = "possesslib_example:kills",
                Anchor = HudAnchor.TopRight,
                Size = new Vector2(80f, 10f),
                Build = container =>
                {
                    Text text = new GameObject("Kills", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                    text.transform.SetParent(container, false);
                    RectTransform rt = (RectTransform)text.transform;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = rt.offsetMax = Vector2.zero;
                    text.font = LocalizedTextManager.current_font;
                    text.fontSize = 8;
                    text.alignment = TextAnchor.MiddleRight;
                    text.color = new Color(1f, 0.8f, 0.8f);
                    text.raycastTarget = false;
                },
                OnUpdate = (container, actor) =>
                {
                    Text text = container.GetComponentInChildren<Text>();
                    int kills = actor.data.kills;
                    if (text != null && text.text.Length != 0 && _lastKills == kills) return;
                    _lastKills = kills;
                    if (text != null) text.text = "Kills: " + kills;
                }
            });

            // 5. Events.
            PossessLibEvents.UnitPossessed += actor =>
            {
                PossessLibApi.ShowToast("Example mod says hi, " + actor.getName() + "!");
                // two starter pouches whenever the unit has none
                if (PossessLibApi.CountCustomItem(actor, HerbPouch) == 0) PossessLibApi.AddCustomItem(actor, HerbPouch, 2);
            };
            PossessLibEvents.ItemUsed += (actor, stack) => LogInfo(actor.getName() + " used " + stack.GetDisplayName());
            PossessLibEvents.ItemCrafted += (actor, recipe, stack) => LogInfo(actor.getName() + " crafted " + recipe.Id);

            LogInfo("Registered with PossessLib " + PossessLibApi.Version);
        }

        private static int _lastKills = -1;
    }
}
