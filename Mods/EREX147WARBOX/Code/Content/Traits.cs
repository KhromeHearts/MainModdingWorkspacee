using System.Collections.Generic;

namespace WarBox.Content;

internal static class WarBoxTraits
{
    public static void Init()
    {
        ActorTrait warbox_unit = new ActorTrait
        {
            id = "warbox_unit",
            needs_to_be_explored = false,
            rarity = Rarity.R2_Epic,
            path_icon = "ui/icons/traits/trait_warbox_unit",
            group_id = "special",
            is_mutation_box_allowed = false,
            can_be_given = false,
            unlocked_with_achievement = false
        };
        warbox_unit.addOpposite("madness");
        warbox_unit.base_stats ??= new BaseStats();
        AssetManager.traits.add(warbox_unit);

        ActorTrait atgm_launcher = new ActorTrait
        {
            id = "atgm_launcher",
            needs_to_be_explored = false,
            rarity = Rarity.R3_Legendary,
            path_icon = "ui/icons/traits/trait_atgm_launcher",
            group_id = "fun",
            is_mutation_box_allowed = false,
            can_be_given = true,
            unlocked_with_achievement = false
        };
        atgm_launcher.addOpposite("bomberman");
        atgm_launcher.base_stats ??= new BaseStats();
        atgm_launcher.combat_actions_ids = new List<string>{ "combat_launch_atgm" };
        atgm_launcher.linkCombatActions();
        AssetManager.traits.add(atgm_launcher);
    }
}