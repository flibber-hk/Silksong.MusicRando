using BepInEx.Configuration;
using Silksong.ModMenu.Plugin;

namespace MusicRando;

internal static class ConfigSettings
{
    public static ConfigEntry<RandomizationStrategyOption>? MusicRandomization;

    public static void Init(ConfigFile config)
    {
        MusicRandomization = config.Bind(
            "General", nameof(MusicRandomization), RandomizationStrategyOption.OnChange, new ConfigDescription(
                "When to randomize the music", null, MenuElementGenerators.CreateRightDescGenerator(false)));
    }
}
