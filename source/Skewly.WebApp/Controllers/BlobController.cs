using Microsoft.AspNetCore.Mvc;
using Skewtech.Common.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Skewly.WebApp.Controllers
{
    public class Blob
    {
        public int Number { get; set; }
        public string Name { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IStore<Blob> Store;

        public BlobController(StoreFactory storeFactory)
        {
            Store = storeFactory.BuildStore<Blob>(true);
        }

        [HttpGet]
        public async Task<Page<Blob>> GetBlobs(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            var query = new QueryAll
            {
                Skip = skip,
                Take = take
            };

            return await Store.Get(query, ct);
        }

        [HttpGet("{id}")]
        public async Task<Blob> GetBlob(string id, CancellationToken ct = default)
        {
            return await Store.Get(id, ct);
        }

        [HttpPost]
        public async Task<string> CreateBlob([FromBody] Blob blob, CancellationToken ct = default)
        {
            return await Store.Post(blob, ct);
        }

        [HttpPut("{id}")]
        public async Task UpdateBlob(string id, [FromBody] Blob data, CancellationToken ct = default)
        {
            await Store.Put(id, data, ct);
        }

        [HttpPatch("{id}")]
        public async Task PatchBlob(string id, [FromBody] object patch, CancellationToken ct = default)
        {
            await Store.Patch(id, patch, ct);
        }

        [HttpDelete("{id}")]
        public async Task DeleteBlob(string id, CancellationToken ct = default)
        {
            await Store.Delete(id, ct);
        }
    }
}
