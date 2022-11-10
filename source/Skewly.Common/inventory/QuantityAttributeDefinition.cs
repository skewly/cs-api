using System.Collections.Generic;
using System.Linq;
using UnitsNet;

namespace Skewly.Common
{
    public class QuantityAttributeDefinition : AttributeDefinition<IQuantity>
    {
        public List<QuantityType> AllowedQuantityTypes { get; set; } = new List<QuantityType>();

        public override ValidationResult ValidateAttribute(IQuantity attribute)
        {
            var result = base.ValidateAttribute(attribute);

            if (result.IsValid && AllowedQuantityTypes.Any() && !AllowedQuantityTypes.Contains(attribute.Type))
            {
                result.Errors.Add($"{Slug} quantity type ({attribute.Type}) doesn't match the allowed quantity type(s).");
            }

            return result;
        }
    }
}
