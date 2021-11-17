using Skewly.Common.Persistence;

namespace Skewly.Providers.elasticsearch
{
    public class MultitenantDocument : Document
    {
        public string Organization { get; set; }
    }
}
