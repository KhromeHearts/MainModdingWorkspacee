using UnityEngine;
using NeoModLoader.api;    // Основные классы NML (IMod, ModDeclare и т.д)
using NeoModLoader.services;    // Сервисы NML, например LogService
using NeoModLoader.General;    // Локализация
using System.IO;    // Работа с файлами и путями
using HarmonyLib;   //патчинг методов

namespace invincibleMod;

public class ModClass : MonoBehaviour, IMod, ILocalizable
{
    private ModDeclare _declare;    // Сохраняет данные мода (имя, версия, путь)
    private GameObject _gameObject;    // Объект Unity, к которому привязан мод
    private Harmony _harmony;   //для патчинга метод игры

    public ModDeclare GetDeclaration() => _declare;
    public GameObject GetGameObject() => _gameObject;
    public string GetUrl() => "";

    string ILocalizable.GetLocaleFilesDirectory(ModDeclare mod)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "Mods", mod.Name, "Locales");
    }

    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        _declare = pModDecl;
        _gameObject = pGameObject;

        LogService.LogInfo($"[{pModDecl.Name}]: Мод загружен \nПОН");

        Traits.Init();

        // Создаём объект Harmony и применяем все патчи
        _harmony = new Harmony("invincibleMod.Patch");
        _harmony.PatchAll();
    }
}