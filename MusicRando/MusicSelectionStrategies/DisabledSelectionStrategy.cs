namespace MusicRando.MusicSelectionStrategies
{
    internal class DisabledSelectionStrategy : SelectionStrategy
    {
        protected override bool TrySelect(MusicCue origToPlay, out string? selected)
        {
            selected = default;
            return false;
        }
    }
}
