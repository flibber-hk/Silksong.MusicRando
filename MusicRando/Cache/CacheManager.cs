using BepInEx;
using System;
using System.IO;
using System.Reflection;

namespace MusicRando.Cache;

public static class CacheManager
{
    /// <summary>
    /// The Silksong version. This is calculated using reflection to avoid it being inlined.
    /// </summary>
    public static string SilksongVersion
    {
        get
        {
            _silksongVersion ??= GetSilksongVersion();
            return _silksongVersion;
        }
    }

    private static string? _silksongVersion;

    private static string GetSilksongVersion() =>
        typeof(Constants)
            .GetField(
                nameof(Constants.GAME_VERSION),
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
            )
            ?.GetRawConstantValue() as string
        ?? "UNKNOWN";

    private static bool MetadataMismatch<T>(CachedObject<T> cached, string earliestAcceptableVersion) where T : class
    {
        if (cached.SilksongVersion == null || cached.SilksongVersion != SilksongVersion)
        {
            return true;
        }

        if (!Version.TryParse(earliestAcceptableVersion, out Version eVersion)) return true;
        if (cached.PluginVersion == null || !Version.TryParse(cached.PluginVersion, out var cVersion)) return true;
        
        // Cached version is too old
        if (cVersion < eVersion) return true;

        return false;
    }

    /// <summary>
    /// Try to load an object of the given type from the cache. If not possible,
    /// then generate the object, cache it and return it.
    /// 
    /// This tool is designed for functions that may change any time the silksong version changes,
    /// but do not change otherwise, and take long enough to compute that caching the result makes sense.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="fileName">The name of the file (not filepath).</param>
    /// <param name="currentPluginVersion">The current version of the plugin. This should be of the standard form `x.x.x`</param>
    /// <param name="earliestAcceptableVersion">The earliest acceptable version of the plugin. This should be set to your plugin's
    /// current version when first defining this function, and set to the plugin's current version each time
    /// you change the implementation of the <paramref name="getter"/> function.</param>
    /// <param name="getter">Function used to compute the object. This will only be computed if the cached value changes.</param>
    /// <returns></returns>
    public static T? GetCached<T>(
        string fileName,
        string currentPluginVersion,
        string earliestAcceptableVersion,
        Func<T> getter) where T : class
    {
        if (!Version.TryParse(currentPluginVersion, out _))
        {
            throw new ArgumentException("Cannot parse current plugin version to Version");
        }

        string filePath = Path.Combine(Paths.CachePath, fileName);

        if (JsonHelper.TryLoadFromFile(filePath, out CachedObject<T>? fromCache)
            && !MetadataMismatch(fromCache, earliestAcceptableVersion))
        {
            return fromCache.Value;
        }

        T? current = getter();

        CachedObject<T> toCache = new()
        {
            SilksongVersion = SilksongVersion,
            PluginVersion = currentPluginVersion,
            Value = current
        };
        toCache.SerializeToFile(filePath);

        return current;
    }
}
