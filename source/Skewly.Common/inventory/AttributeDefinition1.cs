using System.Collections.Generic;
using System.Linq;

namespace Skewly.Common
{
    public abstract class AttributeDefinition<T> : AttributeDefinition
    {
        public List<T> AllowedValues { get; set; } = new List<T>();

        public override ValidationResult ValidateAttribute(object attribute)
        {
            var result = new ValidationResult();

            if (IsMultivalue)
            {
                var collection = (List<T>)attribute;

                if(IsMandatory && !collection.Any())
                {
                    result.Errors.Add($"{Slug} is marked mandatory, but no value(s) were found.");
                }

                foreach (var errors in collection.Select(a => ValidateAttribute(a).Errors))
                {
                    result.Errors.AddRange(errors);
                }
            }
            else
            {
                var item = (T)attribute;

                if (IsMandatory && item == null)
                {
                    result.Errors.Add($"{Slug} is marked mandatory, but no value(s) were found.");
                }
                else
                {
                    result.Errors.AddRange(ValidateAttribute(item).Errors);
                }
            }

            return result;
        }

        public virtual ValidationResult ValidateAttribute(T attribute)
        {
            var result = new ValidationResult();

            if (AllowedValues.Any() && !AllowedValues.Contains(attribute))
            {
                result.Errors.Add($"{Slug} value doesn't match the allowed value(s).");
            }

            return result;
        }
    }
}
