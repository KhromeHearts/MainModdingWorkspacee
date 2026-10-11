using NeoModLoader.services;
using NeoModLoader.ui;

namespace NeoModLoader.Tests;

internal static class ModBrowserTextTests
{
    [Test]
    public static void UploaderBoilerplateIsRemoved()
    {
        // The exact wrapping ModWorkshopService.UploadMod writes around a mod's own description.
        string uploaded = "Cool Mod Uploaded by NeoModLoader\n" +
                          "Cool Mod 由NeoModLoader上传\n\n" +
                          "Adds [b]cool[/b] traits.\n\n" +
                          "ModLoader: https://github.com/WorldBoxOpenMods/ModLoader\n\n" +
                          "模组加载器: https://github.com/WorldBoxOpenMods/ModLoader";

        Assert.Equal("Adds cool traits.", ModBrowserText.Description(new WorkshopModInfo { Description = uploaded }));
        Assert.Equal("Adds cool traits.", ModBrowserText.Description(new WorkshopModInfo
        {
            Description = uploaded.Replace("\n", "\r\n")
        }), "Windows line endings");
        Assert.Equal("Adds cool traits.", ModBrowserText.Description(new WorkshopModInfo
        {
            // Browse queries return a truncated description, which can cut the footer inside its URL.
            Description = uploaded.Substring(0, uploaded.IndexOf("WorldBoxOpenMods", StringComparison.Ordinal))
        }), "truncated footer");
    }

    [Test]
    public static void OtherDescriptionsAreKept()
    {
        Assert.Equal("Just a mod.\nWith two lines.",
            ModBrowserText.Description(new WorkshopModInfo { Description = "Just a mod.\nWith two lines." }));
    }

    [Test]
    public static void BBCodeIsStripped()
    {
        string text = ModBrowserText.StripBBCode(
            "[h1]Features[/h1][list][*]one[*]two[/list][url=https://example.com]link[/url][img]https://x/y.png[/img]");
        Assert.Equal("Features\n\n- one\n- twolink", text);
    }

    [Test]
    public static void TruncateUsesOneLine()
    {
        Assert.Equal("a b", ModBrowserText.Truncate("a\r\nb", 10));
        Assert.Equal("abcdefg...", ModBrowserText.Truncate("abcdefghijklmnop", 10));
        Assert.Equal("", ModBrowserText.Truncate(null, 10));
    }
}
