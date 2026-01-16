using BepInEx.Configuration;

namespace MusicRando;

internal static class ConfigSettings
{
    public static ConfigEntry<RandomizationStrategyOption>? MusicRandomization;

    public static void Init(ConfigFile config)
    {
        MusicRandomization = config.Bind(
            "General", nameof(MusicRandomization), RandomizationStrategyOption.OnChange, new ConfigDescription(
                "When to randomize the music", null));  // TODO - right description for menu entry
    }
}
