using BepInEx;
using Silksong.AssetHelper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace MusicRando;

// Nuuvestigate channels

[BepInAutoPlugin(id: "io.github.flibber-hk.musicrando")]
public partial class MusicRandoPlugin : BaseUnityPlugin
{
    private List<IResourceLocation> _musicCueLocations = new();

    private static System.Random rng = new();

    private void Awake()
    {
        AssetsData.InvokeAfterAddressablesLoaded(FindMusicCues);

        On.AudioManager.ApplyMusicCue += OnApplyMusicCue;

        GameEvents.OnQuitToMenu += UnloadLastHandle;

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
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
        if ((_musicCueLocations?.Count ?? 0) <= 0)
        {
            Logger.LogInfo($"Not replacing {musicCue.name}: no music cue locations");
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
            return;
        }

        if (!GameEvents.IsInGame)
        {
            Logger.LogInfo($"Not replacing {musicCue.name}: in menu");
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
            return;
        }

        if (musicCue.name == "None")
        {
            Logger.LogInfo($"Not replacing musicCue: {musicCue.name}");
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
            return;
        }

        if (_lastOrigMusicCueName == musicCue.name && _lastMusicCueHandle.HasValue)
        {
            Logger.LogInfo($"Repplying {_lastMusicCueHandle.Value.Result.name} over {_lastOrigMusicCueName}");
            orig(self, _lastMusicCueHandle.Value.Result, delayTime, transitionTime, applySnapshot);
            return;
        }

        _lastOrigMusicCueName = musicCue.name;

        // TODO - make sure we apply a different music cue to the previous one we applied
        int idx = rng.Next(_musicCueLocations!.Count);
        IResourceLocation selected = _musicCueLocations[idx];

        AsyncOperationHandle<MusicCue> handle = Addressables.LoadAssetAsync<MusicCue>(selected);
        handle.Completed += theHandle =>
        {
            UnloadLastHandle();
            _lastMusicCueHandle = theHandle;
            Logger.LogInfo($"Applying {theHandle.Result.name} over {_lastOrigMusicCueName}");

            orig(self, theHandle.Result, delayTime, transitionTime, applySnapshot);
        };
    }

    private void UnloadLastHandle()
    {
        if (_lastMusicCueHandle.HasValue)
        {
            Addressables.Release(_lastMusicCueHandle.Value);
        }
    }

    private string? _lastOrigMusicCueName = null;
    private AsyncOperationHandle<MusicCue>? _lastMusicCueHandle = null;

    private void FindMusicCues()
    {
        _musicCueLocations = Addressables.ResourceLocators.First()
            .AllLocations
            .Where(loc => loc.ResourceType == typeof(MusicCue))
            .Where(loc => loc.InternalId.StartsWith("Assets/Audio/MusicCues"))
            .ToList();

        Logger.LogInfo($"Found {_musicCueLocations.Count} music cue locations");
    }
}
