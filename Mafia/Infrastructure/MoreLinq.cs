using System;
using System.Collections.Generic;

namespace Mafia.Infrastructure
{
    public static class MoreLinq
    {
        public static IEnumerable<T> Multiply<T>(this IEnumerable<Tuple<T, int>> iEnumerable)
        {
            if (iEnumerable is null)
                throw new NullReferenceException();
            foreach (var (item, count) in iEnumerable)
                for (var j = 0; j < count; j++)
                    yield return item;
        }
    }
}