using Microsoft.AspNetCore.Mvc;
using Skewly.Common.Persistence;
using System.Threading;
using System.Threading.Tasks;
using Skewly.Common.Models;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Skewly.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly IStore<ApiKey> ApiKeys;
        private readonly IStore<Organization> Organizations;

        public AccessController(IStore<ApiKey> apiKeys, IStore<Organization> organizations)
        {
            ApiKeys = apiKeys;
            Organizations = organizations;
        }

        [Authorize, HttpGet("organizations")]
        public Task<List<Organization>> GetOrganizations()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = Guid.NewGuid().ToString(), Name = "Fake Organization", Subscription = "free" }
            };

            return Task.FromResult(organizations);
        }

        [HttpPut("organization/{id}")]
        public async Task PutOrganization(string id, [FromBody] Organization organization, CancellationToken ct = default)
        {
            await Organizations.Put(id, organization, ct);
        }

        [HttpPost("organization")]
        public async Task<string> PostOrganization([FromBody] Organization organization, CancellationToken ct = default)
        {
            return await Organizations.Post(organization, ct);
        }

        [HttpGet("organization/{orgId}/apikeys/generate")]
        public async Task<string> GenerateApiKey(string orgId, [FromBody] ApiKey key, CancellationToken ct = default)
        {
            return await ApiKeys.Post(new ApiKey { Organization = orgId }, ct);
        }

        [HttpGet("organization/{orgId}")]
        public async Task<Organization> GetOrganization(string id, CancellationToken ct = default)
        {
            return await Organizations.Get(id, ct);
        }
    }
}
