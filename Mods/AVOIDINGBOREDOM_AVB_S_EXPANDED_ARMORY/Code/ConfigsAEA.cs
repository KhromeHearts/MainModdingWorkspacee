using System;
using NeoModLoader;

namespace avbsexpandedarmory{
    internal static class ConfigsAEA{
        public static bool rebalanceVanillaWeapons { get; set; } = false;
        public static bool changeRaritySystem { get; set; } = false;
        public static bool changeItemSprites { get; set; } = false;
        public static int scciVal { get; set; } = 0;
        public static bool rebalanceSpecialModifiers { get; set; } = false;
        public static bool creatureDefaults { get; set; } = false;
        public static bool royalGold { get; set; } = false;
        public static bool considerAbundanceCM { get; set; } = false;
        public static bool considerAgeCM { get; set; } = false;
        public static bool loverDefaults { get; set; } = false;
        public static bool showSomeHiddenStats { get; set; } = false;
        public static bool showSpecialDescription { get; set; } = false;
        public static bool bronzeIronSteelRecipeFix { get; set; } = false;
        public static bool categoryColor { get; set; } = false;

        public static bool flameTierCraft { get; set; } = false;
        public static bool iceTierCraft { get; set; } = false;
        public static bool goldTierCraft { get; set; } = false;
        public static bool helmetCraft { get; set; } = false;
        public static bool headbandCraft { get; set; } = false;
        public static bool armorCraft { get; set; } = false;
        public static bool cloakCraft { get; set; } = false;
        public static bool bootCraft { get; set; } = false;
        public static bool highcleatCraft { get; set; } = false;
        public static bool amuletCraft { get; set; } = false;
        public static bool braceletCraft { get; set; } = false;
        public static bool ringCraft { get; set; } = false;
        public static bool earringCraft { get; set; } = false;
        public static bool bludgeonCraft { get; set; } = false;
        public static bool daggerCraft { get; set; } = false;
        public static bool halberdCraft { get; set; } = false;
        public static bool crossbowCraft { get; set; } = false;
        public static bool slingshotCraft { get; set; } = false;
        public static bool swordCraft { get; set; } = false;
        public static bool axeCraft { get; set; } = false;
        public static bool hammerCraft { get; set; } = false;
        public static bool spearCraft { get; set; } = false;
        public static bool bowCraft { get; set; } = false;
        public static bool stickCraft { get; set; } = false;

        //Normal Configs
        public static void RebalanceVanillaWeapons(bool pCurrentValue){
            if(pCurrentValue == true){rebalanceVanillaWeapons = true;}
            else{rebalanceVanillaWeapons = false;}
        }
        public static void ChangeRaritySystem(bool pCurrentValue){
            if(pCurrentValue == true){changeRaritySystem = true;}
            else{changeRaritySystem = false;}
        }
        public static void ChangeItemSprites(bool pCurrentValue){
            if(pCurrentValue == true){changeItemSprites = true;}
            else{changeItemSprites = false;}
        }
        public static void EvalSCCIConfig(float pCurrentValue){
            if(pCurrentValue == 1){scciVal = 1;}
            if(pCurrentValue == 0){scciVal = -1;}
        }
        public static void RebalanceSpecialModifiers(bool pCurrentValue){
            if(pCurrentValue == true){rebalanceSpecialModifiers = true;}
            else{rebalanceSpecialModifiers = false;}
        }
        public static void CheckCreatureDefaultWeapons(bool pCurrentValue){
            if(pCurrentValue == true){creatureDefaults = true;}
            else{creatureDefaults = false;}
        }
        public static void CheckRoyalGold(bool pCurrentValue){
            if(pCurrentValue == true){royalGold = true;}
            else{royalGold = false;}
        }
        public static void CheckAbundanceCM(bool pCurrentValue){
            if(pCurrentValue == true){considerAbundanceCM = true;}
            else{considerAbundanceCM = false;}
        }
        public static void CheckAgeCM(bool pCurrentValue){
            if(pCurrentValue == true){considerAgeCM = true;}
            else{considerAgeCM = false;}
        }
        public static void CheckLoverDefaults(bool pCurrentValue){
            if(pCurrentValue == true){loverDefaults = true;}
            else{loverDefaults = false;}
        }
        public static void ShowSomeHiddenStats(bool pCurrentValue){
            if(pCurrentValue == true){showSomeHiddenStats = true;}
            else{showSomeHiddenStats = false;}
        }
        public static void ShowSpecialDescription(bool pCurrentValue){
            if(pCurrentValue == true){showSpecialDescription = true;}
            else{showSpecialDescription = false;}
        }
        public static void FixBronzeIronSteelAmuRingRecipeFix(bool pCurrentValue){
            if(pCurrentValue == true){bronzeIronSteelRecipeFix = true;}
            else{bronzeIronSteelRecipeFix = false;}
        }
        public static void CheckCategoryColor(bool pCurrentValue){
            if(pCurrentValue == true){categoryColor = true;}
            else{categoryColor = false;}
        }

        //Crafting Toggles
        public static void CheckFlameTierCraft(bool pCurrentValue){
            if(pCurrentValue == true){flameTierCraft = true;}
            else{flameTierCraft = false;}
        }
        public static void CheckIceTierCraft(bool pCurrentValue){
            if(pCurrentValue == true){iceTierCraft = true;}
            else{iceTierCraft = false;}
        }
        public static void CheckGoldTierCraft(bool pCurrentValue){
            if(pCurrentValue == true){goldTierCraft = true;}
            else{goldTierCraft = false;}
        }
        public static void CheckHelmetCraft(bool pCurrentValue){
            if(pCurrentValue == true){helmetCraft = true;}
            else{helmetCraft = false;}
        }
        public static void CheckHeadbandCraft(bool pCurrentValue){
            if(pCurrentValue == true){headbandCraft = true;}
            else{headbandCraft = false;}
        }
        public static void CheckArmorCraft(bool pCurrentValue){
            if(pCurrentValue == true){armorCraft = true;}
            else{armorCraft = false;}
        }
        public static void CheckCloakCraft(bool pCurrentValue){
            if(pCurrentValue == true){cloakCraft = true;}
            else{cloakCraft = false;}
        }
        public static void CheckBootCraft(bool pCurrentValue){
            if(pCurrentValue == true){bootCraft = true;}
            else{bootCraft = false;}
        }
        public static void CheckHighcleatCraft(bool pCurrentValue){
            if(pCurrentValue == true){highcleatCraft = true;}
            else{highcleatCraft = false;}
        }
        public static void CheckAmuletCraft(bool pCurrentValue){
            if(pCurrentValue == true){amuletCraft = true;}
            else{amuletCraft = false;}
        }
        public static void CheckBraceletCraft(bool pCurrentValue){
            if(pCurrentValue == true){braceletCraft = true;}
            else{braceletCraft = false;}
        }
        public static void CheckRingCraft(bool pCurrentValue){
            if(pCurrentValue == true){ringCraft = true;}
            else{ringCraft = false;}
        }
        public static void CheckEarringCraft(bool pCurrentValue){
            if(pCurrentValue == true){earringCraft = true;}
            else{earringCraft = false;}
        }
        public static void CheckBludgeonCraft(bool pCurrentValue){
            if(pCurrentValue == true){bludgeonCraft = true;}
            else{bludgeonCraft = false;}
        }
        public static void CheckDaggerCraft(bool pCurrentValue){
            if(pCurrentValue == true){daggerCraft = true;}
            else{daggerCraft = false;}
        }
        public static void CheckHalberdCraft(bool pCurrentValue){
            if(pCurrentValue == true){halberdCraft = true;}
            else{halberdCraft = false;}
        }
        public static void CheckCrossbowCraft(bool pCurrentValue){
            if(pCurrentValue == true){crossbowCraft = true;}
            else{crossbowCraft = false;}
        }
        public static void CheckSlingshotCraft(bool pCurrentValue){
            if(pCurrentValue == true){slingshotCraft = true;}
            else{slingshotCraft = false;}
        }
        public static void CheckSwordCraft(bool pCurrentValue){
            if(pCurrentValue == true){swordCraft = true;}
            else{swordCraft = false;}
        }
        public static void CheckAxeCraft(bool pCurrentValue){
            if(pCurrentValue == true){axeCraft = true;}
            else{axeCraft = false;}
        }
        public static void CheckHammerCraft(bool pCurrentValue){
            if(pCurrentValue == true){hammerCraft = true;}
            else{hammerCraft = false;}
        }
        public static void CheckSpearCraft(bool pCurrentValue){
            if(pCurrentValue == true){spearCraft = true;}
            else{spearCraft = false;}
        }
        public static void CheckBowCraft(bool pCurrentValue){
            if(pCurrentValue == true){bowCraft = true;}
            else{bowCraft = false;}
        }
        public static void CheckStickCraft(bool pCurrentValue){
            if(pCurrentValue == true){stickCraft = true;}
            else{stickCraft = false;}
        }
    }
}




