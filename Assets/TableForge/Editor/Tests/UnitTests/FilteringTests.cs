using System.Collections.Generic;
using NUnit.Framework;
using TableForge.DataStructures;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Tests
{
    internal class FilteringTests
    {
        private TableControl _tableControl;
        private List<string> _rowGuids;
        private List<string> _rowPaths;

        private (TableControl, List<string>, List<string>) GetTableControl(int rowCount)
        {
            var tableAttributes = new TableAttributes()
            {
                tableType = TableType.Dynamic,
                columnReorderMode = TableReorderMode.ExplicitReorder,
                rowReorderMode = TableReorderMode.ExplicitReorder,
                columnHeaderVisibility = TableHeaderVisibility.ShowHeaderName,
                rowHeaderVisibility = TableHeaderVisibility.ShowHeaderName,
            };

            VisualElement rootVisualElement = new VisualElement();
            TableControl tableControl = new TableControl(rootVisualElement, tableAttributes, null, null, null);
            rootVisualElement.Add(tableControl);
            
            List<string> rowGuids = new List<string>();
            List<string> rowPaths = new List<string>();
            for (int i = 0; i < rowCount; i++)
            {
                string path = $"{PathUtil.GetTestFolderRelativePath()}/TestMockedData/FilteringData{i}.asset";
                FilteringTestData existingData = AssetDatabase.LoadAssetAtPath<FilteringTestData>(path);
                FilteringTestData data = existingData != null ? existingData : ScriptableObject.CreateInstance<FilteringTestData>();
                
                data.stringValue = i % 2 == 0 ? $"My Row {i}" : $"Item {i}";
                data.intValue = i;
                data.floatValue = i * 1.5f;
                data.boolValue = i % 2 == 0;
                data.enumValue = (TestEnum)(i % 3);
                
                data.stringList = i % 2 == 0 ? new List<string> { $"String {i}", $"Another String {i}" }
                    : new List<string> { $"Item String {i}", $"String {i-1}", $"String {i+1}", $"Another String {i-1}" };
                
                data.intList = new List<int> { i, i + 1, i + 2 };
                data.intStringDictionary = new SerializedDictionary<int, string>
                {
                    { i, $"Dictionary Item {i}" },
                    { i + 1, $"Another Dictionary Item {i + 1}" }
                };
                
                data.serializedFilteringTestData = new SerializedFilteringTestData
                {
                    stringValue = i % 2 == 0 ? $"Serialized Row {i}" : $"Serialized Item {i}",
                    intValue = i * 10,
                    floatValue = i * 2.5f,
                    boolValue = i % 2 != 0,
                    enumValue = (TestEnum)(i % 3),
                    stringList = new List<string> { $"Serialized String {i}", $"Another Serialized String {i}" },
                    intList = new List<int> { i * 2, i * 3, i * 4 },
                    intStringDictionary = new SerializedDictionary<int, string>
                    {
                        { i, $"Serialized Dictionary Item {i}" },
                        { i + 1, $"Another Serialized Dictionary Item {i + 1}" }
                    }
                };
                
                if(existingData == null)
                    AssetDatabase.CreateAsset(data, path);
                rowGuids.Add(AssetDatabase.AssetPathToGUID(path));
                rowPaths.Add(path);
            }
            
            TableMetadata tableMetadata = TableMetadataManager.GetMetadata(rowGuids, "FilteringTestTable", $"{PathUtil.GetTestFolderRelativePath()}/MockedData");
            tableControl.SetTable(TableMetadataManager.GetTable(tableMetadata), metadata:tableMetadata);

            return (tableControl, rowGuids, rowPaths);
        }
        
        [SetUp]
        public void Setup()
        {
            (_tableControl, _rowGuids, _rowPaths) = GetTableControl(5);
        }

        [Test]
        public void FilterWithEmptyInput_ShouldShowAllRows()
        {
            _tableControl.Filterer.Filter("");
            Assert.AreEqual(0, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByGuid_ShouldMatchSingleRow()
        {
            _tableControl.Filterer.Filter($"g:{_rowGuids[0]}");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByPath_WithFullPath_ShouldMatchSingleRow()
        {
            // Arrange
            string shortPath = _rowPaths[0].Replace("Assets/", "");

            // Act
            _tableControl.Filterer.Filter($"path:{shortPath}");

            // Assert
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByPath_WithFolder_ShouldMatchMultipleRows()
        {
            // Arrange
            string folderPath = PathUtil.GetTestFolderRelativePath() + "/MockedData";

            // Act
            _tableControl.Filterer.Filter($"path:{folderPath}");

            // Assert
            Assert.AreEqual(0, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByName_ShouldMatchRowsContaining()
        {
            _tableControl.Filterer.Filter("n:Filtering");
            Assert.AreEqual(0, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByName_MatchRowFullName()
        {
            _tableControl.Filterer.Filter("n:FilteringData0");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByProperty_IntEqual_ShouldMatchSingleRow()
        {
            _tableControl.Filterer.Filter("p:Int Value=2");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByProperty_FloatGreaterThan_ShouldMatchMultipleRows()
        {
            _tableControl.Filterer.Filter("p:Float Value>3.0");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByProperty_BoolEqual_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Bool Value=true");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByProperty_StringContains_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:String Value~=Row");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByProperty_WithColumnLetter_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:$B>2");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByNestedProperty_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Serialized Filtering Test Data.Int Value>=20");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByProperty_WithNotContains_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:String Value!~Row");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByBooleanAnd_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:String Value ~= My Row & p:Int Value<2");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByBooleanOr_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:String Value ~= My Row | p:Int Value=1");
            Assert.AreEqual(1, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByBooleanWithParentheses_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("(p:String Value ~= My Row | p:String Value ~= Item) & p:Float Value<4,1");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByEnum_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Enum Value=Value1");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByNestedString_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Serialized Filtering Test Data.String Value~=Serialized Row");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByMultipleConditions_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int Value>=2 & p:Float Value<6.0 & p:Bool Value=true");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterWithComplexExpression_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("(p:String Value ~= My Row & p:Int Value>0) | (p:Serialized Filtering Test Data.Int Value=10)");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByDeeplyNestedProperty_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Serialized Filtering Test Data.String List~=Another Serialized String 0");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByDictionaryProperty_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int String Dictionary.Value~=Dictionary Item 0");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterWithInvalidSyntax_ShouldShowAllRows()
        {
            _tableControl.Filterer.Filter("invalid:syntax");
            Assert.AreEqual(0, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByListLength_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:String List = 2");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByListContainsManualList_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int List ~= [1, 2]");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByListContainsItem_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int List ~= 1");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByListCountGreaterThan_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:String List>2");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }

        [Test]
        public void FilterByListNotContains_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int List!~1");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }

        [Test]
        public void FilterByListDeepEquality_ShouldMatchSingleRow()
        {
            _tableControl.Filterer.Filter("p:Int List=[0,1,2]");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }

        [Test]
        public void FilterByListDeepEquality_FailsForDifferentOrder()
        {
            _tableControl.Filterer.Filter("p:Int List=[2,1,0]");
            Assert.AreEqual(5, _tableControl.Filterer.HiddenRows.Count);
        }

        [Test]
        public void FilterByListNotEqual_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int List!=[0,1,2]");
            Assert.AreEqual(1, _tableControl.Filterer.HiddenRows.Count);
        }

        [Test]
        public void FilterByNotOperator_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("!(p:Int Value=0)");
            Assert.AreEqual(1, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByNestedPropertyWithColumnLetter_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Serialized Filtering Test Data.$A>20");
            Assert.AreEqual(3, _tableControl.Filterer.HiddenRows.Count);
        }

        [Test]
        public void FilterByDictionaryKey_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:Int String Dictionary.Key~=0");
            Assert.AreEqual(4, _tableControl.Filterer.HiddenRows.Count);
        }
        
        [Test]
        public void FilterByLiteralListNotContains_ShouldMatchRows()
        {
            _tableControl.Filterer.Filter("p:[1, 2] !~ Int Value");
            Assert.AreEqual(2, _tableControl.Filterer.HiddenRows.Count);
        }

    }
}