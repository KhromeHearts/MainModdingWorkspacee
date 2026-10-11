using System;
using System.IO;
using WarBox.Content;
using WarBox.UI;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;

namespace WarBox;

public class WarBox : BasicMod<WarBox>, IReloadable
{
    public static Transform prefab_library;
    internal static Harmony harmony;

    public static bool warbox_factories = true;
    public static bool warbox_army_limits = true;
    public static bool warbox_force_capture = false;
    public static int warbox_army_per_pop = 25;

    [Hotfixable]
    public void Reload()
    {
        var locale_dir = GetLocaleFilesDirectory(GetDeclaration());
        foreach (var file in Directory.GetFiles(locale_dir))
        {
            if (file.EndsWith(".json"))
            {
                LM.LoadLocale(Path.GetFileNameWithoutExtension(file), file);
            }
            else if (file.EndsWith(".csv"))
            {
                LM.LoadLocales(file);
            }
        }
    }

    protected override void OnModLoad()
    {
        prefab_library = new GameObject("PrefabLibrary").transform;
        prefab_library.SetParent(transform);

        WarBoxContent.Init();
        WarBoxUI.Init();     
    }

    public static void Called()
    {
        LogInfo("Hello World From Another!");
    }
}