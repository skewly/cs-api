using Skewly.Common.Persistence;

namespace Skewly.Common.Models
{
    public class Organization : Document
    {
        public string Name { get; set; }
        public string Subscription { get; set; }
    }

    public class OrganizationPermission : Document
    {
        public string Organization { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }

    public class ApiKey : Document
    {
        public bool IsDefault { get; set; } = false;
        public string Organization { get; set; }
    }
}
