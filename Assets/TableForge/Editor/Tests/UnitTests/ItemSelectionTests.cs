using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TableForge.Tests
{
    internal class ItemSelectionTests
    {
        [Test]
        public void ScriptableObjectSelection()
        {
            //Arrange
            SampleTestData testData0 = ScriptableObject.CreateInstance<SampleTestData>();
            SampleTestData testData1 = ScriptableObject.CreateInstance<SampleTestData>();
            SupportedTypes testData2 = ScriptableObject.CreateInstance<SupportedTypes>();

            string[] paths = new[]
            {
                $"{PathUtil.GetTestFolderRelativePath()}/MockedData/SampleTestData_0.asset",
                $"{PathUtil.GetTestFolderRelativePath()}/MockedData/SampleTestData_1.asset",
                $"{PathUtil.GetTestFolderRelativePath()}/MockedData/SampleTestData_2.asset"
            };
            
            //Create the scriptableObjects
            AssetDatabase.CreateAsset(testData0, paths[0]);
            AssetDatabase.CreateAsset(testData1, paths[1]);
            AssetDatabase.CreateAsset(testData2, paths[2]);
            
            //Act
            ItemSelector itemSelector = new ScriptableObjectSelector(paths);
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            
            //Assert
            Assert.AreEqual(2, serializedObjects.Count); //2 scriptableObject types
            Assert.AreEqual(2, serializedObjects[0].Count); //2 instances of SampleTestData_0
            Assert.AreEqual(1, serializedObjects[1].Count); //1 instance of SampleTestData_1
            
            //Check if the serialized objects contain the expected data
            Assert.IsTrue(serializedObjects[0].Any(x => x.Name == testData0.name));
            Assert.IsTrue(serializedObjects[0].Any(x => x.Name == testData1.name));
            Assert.AreEqual(testData2.name, serializedObjects[1][0].Name); 
            
            //Cleanup
            AssetDatabase.DeleteAsset(paths[0]);
            AssetDatabase.DeleteAsset(paths[1]);
            AssetDatabase.DeleteAsset(paths[2]);
        }
    }
}

