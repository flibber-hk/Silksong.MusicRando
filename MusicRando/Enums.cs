using System.ComponentModel;

namespace MusicRando;

public enum RandomizationStrategyOption
{
    [Description("Randomize whenever the music changes")]
    OnChange,

    [Description("Randomize more often than usual")]
    Chaos,

    [Description("Select a song dependent on the vanilla song")]
    Consistent,

    [Description("Randomly decide when to randomize the music")]
    RandoRando,

    [Description("Do not randomize music")]
    Disabled,
}

public enum MusicAction
{
    Ignore,
    Replay,
    Randomize,
}
