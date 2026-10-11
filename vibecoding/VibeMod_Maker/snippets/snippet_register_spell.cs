// snippet_register_spell.cs — Register a custom spell and link it to a trait
// Source pattern: XMM MagicManager

public static void RegisterCustomSpell()
{
    // 1. Create the spell asset
    SpellAsset spell = new SpellAsset();
    spell.id = "spell_fireball";
    spell.action = new AttackAction(ActionLibrary.castFire);  // what happens on cast
    spell.cast_target = CastTarget.Enemy;                      // who to target
    spell.cast_entity = CastEntity.UnitsOnly;                  // only units, not buildings
    spell.chance = 0.75f;                                      // 75% cast chance
    spell.cost_mana = 5;                                       // mana cost
    spell.min_distance = 2f;                                   // minimum range
    spell.can_be_used_in_combat = true;

    // 2. Register it
    AssetManager.spells.add(spell);

    // 3. Create a trait that grants this spell
    ActorTrait trait = new ActorTrait();
    trait.id = "pyromancer";
    trait.group_id = "special";
    trait.path_icon = "ui/Icons/iconFire";
    trait.can_be_given = true;
    trait.can_be_removed = true;
    trait.base_stats = new BaseStats();
    trait.base_stats.set("mana", 20f);          // grant mana pool
    trait.spells_ids = new List<string> { "spell_fireball" };
    trait.linkSpells();                          // IMPORTANT: connects spell IDs to SpellAssets
    AssetManager.traits.add(trait);
}
