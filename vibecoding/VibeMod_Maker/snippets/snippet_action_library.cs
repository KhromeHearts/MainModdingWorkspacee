// snippet_action_library.cs — Using ActionLibrary for transformations and spells
// Source pattern: XMM MagicManager, game API

// ActionLibrary contains static methods for common game actions.
// These are the SAFEST way to trigger effects — they handle all internal
// state changes, animations, and side effects.

public static class ActionLibraryExamples
{
    // ---- Transform a unit into another type ----
    public static void TransformUnit(Actor actor)
    {
        // Turn into zombie (keeps traits, changes asset)
        ActionLibrary.turnIntoZombie(actor, null);

        // Turn into demon
        ActionLibrary.turnIntoDemon(actor, null);

        // Metamorph into any actor type
        ActionLibrary.metamorphInto(actor, "dragon", false);
        // Args: target, new asset ID, remove acquired traits
    }

    // ---- Cast spells (used as AttackAction delegates) ----
    // These are the methods you assign to SpellAsset.action or trait effects:
    //
    //   ActionLibrary.castFire           — sets target on fire
    //   ActionLibrary.castLightning      — lightning strike
    //   ActionLibrary.castTornado        — spawns tornado
    //   ActionLibrary.castCure           — heals target
    //   ActionLibrary.castCurses         — applies curse
    //   ActionLibrary.castBloodRain      — blood rain effect
    //   ActionLibrary.castShieldOnHimself — shield buff
    //   ActionLibrary.castSpellSilence   — silences target
    //   ActionLibrary.castSpawnSkeleton  — summons skeleton
    //   ActionLibrary.teleportRandom     — teleport actor
    //   ActionLibrary.castSpawnGrassSeeds — terrain effect
    //   ActionLibrary.castSpawnFertilizer — terrain fertility

    // ---- Using as trait effect ----
    public static void CreateTraitWithSpellAction()
    {
        ActorTrait trait = new ActorTrait();
        trait.id = "pyromancer";
        trait.action_attack_target = ActionLibrary.castFire;
        // This fires ActionLibrary.castFire on every attack hit
        AssetManager.traits.add(trait);
    }

    // ---- Using in a god power ----
    public static void CreateLightningPower()
    {
        GodPower power = new GodPower();
        power.id = "smite";
        power.click_power_action = (WorldTile pTile, GodPower pPower) =>
        {
            MapBox.spawnLightningBig(pTile, 0.25f);
            foreach (Actor a in World.world.units)
            {
                if (Toolbox.DistTile(a.current_tile, pTile) < 3f)
                {
                    a.getHit(50f, true, AttackType.Divine, null);
                }
            }
            return true;
        };
        AssetManager.powers.add(power);
    }

    // ---- Other useful ActionLibrary methods ----
    public static void OtherActions(Actor actor)
    {
        ActionLibrary.removeUnit(actor);                     // clean removal
        Actor nearest = ActionLibrary.getActorNearPos(actor.current_position); // find nearby
    }
}
