using System.Collections.Generic;

namespace Skewtech.Common.Persistence
{
    public class Page<T> where T : class, new()
    {
        public IEnumerable<WrappedData<T>> Results { get; set; } = new List<WrappedData<T>>();
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
        public long Total { get; set; } = 0;
    }
}
