using Silksong.AssetHelper.ManagedAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;
using LoadableMusicCue = Silksong.AssetHelper.ManagedAssets.ManagedResourceLocation<MusicCue>;

namespace MusicRando.MusicSelectionStrategies;

internal abstract class SelectionStrategy
{
    protected static Random rng = new();

    protected static Dictionary<string, LoadableMusicCue>? MusicResourceLocations;
    protected static List<string> DifferentLocationKeys => MusicResourceLocations?
        .Keys
        .Where(x => (LastSelectedLocationKey ?? string.Empty) != x)
        .ToList()
          ?? [];

    internal static void SetResourceLocations(IEnumerable<IResourceLocation> locs)
    {
        if (MusicResourceLocations != null)
        {
            throw new InvalidOperationException("Can't set locs more than once!");
        }

        MusicResourceLocations = [];
        Dictionary<string, IResourceLocation> theLocs = [];
        foreach (IResourceLocation loc in locs)
        {
            if (loc.InternalId.EndsWith("None.asset"))
            {
                continue;
            }

            if (!MusicResourceLocations.ContainsKey(loc.InternalId))
            {
                MusicResourceLocations.Add(loc.InternalId, new LoadableMusicCue(loc));
                theLocs.Add(loc.InternalId, loc);
                continue;
            }
        }
        MusicRandoPlugin.InstanceLogger.LogInfo($"Found {MusicResourceLocations.Count} distinct music cues");

        GameEvents.OnEnterGame += () =>
        {
            foreach (LoadableMusicCue cue in MusicResourceLocations.Values)
            {
                cue.Load();
            }
        };
        GameEvents.OnQuitToMenu += () =>
        {
            foreach (LoadableMusicCue cue in MusicResourceLocations.Values)
            {
                cue.Unload();
            }
        };
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
            // It's easier to just ignore the menu music
            return MusicAction.Ignore;
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
    internal MusicAction Select(MusicCue origToPlay, out LoadableMusicCue? location)
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
