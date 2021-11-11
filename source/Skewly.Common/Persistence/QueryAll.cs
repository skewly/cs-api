namespace Skewtech.Common.Persistence
{
    public class QueryAll : IQuery
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}
