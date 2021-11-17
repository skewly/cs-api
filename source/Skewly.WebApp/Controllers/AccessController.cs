using Microsoft.AspNetCore.Mvc;
using Skewly.Common.Persistence;
using System.Threading;
using System.Threading.Tasks;
using Skewly.Common.Models;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Skewly.WebApp.Controllers
{
    public class CreateOrganization
    {
        public string Name { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly IStore<ApiKey> ApiKeys;
        private readonly IStore<OrganizationPermission> OrganizationPermissions;
        private readonly IStore<Organization> Organizations;

        public AccessController(IStore<ApiKey> apiKeys, IStore<OrganizationPermission> organizationPermissions, IStore<Organization> organizations)
        {
            ApiKeys = apiKeys;
            OrganizationPermissions = organizationPermissions;
            Organizations = organizations;
        }

        [HttpPost("organizations"), Authorize]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganization body, CancellationToken ct = default)
        {
            var organization = new Organization
            {
                Name = body.Name,
                Subscription = "free"
            };

            // Refactor IStore.Post() to return object instead of just the id
            var organizationId = await Organizations.Post(organization, ct);

            var organizationPermission = new OrganizationPermission
            {
                Organization = organizationId,
                Username = User.Identity.Name,
                Role = "owner"
            };

            _ = await OrganizationPermissions.Post(organizationPermission, ct);

            return new CreatedResult(new Uri($"/access/organizations/{organizationId}", UriKind.Relative), organization);
        }

        [Authorize, HttpGet("organizations")]
        public async Task<Page<Organization>> GetOrganizations(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            var permissionsQuery = new AndQuery(new List<IQuery> {
                new TermQuery
                {
                    Field = "username",
                    Term = User.Identity.Name
                },
                new TermsQuery
                {
                    Field = "role",
                    Terms = new List<string> { "owner", "admin" }
                }
            });

            var permissions = await OrganizationPermissions.Search(new Search { Query = permissionsQuery, Skip = skip, Take = take }, ct);

            var organizationIds = permissions.Results.Select(p => p.Organization);

            var organizationsQuery = new TermsQuery
            {
                Field = "id",
                Terms = organizationIds
            };

            var organizations = await Organizations.Search(new Search { Query = organizationsQuery, Skip = skip, Take = take }, ct);

            return organizations;
        }

        [HttpPut("organization/{id}")]
        public async Task PutOrganization(string id, [FromBody] Organization organization, CancellationToken ct = default)
        {
            await Organizations.Put(id, organization, ct);
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
