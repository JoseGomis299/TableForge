using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TableForge.Tests
{
    internal class TableGenerationTests
    {
        [Test]
        public void NoSerializableTypes_NoCellsGenerated()
        {
            // Arrange
            NonSupportedTypes testData = ScriptableObject.CreateInstance<NonSupportedTypes>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/FallbackScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "FallbackTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); 
            Assert.AreEqual(1, serializedObjects[0].Count); 
            
            Assert.AreEqual(0, table.Rows[1].Cells.Count); 

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }
        
        [Test]
        public void AnimationCurveCellGeneration()
        {
            // Arrange
            AnimationCurveScriptableObject testData = ScriptableObject.CreateInstance<AnimationCurveScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/AnimationCurveScriptableObject.asset";
            
            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "AnimationCurveTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of AnimationCurveScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(AnimationCurveCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void BooleanCellGeneration()
        {
            // Arrange
            BooleanScriptableObject testData = ScriptableObject.CreateInstance<BooleanScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/BooleanScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "BooleanTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of BooleanScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(BoolCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void ColorCellGeneration()
        {
            // Arrange
            ColorScriptableObject testData = ScriptableObject.CreateInstance<ColorScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/ColorScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "ColorTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of ColorScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(ColorCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void EnumCellGeneration()
        {
            // Arrange
            EnumScriptableObject testData = ScriptableObject.CreateInstance<EnumScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/EnumScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "EnumTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of EnumScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(EnumCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void FloatingPointCellGeneration()
        {
            // Arrange
            FloatingPointScriptableObject testData = ScriptableObject.CreateInstance<FloatingPointScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/FloatingPointScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "FloatingPointTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of FloatingPointScriptableObject

            // Check that the cell types for the first row are correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(FloatCell), firstRow.Cells[1].GetType());
            Assert.AreEqual(typeof(DoubleCell), firstRow.Cells[2].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void GradientCellGeneration()
        {
            // Arrange
            GradientScriptableObject testData = ScriptableObject.CreateInstance<GradientScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/GradientScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "GradientTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of GradientScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(GradientCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void IntegralTypesCellGeneration()
        {
            // Arrange
            IntegralTypesScriptableObject testData = ScriptableObject.CreateInstance<IntegralTypesScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/IntegralTypesScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "IntegralTypesTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of IntegralTypesScriptableObject

            // Check that the cell types for the first row are correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(IntCell), firstRow.Cells[1].GetType());
            Assert.AreEqual(typeof(LongCell), firstRow.Cells[2].GetType());
            Assert.AreEqual(typeof(ULongCell), firstRow.Cells[3].GetType());
            Assert.AreEqual(typeof(UIntCell), firstRow.Cells[4].GetType());
            Assert.AreEqual(typeof(ShortCell), firstRow.Cells[5].GetType());
            Assert.AreEqual(typeof(UShortCell), firstRow.Cells[6].GetType());
            Assert.AreEqual(typeof(ByteCell), firstRow.Cells[7].GetType());
            Assert.AreEqual(typeof(SByteCell), firstRow.Cells[8].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void UnityObjectCellGeneration()
        {
            // Arrange
            UnityObjectScriptableObject testData = ScriptableObject.CreateInstance<UnityObjectScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/UnityObjectScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "UnityObjectTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of UnityObjectScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(ReferenceCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void StringCellGeneration()
        {
            // Arrange
            StringScriptableObject testData = ScriptableObject.CreateInstance<StringScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/StringScriptableObject.asset";

            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);

            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "StringTable", null);

            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of StringScriptableObject

            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(StringCell), firstRow.Cells[1].GetType());

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void ListCellGeneration()
        {
            // Arrange
            ListScriptableObject testData = ScriptableObject.CreateInstance<ListScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/ListScriptableObject.asset";
            
            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);
            
            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "ListTable", null);
            
            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of ListScriptableObject
            
            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(ListCell), firstRow.Cells[1].GetType()); // int[]
            Assert.AreEqual(typeof(ListCell), firstRow.Cells[2].GetType()); // List<int>
            
            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }
        
        [Test]
        public void DictionaryCellGeneration()
        {
            // Arrange
            DictionaryScriptableObject testData = ScriptableObject.CreateInstance<DictionaryScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/DictionaryScriptableObject.asset";
            
            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);
            
            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "DictionaryTable", null);
            
            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of DictionaryScriptableObject
            
            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(DictionaryCell), firstRow.Cells[1].GetType());
            
            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void SubitemCellGeneration()
        {
            // Arrange
            ComplexTypeScriptableObject testData = ScriptableObject.CreateInstance<ComplexTypeScriptableObject>();
            string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/ComplexTypeScriptableObject.asset";
            
            // Create the scriptableObject
            AssetDatabase.CreateAsset(testData, path);
            
            // Act
            ItemSelector itemSelector = new ScriptableObjectSelector(new[] { path });
            List<List<ITFSerializedObject>> serializedObjects = itemSelector.GetItemData();
            var table = TableGenerator.GenerateTable(serializedObjects[0], "ComplexTypeTable", null);
            
            // Assert
            Assert.AreEqual(1, serializedObjects.Count); // 1 scriptableObject type
            Assert.AreEqual(1, serializedObjects[0].Count); // 1 instance of ComplexTypeScriptableObject
            
            // Check that the cell type for the first row is correct
            var firstRow = table.Rows[1];
            Assert.AreEqual(typeof(SubItemCell), firstRow.Cells[1].GetType());
            
            //Check that the subtable was created correctly
            var subtable = ((SubTableCell)firstRow.Cells[1]).SubTable;
            Assert.AreEqual(1, subtable.Rows.Count);
            Assert.IsTrue(subtable.Columns[1].Name == "Number");
            Assert.IsTrue(subtable.Columns[2].Name == "Text");
            
            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

    }
}

