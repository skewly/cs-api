using Nest;
using Skewly.Providers.elasticsearch;
using ApiKey = Skewly.Common.Models.ApiKey;

namespace Skewly.WebApp.Stores
{
    public class ApiKeyStore : Store<ApiKey>
    {
        public ApiKeyStore(IElasticClient client) : base(client)
        {

        }

        protected override IPromise<IProperties> PropertyMapping(PropertiesDescriptor<ApiKey> descriptor)
        {
            return base.PropertyMapping(descriptor
                .Keyword(s => s
                    .Name(n => n.Organization)
                )
            );
        }
    }
}
