namespace MusicRando.MusicSelectionStrategies;

internal class RandoRandoSelectionStrategy : SelectionStrategy
{
    protected override bool TrySelect(MusicCue origToPlay, out string? selected)
    {
        if (rng.Next(8) != 5)
        {
            selected = null;
            return false;
        }

        selected = rng.Choose(DifferentLocationKeys);
        return true;
    }
}
