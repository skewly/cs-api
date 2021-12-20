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

    public class PatchApiKey
    {
        public bool IsDefault { get; set; } = false;
    }

    [ApiController]
    [Route("organizations")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IStore<ApiKey> ApiKeys;
        private readonly IStore<OrganizationPermission> OrganizationPermissions;
        private readonly IStore<Organization> Organizations;

        public OrganizationsController(IStore<ApiKey> apiKeys, IStore<OrganizationPermission> organizationPermissions, IStore<Organization> organizations)
        {
            ApiKeys = apiKeys;
            OrganizationPermissions = organizationPermissions;
            Organizations = organizations;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganization body, CancellationToken ct = default)
        {
            var dto = new Organization
            {
                Name = body.Name,
                Subscription = "free"
            };

            var organization = await Organizations.Post(dto, ct);

            var organizationPermission = new OrganizationPermission
            {
                Organization = organization.Id,
                Username = User.Identity.Name,
                Role = "owner"
            };

            _ = await OrganizationPermissions.Post(organizationPermission, ct);

            _ = await ApiKeys.Post(new ApiKey { Organization = organization.Id, IsDefault = true }, ct);

            return new CreatedResult(new Uri($"/access/organizations/{organization.Id}", UriKind.Relative), organization);
        }

        /// <summary>
        /// This should be removed to avoid allowing people to create organizations
        /// </summary>
        [HttpPut("{id}")]
        public async Task PutOrganization(string id, [FromBody] Organization organization, CancellationToken ct = default)
        {
            await Organizations.Put(id, organization, ct);
        }

        [HttpGet]
        public async Task<Page<Organization>> GetOrganizations(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            var permissionsQuery = new AndQuery(new List<IQuery> {
                new TermQuery<OrganizationPermission, string>
                {
                    Field = f => f.Username,
                    Term = User.Identity.Name
                },
                new TermsQuery<OrganizationPermission, string>
                {
                    Field = f => f.Role,
                    Terms = new List<string> { "owner", "admin" }
                }
            });

            var permissions = await OrganizationPermissions.Search(new Search { Query = permissionsQuery, Skip = skip, Take = take }, ct);

            var organizationIds = permissions.Results.Select(p => p.Organization);

            var organizationsQuery = new TermsQuery<Organization, string>
            {
                Field = f => f.Id,
                Terms = organizationIds
            };

            var organizations = await Organizations.Search(new Search { Query = organizationsQuery, Skip = skip, Take = take }, ct);

            return organizations;
        }

        [HttpGet("{orgId}")]
        public async Task<Organization> GetOrganization(string id, CancellationToken ct = default)
        {
            return await Organizations.Get(id, ct);
        }

        [HttpGet("{orgId}/apikeys")]
        public async Task<Page<ApiKey>> GetApiKeys(string orgId, int skip = 0, int take = 50, CancellationToken ct = default)
        {
            // Should validate that the current user has permissions for the requested organization

            var search = new Search
            {
                Query = new AndQuery(new List<IQuery> { new TermQuery<ApiKey, string> { Field = f => f.Organization, Term = orgId } }),
                Skip = skip,
                Take = take
            };

            var response = await ApiKeys.Search(search, ct);

            return response;
        }

        [HttpGet("{orgId}/apikeys/generate")]
        public async Task<ApiKey> GenerateApiKey(string orgId, CancellationToken ct = default)
        {
            return await ApiKeys.Post(new ApiKey { Organization = orgId }, ct);
        }

        [HttpPatch("{orgId}/apikeys/{apiKeyId}")]
        public async Task SetDefaultApiKey(string orgId, string apiKeyId, CancellationToken ct = default)
        {
            var defaultKeySearch = new Search
            {
                Query = new AndQuery(new List<IQuery> { new TermQuery<ApiKey, string> { Field = f => f.Organization, Term = orgId }, new TermQuery<ApiKey, bool> { Field = f => f.IsDefault, Term = true } }),
                Take = 1
            };

            var defaultKeyResponse = await ApiKeys.Search(defaultKeySearch, ct);

            var defaultKey = defaultKeyResponse.Results.FirstOrDefault();

            if (defaultKey != default)
            {
                await ApiKeys.Patch(defaultKey.Id, new PatchApiKey { IsDefault = false }, ct);
            }

            await ApiKeys.Patch(apiKeyId, new PatchApiKey { IsDefault = true }, ct);
        }
    }
}
