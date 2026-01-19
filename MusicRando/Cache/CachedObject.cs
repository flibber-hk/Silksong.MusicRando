namespace MusicRando.Cache;

internal class CachedObject<T> where T : class
{
    public string? SilksongVersion { get; set; }

    public string? PluginVersion { get; set; }

    public required T Value { get; set; }
}
