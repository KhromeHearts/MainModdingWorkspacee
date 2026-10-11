using System.Net;
using System.Net.Http;
using NeoModLoader.constants;
using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.utils;

/// <summary>
///     Loads Workshop preview images as sprites, with a disk cache under the NML folder and an in-memory cache.
/// </summary>
internal static class WorkshopThumbnailLoader
{
    private const int max_concurrent_downloads = 4;
    private const int memory_cache_limit = 512;
    private const long max_image_bytes = 8 * 1024 * 1024;

    // Steam's image CDN resizes on request (verified: a 128x128 JPEG of a few KB instead of the original).
    private const string resize_query = "imw=128&imh=128&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false";

    private static readonly Dictionary<ulong, CachedSprite> memory_cache = new();
    private static readonly Dictionary<ulong, List<Action<ulong, Sprite>>> waiting = new();
    private static readonly HashSet<ulong> reported_failures = new();
    private static readonly SemaphoreSlim download_slots = new(max_concurrent_downloads);
    private static HttpClient http_client;
    private static long use_counter;

    private static string CacheFolder => Path.Combine(Paths.NMLPath, "workshop_thumbnails");

    /// <summary>
    ///     Shown while loading and when an image cannot be loaded.
    /// </summary>
    public static Sprite Placeholder => InternalResourcesGetter.GetIcon();

    /// <summary>
    ///     Get the preview of an item. <paramref name="pOnLoaded" /> runs on the main thread with (file id, sprite):
    ///     immediately when cached, otherwise once with the placeholder and again when loading finishes.
    /// </summary>
    public static void Request(ulong pFileId, string pUrl, Action<ulong, Sprite> pOnLoaded)
    {
        if (string.IsNullOrEmpty(pUrl))
        {
            pOnLoaded(pFileId, Placeholder);
            return;
        }

        string key = UrlKey(pUrl);
        if (memory_cache.TryGetValue(pFileId, out CachedSprite cached) && cached.key == key && cached.sprite != null)
        {
            cached.last_use = ++use_counter;
            pOnLoaded(pFileId, cached.sprite);
            return;
        }

        pOnLoaded(pFileId, Placeholder);
        if (waiting.TryGetValue(pFileId, out List<Action<ulong, Sprite>> callbacks))
        {
            callbacks.Add(pOnLoaded);
            return;
        }

        waiting[pFileId] = new List<Action<ulong, Sprite>> { pOnLoaded };
        _ = LoadAsync(pFileId, pUrl, key);
    }

    private static async Task LoadAsync(ulong pFileId, string pUrl, string pKey)
    {
        Sprite sprite = null;
        try
        {
            string cache_path = Path.Combine(CacheFolder, $"{pFileId}_{pKey}.img");
            byte[] cached_bytes = await Task.Run(() => File.Exists(cache_path) ? File.ReadAllBytes(cache_path) : null)
                .ConfigureAwait(false);
            if (cached_bytes != null)
            {
                sprite = await MainThreadDispatcher.RunAsync(() => CreateSprite(cached_bytes)).ConfigureAwait(false);
            }

            if (sprite == null)
            {
                string separator = pUrl.Contains("?") ? "&" : "?";
                foreach (string url in new[] { pUrl + separator + resize_query, pUrl })
                {
                    byte[] bytes = await DownloadAsync(url).ConfigureAwait(false);
                    if (bytes == null) continue;

                    sprite = await MainThreadDispatcher.RunAsync(() => CreateSprite(bytes)).ConfigureAwait(false);
                    if (sprite == null) continue;

                    await Task.Run(() => WriteCache(cache_path, bytes)).ConfigureAwait(false);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Failed to load the preview of Workshop item {pFileId}: {e.Message}");
        }

        MainThreadDispatcher.Run(() => Deliver(pFileId, pKey, sprite));
    }

    private static void Deliver(ulong pFileId, string pKey, Sprite pSprite)
    {
        if (pSprite != null)
        {
            Store(pFileId, pKey, pSprite);
        }
        else if (reported_failures.Add(pFileId))
        {
            LogService.LogWarning($"No usable preview image for Workshop item {pFileId}, showing the placeholder");
        }

        if (!waiting.TryGetValue(pFileId, out List<Action<ulong, Sprite>> callbacks)) return;
        waiting.Remove(pFileId);
        foreach (Action<ulong, Sprite> callback in callbacks)
        {
            try
            {
                callback(pFileId, pSprite ?? Placeholder);
            }
            catch (Exception e)
            {
                LogService.LogError($"Thumbnail callback failed: {e.Message}");
                LogService.LogException(e);
            }
        }
    }

    private static async Task<byte[]> DownloadAsync(string pUrl)
    {
        await download_slots.WaitAsync().ConfigureAwait(false);
        try
        {
            using HttpResponseMessage response = await GetClient().GetAsync(pUrl).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                LogService.LogWarningConcurrent($"Preview download returned {(int)response.StatusCode}: {pUrl}");
                return null;
            }

            if (response.Content.Headers.ContentLength > max_image_bytes) return null;
            byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return bytes.Length > max_image_bytes ? null : bytes;
        }
        catch (Exception e)
        {
            LogService.LogWarningConcurrent($"Preview download failed ({e.GetBaseException().Message}): {pUrl}");
            return null;
        }
        finally
        {
            download_slots.Release();
        }
    }

    private static HttpClient GetClient()
    {
        if (http_client != null) return http_client;
        // Same TLS 1.2 opt-in as HttpUtils.Request.
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        http_client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        return http_client;
    }

    private static Sprite CreateSprite(byte[] pBytes)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(pBytes))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static void WriteCache(string pPath, byte[] pBytes)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pPath)!);
            File.WriteAllBytes(pPath, pBytes);
        }
        catch (Exception e)
        {
            LogService.LogWarningConcurrent($"Cannot cache a Workshop preview at {pPath}: {e.Message}");
        }
    }

    private static void Store(ulong pFileId, string pKey, Sprite pSprite)
    {
        if (memory_cache.TryGetValue(pFileId, out CachedSprite old) && old.sprite != null && old.sprite != pSprite)
        {
            DestroySprite(old.sprite);
        }

        memory_cache[pFileId] = new CachedSprite { key = pKey, sprite = pSprite, last_use = ++use_counter };
        if (memory_cache.Count <= memory_cache_limit) return;

        ulong oldest = memory_cache.OrderBy(entry => entry.Value.last_use).First().Key;
        DestroySprite(memory_cache[oldest].sprite);
        memory_cache.Remove(oldest);
    }

    private static void DestroySprite(Sprite pSprite)
    {
        UnityEngine.Object.Destroy(pSprite.texture);
        UnityEngine.Object.Destroy(pSprite);
    }

    /// <summary>
    ///     Stable across sessions (string.GetHashCode is not guaranteed to be), so a changed preview URL gets a new
    ///     cache file.
    /// </summary>
    private static string UrlKey(string pUrl)
    {
        ulong hash = 14695981039346656037;
        foreach (char c in pUrl)
        {
            hash ^= c;
            hash *= 1099511628211;
        }

        return hash.ToString("x16");
    }

    private class CachedSprite
    {
        public string key;
        public long last_use;
        public Sprite sprite;
    }
}
