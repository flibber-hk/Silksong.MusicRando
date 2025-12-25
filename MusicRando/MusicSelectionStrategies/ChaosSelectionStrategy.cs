namespace MusicRando.MusicSelectionStrategies;

internal class ChaosSelectionStrategy : SelectionStrategy
{
    protected override bool TrySelect(MusicCue origToPlay, out string? selected)
    {
        selected = rng.Choose(DifferentLocationKeys);
        return true;
    }
}
