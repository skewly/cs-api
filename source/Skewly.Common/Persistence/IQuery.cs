namespace Skewly.Common.Persistence
{
    public interface ISearch
    {
        IQuery Query { get; set; }
        int Skip { get; set; }
        int Take { get; set; }
    }

    public class Search : ISearch
    {
        public IQuery Query { get; set; } = new EmptyQuery();
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }

    public interface IQuery
    {
        
    }
}
