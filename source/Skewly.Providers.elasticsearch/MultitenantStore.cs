using Microsoft.AspNetCore.Http;
using Nest;
using Skewly.Common.Models;
using Skewly.Common.Extensions;
using Skewly.Common.Persistence;
using System.Collections.Generic;

namespace Skewly.Providers.elasticsearch
{
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

        protected override IPromise<IProperties> PropertyMapping(PropertiesDescriptor<T> descriptor)
        {
            return base
                .PropertyMapping(descriptor
                    .Keyword(k => k
                        .Name(n => n.Organization)
                    )
                );
        }

        protected override Routing DetermineRouting()
        {
            if(Accessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                return new Routing(organization.Id);
            }

            return base.DetermineRouting();
        }

        protected override QueryContainer QueryContainerDescriptor(QueryContainerDescriptor<T> descriptor, Common.Persistence.IQuery query)
        {
            if(Accessor.HttpContext.Items.TryGetValue<string, Organization>("organization", out var organization))
            {
                var andQuery = new AndQuery(new List<Common.Persistence.IQuery> { query, new Common.Persistence.TermQuery { Field = "organization", Term = organization.Id } });

                return base.QueryContainerDescriptor(descriptor, andQuery);
            }

            return base.QueryContainerDescriptor(descriptor, query);
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
