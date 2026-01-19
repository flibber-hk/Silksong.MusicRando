using BepInEx;
using BepInEx.Logging;
using MonoDetour.HookGen;
using MusicRando.Cache;
using MusicRando.MusicSelectionStrategies;
using Silksong.AssetHelper;
using Silksong.AssetHelper.Core;
using Silksong.AssetHelper.ManagedAssets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace MusicRando;

// TODOs
// * Consider applying to atmos cues. Certain areas get an atmos cue when you enter them, an example being the
// bellshrine. I don't really want to play music raw but we could do that... it'd be annoying though
// * Nuuvestigate channels - particularly the Sub-Area AudioMixerSnapshot which sometimes kills the music,
// presumably if the music cue is missing a certain channel. Maybe we should fill the missing channels
// with the main channel's music?


[BepInAutoPlugin(id: "io.github.flibber-hk.musicrando")]
[BepInDependency("io.github.flibber-hk.filteredlogs", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(AssetHelperPlugin.Id)]
[MonoDetourTargets(typeof(AudioManager))]
public partial class MusicRandoPlugin : BaseUnityPlugin
{
    private static Dictionary<RandomizationStrategyOption, SelectionStrategy> Strategies { get; set; }

    internal static ManualLogSource InstanceLogger { get; private set; }

    private void Awake()
    {
        InstanceLogger = Logger;

        ConfigSettings.Init(Config);
        GameEvents.Hook();

        Strategies = new()
        {
            [RandomizationStrategyOption.OnChange] = new OnChangeSelectionStrategy(),
            [RandomizationStrategyOption.Chaos] = new ChaosSelectionStrategy(),
            [RandomizationStrategyOption.Consistent] = new ConsistentSelectionStrategy(),
            [RandomizationStrategyOption.RandoRando] = new RandoRandoSelectionStrategy(),
            [RandomizationStrategyOption.Disabled] = new DisabledSelectionStrategy(),
        };

        ConfigSettings.MusicRandomization?.SettingChanged += (sender, e) =>
        {
            SelectionStrategy.Reset();
            _lastMusicCueLocation = null;
            if (Strategies.TryGetValue(ConfigSettings.MusicRandomization.Value, out SelectionStrategy strategy))
            {
                strategy.InitStrategy();
            }
        };

        AddressablesData.InvokeAfterAddressablesLoaded(FindAudioCues);
        Md.AudioManager.ApplyMusicCue.Prefix(OnApplyMusicCue);
        GameEvents.OnQuitToMenu += OnQuitToMenu;

#if DEBUG
        // DebugHooks.Hook(Logger);
#endif

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private ManagedResourceLocation<MusicCue>? _lastMusicCueLocation;

    private void OnApplyMusicCue(AudioManager self, ref MusicCue musicCue, ref float delayTime, ref float transitionTime, ref bool applySnapshot)
    {
        RandomizationStrategyOption option = ConfigSettings.MusicRandomization?.Value ?? RandomizationStrategyOption.OnChange;
        SelectionStrategy strat = Strategies[option];

        MusicAction action = strat.Select(musicCue, out ManagedResourceLocation<MusicCue>? location);

        if (location != null && !location.IsLoaded)
        {
            Logger.LogInfo($"Failed to apply music cue: not loaded");
            action = MusicAction.Ignore;
        }

        if (action == MusicAction.Replay)
        {
            if (_lastMusicCueLocation is not null)
            {
                Logger.LogInfo($"Re-applying {_lastMusicCueLocation.Handle.Result.name}, over {musicCue.name}");
                musicCue = _lastMusicCueLocation.Handle.Result;
                return;
            }
            else
            {
                action = MusicAction.Ignore;
            }
        }
        if (action == MusicAction.Ignore)
        {
            return;
        }

        Logger.LogInfo($"Applying {location!.Handle.Result.name} over {musicCue.name}");
        _lastMusicCueLocation = location;
        musicCue = location.Handle.Result;        
    }

    private void OnQuitToMenu()
    {
        SelectionStrategy.Reset();
        _lastMusicCueLocation = null;
        foreach (SelectionStrategy strat in Strategies.Values)
        {
            strat.InitStrategy();
        }
    }

    private void FindAudioCues()
    {
        Stopwatch sw = Stopwatch.StartNew();

        List<string> musicCueKeys = CacheManager.GetCached(
            "musicrando.musiccues.json", Version, "0.1.0", GetMusicCuePrimaryKeys);

        List<IResourceLocation> musicCueLocations = new();
        foreach (string key in musicCueKeys)
        {
            if (!AddressablesData.MainLocator!.Locate(key, typeof(MusicCue), out IList<IResourceLocation> locs))
            {
                continue;
            }

            musicCueLocations.AddRange(locs);
        }

        SelectionStrategy.SetResourceLocations(musicCueLocations);

        sw.Stop();
        Logger.LogInfo($"Prepared music cue locations in {sw.ElapsedMilliseconds} ms");
    }

    private List<string> GetMusicCuePrimaryKeys()
    {
        List<string> result = [];
        foreach (string pkey in AddressablesData.MainLocator!.Keys.OfType<string>())
        {
            if (AddressablesData.MainLocator!.Locate(pkey, typeof(MusicCue), out _))
            {
                result.Add(pkey);
            }
        }

        return result;
    }
}
