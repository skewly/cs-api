using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Skewly.Common.Persistence
{
    public class EmptyQuery : IQuery
    {
        
    }

    public class TermQuery<T, TValue> : IQuery
    {
        public Expression<Func<T, TValue>> Field { get; set; }
        public TValue Term { get; set; }
    }

    public class TermsQuery<T, TValue> : IQuery
    {
        public Expression<Func<T, TValue>> Field { get; set; }
        public IEnumerable<TValue> Terms { get; set; } = new List<TValue>();
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
