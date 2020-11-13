using System;
using System.Collections.Generic;

namespace Mafia.Infrastructure
{
    public static class MoreLinq
    {
        public static IEnumerable<T> Multiply<T>(this IEnumerable<Tuple<T, int>> iEnumerable)
        {
            foreach (var t in iEnumerable)
            {
                var item = t.Item1;
                var count = t.Item2;
                for (int j = 0; j < count; j++)
                    yield return item;
            }
        }
    }
}