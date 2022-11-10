using System;
using System.Collections.Generic;
using System.Linq;

namespace Skewly.Common
{
    public class MaterialType
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }

        public List<AttributeDefinition> AttributeDefinitions { get; set; }

        public ValidationResult ValidateAttributes(IDictionary<string,object> attributes)
        {
            var result = new ValidationResult();

            foreach(var definition in AttributeDefinitions)
            {                
                if(!attributes.TryGetValue(definition.Slug, out var attribute))
                {
                    attribute = null;
                }

                result.Errors.AddRange(definition.ValidateAttribute(attribute).Errors);
            }

            return result;
        }
    }
}
