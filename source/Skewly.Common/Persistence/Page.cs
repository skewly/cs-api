using System.Collections.Generic;

namespace Skewly.Common.Persistence
{
    public class Page<T> where T : Document, new()
    {
        public IEnumerable<T> Results { get; set; } = new List<T>();
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
        public long Total { get; set; } = 0;
    }
}
