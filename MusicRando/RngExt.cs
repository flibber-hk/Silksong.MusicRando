using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicRando;

internal static class RngExt
{
    public static T Choose<T>(this Random rng, IList<T> things)
    {
        int i = rng.Next(things.Count);
        return things[i];
    }

    public static T Choose<T>(this Random rng, IList<T> things, Func<T, bool> predicate)
    {
        List<T> canChoose = things.Where(predicate).ToList();
        return rng.Choose(canChoose);
    }
}
