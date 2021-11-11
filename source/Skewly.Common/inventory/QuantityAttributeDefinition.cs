using System.Collections.Generic;
using UnitsNet;

namespace Skewtech.Common
{
    public class QuantityAttributeDefinition : AttributeDefinition<IQuantity>
    {
        public List<QuantityType> AllowedQuantityTypes { get; set; } = new List<QuantityType>();
    }
}
