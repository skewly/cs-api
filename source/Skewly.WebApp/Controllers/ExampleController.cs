using Microsoft.AspNetCore.Mvc;
using Skewly.Providers.elasticsearch;
using Skewly.Common.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Skewly.WebApp.Controllers
{
    public class Example : MultitenantDocument
    {
        public int Number { get; set; }
        public string Name { get; set; }
    };

    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        private readonly IStore<Example> Store;

        public ExampleController(IStore<Example> store)
        {
            Store = store;
        }

        [HttpGet]
        public async Task<Page<Example>> SearchExamples(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            var search = new Search
            {
                Skip = skip,
                Take = take
            };

            return await Store.Search(search, ct);
        }

        [HttpGet("{id}")]
        public async Task<Example> GetExample(string id, CancellationToken ct = default)
        {
            return await Store.Get(id, ct);
        }

        [HttpPost]
        public async Task<Example> CreateExample([FromBody] Example Example, CancellationToken ct = default)
        {
            return await Store.Post(Example, ct);
        }

        [HttpPut("{id}")]
        public async Task UpdateExample(string id, [FromBody] Example data, CancellationToken ct = default)
        {
            await Store.Put(id, data, ct);
        }

        [HttpPatch("{id}")]
        public async Task PatchExample(string id, [FromBody] object patch, CancellationToken ct = default)
        {
            await Store.Patch(id, patch, ct);
        }

        [HttpDelete("{id}")]
        public async Task DeleteExample(string id, CancellationToken ct = default)
        {
            await Store.Delete(id, ct);
        }
    }
}
