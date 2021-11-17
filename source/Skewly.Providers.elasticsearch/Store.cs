using Elasticsearch.Net;
using Nest;
using Skewly.Common.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skewly.Providers.elasticsearch
{
    public class Store<T> : IStore<T> where T : Document, new()
    {
        protected IElasticClient Client { get; }

        public Store(IElasticClient client)
        {
            Client = client;

            InitializeStore();
        }

        protected virtual void InitializeStore()
        {
            var exists = Client.Indices.Exists(DetermineIndexName());

            if(!exists.Exists)
            {
                Client.Indices.Create(DetermineIndexName(), c => c
                    .Map<T>(m => m
                        .RoutingField(r => Routing(r))
                        .AutoMap()
                        .Properties(ps => PropertyMapping(ps))
                    )
                );
            }
        }

        protected virtual IRoutingField Routing(RoutingFieldDescriptor<T> descriptor)
        {
            return descriptor;
        }

        protected virtual IPromise<IProperties> PropertyMapping(PropertiesDescriptor<T> descriptor)
        {
            return descriptor
                .Keyword(s => s
                    .Name(n => n.Id)
                );
        }

        protected virtual string DetermineIndexName()
        {
            var index = $"data_{typeof(T).FullName.Replace('.', '-')}".ToLower();

            return index;
        }

        protected virtual Routing DetermineRouting()
        {
            return default;
        }

        protected virtual QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> descriptor, Common.Persistence.IQuery query)
        {
            switch(query)
            {
                case Common.Persistence.TermQuery termQuery:
                    return descriptor.Term(t => t.Field(termQuery.Field).Value(termQuery.Term));
                case Common.Persistence.TermsQuery termsQuery:
                    return descriptor.Terms(t => t.Field(termsQuery.Field).Terms(termsQuery.Terms));
                case Common.Persistence.AndQuery andQuery:
                    return descriptor.Bool(b => b.Must(andQuery.Queries.Select(q => QueryContainerDescriptor(descriptor, q)).ToArray()));
                default:
                    return descriptor.MatchAll();
            }
        }

        protected virtual T BeforeWrite(T obj)
        {
            return obj;
        }

        public async Task<Page<T>> Get(Common.Persistence.IQuery query, CancellationToken ct = default)
        {
            var skip = query.Skip;
            var take = query.Take;

            var response = await Client.SearchAsync<T>(i => i.Index(DetermineIndexName()).Routing(DetermineRouting()).Query(q => QueryContainerDescriptor(q, query)).From(skip).Size(take), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }

            return new Page<T>
            {
                Results = response.Documents.ToList(),
                Skip = skip,
                Take = take,
                Total = response.Total
            };
        }

        public async Task<T> Get(string id, CancellationToken ct = default)
        {
            var response = await Client.GetAsync<T>(id, i => i.Index(DetermineIndexName()).Routing(DetermineRouting()), ct);

            if(!response.IsValid || !response.Found)
            {
                throw response.OriginalException;
            }

            return response.Source;
        }

        public async Task Put(string id, T data, CancellationToken ct = default)
        {
            data = BeforeWrite(data);
            data.Id = id;

            var response = await Client.IndexAsync(data, i => i.Index(DetermineIndexName()).Routing(DetermineRouting()).Id(id), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task<string> Post(T data, CancellationToken ct = default)
        {
            data = BeforeWrite(data);
            if(string.IsNullOrEmpty(data.Id))
            {
                data.Id = Guid.NewGuid().ToString();
            }

            var response = await Client.IndexAsync(data, i => i.Index(DetermineIndexName()).Routing(DetermineRouting()).OpType(OpType.Create), ct);

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

            return response.Id;
        }

        public async Task Patch<TPatch>(string id, TPatch data, CancellationToken ct = default) where TPatch : class
        {
            if(typeof(TPatch).GetProperties().Any(p => p.Name.Equals("Organization")))
            {
                throw new Exception($"Organization is a reserved property and can't be updated.");
            }

            var response = await Client.UpdateAsync<T, TPatch>(id, i => i.Index(DetermineIndexName()).Routing(DetermineRouting()).Doc(data), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task Delete(string id, CancellationToken ct)
        {
            var response = await Client.DeleteAsync<T>(id, i => i.Index(DetermineIndexName()).Routing(DetermineRouting()), ct);

            if(!response.IsValid)
            {
                throw response.OriginalException;
            }
        }
    }
}
