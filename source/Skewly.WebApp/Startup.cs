using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Skewly.Providers.elasticsearch;
using Skewtech.Common.Persistence;
using System;

namespace Skewly.WebApp
{
    public class StoreFactory
    {
        private IServiceProvider ServiceProvider { get; }

        public StoreFactory(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        public IStore<T> BuildStore<T>(bool enableCache = false) where T : class, new()
        {
            var store = ServiceProvider.GetRequiredService<IStore<T>>();

            if(!enableCache)
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

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetValue<string>("Redis:Configuration");
                options.InstanceName = Configuration.GetValue<string>("Redis:InstanceName");
            });

            services.AddSingleton<IElasticClient>((sp) => new ElasticClient(new Uri(Configuration.GetValue<string>("Elasticsearch:Server"))));
            
            services.AddSingleton(typeof(IStore<>), typeof(Store<>));

            services.AddSingleton<StoreFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapControllers();
            });
        }
    }
}
