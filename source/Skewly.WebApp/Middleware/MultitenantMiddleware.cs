using Microsoft.AspNetCore.Http;
using Skewly.Common.Models;
using Skewly.Common.Persistence;
using System;
using System.Threading.Tasks;

namespace Skewly.WebApp.Middleware
{
    public class MultitenantMiddleware
    {
        private readonly RequestDelegate _next;

        public MultitenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IStore<ApiKey> apiKeys, IStore<Organization> organizations)
        {
            try
            {
                var apiKeyId = httpContext.Request.Headers["ApiKey"];

                var apiKey = await apiKeys.Get(apiKeyId);

                var organization = await organizations.Get(apiKey.Organization);

                httpContext.Items.Add("organization", organization);
            }
            catch(Exception ex)
            {
                // Fail silently
            }

            await _next(httpContext);
        }
    }
}
