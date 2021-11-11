using System;
using System.Collections.Generic;

namespace Skewtech.Common
{
    public class Material
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }

        public IDictionary<string,object> Attributes { get; set; }
    }
}
