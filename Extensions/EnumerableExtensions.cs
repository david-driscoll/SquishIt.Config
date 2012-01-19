using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SquishIt.Config.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (T item in @this)
            {
                action(item);
            }
        }
    }
}
