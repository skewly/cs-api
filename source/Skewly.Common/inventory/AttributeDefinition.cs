using System;

namespace Skewly.Common
{
    public abstract class AttributeDefinition
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public bool IsMultivalue { get; set; }
        public bool IsMandatory { get; set; }

        public abstract ValidationResult ValidateAttribute(object attribute);
    }
}
