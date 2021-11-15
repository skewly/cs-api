using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Skewly.Common.Models;
using Skewly.Common.Persistence;
using Skewly.Common.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Skewly.Providers.elasticsearch
{
    public class StoreFactory
    {
        private IServiceProvider ServiceProvider { get; }

        public StoreFactory(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        public IStore<T> BuildStore<T>(bool enableCache = false) where T : Document, new()
        {
            var store = ServiceProvider.GetRequiredService<IStore<T>>();

            if (!enableCache)
            {
                return store;
            }
            else
            {
                var cache = ServiceProvider.GetRequiredService<IDistributedCache>();

                return new CachedStore<T>(store, cache);
            }
        }
    }

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
            return descriptor;
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

        protected virtual QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> descriptor)
        {
            return descriptor.MatchAll();
        }

        protected virtual T BeforeWrite(T obj)
        {
            return obj;
        }

        public async Task<Page<T>> Get(Common.Persistence.IQuery query, CancellationToken ct = default)
        {
            var skip = query.Skip;
            var take = query.Take;

            var response = await Client.SearchAsync<T>(i => i.Index(DetermineIndexName()).Routing(DetermineRouting()).Query(q => QueryContainerDescriptor(q)).From(skip).Size(take), ct);

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

    public class MultitenantDocument : Document
    {
        public string Organization { get; set; }
    }

    public class MultitenantStore<T> : Store<T> where T : MultitenantDocument, new()
    {
        protected IHttpContextAccessor Accessor { get; set; }

        public MultitenantStore(IElasticClient client, IHttpContextAccessor accessor) : base(client)
        {
            Accessor = accessor;
        }

        protected override IRoutingField Routing(RoutingFieldDescriptor<T> descriptor)
        {
            return descriptor.Required();
        }

        protected override Routing DetermineRouting()
        {
            if(Accessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                return new Routing(organization.Id);
            }

            return base.DetermineRouting();
        }

        protected override QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> descriptor)
        {
            if(Accessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                return descriptor.Term(t => t.Field(f => f.Organization).Value(organization.Id));
            }

            return base.QueryContainerDescriptor(descriptor);
        }

        protected override T BeforeWrite(T obj)
        {
            if (Accessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                obj.Organization = organization.Id;
            }

            return base.BeforeWrite(obj);
        }
    }
}
