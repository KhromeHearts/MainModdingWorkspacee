using NeoModLoader.api;
using UnityEngine;

namespace invincibleMod;

public static class Traits
{
    public static void Init()
    {
        // Создаём черту
        ActorTrait MTinvincible = new ActorTrait() {
            id = "invincibleT",   // ID черты (ключ для локализации)
            group_id = "fate",   // к какой группе относится
            path_icon = "ui/Icons/iconInvincible", //Путь к иконке черты
            rarity = Rarity.R2_Epic,
            needs_to_be_explored = false // не нужно искать черту, чтобы ее открыть
        };
        // Добавляем черту в игру
        AssetManager.traits.add(MTinvincible);
    }
}