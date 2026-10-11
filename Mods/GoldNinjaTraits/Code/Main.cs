using UnityEngine;
using NeoModLoader.api;
using NeoModLoader.services;
using NeoModLoader.General;
using System.IO;
using HarmonyLib;

namespace GoldNinjaTraits;

public class ModClass : MonoBehaviour, IMod
{
    private ModDeclare _declare;
    private GameObject _gameObject;
    private Harmony _harmony;

    // REQUIRED by IMod
    public ModDeclare GetDeclaration() => _declare;

    // REQUIRED by IMod
    public GameObject GetGameObject() => _gameObject;

    // REQUIRED by IMod (can be empty)
    public string GetUrl() => "";

    public void OnLoad(ModDeclare modDecl, GameObject gameObject)
    {
        _declare = modDecl;
        _gameObject = gameObject;

        LogService.LogInfo("[GoldNinjaTraits] Loaded");

        // Register trait
        Traits.Init();

        // Apply Harmony patch
        _harmony = new Harmony("GoldNinjaTraits.Patch");
        _harmony.PatchAll();
    }
}
