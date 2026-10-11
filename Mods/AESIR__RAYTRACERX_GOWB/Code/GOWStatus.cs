using System.Collections.Generic;
using UnityEngine;

namespace GOWB
{
    internal static class GOWStatus
    {
        public static void Init()
        {
            if (AssetManager.status.get("Thunder god's rage") != null)
            {
                return;
            }

            Material material = LibraryMaterials.instance.dict["mat_world_object_lit"];

            StatusAsset thorAura = new StatusAsset();
            thorAura.id = "Thunder god's rage";
            thorAura.locale_id = "status_title_thunder_gods_rage";
            thorAura.locale_description = "status_description_thunder_gods_rage";
            thorAura.texture = "thor_lightning_aura";
            thorAura.animated = true;
            thorAura.animation_speed = 0.1f;
            thorAura.base_stats = new BaseStats();
            thorAura.base_stats.set("armor", 40f);
            thorAura.base_stats.set("multiplier_damage", 0.40f);
            thorAura.base_stats.set("attack_speed", 0.40f);
            thorAura.base_stats.set("mass",  90f);
            thorAura.duration = 30f;
            thorAura.path_icon = "ui/icons/ThorAura";
            thorAura.draw_light_area = true;
            thorAura.draw_light_size = 0.25f;
            thorAura.random_flip = true;
            thorAura.tier = StatusTier.Advanced;
            thorAura.action_interval = 0.4f;
            thorAura.action = new WorldAction(thoraurara);
            thorAura.need_visual_render = true;
            thorAura.material = material;
            thorAura.sprite_list = SpriteTextureLoader.getSpriteList("effects/thor_lightning_aura", false);
            thorAura.remove_status = new[] { "frozen", "Vines Spell", "Talon Effect", "stunned" };
            thorAura.opposite_status = new[] { "frozen" };
            AssetManager.status.add(thorAura);

            StatusAsset bifrostShield = new StatusAsset();
            bifrostShield.id = "Bifrost Shield";
            bifrostShield.locale_id = "status_title_bifrost_shield";
            bifrostShield.locale_description = "status_description_bifrost_shield";
            bifrostShield.texture = "bifrost_shield";
            bifrostShield.animated = true;
            bifrostShield.animation_speed = 0.25f;
            bifrostShield.base_stats = new BaseStats();
            bifrostShield.base_stats.set("armor", 70f);
            bifrostShield.base_stats.set("multiplier_damage", 0.25f);
            bifrostShield.base_stats.set("critical_chance", 0.25f);
            bifrostShield.base_stats.set("mass",  100f);
            bifrostShield.duration = 30f;
            bifrostShield.path_icon = "ui/icons/BifrostShield";
            bifrostShield.draw_light_area = true;
            bifrostShield.draw_light_size = 0.5f;
            bifrostShield.tier = StatusTier.Advanced;
            bifrostShield.need_visual_render = true;
            bifrostShield.material = material;
            bifrostShield.sprite_list = SpriteTextureLoader.getSpriteList("effects/bifrost_shield", false);
            bifrostShield.remove_status = new[] { "burning", "frozen", "poisoned", "shield", "Vines Spell", "slowness", "Talon Effect", "stunned" };
            bifrostShield.opposite_status = new[] { "frozen" };
            AssetManager.status.add(bifrostShield);

            StatusAsset fireShield = new StatusAsset();
            fireShield.id = "Fire Shield";
            fireShield.locale_id = "status_title_fire_shield";
            fireShield.locale_description = "status_description_fire_shield";
            fireShield.texture = "fire_shield";
            fireShield.animated = true;
            fireShield.animation_speed = 0.25f;
            fireShield.base_stats = new BaseStats();
            fireShield.base_stats.set("armor", 70f);
            fireShield.base_stats.set("multiplier_damage", 0.25f);
            fireShield.base_stats.set("critical_damage_multiplier", 0.25f);
            fireShield.base_stats.set("mass",  100f);
            fireShield.duration = 30f;
            fireShield.path_icon = "ui/icons/FireShield";
            fireShield.draw_light_area = true;
            fireShield.draw_light_size = 0.5f;
            fireShield.tier = StatusTier.Advanced;
            fireShield.need_visual_render = true;
            fireShield.material = material;
            fireShield.sprite_list = SpriteTextureLoader.getSpriteList("effects/fire_shield", false);
            fireShield.remove_status = new[] { "burning", "frozen", "shield", "Vines Spell", "slowness", "Talon Effect", "stunned" };
            fireShield.opposite_status = new[] { "frozen" };
            AssetManager.status.add(fireShield);

            StatusAsset frostShield = new StatusAsset();
            frostShield.id = "Frost Shield";
            frostShield.locale_id = "status_title_frost_shield";
            frostShield.locale_description = "status_description_frost_shield";
            frostShield.texture = "frost_shield";
            frostShield.animated = true;
            frostShield.animation_speed = 0.25f;
            frostShield.base_stats = new BaseStats();
            frostShield.base_stats.set("armor", 70f);
            frostShield.base_stats.set("multiplier_damage", 0.25f);
            frostShield.base_stats.set("attack_speed", 0.25f);
            frostShield.base_stats.set("mass",  100f);
            frostShield.duration = 30f;
            frostShield.path_icon = "ui/icons/FrostShield";
            frostShield.draw_light_area = true;
            frostShield.draw_light_size = 0.5f;
            frostShield.tier = StatusTier.Advanced;
            frostShield.need_visual_render = true;
            frostShield.material = material;
            frostShield.sprite_list = SpriteTextureLoader.getSpriteList("effects/frost_shield", false);
            frostShield.remove_status = new[] { "burning", "frozen", "shield", "Vines Spell", "slowness", "Talon Effect", "stunned" };
            frostShield.opposite_status = new[] { "frozen" };
            AssetManager.status.add(frostShield);

            StatusAsset spartanRage = new StatusAsset();
            spartanRage.id = "Spartan Rage";
            spartanRage.locale_id = "status_title_spartan_rage";
            spartanRage.locale_description = "status_description_spartan_rage";
            spartanRage.texture = "spartan_rage";
            spartanRage.animated = true;
            spartanRage.animation_speed = 0.15f;
            spartanRage.base_stats = new BaseStats();
            spartanRage.base_stats.set("armor", 50f);
            spartanRage.base_stats.set("damage", 30f);
            spartanRage.base_stats.set("critical_chance", 0.50f);
            spartanRage.base_stats.set("critical_damage_multiplier", 0.50f);
            spartanRage.base_stats.set("multiplier_damage", 0.40f);
            spartanRage.base_stats.set("multiplier_speed", 0.40f);
            spartanRage.base_stats.set("attack_speed", 0.40f);
            spartanRage.base_stats.set("mass", 120f);
            spartanRage.duration = 30f;
            spartanRage.path_icon = "ui/icons/SpartanRage";
            spartanRage.draw_light_area = true;
            spartanRage.draw_light_size = 0.25f;
            spartanRage.random_flip = true;
            spartanRage.tier = StatusTier.Advanced;
            spartanRage.action_interval = 0.4f;
            spartanRage.action = new WorldAction(spartanragera);
            spartanRage.need_visual_render = true;
            spartanRage.material = material;
            spartanRage.sprite_list = SpriteTextureLoader.getSpriteList("effects/spartan_rage", false);
            spartanRage.remove_status = new[] { "frozen", "Vines Spell", "slowness", "Talon Effect", "stunned" };
            spartanRage.opposite_status = new[] { "frozen" };
            AssetManager.status.add(spartanRage);

            StatusAsset bifrostArmor = new StatusAsset();
            bifrostArmor.id = "Bifrost Armor";
            bifrostArmor.locale_id = "status_title_bifrost_armor";
            bifrostArmor.locale_description = "status_description_bifrost_armor";
            bifrostArmor.texture = "bifrost_armor";
            bifrostArmor.animated = true;
            bifrostArmor.animation_speed = 0.2f;
            bifrostArmor.base_stats = new BaseStats();
            bifrostArmor.base_stats.set("armor", 50f);
            bifrostArmor.base_stats.set("damage", 20f);
            bifrostArmor.base_stats.set("multiplier_speed", 0.30f);
            bifrostArmor.base_stats.set("mass", 80f);
            bifrostArmor.duration = 30f;
            bifrostArmor.path_icon = "ui/icons/BifrostArmor";
            bifrostArmor.draw_light_area = true;
            bifrostArmor.draw_light_size = 0.15f;
            bifrostArmor.tier = StatusTier.Advanced;
            bifrostArmor.need_visual_render = true;
            bifrostArmor.material = material;
            bifrostArmor.sprite_list = SpriteTextureLoader.getSpriteList("effects/bifrost_armor", false);
            bifrostArmor.remove_status = new[] { "burning", "frozen", "slowness", "Talon Effect", "stunned" };
            bifrostArmor.opposite_status = new[] { "frozen" };
            AssetManager.status.add(bifrostArmor);

            StatusAsset bindingSpell = new StatusAsset();
            bindingSpell.id = "Binding Spell";
            bindingSpell.locale_id = "status_title_binding_spell";
            bindingSpell.locale_description = "status_description_binding_spell";
            bindingSpell.texture = "binding_spell";
            bindingSpell.animated = true;
            bindingSpell.animation_speed = 0.3f;
            bindingSpell.base_stats = new BaseStats();
            bindingSpell.base_stats.set("multiplier_damage", -1f);
            bindingSpell.base_stats.set("multiplier_attack_speed", -1f);
            bindingSpell.base_stats.set("multiplier_speed", -1f);
            bindingSpell.base_stats.set("mass", 120f);
            bindingSpell.duration = 30f;
            bindingSpell.allow_timer_reset = false;
            bindingSpell.path_icon = "ui/icons/BindingSpell";
            bindingSpell.draw_light_area = true;
            bindingSpell.draw_light_size = 0.1f;
            bindingSpell.tier = StatusTier.Advanced;
            bindingSpell.need_visual_render = true;
            bindingSpell.material = material;
            bindingSpell.sprite_list = SpriteTextureLoader.getSpriteList("effects/binding_spell", false);
            bindingSpell.opposite_traits = new[] { "Odin", "Freya" };
            bindingSpell.remove_status = new[] { "Bifrost Armor", "Spartan Rage", "Thunder god's rage" };
            AssetManager.status.add(bindingSpell);

            StatusAsset noFriendlyFire = new StatusAsset();
            noFriendlyFire.id = "No friendly fire";
            noFriendlyFire.locale_id = "status_title_no_friendly_fire";
            noFriendlyFire.locale_description = "status_description_no_friendly_fire";
            noFriendlyFire.base_stats = new BaseStats();
            noFriendlyFire.base_stats.set("armor", 1f);
            noFriendlyFire.duration = 30f;
            noFriendlyFire.allow_timer_reset = true;
            noFriendlyFire.path_icon = "ui/icons/Trollface";
            noFriendlyFire.remove_status = new[] { "burning", "frozen" };
            noFriendlyFire.opposite_status = new[] { "frozen" };
            AssetManager.status.add(noFriendlyFire);

            StatusAsset dodgeEffect = new StatusAsset();
            dodgeEffect.id = "Dodge Effect";
            dodgeEffect.locale_id = "status_title_dodged";
            dodgeEffect.locale_description = "status_description_dodged";
            dodgeEffect.base_stats = new BaseStats();
            dodgeEffect.base_stats.set("skill_combat", 0.75f);
            dodgeEffect.base_stats.set("armor", 100f);
            dodgeEffect.duration = 30f;
            dodgeEffect.allow_timer_reset = true;
            dodgeEffect.path_icon = "ui/icons/DodgeEffect";
            dodgeEffect.remove_status = new[] { "burning", "stunned" };
            dodgeEffect.tier = StatusTier.Advanced;
            AssetManager.status.add(dodgeEffect);

            StatusAsset vinesSpell = new StatusAsset();
            vinesSpell.id = "Vines Spell";
            vinesSpell.locale_id = "status_title_rot";
            vinesSpell.locale_description = "status_description_rot";
            vinesSpell.texture = "vines_spell";
            vinesSpell.animated = false;
            vinesSpell.base_stats = new BaseStats();
            vinesSpell.base_stats.set("multiplier_damage", -1f);
            vinesSpell.base_stats.set("multiplier_health", -0.5f);
            vinesSpell.base_stats.set("multiplier_speed", -1f);
            vinesSpell.base_stats.set("multiplier_attack_speed", -1f);
            vinesSpell.base_stats.set("mass", 120f);
            vinesSpell.duration = 30f;
            vinesSpell.path_icon = "ui/icons/VinesSpell";
            vinesSpell.action_interval = 1f;
            vinesSpell.action = new WorldAction(vinesspellra);
            vinesSpell.need_visual_render = true;
            vinesSpell.material = material;
            vinesSpell.sprite_list = SpriteTextureLoader.getSpriteList("effects/vines_spell", false);
            vinesSpell.opposite_status = new[] { "Bifrost Shield", "Fire Shield", "Frost Shield", "Bifrost Armor", "Spartan Rage", "Thunder god's rage" };
            AssetManager.status.add(vinesSpell);

            StatusAsset knockbackResistance = new StatusAsset();
            knockbackResistance.id = "Knockback Resistance";
            knockbackResistance.locale_id = "status_title_knockback_resistance";
            knockbackResistance.locale_description = "status_description_knockback_resistance";
            knockbackResistance.base_stats = new BaseStats();
            knockbackResistance.base_stats.set("mass", 140f);
            knockbackResistance.duration = 30f;
            knockbackResistance.allow_timer_reset = true;
            knockbackResistance.path_icon = "ui/icons/KnockbackResistance";
            AssetManager.status.add(knockbackResistance);

            StatusAsset curseCured = new StatusAsset();
            curseCured.id = "Curse Cured";
            curseCured.locale_id = "status_title_cured";
            curseCured.locale_description = "status_description_cured";
            curseCured.texture = "curse_cured";
            curseCured.animated = true;
            curseCured.animation_speed = 0.25f;
            curseCured.base_stats = new BaseStats();
            curseCured.base_stats.set("multiplier_health", 1f);
            curseCured.base_stats.set("armor", 100f);
            curseCured.duration = 60f;
            curseCured.path_icon = "ui/icons/CurseCured";
            curseCured.draw_light_area = true;
            curseCured.draw_light_size = 0.2f;
            curseCured.need_visual_render = true;
            curseCured.material = material;
            curseCured.sprite_list = SpriteTextureLoader.getSpriteList("effects/curse_cured", false);
            AssetManager.status.add(curseCured);

            StatusAsset talonEffect = new StatusAsset();
            talonEffect.id = "Talon Effect";
            talonEffect.locale_id = "status_title_talon_shock";
            talonEffect.locale_description = "status_description_talon_shock";
            talonEffect.texture = "talon_effect";
            talonEffect.animated = true;
            talonEffect.random_frame = true;
            talonEffect.animation_speed = 0.25f;
            talonEffect.animation_speed_random = 0.08f;
            talonEffect.base_stats = new BaseStats();
            talonEffect.base_stats.set("multiplier_damage", -0.3f);
            talonEffect.base_stats.set("multiplier_attack_speed", -0.8f);
            talonEffect.base_stats.set("multiplier_speed", -0.8f);
            talonEffect.base_stats.set("mass", 120f);
            talonEffect.duration = 30f;
            talonEffect.path_icon = "ui/icons/TalonEffect";
            talonEffect.action_interval = 0.25f;
            talonEffect.draw_light_area = true;
            talonEffect.draw_light_size = 0.2f;
            talonEffect.tier = StatusTier.Advanced;
            talonEffect.action = new WorldAction(taloneffectra);
            talonEffect.need_visual_render = true;
            talonEffect.material = material;
            talonEffect.sprite_list = SpriteTextureLoader.getSpriteList("effects/talon_effect", false);
            talonEffect.opposite_traits = new[] { "Jormungandr", "Heimdall" };
            talonEffect.opposite_status = new[] { "shield", "Spartan Rage", "Thunder god's rage", "Fire Shield", "Bifrost Shield", "Frost Shield", "Bifrost Armor" };
            AssetManager.status.add(talonEffect);

            StatusAsset burning = AssetManager.status.get("burning");
            if (burning != null)
            {
                List<string> opposite = new List<string>();
                if (burning.opposite_status != null)
                {
                    opposite.AddRange(burning.opposite_status);
                }
                if (!opposite.Contains("Bifrost Shield")) opposite.Add("Bifrost Shield");
                if (!opposite.Contains("Fire Shield")) opposite.Add("Fire Shield");
                if (!opposite.Contains("Bifrost Armor")) opposite.Add("Bifrost Armor");
                if (!opposite.Contains("No friendly fire")) opposite.Add("No friendly fire");
                if (!opposite.Contains("Dodge Effect")) opposite.Add("Dodge Effect");
                if (!opposite.Contains("Frost Shield")) opposite.Add("Frost Shield");
                burning.opposite_status = opposite.ToArray();
            }

            StatusAsset frozen = AssetManager.status.get("frozen");
            if (frozen != null)
            {
                List<string> opposite = new List<string>();
                if (frozen.opposite_status != null)
                {
                    opposite.AddRange(frozen.opposite_status);
                }
                if (!opposite.Contains("Bifrost Shield")) opposite.Add("Bifrost Shield");
                if (!opposite.Contains("Fire Shield")) opposite.Add("Fire Shield");
                if (!opposite.Contains("Bifrost Armor")) opposite.Add("Bifrost Armor");
                if (!opposite.Contains("Spartan Rage")) opposite.Add("Spartan Rage");
                if (!opposite.Contains("Thunder god's rage")) opposite.Add("Thunder god's rage");
                if (!opposite.Contains("No friendly fire")) opposite.Add("No friendly fire");
                if (!opposite.Contains("Frost Shield")) opposite.Add("Frost Shield");
                frozen.opposite_status = opposite.ToArray();
            }

            StatusAsset stunned = AssetManager.status.get("stunned");
            if (frozen != null)
            {
                List<string> opposite = new List<string>();
                if (stunned.opposite_status != null)
                {
                    opposite.AddRange(stunned.opposite_status);
                }
                if (!opposite.Contains("Bifrost Shield")) opposite.Add("Bifrost Shield");
                if (!opposite.Contains("Fire Shield")) opposite.Add("Fire Shield");
                if (!opposite.Contains("Bifrost Armor")) opposite.Add("Bifrost Armor");
                if (!opposite.Contains("Spartan Rage")) opposite.Add("Spartan Rage");
                if (!opposite.Contains("Thunder god's rage")) opposite.Add("Thunder god's rage");
                if (!opposite.Contains("No friendly fire")) opposite.Add("No friendly fire");
                if (!opposite.Contains("Frost Shield")) opposite.Add("Frost Shield");
                stunned.opposite_status = opposite.ToArray();
            }

            StatusAsset poisoned = AssetManager.status.get("poisoned");
            if (poisoned != null)
            {
                List<string> opposite = new List<string>();
                if (poisoned.opposite_status != null)
                {
                    opposite.AddRange(poisoned.opposite_status);
                }
                if (!opposite.Contains("Bifrost Shield")) opposite.Add("Bifrost Shield");
                poisoned.opposite_status = opposite.ToArray();
            }

            StatusAsset slowness = AssetManager.status.get("slowness");
            if (slowness != null)
            {
                List<string> opposite = new List<string>();
                if (slowness.opposite_status != null)
                {
                    opposite.AddRange(slowness.opposite_status);
                }
                if (!opposite.Contains("Bifrost Shield")) opposite.Add("Bifrost Shield");
                if (!opposite.Contains("Fire Shield")) opposite.Add("Fire Shield");
                if (!opposite.Contains("Spartan Rage")) opposite.Add("Spartan Rage");
                if (!opposite.Contains("Frost Shield")) opposite.Add("Frost Shield");
                slowness.opposite_status = opposite.ToArray();

                List<string> oppositeTraits = new List<string>();
                if (slowness.opposite_traits != null)
                {
                    oppositeTraits.AddRange(slowness.opposite_traits);
                }
                if (!oppositeTraits.Contains("Jormungandr")) oppositeTraits.Add("Jormungandr");
                slowness.opposite_traits = oppositeTraits.ToArray();
            }

            StatusAsset shield = AssetManager.status.get("shield");
            if (shield != null)
            {
                List<string> remove = new List<string>();
                if (shield.remove_status != null)
                {
                    remove.AddRange(shield.remove_status);
                }
                if (!remove.Contains("Talon Effect")) remove.Add("Talon Effect");
                shield.remove_status = remove.ToArray();
            }
        }

        public static bool taloneffectra(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget == null || !pTarget.isAlive())
            {
                return false;
            }

            int damage = (int)(pTarget.getMaxHealth() * 0.01f) + 1;
            if (pTarget.isBuilding() && pTarget.b != null && pTarget.b.isRuin())
            {
                damage = (int)(damage * 0.025f + 1f);
            }

            pTarget.getHit(damage, true, AttackType.Fire, null, true, false);
            if (pTarget.isActor() && pTarget.a != null)
            {
                pTarget.a.spawnParticle(Toolbox.color_yellow);
                pTarget.a.startShake(0.4f, 0.2f, true, false);
            }

            return true;
        }

        public static bool vinesspellra(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget != null && pTarget.isActor() && pTarget.a != null)
            {
                pTarget.a.spawnParticle(Toolbox.color_green);
            }
            return true;
        }

        public static bool spartanragera(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget != null && pTarget.isActor() && pTarget.a != null)
            {
                pTarget.a.spawnParticle(Toolbox.color_fire);
                if (pTarget.a.hasTrait("cursed"))
                {
                    pTarget.a.removeTrait("cursed");
                }
                if (pTarget.a.hasStatus("stunned"))
                {
                    pTarget.a.finishStatusEffect("stunned");
                }
            }
            return true;
        }

        public static bool thoraurara(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget != null && pTarget.isActor() && pTarget.a != null)
            {
                if (pTarget.a.hasStatus("stunned"))
                {
                    pTarget.a.finishStatusEffect("stunned");
                }
            }
            return true;
        }
    }
}
