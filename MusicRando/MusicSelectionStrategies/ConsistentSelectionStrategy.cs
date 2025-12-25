using System;
using System.Collections.Generic;
using System.Text;

namespace MusicRando.MusicSelectionStrategies;

internal class ConsistentSelectionStrategy : SelectionStrategy
{
    private readonly Dictionary<string, string> _previouslySelected = [];
    private string? _lastOrigMusicCue = null;

    public override void InitStrategy()
    {
        _previouslySelected.Clear();
        _lastOrigMusicCue = null;
    }

    protected override bool TrySelect(MusicCue origToPlay, out string? selected)
    {
        if (_lastOrigMusicCue == origToPlay.name)
        {
            selected = default;
            return false;
        }

        if (_previouslySelected.TryGetValue(origToPlay.name, out selected))
        {
            return true;
        }

        // TODO - remove things that have already been selected
        selected = rng.Choose(DifferentLocationKeys);

        _previouslySelected[origToPlay.name] = selected;
        return true;
    }
}
