using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Skewly.Common.Models;
using Skewly.Common.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Skewly.WebApp.RateLimit
{
    public class OrganizationClientResolveContributor : IClientResolveContributor
    {
        public Task<string> ResolveClientAsync(HttpContext httpContext)
        {
            var client = string.Empty;

            if(httpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                client = organization.Id;
            }

            return Task.FromResult(client);
        }
    }

    public class CustomRateLimitConfiguration : RateLimitConfiguration
    {
        public CustomRateLimitConfiguration(IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) : base(ipOptions, clientOptions)
        {

        }

        public override void RegisterResolvers()
        {
            ClientResolvers.Add(new OrganizationClientResolveContributor());

            base.RegisterResolvers();
        }
    }

    public class ClientSubscriptionPolicyStore : IClientPolicyStore
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly IList<ClientRateLimitPolicy> Policies;

        public ClientSubscriptionPolicyStore(IHttpContextAccessor accessor)
        {
            HttpContextAccessor = accessor;
            Policies = new List<ClientRateLimitPolicy>();

            Task.Run(() => SeedAsync());
        }

        public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            var exists = false;

            if(HttpContextAccessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                exists = Policies.Any(p => p.ClientId.Equals(organization.Subscription));
            }

            return Task.FromResult(exists);
        }

        public Task<ClientRateLimitPolicy> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var policy = Policies.FirstOrDefault(p => p.ClientId.Equals("free"));

            if (HttpContextAccessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                policy = Policies.FirstOrDefault(p => p.ClientId.Equals(organization.Subscription));
            }

            return Task.FromResult(policy);
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SeedAsync()
        {
            Policies.Add(new ClientRateLimitPolicy
            {
                ClientId = "free",
                Rules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*:/*",
                        Period = "1m",
                        Limit = 1
                    }
                }
            });

            Policies.Add(new ClientRateLimitPolicy
            {
                ClientId = "individual",
                Rules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*:/*",
                        Period = "1m",
                        Limit = 5
                    }
                }
            });

            Policies.Add(new ClientRateLimitPolicy
            {
                ClientId = "team",
                Rules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*:/*",
                        Period = "1m",
                        Limit = 10
                    }
                }
            });

            Policies.Add(new ClientRateLimitPolicy
            {
                ClientId = "enterprise",
                Rules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*:/*",
                        Period = "1m",
                        Limit = 20
                    }
                }
            });

            return Task.CompletedTask;
        }

        public Task SetAsync(string id, ClientRateLimitPolicy entry, System.TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
