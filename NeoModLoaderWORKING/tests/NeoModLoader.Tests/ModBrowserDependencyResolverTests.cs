using NeoModLoader.services;

namespace NeoModLoader.Tests;

internal static class ModBrowserDependencyResolverTests
{
    private static WorkshopModInfo Item(ulong pId, string pTitle, bool pVerified = true, ulong pSubscribers = 0)
    {
        return new WorkshopModInfo
        {
            FileId = pId,
            Title = pTitle,
            Subscriptions = pSubscribers,
            Tags = new[] { pVerified ? "mod" : "unverified mod" }
        };
    }

    [Test]
    public static void SearchTermsDropLeadingWords()
    {
        Assert.SequenceEqual(new[] { "AUTHOR COOL MOD", "COOL MOD", "MOD" },
            ModBrowserDependencyResolver.BuildSearchTerms("AUTHOR_COOL_MOD"));
        Assert.SequenceEqual(new[] { "INMNY X" },
            ModBrowserDependencyResolver.BuildSearchTerms("INMNY_X"), "single-letter remainders are skipped");
        Assert.Equal(0, ModBrowserDependencyResolver.BuildSearchTerms("").Count);
    }

    [Test]
    public static void TitlesMatchTheNamePartOfTheUid()
    {
        Assert.True(ModBrowserDependencyResolver.TitleMatchesUid("Cool Mod", "AUTHOR_COOL_MOD"));
        Assert.True(ModBrowserDependencyResolver.TitleMatchesUid("cool-mod", "AUTHOR_COOL_MOD"));
        Assert.True(ModBrowserDependencyResolver.TitleMatchesUid("Cool Mod!", "AUTHOR_COOL_MOD_"),
            "trailing punctuation becomes a trailing underscore on both sides");
        Assert.True(ModBrowserDependencyResolver.TitleMatchesUid("MY GUID", "MY_GUID"), "explicit GUID equal to title");
        Assert.False(ModBrowserDependencyResolver.TitleMatchesUid("CoolMod", "AUTHOR_COOL_MOD"));
        Assert.False(ModBrowserDependencyResolver.TitleMatchesUid("Mod Pack", "AUTHOR_COOL_MOD"));
        Assert.False(ModBrowserDependencyResolver.TitleMatchesUid("", "AUTHOR_COOL_MOD"));
    }

    [Test]
    public static void CandidatesPreferSpecificVerifiedPopularMatches()
    {
        var items = new[]
        {
            Item(1, "Mod", pSubscribers: 10_000),
            Item(2, "Cool Mod", pVerified: false, pSubscribers: 900),
            Item(3, "Cool Mod", pSubscribers: 100),
            Item(4, "Cool Mod", pSubscribers: 500),
            Item(5, "Unrelated", pSubscribers: 99_999),
            new WorkshopModInfo { FileId = 3080294469, Title = "Author Cool Mod", Tags = new[] { "mod loader" } }
        };

        WorkshopModInfo pick = ModBrowserDependencyResolver.PickCandidate("AUTHOR_COOL_MOD", items);

        Assert.Equal(4UL, pick.FileId, "longest title match, verified, then most subscribed");
        Assert.Null(ModBrowserDependencyResolver.PickCandidate("NOBODY_NOTHING", items));
    }
}
