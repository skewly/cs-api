using System.Collections.Generic;

namespace Skewly.Common.Persistence
{
    public class EmptyQuery : IQuery
    {
        
    }

    public class TermQuery : IQuery
    {
        public string Field { get; set; }
        public string Term { get; set; }
    }

    public class TermsQuery : IQuery
    {
        public string Field { get; set; }
        public IEnumerable<string> Terms { get; set; } = new List<string>();
    }

    public class AndQuery : IQuery
    {
        public IEnumerable<IQuery> Queries { get; set; }

        public AndQuery(IEnumerable<IQuery> queries)
        {
            Queries = queries;
        }
    }
}
