using System.Collections.Generic;

namespace Skewly.Common.Persistence
{
    public class QueryBase : IQuery
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }

    public class TermQuery : QueryBase
    {
        public string Field { get; set; }
        public string Term { get; set; }
    }

    public class TermsQuery : QueryBase
    {
        public string Field { get; set; }
        public IEnumerable<string> Terms { get; set; } = new List<string>();
    }

    public class AndQuery : QueryBase
    {
        public IEnumerable<IQuery> Queries { get; set; }

        public AndQuery(IEnumerable<IQuery> queries)
        {
            Queries = queries;
        }
    }
}
