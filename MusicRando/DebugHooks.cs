#if DEBUG
using BepInEx.Logging;
using System.Collections;
using UnityEngine.Audio;

namespace MusicRando;

internal static class DebugHooks
{
    private static ManualLogSource? _log;

    public static void Hook(ManualLogSource log)
    {
        _log = log;

        On.AudioManager.BeginApplyMusicSnapshot += AudioManager_BeginApplyMusicSnapshot;
        On.AudioManager.BeginApplyMusicCue += AudioManager_BeginApplyMusicCue;
        On.AudioManager.BeginApplyAtmosCue += AudioManager_BeginApplyAtmosCue;
    }

    private static void AudioManager_BeginApplyAtmosCue(On.AudioManager.orig_BeginApplyAtmosCue orig, AudioManager self, AtmosCue atmosCue, float transitionTime)
    {
        _log?.LogInfo($"{nameof(AudioManager.BeginApplyAtmosCue)}: {atmosCue.name} :: {transitionTime}");
        orig(self, atmosCue, transitionTime);
    }

    private static IEnumerator AudioManager_BeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime)
    {
        _log?.LogInfo($"{nameof(AudioManager.BeginApplyMusicCue)}: {musicCue.name} :: {delayTime}");
        return orig(self, musicCue, delayTime);
    }

    private static IEnumerator AudioManager_BeginApplyMusicSnapshot(On.AudioManager.orig_BeginApplyMusicSnapshot orig, AudioMixerSnapshot snapshot, float delayTime, float transitionTime, bool blockMusicMarker)
    {
        _log?.LogInfo($"{nameof(AudioManager.BeginApplyMusicSnapshot)}: {snapshot.name} :: {delayTime}/{transitionTime}/{blockMusicMarker}");
        return orig(snapshot, delayTime, transitionTime, blockMusicMarker);
    }
}
#endif
