using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using UnitsNet;

namespace Skewly.Common.Tests
{
    [TestClass]
    public class InventoryTests
    {
        [TestMethod]
        public void ValidateAttributesTest()
        {
            var cpuType = new MaterialType
            {
                Id = Guid.NewGuid(),
                Slug = "cpu",
                Name = "CPU",
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new StringAttributeDefinition()
                    {
                        Id = Guid.NewGuid(),
                        Slug = "socket",
                        AllowedValues = new List<string>() { "AM4", "LGA1151", "LGA2011-3" }
                    },
                    new QuantityAttributeDefinition()
                    {
                        Id = Guid.NewGuid(),
                        Slug = "clockSpeed",
                        AllowedQuantityTypes = new List<QuantityType>() { QuantityType.Frequency }
                    },
                    new QuantityAttributeDefinition()
                    {
                        Id = Guid.NewGuid(),
                        Slug = "speed",
                        AllowedQuantityTypes = new List<QuantityType>() { QuantityType.Speed },
                        IsMandatory = true
                    }
                }
            };

            var ryzenCpu = new Material
            {
                Id = Guid.NewGuid(),
                Slug = "amd-ryzen-7-5800x",
                Name = "AMD Ryzen 7 5800x 8-core 16-thread Desktop Processor",
                Attributes = new Dictionary<string, object>()
                {
                    { "socket", "AM4" },
                    { "clockSpeed", Frequency.FromGigahertz(3.4) },
                    { "speed", Speed.FromMilesPerHour(60) }
                }
            };

            var validationResult = cpuType.ValidateAttributes(ryzenCpu.Attributes);

            Assert.IsTrue(validationResult.IsValid);
        }
    }
}
