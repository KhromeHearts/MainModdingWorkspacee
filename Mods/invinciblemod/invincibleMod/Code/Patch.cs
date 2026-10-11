using HarmonyLib;

namespace invincibleMod
{
    // Указываем, что патчим метод "getHit" класса Actor
    [HarmonyPatch(typeof(Actor), "getHit")]
    public class InvincibleTraitPatch
    {
        // Prefix вызывается перед оригинальной функцией getHit()
        // (__instance — это сам актёр, pDamage — урон, который он должен получить)
        static bool Prefix(Actor __instance, float pDamage)
        {
            if (__instance.hasTrait("invincibleT"))
            {
                return false;   // пропускаем ориг. метод (урон не наносится)
            }

            return true;    //если черты нет, то ориг. метод выполняется
        }
    }
}