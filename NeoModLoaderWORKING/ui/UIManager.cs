using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.ui;

internal static class UIManager
{
    public static void init()
    {
        InformationWindow.CreateWindow("Information", "Information Title");
        ModListWindow.CreateAndInit("NeoModList");
        NewModListWindow.CreateAndInit("NMLMenu");
        ModUploadWindow.CreateAndInit("ModUpload");
        ModUploadingProgressWindow.CreateAndInit("ModUploadingProgress");
        ModUploadAuthenticationWindow.CreateAndInit("ModUploadAuthentication");
        ModConfigureWindow.CreateAndInit("ModConfigure");
        PowerButtonCreator.AddButtonToTab(
            PowerButtonCreator.CreateWindowButton("NML_ModsList", "NeoModList",
                                                  InternalResourcesGetter.GetIcon()),
            PowerButtonCreator.GetTab(PowerTabNames.Main),
          22);
        
        PowerButtonCreator.AddButtonToTab(
            PowerButtonCreator.CreateWindowButton("NewNML_ModsList", "NMLMenu",
                                                  InternalResourcesGetter.GetIcon()),
            PowerButtonCreator.GetTab(PowerTabNames.Main),
            23);

        PowerButtonCreator.AddButtonToTab(
            PowerButtonCreator.CreateSimpleButton("NML_ModBrowser", NewModListWindow.OpenBrowser,
                                                  SpriteTextureLoader.getSprite("ui/icons/iconSteam")),
            PowerButtonCreator.GetTab(PowerTabNames.Main),
            24);
    }
}