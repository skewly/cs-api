using Skewly.Common.Persistence;

namespace Skewly.Common.Models
{
    public class Organization : Document
    {
        public string Name { get; set; }
        public string Subscription { get; set; }
    }

    public class ApiKey : Document
    {
        public string Organization { get; set; }
    }
}
