namespace Skewly.Common.Persistence
{
    public interface IQuery
    {
        int Skip { get; set; }
        int Take { get; set; }
    }
}
