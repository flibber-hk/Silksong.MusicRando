namespace MusicRando.MusicSelectionStrategies;

internal class OnChangeSelectionStrategy : SelectionStrategy
{
    private string? _lastOrigMusicCue = null;

    public override void InitStrategy()
    {
        _lastOrigMusicCue = null;
    }

    protected override bool TrySelect(MusicCue origToPlay, out string? selected)
    {
        if (origToPlay.name == _lastOrigMusicCue)
        {
            selected = default;
            return false;
        }

        _lastOrigMusicCue = origToPlay.name;
        selected = rng.Choose(DifferentLocationKeys);
        return true;
    }
}
