using System;

namespace Skewtech.Common
{
    public abstract class AttributeDefinition
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public bool IsMultivalue { get; set; }

        public abstract bool ValidateAttribute(object attribute);
    }
}
