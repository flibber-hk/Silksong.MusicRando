using BepInEx;
using MusicRando.MusicSelectionStrategies;
using Silksong.AssetHelper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace MusicRando;

// TODOs
// Apply to atmos cues as well (plus config) (? - maybe not)
// Nuuvestigate channels


[BepInAutoPlugin(id: "io.github.flibber-hk.musicrando")]
[BepInDependency("io.github.flibber-hk.filteredlogs", BepInDependency.DependencyFlags.SoftDependency)]
public partial class MusicRandoPlugin : BaseUnityPlugin
{
    private static Dictionary<RandomizationStrategyOption, SelectionStrategy> Strategies { get; set; }

    private void Awake()
    {
        ConfigSettings.Init(Config);
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
            if (Strategies.TryGetValue(ConfigSettings.MusicRandomization.Value, out SelectionStrategy strategy))
            {
                strategy.InitStrategy();
            }
        };

        AssetsData.InvokeAfterAddressablesLoaded(FindAudioCues);
        On.AudioManager.ApplyMusicCue += OnApplyMusicCue;
        GameEvents.OnQuitToMenu += OnQuitToMenu;

#if DEBUG
        FilteredLogs.API.ApplyFilter(Name);
        DebugHooks.Hook(Logger);
#endif

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void OnQuitToMenu()
    {
        UnloadLastHandle();
        SelectionStrategy.Reset();
        foreach (SelectionStrategy strat in Strategies.Values)
        {
            strat.InitStrategy();
        }
    }

    private void OnApplyMusicCue(
        On.AudioManager.orig_ApplyMusicCue orig,
        AudioManager self,
        MusicCue musicCue,
        float delayTime,
        float transitionTime,
        bool applySnapshot
        )
    {
        RandomizationStrategyOption option = ConfigSettings.MusicRandomization?.Value ?? RandomizationStrategyOption.OnChange;
        SelectionStrategy strat = Strategies[option];

        MusicAction action = strat.Select(musicCue, out IResourceLocation? location);

        if (action == MusicAction.Replay)
        {
            if (_lastMusicCueHandle.HasValue)
            {
                orig(self, _lastMusicCueHandle.Value.Result, delayTime, transitionTime, applySnapshot);
                return;
            }
            else
            {
                action = MusicAction.Ignore;
            }
        }
        if (action == MusicAction.Ignore)
        {
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
            return;
        }

        AsyncOperationHandle<MusicCue> handle = Addressables.LoadAssetAsync<MusicCue>(location!);

        string origName = musicCue.name;
        handle.Completed += theHandle =>
        {
            UnloadLastHandle();
            _lastMusicCueHandle = theHandle;
            Logger.LogInfo($"Applying {theHandle.Result.name} over {origName}");

            orig(self, theHandle.Result, delayTime, transitionTime, applySnapshot);
        };
    }

    private void UnloadLastHandle()
    {
        if (_lastMusicCueHandle.HasValue)
        {
            Addressables.Release(_lastMusicCueHandle.Value);
            _lastMusicCueHandle = null;
        }
    }

    private AsyncOperationHandle<MusicCue>? _lastMusicCueHandle { get; set; } = null;

    private void FindAudioCues()
    {
        List<IResourceLocation> musicCueLocations = Addressables.ResourceLocators.First()
            .AllLocations
            .Where(loc => loc.ResourceType == typeof(MusicCue))
            .Where(loc => loc.InternalId.StartsWith("Assets/Audio/MusicCues"))
            .ToList();
        SelectionStrategy.SetResourceLocations(musicCueLocations);

        /*
        _atmosCueLocations = Addressables.ResourceLocators.First()
            .AllLocations
            .Where(loc => loc.ResourceType == typeof(AtmosCue))
            .Where(loc => loc.InternalId.StartsWith("Assets/Audio/AtmosCues"))
            .ToList();
        */

        Logger.LogInfo($"Found {musicCueLocations.Count} music cue locations");
    }
}
