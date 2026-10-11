using System;
using NeoModLoader.api.attributes;

namespace WarBox.Content;

internal static class WarBoxContent
{
    public static void Init()
    {
        WarBoxMisc.Init();
        WarBoxBuildings.Init();
        WarBoxEWE.Init();
        WarBoxTraits.Init();
        WarBoxCulture.Init();
        WarBoxGodPowers.Init();
        WarBoxActors.Init();
        WarBoxPatches.Init();
    }
}