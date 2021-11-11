using System.Collections.Generic;
using System.Linq;

namespace Skewtech.Common
{
    public abstract class AttributeDefinition<T> : AttributeDefinition
    {
        public List<T> AllowedValues { get; set; } = new List<T>();

        public override bool ValidateAttribute(object attribute)
        {
            if(IsMultivalue)
            {
                var collection = (List<T>)attribute;

                return collection.All(a => ValidateAttribute(a));
            }

            var item = (T)attribute;

            return ValidateAttribute(item);
        }

        public virtual bool ValidateAttribute(T attribute)
        {
            return !AllowedValues.Any() || AllowedValues.Contains(attribute);
        }
    }
}
