// snippet_add_trait.cs — Add a custom trait with stat bonuses and an effect
// Source pattern: InfinityTraits, XMM MagicManager

using NeoModLoader.General;

public static void AddCustomTrait()
{
    // 1. Create a trait group (optional, for UI organization)
    ActorTraitGroupAsset group = new ActorTraitGroupAsset();
    group.id = "my_trait_group";
    group.name = "trait_group_my_trait_group";  // locale key
    group.color = "#00f7ff";
    AssetManager.trait_groups.add(group);

    // 2. Create the trait
    ActorTrait trait = new ActorTrait();
    trait.id = "fire_blood";
    trait.group_id = "my_trait_group";           // links to group above
    trait.path_icon = "ui/Icons/iconFire";       // game resource path
    trait.base_stats = new BaseStats();
    trait.base_stats.set("health", 50f);         // +50 health
    trait.base_stats.set("damage", 15f);         // +15 damage
    trait.base_stats.set("speed", 10f);          // +10 speed
    trait.rate_birth = 5;                        // 5% chance at birth
    trait.rate_inherit = 50;                     // 50% from parents
    trait.can_be_given = true;
    trait.can_be_removed = true;
    trait.special_effect_interval = 2f;          // runs every 2 seconds

    // 3. Add a periodic effect (runs on the actor each interval)
    trait.action_special_effect = (Actor pTarget, WorldTile pTile) =>
    {
        // Heal 5 HP every 2 seconds
        if (pTarget.hasHealth())
        {
            pTarget.changeHealth(5);
        }
        return false;  // return value unused for most effects
    };

    // 4. Register the trait
    AssetManager.traits.add(trait);
    trait.unlock(true);  // makes it visible in the trait editor

    // 5. Add localization
    LM.AddToCurrentLocale("trait_fire_blood", "Fire Blood");
    LM.AddToCurrentLocale("trait_fire_blood_info", "Burns with inner flame. +50 HP, +15 DMG. Heals 5 HP every 2s.");
    LM.AddToCurrentLocale("trait_group_my_trait_group", "My Custom Traits");
    LM.ApplyLocale(true);
}
