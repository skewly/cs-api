using Nest;
using Skewtech.Common.Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skewly.Providers.elasticsearch
{
    public class Store<T> : IStore<T> where T : class, new()
    {
        protected IElasticClient Client { get; }

        public Store(IElasticClient client)
        {
            Client = client;
        }

        protected virtual string DetermineIndexName()
        {
            var index = $"data_{typeof(T).FullName.Replace('.', '-')}".ToLower();

            return index;
        }

        public async Task<Page<T>> Get(Skewtech.Common.Persistence.IQuery query, CancellationToken ct = default)
        {
            var skip = query.Skip;
            var take = query.Take;

            var response = await Client.SearchAsync<T>(i => i.Index(DetermineIndexName()).MatchAll().From(skip).Size(take), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }

            return new Page<T>
            {
                Results = response.Hits.Select(h => new WrappedData<T>
                {
                    Id = h.Id,
                    Version = h.Version,
                    Data = h.Source
                }).ToList(),
                Skip = skip,
                Take = take,
                Total = response.Total
            };
        }

        public async Task<T> Get(string id, CancellationToken ct = default)
        {
            var response = await Client.GetAsync<T>(id, i => i.Index(DetermineIndexName()) , ct);

            if(!response.IsValid || !response.Found)
            {
                throw response.OriginalException;
            }

            return response.Source;
        }

        public async Task Put(string id, T data, CancellationToken ct = default)
        {
            var response = await Client.IndexAsync(data, i => i.Index(DetermineIndexName()).Id(id), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task<string> Post(T data, CancellationToken ct = default)
        {
            var response = await Client.IndexAsync(data, i => i.Index(DetermineIndexName()), ct);

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

            return response.Id;
        }

        public async Task Patch<TPatch>(string id, TPatch data, CancellationToken ct = default) where TPatch : class
        {
            var response = await Client.UpdateAsync<T, TPatch>(id, i => i.Index(DetermineIndexName()).Doc(data), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task Delete(string id, CancellationToken ct)
        {
            var response = await Client.DeleteAsync<T>(id, i => i.Index(DetermineIndexName()), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }
        }
    }
}
