using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Skewly.WebApp.Extensions;
using Skewly.WebApp.Middleware;
using Skewly.WebApp.RateLimit;
using StackExchange.Redis;
using System;

namespace Skewly.WebApp
{
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
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                );
            });

            services.AddHttpContextAccessor();

            services.AddControllersWithViews();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetValue<string>("Redis:Configuration");
                options.InstanceName = Configuration.GetValue<string>("Redis:InstanceName");
            });

            services.AddSingleton<IElasticClient>((sp) =>
            {
                var connectionSettings = new ConnectionSettings(new Uri(Configuration.GetValue<string>("Elasticsearch:Server")));

                connectionSettings.DisableDirectStreaming();

                return new ElasticClient(connectionSettings);
            });

            services.AddStores();

            // Rate Limiting
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(Configuration.GetValue<string>("Redis:Configuration")));
            services.AddRedisRateLimiting();
            services.AddSingleton<IClientPolicyStore, ClientSubscriptionPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = "https://dev-ko-0isuz.us.auth0.com/";
                options.Audience = "https://api.skewly.io/";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();

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

            app.UseWhen(context => !context.Request.Path.Equals("/"), appBuilder =>
            {
                appBuilder.UseMiddleware<MultitenantMiddleware>();
                appBuilder.UseClientRateLimiting();
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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
