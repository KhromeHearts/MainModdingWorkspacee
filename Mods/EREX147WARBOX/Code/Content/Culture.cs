using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.General;
using System.Linq;

namespace WarBox.Content;

internal static class WarBoxCulture
{
    public static void Init()
    {
        AddTechItems();
    }

    private static void AddTechItems()
    {
        CultureTrait firearms = new CultureTrait
        {
            id = "craft_firearms",
            group_id = "craft",
            path_icon = "ui/icons/culture/culture_trait_firearms.png",
            value = 10f,
            is_weapon_trait = true,
            priority = -2,
            rarity = Rarity.R3_Legendary
        };
        firearms.addWeaponSubtype("gun");
        AssetManager.culture_traits.add(firearms);
        EnableForAll(firearms.id);

        CultureTrait vehicle_factories = new CultureTrait
        {
            id = "vehicle_factories",
            group_id = "buildings",
            path_icon = "ui/icons/culture/culture_trait_vehicle_factories.png",
            value = 1f,
            priority = -2,
            rarity = Rarity.R3_Legendary
        };
        AssetManager.culture_traits.add(vehicle_factories);
        EnableForAll(vehicle_factories.id);
    }

    private static void EnableForAll(string id)
    {
        foreach (ActorAsset race in AssetManager.actor_library.list.Where(race => race.canBecomeSapient() == true))
        {
            race.addCultureTrait(id);
        }
    }
}