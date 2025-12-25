using Silksong.AssetHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace MusicRando.MusicSelectionStrategies;

internal abstract class SelectionStrategy
{
    protected static Random rng = new();

    protected static Dictionary<string, IResourceLocation>? MusicResourceLocations;
    protected static List<string> DifferentLocationKeys => MusicResourceLocations?
        .Keys
        .Where(x => (LastSelectedLocationKey ?? string.Empty) != x)
        .ToList()
          ?? [];

    internal static void SetResourceLocations(IEnumerable<IResourceLocation> locs)
    {
        MusicResourceLocations = locs.ToDictionary(x => x.InternalId);
    }

    /// <summary>
    /// Select a music cue to play when the game tries to play a music cue.
    /// </summary>
    /// <param name="origToPlay">The music cue that would be played</param>
    /// <param name="selected">The key to the location of the selected music cue.</param>
    /// <returns>True if the music cue should be replaced.</returns>
    protected abstract bool TrySelect(MusicCue origToPlay, out string? selected);

    /// <summary>
    /// Handling for when this strategy is selected (either in the menu or when loaded).
    /// </summary>
    public virtual void InitStrategy() { }

    // Private method to run the selection strategy
    private MusicAction RunSelection(MusicCue origToPlay, out string? location)
    {
        location = default;

        if ((MusicResourceLocations?.Count ?? 0) <= 0)
        {
            // Should not happen
            return MusicAction.Ignore;
        }

        if (!GameEvents.IsInGame)
        {
            if (ConfigSettings.IncludeMenuMusic?.Value != true)
            {
                // Don't change menu music unless they ask for it
                return MusicAction.Ignore;
            }
        }

        if (origToPlay.name == "None")
        {
            // If the game plays no music we also play no music
            return MusicAction.Ignore;
        }

        bool b = TrySelect(origToPlay, out location);

        return b ? MusicAction.Randomize : MusicAction.Replay;
    }

    // Run the selection, and then assign relevant static members on this class
    internal MusicAction Select(MusicCue origToPlay, out IResourceLocation? location)
    {
        MusicAction action = RunSelection(origToPlay, out string? locationKey);

        if (action == MusicAction.Ignore || action == MusicAction.Replay)
        {
            location = default;
            return action;
        }

        else
        {
            LastSelectedLocationKey = locationKey;
            location = MusicResourceLocations![locationKey!];
            return action;
        }
    }

    protected static string? LastSelectedLocationKey { get; private set; }

    public static void Reset()
    {
        LastSelectedLocationKey = null;
    }
}
