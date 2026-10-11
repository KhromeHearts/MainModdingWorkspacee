using NeoModLoader.services;

namespace NeoModLoader.Tests;

internal static class ModBrowserSettingsTests
{
    private static string TempFile()
    {
        return Path.Combine(Path.GetTempPath(), $"nml_browser_settings_{Guid.NewGuid():N}.json");
    }

    [Test]
    public static void MissingFileGivesDefaults()
    {
        ModBrowserSettings settings = ModBrowserSettings.Load(TempFile());

        Assert.Equal(WorkshopBrowseSort.MostSubscribed, settings.Sort);
        Assert.True(settings.IncludeVerified && settings.IncludeUnverified);
        Assert.False(settings.WarningAccepted);
    }

    [Test]
    public static void RoundTrip()
    {
        string path = TempFile();
        try
        {
            var settings = new ModBrowserSettings { WarningAccepted = true };
            settings.SetQuery(new WorkshopBrowseQuery
            {
                Scope = WorkshopBrowseScope.Subscribed,
                Sort = WorkshopBrowseSort.RecentlyUpdated,
                SearchText = "traits",
                IncludeVerified = false,
                IncludeUnverified = true
            });
            settings.Save(path);

            Assert.Contains("\"RecentlyUpdated\"", File.ReadAllText(path));
            WorkshopBrowseQuery query = ModBrowserSettings.Load(path).ToQuery();
            Assert.Equal(WorkshopBrowseScope.Subscribed, query.Scope);
            Assert.Equal(WorkshopBrowseSort.RecentlyUpdated, query.Sort);
            Assert.Equal("", query.SearchText, "search text lasts one session only");
            Assert.False(File.ReadAllText(path).Contains("traits"));
            Assert.False(query.IncludeVerified);
            Assert.True(query.IncludeUnverified);
            Assert.True(ModBrowserSettings.Load(path).WarningAccepted);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public static void SearchTextFromOlderFilesIsIgnored()
    {
        string path = TempFile();
        try
        {
            File.WriteAllText(path, "{ \"sort\": \"Relevance\", \"search_text\": \"modernbox\" }");
            WorkshopBrowseQuery query = ModBrowserSettings.Load(path).ToQuery();
            Assert.Equal("", query.SearchText);
            Assert.Equal(WorkshopBrowseSort.MostSubscribed, query.Sort, "Relevance needs search text");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public static void InvalidFilesFallBackToDefaults()
    {
        string path = TempFile();
        try
        {
            File.WriteAllText(path, "{ \"sort\": \"NoSuchSort\" ");
            Assert.Equal(WorkshopBrowseSort.MostSubscribed, ModBrowserSettings.Load(path).Sort);

            File.WriteAllText(path, "{ \"include_verified\": false, \"include_unverified\": false }");
            ModBrowserSettings settings = ModBrowserSettings.Load(path);
            Assert.True(settings.IncludeVerified && settings.IncludeUnverified, "an empty tag filter is reset");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
