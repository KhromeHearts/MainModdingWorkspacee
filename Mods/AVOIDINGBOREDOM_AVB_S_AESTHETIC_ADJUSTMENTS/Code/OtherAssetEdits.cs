using System.Text;
using UnityEngine;
using NeoModLoader.api;

namespace aaaMod{
    internal static class OtherAssetEdits{
        public static void init(){
//Summon Angle
            PlotAsset editAngleRite = AssetManager.plots_library.get("summon_angles");
            editAngleRite.path_icon = $"ui/Icons/angleRiteNew";
//Summon Demon
            PlotAsset editDemonRite = AssetManager.plots_library.get("summon_demons");
            editDemonRite.path_icon = $"ui/Icons/demonRiteNew";
//Summon Skeleton
            PlotAsset editSkeletonRite = AssetManager.plots_library.get("summon_skeletons");
            editSkeletonRite.path_icon = $"ui/Icons/skeletonRiteNew";
//Summon Living Plants
            PlotAsset editLivingPlantRite = AssetManager.plots_library.get("summon_living_plants");
            editLivingPlantRite.path_icon = $"ui/Icons/livingplantRiteNew";
//Join Alliance
            PlotAsset editAllianceJoin = AssetManager.plots_library.get("alliance_join");
            editAllianceJoin.path_icon = $"ui/Icons/plot_alliance_joinNew";
//Cloud Acid
            GodPower editCloudAcidPower = AssetManager.powers.get("cloud_acid");
            editCloudAcidPower.path_icon  = "iconCloudAcidNew";
//Cloud Lava
            GodPower editCloudLavaPower = AssetManager.powers.get("cloud_lava");
            editCloudLavaPower.path_icon  = "iconCloudLavaNew";
//Cloud Rain
            GodPower editCloudRainPower = AssetManager.powers.get("cloud_rain");
            editCloudRainPower.path_icon  = "iconCloudRainNew";
//Sponge
            GodPower editSpongePower = AssetManager.powers.get("sponge");
            editSpongePower.path_icon  = "iconSpongeNew";

            LocalizedTextManager.add("sponge", "Broom", true, "", true);
//Sickle
            GodPower editSicklePower = AssetManager.powers.get("sickle");
            editSicklePower.path_icon  = "iconSickleNew";
//Pickaxe
            GodPower editPickaxePower = AssetManager.powers.get("pickaxe");
            editPickaxePower.path_icon  = "iconPickaxeNew";
//Axe
            GodPower editAxePower = AssetManager.powers.get("axe");
            editAxePower.path_icon  = "iconAxeNew";
//Atomic Bomb
            GodPower editAtomicBombPower = AssetManager.powers.get("atomic_bomb");
            editAtomicBombPower.path_icon  = "iconAtomicBombNew";
//Antimatter Bomb
            GodPower editAntimatterBombPower = AssetManager.powers.get("antimatter_bomb");
            editAntimatterBombPower.path_icon  = "iconAntimatterBombNew";

            DropAsset editAntimatterBombdrop = AssetManager.drops.get("antimatter_bomb");
            editAntimatterBombdrop.cached_sprites = new Sprite[] {Resources.Load<Sprite>("drops/" + "drop_antimatter_bomb_0"), Resources.Load<Sprite>("drops/" + "drop_antimatter_bomb_1"),
            Resources.Load<Sprite>("drops/" + "drop_antimatter_bomb_2"), Resources.Load<Sprite>("drops/" + "drop_antimatter_bomb_3")};
            editAntimatterBombdrop.animated = true;
            editAntimatterBombdrop.animation_speed = 0.05f;
//Friendship
            GodPower editFriendshipPower = AssetManager.powers.get("friendship");
            editFriendshipPower.path_icon  = "iconFriendshipNew_Des";
//Spite [Not working for some reason]
            GodPower editSpitePower = AssetManager.powers.get("spite");
            editSpitePower.path_icon = "iconSprite";
//Tnt
            GodPower editTntPower = AssetManager.powers.get("tnt");
            editTntPower.path_icon = "iconTntNew";
//Tnt Timed
            GodPower editTntTimedPower = AssetManager.powers.get("tnt_timed");
            editTntTimedPower.path_icon = "iconTntTimedNew";
//Water Bomb
            GodPower editWaterBombPower = AssetManager.powers.get("water_bomb");
            editWaterBombPower.path_icon = "iconWaterBombNew";
//Landmine
            GodPower editLandminePower = AssetManager.powers.get("landmine");
            editLandminePower.path_icon = "iconLandmineNew";
//Robot Santa
            GodPower editRobotSantaPower = AssetManager.powers.get("robot_santa");
            editRobotSantaPower.path_icon = "iconSantaNew";
//Dispel
            GodPower editDispelPower = AssetManager.powers.get("dispel");
            editDispelPower.path_icon = "iconDispelNew";
//Conway 1 (Purple)
            GodPower editConwayPurplePower = AssetManager.powers.get("conway");
            editConwayPurplePower.path_icon = "iconConwayLifeNew";
//Conway 2 (Green)
            GodPower editConwayGreenPower = AssetManager.powers.get("conway_inverse");
            editConwayGreenPower.path_icon = "iconConwayLife2New";
//SpiteFix
            GodPower fixSpitePower = AssetManager.powers.get("spite");
            fixSpitePower.path_icon = "iconSpite2";
//FingerFix
            GodPower fixFingerPower = AssetManager.powers.get("finger");
            fixFingerPower.path_icon = "iconFinger";
//LootRainEditFix
            GodPower fixLootRainEditPower = AssetManager.powers.get("equipment_rain_edit");
            fixLootRainEditPower.path_icon = "iconRainLootEdit";
//GammaRainFix
            GodPower fixGammaRainPower = AssetManager.powers.get("traits_gamma_rain");
            fixGammaRainPower.path_icon = "iconRainGamma";
//OmegaRainFix
            GodPower fixOmegaRainPower = AssetManager.powers.get("traits_omega_rain");
            fixOmegaRainPower.path_icon = "iconRainOmega";
//DeltaRainFix
            GodPower fixDeltaRainPower = AssetManager.powers.get("traits_delta_rain");
            fixDeltaRainPower.path_icon = "iconRainDelta";
//WormAndPrinterIcons (for strong mind trait tooltip)
            ActorAsset editPrinter = AssetManager.actor_library.get("printer");
            editPrinter.icon = "iconPrinterSpiral";
            ActorAsset editWorm = AssetManager.actor_library.get("worm");
            editWorm.icon = "iconWorm";
//FuseFix
            GodPower fixFuse = AssetManager.powers.get("fuse");
            fixFuse.path_icon = "iconFuse_Fire";
//FireworkFix
            GodPower fixFirework = AssetManager.powers.get("fireworks");
            fixFirework.path_icon = "iconFirework";
//TimeScales (3)
            WorldTimeScaleAsset tenx = AssetManager.time_scales.get("x10");
            tenx.path_icon = "ui/Icons/age_duration10x";
            WorldTimeScaleAsset fiveteenx = AssetManager.time_scales.get("x15");
            fiveteenx.path_icon = "ui/Icons/age_duration15x";
            WorldTimeScaleAsset twenx = AssetManager.time_scales.get("x20");
            twenx.path_icon = "ui/Icons/age_duration20x";
//ItemCategories (10)
            ItemGroupAsset helmet = AssetManager.item_groups.get("helmet");
            helmet.color = "#bad0ff";
            ItemGroupAsset armor = AssetManager.item_groups.get("armor");
            armor.color = "#bad0ff";
            ItemGroupAsset boots = AssetManager.item_groups.get("boots");
            boots.color = "#bad0ff";
            ItemGroupAsset amulet = AssetManager.item_groups.get("amulet");
            amulet.color = "#baffdf";
            ItemGroupAsset ring = AssetManager.item_groups.get("ring");
            ring.color = "#baffdf";
            ItemGroupAsset sword = AssetManager.item_groups.get("sword");
            sword.color = "#ffbaba";
            ItemGroupAsset axe = AssetManager.item_groups.get("axe");
            axe.color = "#ffbaba";
            ItemGroupAsset hammer = AssetManager.item_groups.get("hammer");
            hammer.color = "#ffbaba";
            ItemGroupAsset spear = AssetManager.item_groups.get("spear");
            spear.color = "#ffbaba";
            ItemGroupAsset bow = AssetManager.item_groups.get("bow");
            bow.color = "#e6baff";
            ItemGroupAsset staff = AssetManager.item_groups.get("staff");
            staff.color = "#fffeba";
            ItemGroupAsset firearm = AssetManager.item_groups.get("firearm");
            firearm.color = "#fffeba";
//Pollinating
            SubspeciesTrait pollinate = AssetManager.subspecies_traits.get("pollinating");
            pollinate.path_icon = "ui/Icons/iconPollinating";
            pollinate.cached_sprite = Resources.Load<Sprite>("ui/Icons/iconPollinating");
//BeetlePositionFix
            ActorAsset beetle = AssetManager.actor_library.get("beetle");
            beetle.icon = "iconBeetleNew";
//Phenotype Fixes
            PhenotypeAsset jungle = AssetManager.phenotype_library.get("jungle");
            jungle.shades_from = "#2EB829";
            jungle.shades_to = "#03803D";
            PhenotypeAsset pinkyellowmushroom = AssetManager.phenotype_library.get("pink_yellow_mushroom");
            pinkyellowmushroom.shades_from = "#FFAC26";
            pinkyellowmushroom.shades_to = "#F12C78";
            PhenotypeAsset swamp = AssetManager.phenotype_library.get("swamp");
            swamp.shades_from = "#367F58";
            swamp.shades_to = "#784D85";

//Phenotype Locale Edits-------------------------------------------------------------------------------------------------------
            foreach (PhenotypeAsset pt in AssetManager.phenotype_library.list){
                SubspeciesTrait pt2 = AssetManager.subspecies_traits.get($"phenotype_skin_{pt.id}");
                pt2.special_locale_id = $"pt_{pt.id}";
                LocalizedTextManager.add(pt2.special_locale_id, OtherAssetEdits.fixLowercaseAndUnderscore(pt.id), false, "", true);
            }
        }
        public static string fixLowercaseAndUnderscore(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            string inputSpaced = input.Replace("_", " ");
            int firstlettercount = 0;
            bool needsCapitalized = false;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char i in inputSpaced)
            {
                firstlettercount++;
                if (firstlettercount == 1)
                {
                    stringBuilder.Append(char.ToUpper(i));
                }
                if (char.IsWhiteSpace(i))
                {
                    stringBuilder.Append(i);
                    needsCapitalized = true;
                }
                else if (needsCapitalized)
                {
                    stringBuilder.Append(char.ToUpper(i));
                    needsCapitalized = false;
                }
                else if (firstlettercount != 1)
                {
                    stringBuilder.Append(i);
                }
            }
            return stringBuilder.ToString();
		}
    }	
}





