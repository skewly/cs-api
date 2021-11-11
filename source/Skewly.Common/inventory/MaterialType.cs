using System;
using System.Collections.Generic;
using System.Linq;

namespace Skewtech.Common
{
    public class MaterialType
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }

        public List<AttributeDefinition> AttributeDefinitions { get; set; }

        public bool ValidateAttributes(IDictionary<string,object> attributes)
        {
            var isValid = true;

            foreach(var attribute in attributes)
            {
                var definition = AttributeDefinitions.FirstOrDefault(d => d.Slug.Equals(attribute.Key));

                if(definition != default)
                {
                    isValid &= definition.ValidateAttribute(attribute.Value);
                }
                else
                {
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}
