using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Skewly.Providers.elasticsearch;
using Skewly.WebApp.Controllers;
using Skewly.Common.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Skewly.Common.Models;
using Skewly.WebApp.Stores;

namespace Skewly.WebApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static void AddStore<T>(this IServiceCollection services, bool cache = false) where T : Document, new()
        {
            if(cache)
            {
                services.AddSingleton<IStore<T>>((provider) =>
                {
                    var client = provider.GetRequiredService<IElasticClient>();
                    var cache = provider.GetRequiredService<IDistributedCache>();

                    var store = new Store<T>(client);

                    return new CachedStore<T>(store, cache);
                });
            }
            else
            {
                services.AddSingleton<IStore<T>>((provider) =>
                {
                    var client = provider.GetRequiredService<IElasticClient>();

                    return new Store<T>(client);
                });
            }
        }

        private static void AddMultitenantStore<T>(this IServiceCollection services, bool cache = false) where T : MultitenantDocument, new()
        {
            if (cache)
            {
                services.AddSingleton<IStore<T>>((provider) =>
                {
                    var client = provider.GetRequiredService<IElasticClient>();
                    var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                    var cache = provider.GetRequiredService<IDistributedCache>();

                    var store = new MultitenantStore<T>(client, accessor);

                    return new CachedStore<T>(store, cache);
                });
            }
            else
            {
                services.AddSingleton<IStore<T>>((provider) =>
                {
                    var client = provider.GetRequiredService<IElasticClient>();
                    var accessor = provider.GetRequiredService<IHttpContextAccessor>();

                    return new MultitenantStore<T>(client, accessor);
                });
            }
        }

        public static void AddStores(this IServiceCollection services)
        {
            services.AddSingleton<IStore<Common.Models.ApiKey>>((provider) =>
            {
                var client = provider.GetRequiredService<IElasticClient>();
                var cache = provider.GetRequiredService<IDistributedCache>();

                var store = new ApiKeyStore(client);

                return new CachedStore<Common.Models.ApiKey>(store, cache);
            });

            services.AddStore<OrganizationPermission>(true);
            services.AddStore<Organization>(true);
            services.AddMultitenantStore<Example>();
        }
    }
}
