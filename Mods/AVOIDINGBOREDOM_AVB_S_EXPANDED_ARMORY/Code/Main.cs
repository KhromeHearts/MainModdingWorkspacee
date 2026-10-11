/*Hey! If your looking at this mod code to get an idea on how to add items into your own mod, 
feel free to dm me on discord if your confused why things on your end don't seem to be working.-Avb*/
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using NeoModLoader.api;
using UnityEngine;
using Beebyte.Obfuscator;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using ai;
using ai.behaviours;

namespace avbsexpandedarmory{
    class Main : BasicMod<Main>{
        public static Main instance;
        static Harmony _harmony;
        protected override void OnModLoad(){
            Debug.Log($"<color=#FF4B33>Avb's Expanded Armory mod report: Loaded patch to prevent \"Weapon Texture is Missing\" error spam</color>");
            Debug.Log($"<color=#FF4B33>Avb's Expanded Armory mod report: Minimum possible attack speed lowered from 0.5 to 0.25</color>");

            FillInItemsAndExtraChanges.init();
            BasicWeaponItems.init();
            BasicGearItems.init();
            SpecialItems.init();
            LoadWeaponSprites.fixItemModifiersAndSpells();
            if(ConfigsAEA.rebalanceVanillaWeapons == true){ItemRebalance.init();}
            if(ConfigsAEA.creatureDefaults == true){Creatures.init();}
            if(ConfigsAEA.loverDefaults == true){MetaLoverTraits.init();}
            if (ConfigsAEA.changeRaritySystem == true){ItemRaritySystem.init();}

            if (!Config.preload_windows){
                Debug.Log($"<color=#FF4B33>Avb's Expanded Armory mod report: Preload Windows setting is disabled, forced game to preload custom weapon sprites</color>");
                SmoothLoader.add(delegate{HandRendererTexturePreloader.launch();}, "Preload hand item textures...", false, 0.2f, false);
            }
            _harmony = new Harmony("AvbItems");
            _harmony.PatchAll();
        }
    }
    
    //---Credit for code help: This page uses minorly modified LoadSpritesPatch patch from the Modernbox mod
    //---This mod received no direct assistance from the contributors of Modernbox mod and was made by referencing its code
    [HarmonyPatch(typeof(ItemLibrary), "loadSprites")]
    public static class LoadMiniSpritesPatch{
        static void Postfix(ItemLibrary __instance){
            LoadWeaponSprites.init();
            Type RecievePreload = typeof(DynamicSprites).Assembly.GetType("HandRendererTexturePreloader");  
            MethodInfo ExtendedPreload = RecievePreload.GetMethod("preloadItemsIntoAtlas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            ExtendedPreload.Invoke(null, null);
        }
    }

    [HarmonyPatch(typeof(ItemCrafting), "craftItem")]
    public static class CraftingPatch{
        static bool Prefix(ref bool __result, Actor pActor, string pCreatorName, EquipmentType pType, int pTries, City pCity){
            //start of subtype selection
            string tEquipmentSubtype = null;
            if (pType == EquipmentType.Weapon){
                if (pActor.hasCulture()){
                    List<string> validSubtypes = new List<string>();
                    foreach (string subtype in pActor.culture._preferred_weapons_craft_subtypes){ //this only addresses culture traits with a subtype (so not for special items: ex. flame sword)
                        validSubtypes.Add(subtype); //armor related culture traits do not have a subtype directly attached to them, so no need to filter those out
                    }
                    if (validSubtypes.Count == 0){
                        tEquipmentSubtype = null;
                    }
                    else{
                        tEquipmentSubtype = validSubtypes.GetRandom<string>();
                    }
                }
                if (string.IsNullOrEmpty(tEquipmentSubtype) && ItemLibrary.default_weapon_pool.Length > 0){
                    tEquipmentSubtype = ItemLibrary.default_weapon_pool.GetRandom<string>();
                }
            }
            else if (pType == EquipmentType.Helmet){
                List<string> headGear = new List<string> {"helmet", "headband"};
                if (!ConfigsAEA.helmetCraft){headGear.Remove("helmet");}
                if (!ConfigsAEA.headbandCraft){headGear.Remove("headband");}
                if (headGear.Count == 2){
                    if (pActor.hasCulture()){
                        if (pActor.culture.hasTrait("Helmet Lovers")){
                            tEquipmentSubtype = "helmet";
                        }
                        else if (pActor.culture.hasTrait("Headband Lovers")){
                            tEquipmentSubtype = "headband";
                        }
                        else{
                            tEquipmentSubtype = headGear.GetRandom<string>();
                        }
                    }
                    else {
                        tEquipmentSubtype = headGear.GetRandom<string>();
                    }
                }
                else if (headGear.Count == 1){
                    tEquipmentSubtype = headGear[0];
                }
                else{
                    __result = false;
                    return false;
                }
            }
            else if (pType == EquipmentType.Armor){
                List<string> chestGear = new List<string> {"armor", "cloak"};
                if (!ConfigsAEA.armorCraft){chestGear.Remove("armor");}
                if (!ConfigsAEA.cloakCraft){chestGear.Remove("cloak");}
                if (chestGear.Count == 2){
                    if (pActor.hasCulture()){
                        if (pActor.culture.hasTrait("Armor Lovers")){
                            tEquipmentSubtype = "armor";
                        }
                        else if (pActor.culture.hasTrait("Cloak Lovers")){
                            tEquipmentSubtype = "cloak";
                        }
                        else{
                            tEquipmentSubtype = chestGear.GetRandom<string>();
                        }
                    }
                    else {
                        tEquipmentSubtype = chestGear.GetRandom<string>();
                    }
                }
                else if (chestGear.Count == 1){
                    tEquipmentSubtype = chestGear[0];
                }
                else{
                    __result = false;
                    return false;
                }
            }
            else if (pType == EquipmentType.Boots){
                List<string> footGear = new List<string> {"boots", "highcleats"};
                if (!ConfigsAEA.bootCraft){footGear.Remove("boots");}
                if (!ConfigsAEA.highcleatCraft){footGear.Remove("highcleats");}
                if (footGear.Count == 2){
                    if (pActor.hasCulture()){
                        if (pActor.culture.hasTrait("Boot Lovers")){
                            tEquipmentSubtype = "boots";
                        }
                        else if (pActor.culture.hasTrait("Highcleat Lovers")){
                            tEquipmentSubtype = "highcleats";
                        }
                        else{
                            tEquipmentSubtype = footGear.GetRandom<string>();
                        }
                    }
                    else {
                        tEquipmentSubtype = footGear.GetRandom<string>();
                    }
                }
                else if (footGear.Count == 1){
                    tEquipmentSubtype = footGear[0];
                }
                else{
                    __result = false;
                    return false;
                }
            }
            else if (pType == EquipmentType.Amulet){
                List<string> loopGear = new List<string> {"amulet", "bracelet"};
                if (!ConfigsAEA.amuletCraft){loopGear.Remove("amulet");}
                if (!ConfigsAEA.braceletCraft){loopGear.Remove("bracelet");}
                if (loopGear.Count == 2){
                    if (pActor.hasCulture()){
                        if (pActor.culture.hasTrait("Amulet Lovers")){
                            tEquipmentSubtype = "amulet";
                        }
                        else if (pActor.culture.hasTrait("Bracelet Lovers")){
                            tEquipmentSubtype = "bracelet";
                        }
                        else{
                            tEquipmentSubtype = loopGear.GetRandom<string>();
                        }
                    }
                    else {
                        tEquipmentSubtype = loopGear.GetRandom<string>();
                    }
                }
                else if (loopGear.Count == 1){
                    tEquipmentSubtype = loopGear[0];
                }
                else{
                    __result = false;
                    return false;
                }
            }
            else if (pType == EquipmentType.Ring){
                List<string> ringGear = new List<string> {"ring", "earrings"};
                if (!ConfigsAEA.ringCraft){ringGear.Remove("ring");}
                if (!ConfigsAEA.earringCraft){ringGear.Remove("earrings");}
                if (ringGear.Count == 2){
                    if (pActor.hasCulture()){
                        if (pActor.culture.hasTrait("Ring Lovers")){
                            tEquipmentSubtype = "ring";
                        }
                        else if (pActor.culture.hasTrait("Earring Lovers")){
                            tEquipmentSubtype = "earrings";
                        }
                        else{
                            tEquipmentSubtype = ringGear.GetRandom<string>();
                        }
                    }
                    else {
                        tEquipmentSubtype = ringGear.GetRandom<string>();
                    }
                }
                else if (ringGear.Count == 1){
                    tEquipmentSubtype = ringGear[0];
                }
                else{
                    __result = false;
                    return false;
                }
            }

            if (tEquipmentSubtype == null){
                //Debug.Log($"OH NO tEquipmentSubtype IS NULL!");
                return false;
            }
            //end of subtype selection & start of special item selection (flame, ice, etc)
            List<EquipmentAsset> targetedItems = new List<EquipmentAsset>();
            if (pActor.hasCulture() && pActor.culture.hasPreferredWeaponsToCraft()){
                foreach (EquipmentAsset item in pActor.culture.getPreferredWeaponAssets()){
                    if (pType == EquipmentType.Weapon){
                        if(item.id.Contains("_" + tEquipmentSubtype) || item.group_id.Equals("staff") || item.group_id.Equals("firearm")){
                            targetedItems.Add(item);
                            //Debug.Log(item.id);
                        }
                    }
                    else {
                        if(item.id.Contains("_" + tEquipmentSubtype)){
                            targetedItems.Add(item);
                            //Debug.Log(item.id);
                        }
                    }
                }
            }
            //end of special item selection

            EquipmentAsset tItemAssetToCraft = null;
            ActorEquipmentSlot tActorSlot = pActor.equipment.getSlot(pType);
            Item tCurrentItem = tActorSlot.getItem();
            if (tCurrentItem != null && tCurrentItem.isCursed()){
                __result = false;
                return false;
            }
            int tCurrentItemValue = (tCurrentItem != null) ? tCurrentItem.asset.equipment_value : 0;
            if (targetedItems.Count > 0 && Randy.randomBool()){
                tItemAssetToCraft = ItemCrafting.getItemAssetToCraft(pActor, targetedItems, pCity, tCurrentItemValue, true);
            }
            if (tItemAssetToCraft == null){
                List<EquipmentAsset> tItemsOfType = new List<EquipmentAsset>(AssetManager.items.equipment_by_subtypes[tEquipmentSubtype]);
                //List<EquipmentAsset> tItemsOfType = AssetManager.items.equipment_by_subtypes[tEquipmentSubtype]; //old creation
                if (ConfigsAEA.royalGold == true && !pActor.hasClan()){
                    tItemsOfType.Remove(tItemsOfType.FirstOrDefault(item => item.id.Contains("gold")));
                }
                if ((pCity.getResourcesAmount("common_metals") < 80 && ConfigsAEA.considerAbundanceCM) || (pCity.getAge() < 80 && ConfigsAEA.considerAgeCM)){
                    tItemsOfType.Remove(tItemsOfType.FirstOrDefault(item => item.id.Contains("steel")));
                    if ((pCity.getResourcesAmount("common_metals") < 60 && ConfigsAEA.considerAbundanceCM) || (pCity.getAge() < 60 && ConfigsAEA.considerAgeCM)){
                        tItemsOfType.Remove(tItemsOfType.FirstOrDefault(item => item.id.Contains("iron")));
                        if ((pCity.getResourcesAmount("common_metals") < 40 && ConfigsAEA.considerAbundanceCM) || (pCity.getAge() < 40 && ConfigsAEA.considerAgeCM)){
                            tItemsOfType.Remove(tItemsOfType.FirstOrDefault(item => item.id.Contains("gold")));
                            if ((pCity.getResourcesAmount("common_metals") < 20 && ConfigsAEA.considerAbundanceCM) || (pCity.getAge() < 20 && ConfigsAEA.considerAgeCM)){
                                tItemsOfType.Remove(tItemsOfType.FirstOrDefault(item => item.id.Contains("bronze")));
                            }
                        }
                    }
                }

                tItemAssetToCraft = ItemCrafting.getItemAssetToCraft(pActor, tItemsOfType, pCity, tCurrentItemValue, false);
            }
            if (tItemAssetToCraft == null)
            {
                __result = false;
                return false;
            }
            Item tItem = World.world.items.generateItem(tItemAssetToCraft, pActor.kingdom, pCreatorName, pTries, pActor, 0, false);
            if (tActorSlot.isEmpty())
            {
                tActorSlot.setItem(tItem, pActor);
            }
            else
            {
                Item tOldItem = tActorSlot.getItem();
                tActorSlot.takeAwayItem();
                pCity.tryToPutItem(tOldItem);
                tActorSlot.setItem(tItem, pActor);
            }
            //Fixing the bug that was caused by older versions (destroying item in the wrong slot)
            tActorSlot = pActor.equipment.getSlot(EquipmentType.Weapon);
            if (!tActorSlot.isEmpty() && tActorSlot._item._asset.equipment_type != EquipmentType.Weapon)
            {
                tActorSlot._item.clearUnit();
                tActorSlot._item = null;
                Debug.Log("Removed a non-weapon from a unit's weapon slot (AEA mod)");
            }
            //End of Fix (see EquipmentSlotPatch for how error is addressed before units craft)
            pActor.spendMoney(tItemAssetToCraft.get_total_cost);
            if (tItemAssetToCraft.cost_resource_id_1 != "none")
            {
                pCity.takeResource(tItemAssetToCraft.cost_resource_id_1, tItemAssetToCraft.cost_resource_1);
            }
            if (tItemAssetToCraft.cost_resource_id_2 != "none")
            {
                pCity.takeResource(tItemAssetToCraft.cost_resource_id_2, tItemAssetToCraft.cost_resource_2);
            }
            __result = true;
            return false;
        }
    }

    //Critical Damage Multiplier fix, always got int-ed down to two
    [HarmonyPatch(typeof(MapBox), "applyAttack")]
    public static class CritDamageMultiplierIntFix{
        static bool Prefix(ref AttackDataResult __result, AttackData pData, BaseSimObject pTargetToCheck){
            bool tIsTargetActor = pTargetToCheck.isActor();
            Actor tTargetActor = pTargetToCheck.a;
            ProjectileAsset tProjectileAsset = null;
            if (pData.is_projectile)
            {
                tProjectileAsset = AssetManager.projectiles.get(pData.projectile_id);
            }
            if (tIsTargetActor && ControllableUnit.isControllingUnit(tTargetActor) && tTargetActor.hasMeleeAttack() && tTargetActor.isJustAttacked())
            {
                CombatActionLibrary.combat_action_deflect.action_actor(pTargetToCheck.a, pData, 0f, 0f);
                __result = new AttackDataResult(ApplyAttackState.Deflect, pTargetToCheck.a.data.id);
                return false;
            }
            CombatActionAsset tCombatAsset;
            if (tIsTargetActor && tTargetActor.tryToUseAdvancedCombatAction(tTargetActor.getCombatActionPool(CombatActionPool.BEFORE_HIT_DEFLECT), null, out tCombatAsset))
            {
                tCombatAsset.action_actor(pTargetToCheck.a, pData, 0f, 0f);
                __result = new AttackDataResult(ApplyAttackState.Deflect, pTargetToCheck.a.data.id);
                return false;
            }
            bool tCanBeBlocked = false;
            if (tProjectileAsset != null && tProjectileAsset.can_be_blocked)
            {
                tCanBeBlocked = true;
            }
            if (tCanBeBlocked && tIsTargetActor && tTargetActor.tryToUseAdvancedCombatAction(tTargetActor.getCombatActionPool(CombatActionPool.BEFORE_HIT_BLOCK), null, out tCombatAsset))
            {
                tCombatAsset.action_actor(pTargetToCheck.a, pData, 0f, 0f);
                __result = AttackDataResult.Block;
                return false;
            }
            if (tIsTargetActor && tTargetActor.tryToUseAdvancedCombatAction(tTargetActor.getCombatActionPool(CombatActionPool.BEFORE_HIT), null, out tCombatAsset))
            {
                tCombatAsset.action_actor(tTargetActor, pData, 0f, 0f);
                __result = AttackDataResult.Continue;
                return false;
            }
            float pDamage = Randy.randomFloat(pData.damage_range * (float)pData.damage, (float)pData.damage);
            if (pData.critical)
            {
                //pDamage *= pData.critical_damage_multiplier;
                pDamage *= pData.initiator.stats["critical_damage_multiplier"];
            }
            if (pData.initiator.isActor() && pTargetToCheck.isAlive())
            {
                pData.initiator.a.addExperience(2);
            }
            bool pFlash = true;
            AttackType attack_type = pData.attack_type; 
            BaseSimObject initiator = pData.initiator;
            bool metallic_weapon = pData.metallic_weapon;
            pTargetToCheck.getHit(pDamage, pFlash, attack_type, initiator, pData.skip_shake, metallic_weapon, true);
            if (!pTargetToCheck.hasHealth())
            {
                ActorTool.applyForceToUnit(pData, pTargetToCheck, 1f, false);
            }
            else
            {
                ActorTool.applyForceToUnit(pData, pTargetToCheck, 0.5f, true);
            }
            if (pData.initiator.isActor())
            {
                pData.initiator.a.attackTargetActions(pTargetToCheck, pData.hit_tile);
            }
            if (tIsTargetActor && pData.initiator.isActor() && !pTargetToCheck.hasHealth() && pData.initiator.a.needsFood() && pData.initiator.a.subspecies.diet_meat && tTargetActor.asset.source_meat)
            {
                pData.initiator.a.addNutritionFromEating(70, true, false);
                pData.initiator.a.countConsumed();
            }
            __result = AttackDataResult.Hit;
            return false;
        }
    }

    //Transmute patch for gear items
    [HarmonyPatch(typeof(Item), "transmute")] 
    public static class TransmutePatch{
        static bool Prefix(Item __instance){
            __instance.clearForReforge();
            bool tIsCheatEnabled = __instance.isCheatEnabled();
            using (ListPool<EquipmentAsset> tPossibleAssets = new ListPool<EquipmentAsset>()){
                List<EquipmentAsset> tPotAssets;
                if (__instance._asset.is_pool_weapon){
                    if (tIsCheatEnabled){
                        tPotAssets = AssetManager.items.pot_weapon_assets_all;
                    }
                    else{
                        tPotAssets = AssetManager.items.pot_weapon_assets_unlocked;
                    }
                }
                else if (tIsCheatEnabled){
                    tPotAssets = new List<EquipmentAsset>(AssetManager.items.pot_equipment_by_groups_all[__instance._asset.group_id]);
                    if (__instance._asset.group_id.Equals("helmet")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_all["headband"]);
                    }
                    else if (__instance._asset.group_id.Equals("headband")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_all["helmet"]);
                    }
                    else if (__instance._asset.group_id.Equals("armor")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_all["cloak"]);
                    }
                    else if (__instance._asset.group_id.Equals("cloak")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_all["armor"]);
                    }
                }
                else{
                    tPotAssets = new List<EquipmentAsset>(AssetManager.items.pot_equipment_by_groups_unlocked[__instance._asset.group_id]);
                    if (__instance._asset.group_id.Equals("helmet")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_unlocked["headband"]);
                    }
                    else if (__instance._asset.group_id.Equals("headband")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_unlocked["helmet"]);
                    }
                    else if (__instance._asset.group_id.Equals("armor")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_unlocked["cloak"]);
                    }
                    else if (__instance._asset.group_id.Equals("cloak")){
                        tPotAssets.AddRange(AssetManager.items.pot_equipment_by_groups_unlocked["armor"]);
                    }
                }
                foreach (EquipmentAsset tEquipmentAsset in tPotAssets){
                    if (tEquipmentAsset.isAvailable()){
                        tPossibleAssets.Add(tEquipmentAsset);
                    }
                }
                tPossibleAssets.Shuffle<EquipmentAsset>();
                foreach (EquipmentAsset ptr in tPossibleAssets){
                    EquipmentAsset tAsset = ptr;
                    if (__instance._asset != tAsset){
                        __instance.setAsset(tAsset);
                        break;
                    }
                }
                __instance.reforge(5);
                __instance.calculateValues();
            }
            return false;
        }
    }

    //Fixing the bug that was caused by older versions (avoiding sending the error)
    [HarmonyPatch(typeof(ItemRendering), "getItemMainSpriteFrame")]
    public static class EquipmentSlotPatch{
        static bool Prefix(ref Sprite __result, IHandRenderer pHandRendererAsset){
            if (pHandRendererAsset == null)
            {
                __result = null;
                return false;
            }
            Sprite[] tSpriteList = pHandRendererAsset.getSprites();
            if (tSpriteList == null){
                __result = null;
                Debug.LogError("<color=#FF4B33>Item with no sprite in weapon slot (Possibly an AEA mod bug, make sure the Preload Windows setting is on, then relaunch the game) [1]</color>");
                return false;
            }
            if (tSpriteList.Length > 1)
            {
                __result = AnimationHelper.getSpriteFromList(0, tSpriteList, 5f);
                return false;
            }
            if (tSpriteList.Length != 1){
                Debug.LogError("<color=#FF4B33>Nothing in tSpriteList (Possibly an AEA mod bug)</color>");
                return false;
            }
            if (tSpriteList[0] == null) {
                __result = null;
                Debug.LogError("<color=#FF4B33>Item with no sprite in weapon slot (Possibly an AEA mod bug, make sure the Preload Windows setting is on, then relaunch the game) [2]</color>");
                return false;
            }
            __result = tSpriteList[0];
            return false;
        }
    }

    //Debug Patch
    [HarmonyPatch(typeof(ActorEquipmentSlot), "setItem")] 
    public static class DebugSlotPatch{
        static bool Prefix(ActorEquipmentSlot __instance, Item pItem, Actor pActor){
            if (__instance.type == EquipmentType.Weapon && pItem._asset.equipment_type != EquipmentType.Weapon){
                Debug.LogError("<color=#FF4B33>Game tried to slot a non-weapon into the weapon slot (Most likely an AEA mod bug, please report if you're able) [a]</color>");
                return false;
            }
            else if (__instance.type != EquipmentType.Weapon && pItem._asset.equipment_type == EquipmentType.Weapon){
                Debug.LogError("<color=#FF4B33>Game tried to slot a weapon into a non-weapon slot (Most likely an AEA mod bug, please report if you're able) [b]</color>");
                return false;
            }

            if (!__instance.isEmpty())
            {
                __instance.takeAwayItem();
            }
            __instance._item = pItem;
            __instance._item.setUnitHasIt(pActor);
            pActor.setStatsDirty();
            return false;
        }
    }

    //Log Patch
    [HarmonyPatch(typeof(Debug), "LogError", new Type[] { typeof(object) })] 
    public static class LogPatch{
        static bool Prefix(object message){
            string logMessage = message?.ToString();

            if (logMessage != null && logMessage.Contains("Weapon Texture is Missing")){
                return false;
            }
            return true;
        }
    }

    //Item Tooltip Patches (3)
    [HarmonyPatch(typeof(TooltipLibrary), "showEquipment")] 
    public static class ItemTooltipPatch1{
        static bool Prefix(TooltipLibrary __instance, Tooltip pTooltip, string pType, TooltipData pData){
            Item tItem = pData.item;
            Component component = pTooltip.transform.Find("Description/IconBG/LegendaryBG");
            Image component2 = pTooltip.transform.Find("Description/IconBG/ItemIcon").GetComponent<Image>();
            Text tEquipmentType = pTooltip.transform.Find("Equipment Type/EquipmentText").GetComponent<Text>();
            EquipmentAsset tItemAsset = tItem.getAsset();
            Sprite tSprite = tItem.getSprite();
            component2.sprite = tSprite;
            Text tDescriptionComponent = pTooltip.transform.Find("Item Description/item_description_text").GetComponent<Text>();
            string tColor = tItem.getQualityColor();
            Component component3 = pTooltip.transform.FindRecursive("Stats");
            bool tUnlocked = tItemAsset.isAvailable();
            string tName = tItem.getName(true);
            pTooltip.name.text = Toolbox.coloredText(tName, tColor, false);
            if (!tUnlocked){
                if (tItemAsset.unlocked_with_achievement){
                    string tText = LocalizedTextManager.getText("item_locked_tooltip_text_achievement", null, false);
                    string tAchievementIdTranslated = "<color=#00ffffff>" + tItemAsset.getAchievementLocaleID().Localize() + "</color>";
                    tText = tText.Replace("$achievement_id$", tAchievementIdTranslated);
                    tDescriptionComponent.text = tText;
                }
                else{
                    tDescriptionComponent.text = LocalizedTextManager.getText("item_locked_tooltip_text_exploration", null, false);
                }
            }
            else
            {
                //1/3------modifier changes------
                using (ListPool<TooltipModContainerInfo> tListMods = BaseStatsHelper.getItemModsBase(tItem)){
                    foreach (TooltipModContainerInfo ptr in tListMods){
                        TooltipModContainerInfo tContainer = ptr;
                        string modifier = LocalizedTextManager.getText(tContainer.asset.getLocaleID(), null, false);
                        string modColor = "#45FFFE";
                        if (modifier.Equals("Eternal") || modifier.Equals("Cursed") || modifier.Equals("Divine Rune")){
                            modColor = "#8ffffe";
                        }
                        else if (modifier.Equals("Flame") || modifier.Equals("Ice") || modifier.Equals("Poison") || modifier.Equals("Slowness") 
                        || modifier.Equals("Stun") || modifier.Equals("Measurement") || modifier.Equals("Initiative") || modifier.Equals("Silence")){
                            modColor = "#37cccb";
                        }
                        else if (modifier.Equals("Reaper's Curse")){
                            modColor = "#cc3737";
                        }
                        BaseStatsHelper.addStatValues(pTooltip.stats_description, pTooltip.stats_values, Toolbox.coloredText("+" + modifier, modColor, false), Toolbox.coloredText(tContainer.string_pluses, modColor, false));
                        BaseStatsHelper.addLineBreak(pTooltip.stats_description, pTooltip.stats_values);
                    }
                }
                //2/3------stat changes------
                BaseStatsHelper.calcBaseStatsBase(tItemAsset.base_stats);
                foreach (BaseStatsContainer tBaseStats in BaseStatsHelper._stats_container_positive){
                    if (!tBaseStats.id.Equals("damage_range") && !tBaseStats.id.Equals("targets") && !tBaseStats.id.Equals("projectiles")){
                        BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, tBaseStats, true, true, "#43FF43", false);
                    }
                }
                foreach (BaseStatsContainer tBaseStats2 in BaseStatsHelper._stats_container_negative){
                    BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, tBaseStats2, true, true, "#43FF43", false);
                }
                int damagerangeIndex = BaseStatsHelper._stats_container_positive.FindIndex(s => s.id == "damage_range");
                int targetsIndex = BaseStatsHelper._stats_container_positive.FindIndex(s => s.id == "targets");
                int projectilesIndex = BaseStatsHelper._stats_container_positive.FindIndex(s => s.id == "projectiles");
                if (damagerangeIndex != -1){
                    BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, BaseStatsHelper._stats_container_positive[damagerangeIndex], true, false, "#FCE305", false);
                }
                if (targetsIndex != -1){
                    BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, BaseStatsHelper._stats_container_positive[targetsIndex], true, false, "#FCE305", false);
                }
                if (projectilesIndex != -1){
                    BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, BaseStatsHelper._stats_container_positive[projectilesIndex], true, false, "#FCE305", false);
                }
                //3/3------end of changes------
                pTooltip.addLineBreak();
                pTooltip.addLineText("durability", tItem.getDurabilityString(), null, false, true, 21);
                if (tItem.data.kills > 0){
                    pTooltip.addLineBreak();
                    pTooltip.addItemText("creature_statistics_kills", (float)tItem.data.kills, false, true, false, "#FF9B1C", false);
                }
                string tDescriptionId = tItemAsset.id + "_description";
                bool tLocaleExists = LocalizedTextManager.stringExists(tDescriptionId);
                tDescriptionComponent.transform.parent.gameObject.SetActive(tLocaleExists);
                if (tLocaleExists){
                    tDescriptionComponent.text = LocalizedTextManager.getText(tDescriptionId, null, false);
                }
                component3.gameObject.SetActive(tUnlocked);
                Rarity tItemQuality = tItem.getQuality();
                component.gameObject.SetActive(tItemQuality == Rarity.R3_Legendary || tItemQuality == (Rarity)4 || tItemQuality == (Rarity)5);
                pTooltip.description.alignment = TextAnchor.MiddleLeft;
                tEquipmentType.GetComponent<LocalizedText>().setKeyAndUpdate(tItem.getItemKeyType());
                tEquipmentType.color = Toolbox.makeColor(tColor);
                string tCreationDescription = Toolbox.coloredText(tItem.getItemDescription(), "#FFFFFF", false);
                pTooltip.setDescription(tCreationDescription, null);
                __instance.showTabBannerTip(pTooltip, pData);
                //------description_2 extra info------
                if (ConfigsAEA.showSpecialDescription == true){
                    pTooltip.setBottomDescription($"Cost: money {tItemAsset.cost_gold + tItemAsset.cost_coins_resources}, {tItemAsset.cost_resource_id_1} {tItemAsset.cost_resource_1}, {tItemAsset.cost_resource_id_2} {tItemAsset.cost_resource_2}\nItem Value: {tItem._item_value - 2}", null);
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(TooltipLibrary), "showEquipmentInEditor")] 
    public static class ItemTooltipPatch2{
        static bool Prefix(Tooltip pTooltip, string pType, TooltipData pData){
            EquipmentAsset tItemAsset = pData.item_asset;
            string tHiddenText = LocalizedTextManager.getText("achievement_tip_hidden", null, false);
            if (!tItemAsset.isAvailable())
            {
                pTooltip.name.text = tHiddenText;
                if (tItemAsset.unlocked_with_achievement)
                {
                    string tText = LocalizedTextManager.getText("item_locked_tooltip_text_achievement", null, false);
                    string tAchievementIdTranslated = "<color=#00ffffff>" + tItemAsset.getAchievementLocaleID().Localize() + "</color>";
                    tText = tText.Replace("$achievement_id$", tAchievementIdTranslated);
                    pTooltip.setDescription(tText, null);
                }
                else
                {
                    pTooltip.setDescription(LocalizedTextManager.getText("item_locked_tooltip_text_exploration", null, false), null);
                }
                pTooltip.transform.FindRecursive("Stats").gameObject.SetActive(false);
                return false;
            }
            string tName;
            string tMaterial;
            ItemTools.getTooltipTitle(tItemAsset, out tName, out tMaterial);
            pTooltip.name.text = tMaterial + tName;
            string descriptionID = tItemAsset.getDescriptionID();
            string tDescription = (descriptionID != null) ? descriptionID.Localize() : null;
            if (!string.IsNullOrEmpty(tDescription))
            {
                pTooltip.setDescription(tDescription, null);
            }
            else
            {
                pTooltip.resetDescription();
            }
            if (!string.IsNullOrEmpty(pData.tip_description_2))
            {
                string tLocalizedDescription2 = LocalizedTextManager.getText(pData.tip_description_2, null, false);
                pTooltip.setBottomDescription(tLocalizedDescription2, null);
            }
            //1/3------stat changes------
            BaseStatsHelper.calcBaseStatsBase(tItemAsset.base_stats);
            foreach (BaseStatsContainer tBaseStats in BaseStatsHelper._stats_container_positive){
                if (!tBaseStats.id.Equals("damage_range") && !tBaseStats.id.Equals("targets") && !tBaseStats.id.Equals("projectiles")){
                    BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, tBaseStats, true, true, "#43FF43", false);
                }
            }
            foreach (BaseStatsContainer tBaseStats2 in BaseStatsHelper._stats_container_negative){
                BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, tBaseStats2, true, true, "#43FF43", false);
            }
            int damagerangeIndex = BaseStatsHelper._stats_container_positive.FindIndex(s => s.id == "damage_range");
            int targetsIndex = BaseStatsHelper._stats_container_positive.FindIndex(s => s.id == "targets");
            int projectilesIndex = BaseStatsHelper._stats_container_positive.FindIndex(s => s.id == "projectiles");
            if (damagerangeIndex != -1){
                BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, BaseStatsHelper._stats_container_positive[damagerangeIndex], true, false, "#FCE305", false);
            }
            if (targetsIndex != -1){
                BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, BaseStatsHelper._stats_container_positive[targetsIndex], true, false, "#FCE305", false);
            }
            if (projectilesIndex != -1){
                BaseStatsHelper.showBaseStatLine(pTooltip.stats_description, pTooltip.stats_values, BaseStatsHelper._stats_container_positive[projectilesIndex], true, false, "#FCE305", false);
            }
            //2/3------special tooltiphanges------
            if (ConfigsAEA.showSpecialDescription == true){
                pTooltip.setBottomDescription($"Cost: money {tItemAsset.cost_gold + tItemAsset.cost_coins_resources}, {tItemAsset.cost_resource_id_1} {tItemAsset.cost_resource_1}, {tItemAsset.cost_resource_id_2} {tItemAsset.cost_resource_2}\nItem Value: {tItemAsset.equipment_value}", null);
            }
            //3/3------end of changes------
            return false;
        }
    }
    [HarmonyPatch(typeof(BaseStatsHelper), "showItemModsRows")] 
    public static class ItemTooltipPatch3{
        static bool Prefix(BaseStatsHelper.KeyValueFieldGetter pFieldsFabric, Item pItem){
            using (ListPool<TooltipModContainerInfo> tListMods = BaseStatsHelper.getItemModsBase(pItem)){
                foreach (TooltipModContainerInfo ptr in tListMods){
                    TooltipModContainerInfo tContainer = ptr;
                    string modifier = LocalizedTextManager.getText(tContainer.asset.getLocaleID(), null, false);
                    string modColor = "#45FFFE";
                    if (modifier.Equals("Eternal") || modifier.Equals("Cursed") || modifier.Equals("Divine Rune")){
                        modColor = "#8ffffe";
                    }
                    else if (modifier.Equals("Flame") || modifier.Equals("Ice") || modifier.Equals("Poison") || modifier.Equals("Slowness") || 
                    modifier.Equals("Stun") || modifier.Equals("Measurement") || modifier.Equals("Initiative") || modifier.Equals("Silence")){
                        modColor = "#37cccb";
                    }
                    else if (modifier.Equals("Reaper's Curse")){
                        modColor = "#cc3737";
                    }
                    string tStats = Toolbox.coloredText("+" + modifier, modColor, false);
                    string tValues = Toolbox.coloredText(tContainer.string_pluses, modColor, false);
                    KeyValueField keyValueField = pFieldsFabric(tContainer.asset.getLocaleID());
                    keyValueField.name_text.text = tStats;
                    keyValueField.value.text = tValues;
                }
            } 
            return false;
        }
    }
    [HarmonyPatch(typeof(ActorEquipment), "initDictionary")] 
    public static class ItemActorBarSortPatch{
        static bool Prefix(ActorEquipment __instance){
            __instance._dictionary = new Dictionary<EquipmentType, ActorEquipmentSlot>();
            __instance._dictionary.Add(EquipmentType.Weapon, __instance.weapon);
            __instance._dictionary.Add(EquipmentType.Helmet, __instance.helmet);
            __instance._dictionary.Add(EquipmentType.Armor, __instance.armor);
            __instance._dictionary.Add(EquipmentType.Boots, __instance.boots);
            __instance._dictionary.Add(EquipmentType.Ring, __instance.ring);
            __instance._dictionary.Add(EquipmentType.Amulet, __instance.amulet);
            return false;
        }
    }
    //-------------------------------------------------------------------------
    [HarmonyPatch(typeof(Actor), "tryToAttack")] 
    public static class ItemHybridAttackTypePatch{
        static bool Prefix(Actor __instance, ref bool __result, BaseSimObject pTarget, bool pDoChecks = true, Action pKillAction = null, Vector3 pAttackPosition = default(Vector3), Kingdom pForceKingdom = null, WorldTile pTileTarget = null, float pBonusAreOfEffect = 0f){
            //change1----
            bool isTrispikeInMeleeRange = false;
            if (__instance.hasWeapon() && __instance.equipment.weapon.getItem().getAsset().id.Equals("trispike") && pTarget != null && Toolbox.SquaredDistVec2Float(__instance.current_position, pTarget.current_position) < 3){
                isTrispikeInMeleeRange = true;
            }
            //change1----
            if (pDoChecks){
                if ((__instance.hasMeleeAttack() || isTrispikeInMeleeRange) && pTarget != null && pTarget.position_height > 0f){
                    __result = false;
                    return false;
                }
                if (__instance.isInWaterAndCantAttack()){
                    __result = false;
                    return false;
                }
                if (!__instance.isAttackPossible()){
                    __result = false;
                    return false;
                }
                if (pTarget != null && !__instance.isInAttackRange(pTarget)){
                    __result = false;
                    return false;
                }
            }
            float tZ = 0f;
            float tTargetSize = 0f;
            Vector3 tTargetPos;
            if (pTarget != null){
                tTargetPos = pTarget.current_position;
                tZ = pTarget.getHeight();
                tTargetSize = pTarget.stats["size"];
            }
            else{
                tTargetPos = pAttackPosition;
            }
            bool tPossessed = __instance._has_status_possessed;
            __instance.startAttackCooldown();
            __instance.punchTargetAnimation(tTargetPos, true, __instance.hasRangeAttack(), 40f);
            Vector3 tAttackPosition = new Vector3(tTargetPos.x, tTargetPos.y);
            if (pTarget != null && pTarget.isActor() && pTarget.a.is_moving && pTarget.isFlying()){
                tAttackPosition = Vector3.MoveTowards(tAttackPosition, pTarget.a.next_step_position, tTargetSize * 3f);
            }
            Vector3 tCurrentPos = __instance.current_position;
            float tDist = Vector2.Distance(tCurrentPos, tTargetPos) + tZ;
            Vector3 tAttackVector = Toolbox.getNewPoint(tCurrentPos.x, tCurrentPos.y, tAttackPosition.x, tAttackPosition.y, tDist - tTargetSize, true);
            string tProjectileID = __instance.getWeaponAsset().projectile;
            //change2----
            bool tRangeAttack = __instance.hasRangeAttack();
            if (isTrispikeInMeleeRange == true){
                tRangeAttack = false;
            }
            //change2----
            Kingdom tKingdomForData = pForceKingdom ?? __instance.kingdom;
            WorldTile tHitTileTarget = pTileTarget ?? ((pTarget != null) ? pTarget.current_tile : null);
            Kingdom pKingdom = tKingdomForData;
            Vector3 pInitiatorPosition = tCurrentPos;
            AttackData tAttackData = new AttackData(__instance, tHitTileTarget, tAttackVector, pInitiatorPosition, pTarget, pKingdom, AttackType.Weapon, __instance.haveMetallicWeapon(), true, tRangeAttack, tProjectileID, pKillAction, pBonusAreOfEffect);
            bool result;
            using (ListPool<CombatActionAsset> tRandomPool = new ListPool<CombatActionAsset>()){
                if (__instance.hasSpells() && __instance.canUseSpells() && !tPossessed){
                    __instance.addToAttackPool(CombatActionLibrary.combat_cast_spell, tRandomPool);
                }
                CombatActionAsset tCombatAsset;
                bool tCombatActionDone;
                if (tRandomPool.Count > 0){
                    if (__instance.hasMeleeAttack() || isTrispikeInMeleeRange){
                        __instance.addToAttackPool(CombatActionLibrary.combat_attack_melee, tRandomPool);
                    }
                    else{
                        __instance.addToAttackPool(CombatActionLibrary.combat_attack_range, tRandomPool);
                    }
                    tCombatAsset = tRandomPool.GetRandom<CombatActionAsset>();
                    tCombatActionDone = tCombatAsset.action(tAttackData);
                    if (!tCombatActionDone && !tCombatAsset.basic){
                        if (__instance.hasMeleeAttack() || isTrispikeInMeleeRange){
                            tCombatActionDone = CombatActionLibrary.combat_attack_melee.action(tAttackData);
                        }
                        else{
                            tCombatActionDone = CombatActionLibrary.combat_attack_range.action(tAttackData);
                        }
                    }
                }
                else{
                    //change3----
                    if (__instance.hasMeleeAttack() || isTrispikeInMeleeRange){
                        tCombatAsset = CombatActionLibrary.combat_attack_melee;
                        if (__instance.hasWeapon() && __instance.equipment.weapon.getItem().getAsset().id.Equals("trispike")){
                            //Debug.Log("Melee attack with trispike");
                        }
                    }
                    else{
                        tCombatAsset = CombatActionLibrary.combat_attack_range;
                        if (__instance.hasWeapon() && __instance.equipment.weapon.getItem().getAsset().id.Equals("trispike")){
                            //Debug.Log("Range attack with trispike");
                        }
                    }
                    tCombatActionDone = tCombatAsset.action(tAttackData);
                    //change3----
                }
                if (tCombatActionDone){
                    __instance.spendStamina(tCombatAsset.cost_stamina);
                    __instance.spendMana(tCombatAsset.cost_mana);
                }
                if (tCombatAsset.play_unit_attack_sounds){
                    __instance.makeSoundAttack();
                }
                if (__instance.needsFood() && Randy.randomBool()){
                    __instance.decreaseNutrition(-1);
                }
                float tRecoil = __instance.stats.get("recoil");
                if (tRecoil > 0f){
                    __instance.calculateForce(__instance.current_position.x, __instance.current_position.y, tAttackPosition.x, tAttackPosition.y, tRecoil, 0f, false);
                }
                result = true;
            }
            __result = result;
            return false;
        }
    }
}




