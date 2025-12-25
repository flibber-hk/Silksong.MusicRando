using BepInEx.Configuration;
using System.ComponentModel;

namespace MusicRando;

internal static class ConfigSettings
{
    // TODO - implement this
    public static ConfigEntry<RandomizationStrategyOption>? MusicRandomization;

    // TODO - implement this
    public static ConfigEntry<bool>? IncludeMenuMusic;


    public static void Init(ConfigFile config)
    {
        MusicRandomization = config.Bind(
            "General", nameof(MusicRandomization), RandomizationStrategyOption.OnChange, new ConfigDescription(
                "When to randomize the music", null));  // TODO - right description for menu entry

        IncludeMenuMusic = config.Bind(
            "General", nameof(IncludeMenuMusic), false, new ConfigDescription(
                "Whether to randomize the main menu music"));
    }
}
