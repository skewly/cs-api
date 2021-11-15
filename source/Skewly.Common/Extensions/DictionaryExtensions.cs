using System.Collections.Generic;

namespace Skewly.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool TryGetValue<TKey, TValue>(this IDictionary<object, object> dictionary, TKey key, out TValue value) where TValue : class
        {
            value = default;

            if (dictionary.TryGetValue(key, out var obj))
            {
                value = (TValue)obj;
            }

            return value != default;
        }
    }
}
