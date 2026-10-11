using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.api;
using NeoModLoader.api.attributes;

namespace aaaMod{
    class Main : BasicMod<Main>{
        public static Main instance;
        static Harmony _harmony;
        protected override void OnModLoad(){
            if (Configs.UnevoColor == true){
                CreaturesPowersUnevo.init();
            }
            if (Configs.EvoColor == true){
                CreaturesPowersEvo.init();
            }
            if (Configs.PhenoSort == true){
                PhenotypePriorities.init();
            }
            OtherAssetEdits.init();
            Actors.init();
            _harmony = new Harmony("AvbCCC");
            _harmony.PatchAll();
        }
    }

    //Button Patches---------------------------------------------------------------------------------------------------------
    public class CheckIcons{
        public static void checkOtherPowerButtons()
        {
            foreach (PowerButton power in PowerButton.power_buttons)
            {
                
                PowerButton tButton = power;
                if (tButton == null){
                    break;
                }
                if (tButton.godPower.path_icon != null)
                {
                    tButton.icon.sprite = SpriteTextureLoader.getSprite($"ui/Icons/" + tButton.godPower.path_icon);
                }
                if (tButton.godPower.id.Equals("spite"))
                {
                    tButton.icon.sprite = SpriteTextureLoader.getSprite($"ui/Icons/iconSpite2");
                }
                if (tButton.godPower.type == PowerActionType.PowerSpawnBuilding){   
                    tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/buildingbutton");
                }
                if (tButton.godPower.id.Contains("printer")){   
                    tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/terraformerbuttonEmpty");
                }
            }
        }	
    }
    [HarmonyPatch(typeof(PowerButton), "checkActorSpawnButtons")]
	public class buttonIconPatchActors{
		static bool Prefix()
    	{
            Color tColorNoPopulation = new Color(0.75f, 0.75f, 0.75f, 0.9f);
            foreach (KeyValuePair<ActorAsset, PowerButton> tPair in PowerButton.actor_spawn_buttons)
            {
                if (tPair.Key == null || tPair.Value == null){
                    break;
                }
                ActorAsset tAsset = tPair.Key;
                PowerButton tButton = tPair.Value;
                if (tAsset.isAvailable())
                {
                    if (tButton.godPower.path_icon != null)
                    {
                        tButton.icon.sprite = SpriteTextureLoader.getSprite($"ui/Icons/" + tButton.godPower.path_icon);
                    }
                    if (tAsset.countPopulation() > 0 || tButton.godPower.id == "crabzilla")
                    {
                        if (tButton.godPower.id.Contains("civ") || tButton.godPower.id == "bandit"){   
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/evolvedbuttonExist");
                        }
                        else if (tButton.godPower.id == "human" || tButton.godPower.id == "elf" || tButton.godPower.id == "orc" || tButton.godPower.id == "dwarf"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/goldbuttonExist");
                        }
                        else if (tButton.godPower.id == "evil_mage" || tButton.godPower.id == "white_mage" || tButton.godPower.id == "druid" || tButton.godPower.id == "necromancer" || tButton.godPower.id == "plague_doctor"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/magebuttonExist");
                        }
                        else if (tButton.godPower.id.Contains("ant_") || tButton.godPower.id == "god_finger" || tButton.godPower.id == "worm" || tButton.godPower.id == "sand_spider"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/terraformerbuttonExist");
                        }
                        else if (tButton.godPower.id == "greg"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/gregbuttonExist");
                        }
                        else if (tButton.godPower.id == "crabzilla"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/crabzillabuttonBoth");
                        }
                        else {
                            tButton._image.sprite = ToolbarButtons.getSpriteButtonUnitExists();
                        }
                        tButton.icon.color = Toolbox.color_white;
                    }
                    else
                    {
                        if (tButton.godPower.id.Contains("civ") || tButton.godPower.id == "bandit"){   
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/evolvedbuttonEmpty");
                        }
                        else if (tButton.godPower.id == "human" || tButton.godPower.id == "elf" || tButton.godPower.id == "orc" || tButton.godPower.id == "dwarf"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/goldbuttonEmpty");
                        }
                        else if (tButton.godPower.id == "evil_mage" || tButton.godPower.id == "white_mage" || tButton.godPower.id == "druid" || tButton.godPower.id == "necromancer" || tButton.godPower.id == "plague_doctor"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/magebuttonEmpty");
                        }
                        else if (tButton.godPower.id.Contains("ant_") || tButton.godPower.id == "god_finger" || tButton.godPower.id == "worm" || tButton.godPower.id == "sand_spider"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/terraformerbuttonEmpty");
                        }
                        else if (tButton.godPower.id == "greg"){
                            tButton._image.sprite = SpriteTextureLoader.getSprite($"ui/Icons/gregbuttonEmpty");
                        }
                        else {
                            tButton._image.sprite = ToolbarButtons.getSpriteButtonNormal();
                        }
                        tButton.icon.color = tColorNoPopulation;
                    }
                }
                else
                {
                    tButton.icon.color = Toolbox.color_black;
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(ToolbarButtons), "Update")]
	public class buttonIconPatchMain{
		static bool Prefix()
    	{
            if (Time.frameCount % 30 == 0)
            {
                CheckIcons.checkOtherPowerButtons();
                PowerButton.checkActorSpawnButtons();
            }
            return false;
        }
    }

    //Phenotype Nameplates---------------------------------------------------------------------------------------------------------
    public class SpecialPhenotypeSprite{
        public static bool ColorsAreClose(Color a, Color b, float threshold = 0.05f)
        {
            return Mathf.Abs(a.r - b.r) < threshold && Mathf.Abs(a.g - b.g) < threshold && Mathf.Abs(a.b - b.b) < threshold;
        }
        public static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color col)){
                return col;
            }
            return Color.white; 
        }
        public static Sprite constructPhenoModifiedIcon(Actor pActor, int phenoIndex = 1){
            if(pActor != null){
                string fixForLogic = pActor.getActorAsset().icon.Replace("New", "");

                Sprite baseActorSprite = Resources.Load<Sprite>("ui/Icons/" + fixForLogic + "_PhenoA"); //normal sprites already have /civs
                if (baseActorSprite == null && (Configs.UnevoColor == true || Configs.EvoColor == true)){
                    baseActorSprite = Resources.Load<Sprite>("ui/Icons/civs/" + fixForLogic + "_PhenoA"); //moved the new sprites to civs folder
                }
                if (baseActorSprite == null){
                    //Debug.LogError("GameResources/ui/Icons/" + pActor.getActorAsset().icon + " PhenoA is NULL!");
                    return null;
                }

                Color[] targetGreens = new Color[] {HexToColor("b8ff96"), HexToColor("00ff00"), HexToColor("00af00"), HexToColor("4a831f")};
                Color[] newColors = new Color[] {HexToColor("220000"), HexToColor("660000"), HexToColor("BB0000"), HexToColor("FF0000")};

                PhenotypeAsset selectedPhenotype = AssetManager.phenotype_library.list[phenoIndex - 1];
                newColors[0] = selectedPhenotype.colors[0];
                newColors[1] = selectedPhenotype.colors[1];
                newColors[2] = selectedPhenotype.colors[2];
                newColors[3] = selectedPhenotype.colors[3];

                Texture2D sourceTex = baseActorSprite.texture;

                Texture2D newTex = new Texture2D(sourceTex.width, sourceTex.height);
                newTex.filterMode = FilterMode.Point;
                newTex.wrapMode = TextureWrapMode.Clamp;

                Color[] pixels = sourceTex.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    for (int j = 0; j < targetGreens.Length; j++)
                    {
                        if (ColorsAreClose(pixels[i], targetGreens[j]))
                        {
                            pixels[i] = newColors[j];
                            break;
                        }
                    }
                }

                newTex.SetPixels(pixels);
                newTex.Apply();

                return Sprite.Create(newTex, baseActorSprite.rect, new Vector2(0.5f, 0.5f), baseActorSprite.pixelsPerUnit);
            }
            else {
                return null;
            }
        }

    }
    [HarmonyPatch(typeof(NameplateText), "showTextKingdom")]
    public static class NameplateTextPTKingdom{
        static void Postfix(NameplateText __instance, Kingdom pMetaObject, Vector2 pPosition){
            if (pMetaObject == null || pMetaObject.king == null || pMetaObject.king.subspecies == null) return;
            
            Sprite coloredKingdomIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(
                pMetaObject.king,
                pMetaObject.king.subspecies._cached_phenotype_index_for_banner
            );
            if (coloredKingdomIcon != null && Configs.PhenoNameplates == true){
                __instance.showSpecies(coloredKingdomIcon);
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextArmy")]
    public static class NameplateTextPTArmy{
        static void Postfix(NameplateText __instance, Army pMetaObject, Vector3 pPosition){
            if (pMetaObject.hasCaptain()){
                Sprite coloredArmyIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(pMetaObject._captain, pMetaObject._captain.subspecies._cached_phenotype_index_for_banner);
                if (coloredArmyIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredArmyIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextCity")]
    public static class NameplateTextPTCity{
        static void Postfix(NameplateText __instance, City pMetaObject, Vector2 pPosition){
            if (pMetaObject.getMainSubspecies() != null && pMetaObject.hasLeader()){
                Sprite coloredCityIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(pMetaObject.leader, pMetaObject.leader.subspecies._cached_phenotype_index_for_banner);
                if (coloredCityIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredCityIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextClanCity")]
    public static class NameplateTextPTClanCity{
        static void Postfix(NameplateText __instance, Clan pMetaObject, City pCity){
            if (pMetaObject.hasChief()){
                Actor selectedActor = World.world.units.get(pMetaObject.data.chief_id);
                Sprite coloredClanCityIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(selectedActor, selectedActor.subspecies._cached_phenotype_index_for_banner);
                if (coloredClanCityIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredClanCityIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextClanFluid")]
    public static class NameplateTextPTClanFluid{
        static void Postfix(NameplateText __instance, Clan pMetaObject, Vector3 pPosition){
            if (pMetaObject.hasChief()){
                Actor selectedActor = World.world.units.get(pMetaObject.data.chief_id);
                Sprite coloredClanFluidIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(selectedActor, selectedActor.subspecies._cached_phenotype_index_for_banner);
                if (coloredClanFluidIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredClanFluidIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextCulture")]
    public static class NameplateTextPTCulture{
        static void Postfix(NameplateText __instance, Culture pMetaObject, Vector3 pPosition){
            Actor selectedActor = null;
            Subspecies selectedSubspecies = null;
            if (pMetaObject.units.Count != 0)
            {   
                selectedActor = pMetaObject.units[0];
                selectedSubspecies = pMetaObject.units[0].subspecies;
            }
            if (selectedActor != null){
                Sprite coloredCultureIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(selectedActor, selectedSubspecies._cached_phenotype_index_for_banner);
                if (coloredCultureIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredCultureIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextLanguage")]
    public static class NameplateTextPTLanguage{
        static void Postfix(NameplateText __instance, Language pMetaObject, Vector3 pPosition){
            Actor selectedActor = null;
            Subspecies selectedSubspecies = null;
            if (pMetaObject.units.Count != 0)
            {   
                selectedActor = pMetaObject.units[0];
                selectedSubspecies = pMetaObject.units[0].subspecies;
            }
            if (selectedActor != null){
                Sprite coloredLanguageIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(selectedActor, selectedSubspecies._cached_phenotype_index_for_banner);
                if (coloredLanguageIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredLanguageIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextReligion")]
    public static class NameplateTextPTReligion{
        static void Postfix(NameplateText __instance, Religion pMetaObject, Vector3 pPosition){
            Actor selectedActor = null;
            Subspecies selectedSubspecies = null;
            if (pMetaObject.units.Count != 0)
            {   
                selectedActor = pMetaObject.units[0];
                selectedSubspecies = pMetaObject.units[0].subspecies;
            }
            if (selectedActor != null){
                Sprite coloredReligionIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(selectedActor, selectedSubspecies._cached_phenotype_index_for_banner);
                if (coloredReligionIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredReligionIcon);
                }
            }
        }
    }
    [HarmonyPatch(typeof(NameplateText), "showTextSubspecies")]
    public static class NameplateTextPTSubspecies{
        static void Postfix(NameplateText __instance, Subspecies pMetaObject, Vector3 pPosition){
            Actor selectedActor = null;
            if (pMetaObject.units.Count != 0)
            {   
                selectedActor = pMetaObject.units[0];
            }
            if (selectedActor != null){
                Sprite coloredSubspeciesIcon = SpecialPhenotypeSprite.constructPhenoModifiedIcon(selectedActor, __instance._banner_subspecies.meta_object._cached_phenotype_index_for_banner);
                if (coloredSubspeciesIcon != null && Configs.PhenoNameplates == true){
                    __instance.showSpecies(coloredSubspeciesIcon);
                }
            }
        }
    }

    //Item Sprite Loading---------------------------------------------------------------------------------------------------------
    //---Credit for code help: This page uses minorly modified LoadSpritesPatch patch from the Modernbox mod
    //---This mod received no direct assistance from contributors of the Modernbox mod and was made by referencing its code
    [HarmonyPatch(typeof(ItemLibrary), "loadSprites")]
    public static class LoadSpritesPatch1{
        static void Postfix(object __instance){
            WeaponsProjectilesEffects.init();
            Type RecievePreload = typeof(DynamicSprites).Assembly.GetType("HandRendererTexturePreloader");
            MethodInfo ExtendedPreload = RecievePreload.GetMethod("preloadItemsIntoAtlas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            ExtendedPreload.Invoke(null, null);
        }
    }
}




