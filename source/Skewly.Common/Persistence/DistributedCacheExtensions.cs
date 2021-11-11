using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Skewtech.Common.Persistence
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T> Get<T>(this IDistributedCache cache, string key, CancellationToken ct = default) where T : class
        {
            var str = await cache.GetStringAsync(key, ct);

            if(str != default)
            {
                return JsonConvert.DeserializeObject<T>(str);
            }

            return default;
        }

        public static async Task Set<T>(this IDistributedCache cache, string key, T obj, CancellationToken ct = default) where T : class
        {
            var str = JsonConvert.SerializeObject(obj);

            await cache.SetStringAsync(key, str, ct);
        }
    }
}
