using System.Threading;
using System.Threading.Tasks;

namespace Skewly.Common.Persistence
{
    public interface IStore<T> where T : Document, new()
    {
        Task<Page<T>> Get(IQuery query, CancellationToken ct = default); 
        Task<T> Get(string id, CancellationToken ct = default);
        Task Put(string id, T data, CancellationToken ct = default);
        Task<string> Post(T data, CancellationToken ct = default);
        Task Patch<TPatch>(string id, TPatch data, CancellationToken ct = default) where TPatch : class;
        Task Delete(string id, CancellationToken ct = default);
    }
}
