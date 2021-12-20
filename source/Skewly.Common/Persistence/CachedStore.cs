using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Skewly.Common.Persistence
{
    public class CachedStore<T> : IStore<T> where T : Document, new()
    {
        private IStore<T> Store { get; }
        private IDistributedCache Cache { get; }

        public CachedStore(IStore<T> store, IDistributedCache cache)
        {
            Store = store;
            Cache = cache;
        }

        private string DetermineCacheKey(string id)
        {
            var index = $"data_{typeof(T).FullName.Replace('.', '-')}_{id}".ToLower();

            return index;
        }

        public async Task<Page<T>> Search(ISearch search, CancellationToken ct)
        {
            // We don't cache queries because the keys wouldn't promote uniqueness
            return await Store.Search(search, ct);
        }

        public async Task<T> Get(string id, CancellationToken ct)
        {
            var cacheKey = DetermineCacheKey(id);

            var data = await Cache.Get<T>(cacheKey, ct);

            if (data == default)
            {
                data = await Store.Get(id, ct);

                await Cache.Set(cacheKey, data, ct);
            }

            return data;
        }

        public async Task Put(string id, T data, CancellationToken ct)
        {
            await Store.Put(id, data, ct);
            await Cache.RemoveAsync(DetermineCacheKey(id), ct);
        }

        public async Task<T> Post(T data, CancellationToken ct)
        {
            return await Store.Post(data, ct);
        }

        public async Task Patch<TPatch>(string id, TPatch data, CancellationToken ct) where TPatch : class
        {
            await Store.Patch(id, data, ct);
            await Cache.RemoveAsync(DetermineCacheKey(id), ct);
        }

        public async Task Delete(string id, CancellationToken ct)
        {
            await Store.Delete(id, ct);
            await Cache.RemoveAsync(DetermineCacheKey(id), ct);
        }
    }
}
