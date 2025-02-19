using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TableForge.Tests
{
    public class SerializationTests
    {
        [Test]
        public void GetSerializableFields()
        {
            //Arrange
            Type type = typeof(SampleTestData);
            
            //Act
            List<TFFieldInfo> fields = SerializationUtil.GetSerializableFields(type, null);
            
            //Assert
            //Check if the serializable fields are included
            Assert.IsTrue(fields.Any(x => x.Name == "intField"));
            Assert.IsTrue(fields.Any(x => x.Name == "floatField"));
            Assert.IsTrue(fields.Any(x => x.Name == "<PublicIntProperty>k__BackingField"));
            Assert.IsTrue(fields.Any(x => x.Name == "<PublicFloatProperty>k__BackingField"));
            Assert.IsTrue(fields.Any(x => x.Name == "gradient"));
            Assert.IsTrue(fields.Any(x => x.Name == "color"));
            Assert.IsTrue(fields.Any(x => x.Name == "animationCurve"));
            Assert.IsTrue(fields.Any(x => x.Name == "unityObjectReference"));
            Assert.IsTrue(fields.Any(x => x.Name == "nestedData"));
            Assert.IsTrue(fields.Any(x => x.Name == "nestedDataList"));
            Assert.IsTrue(fields.Any(x => x.Name == "intArray"));
            Assert.IsTrue(fields.Any(x => x.Name == "vector2"));
            Assert.IsTrue(fields.Any(x => x.Name == "vector4"));
            Assert.IsTrue(fields.Any(x => x.Name == "vector3Array"));
            Assert.IsTrue(fields.Any(x => x.Name == "sampleEnum"));
            Assert.IsTrue(fields.Any(x => x.Name == "stringToIntDictionary"));
            
            //Check if the fields non-serializable fields are not included
            Assert.IsTrue(fields.All(x => x.Name != "_privateIntField"));
            Assert.IsTrue(fields.All(x => x.Name != "_privateFloatField"));
            Assert.IsTrue(fields.All(x => x.Name != "nestedData3D"));
            Assert.IsTrue(fields.All(x => x.Name != "vector3"));
            Assert.IsTrue(fields.All(x => x.Name != "hiddenDictionary"));
            Assert.IsTrue(fields.All(x => x.Name != "intList2D"));
            Assert.IsTrue(fields.All(x => x.Name != "stringToNestedDataDictionary"));
            Assert.IsTrue(fields.All(x => x.Name != "nestedDataToIntDictionary"));
        }
       
    }
}

