using System.Collections.Generic;
using NUnit.Framework;
using TableForge.Editor;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Tests
{
    internal class ExcelFunctionsTests
    {
        private TableControl _tableControl;
        private List<string> _rowGuids;
        private List<string> _rowPaths;
        private List<ExcelFunctionTestData> _createdData = new List<ExcelFunctionTestData>();

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
            for (int i = 1; i <= rowCount; i++)
            {
                ExcelFunctionTestData data = ScriptableObject.CreateInstance<ExcelFunctionTestData>();
                _createdData.Add(data);
                
                data.stringValue1 = $"String Value {i}";
                data.intValue = i;
                data.floatValue = i * 1.5f;
                data.boolValue = i % 2 == 0;
                data.doubleValue = i * 2.5;
                data.longValue = i * 1000L;
                data.ulongValue = (ulong)((1 << i) + int.MaxValue);
                
                data.innerData = new SerializedExcelFunctionTestData()
                {
                    stringValue1 = $"Inner String Value {i}",
                    intValue = i * 10,
                    floatValue = i * 2.5f,
                    boolValue = i % 3 == 0,
                    doubleValue = i * 3.5,
                    longValue = i * 2000L,
                    ulongValue = (ulong)((2 << i) + int.MaxValue),
                    innerData = new SerializedExcelFunctionTestData2()
                    {
                        stringValue1 = $"Nested Inner String Value {i}",
                        stringValue2 = null,
                        intValue = i,
                        floatValue = i * 4.5f,
                    }
                };
                
                data.innerData2 = new SerializedExcelFunctionTestData()
                {
                    stringValue1 = $"Inner String Value 2 {i}",
                    intValue = i * 20,
                    floatValue = i * 3.5f,
                    boolValue = i % 4 == 0,
                    doubleValue = i * 4.5,
                    longValue = i * 3000L,
                    ulongValue = (ulong)(1 << (i + 34)),
                    innerData = new SerializedExcelFunctionTestData2()
                    {
                        stringValue1 = $"Nested Inner String Value {i}",
                        stringValue2 = $"Nested Inner String Value 2 {i}",
                        intValue = i * 2,
                        floatValue = i * 6.5f,
                    }
                };
                
                data.innerDataArray = new SerializedExcelFunctionTestData[]
                {
                   (SerializedExcelFunctionTestData) data.innerData.CreateShallowCopy(),
                   (SerializedExcelFunctionTestData) data.innerData2.CreateShallowCopy()
                };
                
                data.innerDataArray2 = new SerializedExcelFunctionTestData[]
                {
                     (SerializedExcelFunctionTestData) data.innerData.CreateShallowCopy(),
                     (SerializedExcelFunctionTestData) data.innerData2.CreateShallowCopy()
                };
                
                string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/FilteringData{i}.asset";
                AssetDatabase.CreateAsset(data, path);
                rowGuids.Add(AssetDatabase.AssetPathToGUID(path));
                rowPaths.Add(path);
            }
            
            TableMetadata tableMetadata = TableMetadataManager.CreateMetadata(rowGuids, "TestTable", PathUtil.GetTestFolderRelativePath());
            tableControl.SetTable(TableMetadataManager.GetTable(tableMetadata));

            return (tableControl, rowGuids, rowPaths);
        }
        
        [SetUp]
        public void Setup()
        {
            (_tableControl, _rowGuids, _rowPaths) = GetTableControl(5);
        }
        
        [TearDown]
        public void Teardown()
        {
            DeleteAssociatedAssets();
            _createdData.Clear();
        }
        
        private void DeleteAssociatedAssets()
        {
            foreach (var data in _createdData)
            {
                if (data != null)
                {
                    string path = AssetDatabase.GetAssetPath(data);
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }
            }
            AssetDatabase.Refresh();
        }
        
        [Test]
        public void Sum_NumericValues()
        {
            string function = "=SUM($C$1;C$5;$C3;10;2;C1:D5;SUM(5);SUM(2;C1))";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; //E1 (double value cell) 
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(66.5, resultCell.GetValue()); //1 + 5 + 3 + 10 + 2 + (1+2+3+4+5+1.5+3+4.5+6+7.5) + 5 + (2+1) = 66.5
        }
        
        [Test]
        public void Sum_NestedFunctions()
        {
            string function = "=SUM(SUM($C$1;C$5;$C3;10;2;C1:D5);SUM(5;SUM(10));SUM(2;C1))";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; //E1 (double value cell) 
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(76.5, resultCell.GetValue()); //1 + 5 + 3 + 10 + 2 + (1+2+3+4+5+1.5+3+4.5+6+7.5) + (5+10) + (2+1) = 76.5
        }
        
        [Test]
        public void Sum_InvalidValues()
        {
            string function = "=SUM(C1;\"Invalid\")";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; //E1 (double value cell) 
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            double initialValue = (double) resultCell.GetValue();
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Invalid arguments for function 'SUM'");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Function evaluation failed for input: {function}");
            Assert.AreEqual(initialValue, resultCell.GetValue()); //Invalid argument makes the cell maintain its original value
        }
        
        [Test]
        public void Average_NumericValues()
        {
            string function = "=AVERAGE(C1:C5;SUM(5);SUM(1;C1);C2)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; //E1 (double value cell) 
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(3, resultCell.GetValue()); //Sum is 25, average is 24 / 8 = 3
        }
        
        [Test]
        public void Count_NonEmptyCells()
        {
            string function = "=COUNT(C1:C5;D1:D5)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(10, resultCell.GetValue()); // 5 int + 5 float = 10
        }

        [Test]
        public void Count_MixedTypes()
        {
            string function = "=COUNT(C1;D1;E1;F1;G1;H1;I1)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(7, resultCell.GetValue()); // All 7 values exist
        }

        [Test]
        public void CountIf_WithCriteria()
        {
            string function = "=COUNTIF(C1:C5; \">2\")";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(3, resultCell.GetValue()); // Values 3,4,5 meet condition
        }

        [Test]
        public void CountIf_WithTextCriteria()
        {
            string function = "=COUNTIF(A1:A5; \"=String Value 1\")";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1, resultCell.GetValue()); // Only "String Value 1"
        }

        [Test]
        public void If_SimpleCondition()
        {
            string function = "=IF(C1>2; \"Yes\"; \"No\")";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[1]; // A1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual("No", resultCell.GetValue()); // 1 is not >2
        }

        [Test]
        public void If_NestedCondition()
        {
            string function = "=IF(C1<5; IF(2>C1; \"Mid\"; \"Low\"); \"High\")";
            Cell resultCell = _tableControl.TableData.Rows[4].Cells[1]; // A4 (i=4)
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual("Mid", resultCell.GetValue()); // 1 is <5 and <2
        }

        [Test]
        public void Max_NumericValues()
        {
            string function = "=MAX(C1:C5; D1:D5; E1; 100)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(100, resultCell.GetValue());
        }

        [Test]
        public void Max_WithNestedFunctions()
        {
            string function = "=MAX(SUM(C1:C2); MIN(C4:C5); 10)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(10, resultCell.GetValue()); // SUM=3, MIN=4, MAX=10
        }

        [Test]
        public void Min_NumericValues()
        {
            string function = "=MIN(C1:C5; D1:D5; E1; -10)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(-10, resultCell.GetValue());
        }

        [Test]
        public void Min_WithMixedReferences()
        {
            string function = "=MIN(C$1; $C3; C1:C5; I1.C1:I5.C1)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1, resultCell.GetValue()); // All values >=1
        }

        [Test]
        public void SumIf_BasicCondition()
        {
            string function = "=SUMIF(C1:C5; \">2\"; D1:D5)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(18.0, resultCell.GetValue()); // 4.5 + 6.0 + 7.5 = 18.0
        }

        [Test]
        public void SumIf_WithBoolComparison()
        {
            string function = "=SUMIF(H1:H5; \"true\"; C1:C5)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(6.0, resultCell.GetValue()); // Even rows: 2+4=6
        }
        
        [Test]
        public void SumIf_WithTwoArguments()
        {
            string function = "=SUMIF(C1:C5; \"<=2\")";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(3.0, resultCell.GetValue()); // 1 + 2 = 3.0
        }

        [Test]
        public void SumIf_WithTextCondition()
        {
            string function = "=SUMIF(A1:A5; \"=String Value 1\"; C1:C5)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1, resultCell.GetValue()); // Only first row
        }

        [Test]
        public void Complex_NestedFunctions()
        {
            string function = "=SUM(COUNTIF(C1:C5; \">2\"); MAX(C1:C5); MIN(I1.C1:I5.C1))";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(18.0, resultCell.GetValue()); // COUNTIF=3 + MAX=5 + MIN=10 (inner int values start at 10)
        }

        [Test]
        public void InvalidFunctionName_ShowsError()
        {
            string function = "=INVALID(C1:C5)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5];
            double initialValue = (double)resultCell.GetValue();
            
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Function evaluation error for input: =INVALID(C1:C5)\nError: Function 'INVALID' is not supported.");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }

        [Test]
        public void MismatchedParentheses_ShowsError()
        {
            string function = "=SUM(C1:C5";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5];
            double initialValue = (double)resultCell.GetValue();
            
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Function evaluation error for input: =SUM(C1:C5\nExpected type: System.Double, but got: System.String\nError: Input string was not in a correct format.");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }

        [Test]
        public void ReferenceOutOfRange_ShowsError()
        {
            string function = "=SUM(C1:C10)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5];
            double initialValue = (double)resultCell.GetValue();
            
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Function evaluation error for input: =SUM(C1:C10)\nError: Could not resolve range: C1:C10");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }
        
        [Test]
        public void ComplexReferenceParsing()
        {
            string function = "=SUM(C1:C5; D1:D5; E1; I1.C1; K1.I2.C1; I1.I1.C1:I5.I1.C1; I1.I1.C1:L5.I2.C1; K1.C1:L5.C2)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);

            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1102.0, resultCell.GetValue()); // 15 + 22.5 + 2.5 + 10 + 2 + 15 + (15+30+30+60)[135] + (150+300+150+300)[900] = 1093
        }
        
        [Test]
        public void Abs_PositiveValue()
        {
            string function = "=ABS(10)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(10, resultCell.GetValue());
        }

        [Test]
        public void Abs_NegativeValue()
        {
            string function = "=ABS(-10)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(10, resultCell.GetValue());
        }

        [Test]
        public void Abs_CellReference()
        {
            string function = "=ABS(D1)"; // D1 = 1.5f
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1.5, resultCell.GetValue());
        }

        [Test]
        public void And_AllTrue()
        {
            string function = "=AND(true; true; true)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void And_OneFalse()
        {
            string function = "=AND(true; false; true)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(false, resultCell.GetValue());
        }

        [Test]
        public void And_WithCellReferences()
        {
            string function = "=AND(C1 > 0; D1 < 2; H1 = false)"; // C1=1>0, D1=1.5<2, H1=false
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void Or_OneTrue()
        {
            string function = "=OR(false; false; true)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void Or_AllFalse()
        {
            string function = "=OR(false; false; false)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(false, resultCell.GetValue());
        }

        [Test]
        public void Or_WithCellReferences()
        {
            string function = "=OR(C1 > 10; D1 < 2; H1 = true)"; // C1=1<10, D1=1.5<2, H1=false
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void Not_True()
        {
            string function = "=NOT(true)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(false, resultCell.GetValue());
        }

        [Test]
        public void Not_False()
        {
            string function = "=NOT(false)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void Not_WithCellReference()
        {
            string function = "=NOT(H1)"; // H1 = false (row1)
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }
        
        [Test]
        public void Not_WithNonValidArgument()
        {
            string function = "=NOT(5)"; // 5 is not a boolean
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            bool initialValue = (bool) resultCell.GetValue();
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Invalid arguments for function 'NOT'");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Function evaluation failed for input: {function}");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }
        
        [Test]
        public void Not_WithNonValidFunctionArgument()
        {
            string function = "=NOT(SUM(5))"; // SUM(5) returns a number, not a boolean
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            bool initialValue = (bool) resultCell.GetValue();
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Invalid arguments for function 'NOT'");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Function evaluation failed for input: {function}");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }

        [Test]
        public void Xor_BothTrue()
        {
            string function = "=XOR(true; true)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(false, resultCell.GetValue());
        }

        [Test]
        public void Xor_BothFalse()
        {
            string function = "=XOR(false; false)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(false, resultCell.GetValue());
        }

        [Test]
        public void Xor_OneTrue()
        {
            string function = "=XOR(true; false)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void Xor_WithCellReferences()
        {
            string function = "=XOR(H1; H2)"; // H1=false, H2=true
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[8]; // H1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(true, resultCell.GetValue());
        }

        [Test]
        public void Divide_NormalOperation()
        {
            string function = "=DIVIDE(10; 2)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(5.0, resultCell.GetValue());
        }

        [Test]
        public void Divide_ByZero()
        {
            string function = "=DIVIDE(10; 0)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            double initialValue = (double) resultCell.GetValue();
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Function evaluation error for input: {function}\nError: Division by zero in DIVIDE function.");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }

        [Test]
        public void Divide_WithCellReferences()
        {
            string function = "=DIVIDE(C5; C1)"; // C5=5, C1=1
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(5.0, resultCell.GetValue());
        }

        [Test]
        public void Multiply_NormalOperation()
        {
            string function = "=MULTIPLY(3; 4)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(12.0, resultCell.GetValue());
        }

        [Test]
        public void Multiply_WithCellReferences()
        {
            string function = "=MULTIPLY(C2; D2)"; // C2=2, D2=3.0f
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(6.0, resultCell.GetValue());
        }

        [Test]
        public void Mod_NormalOperation()
        {
            string function = "=MOD(10; 3)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1.0, resultCell.GetValue());
        }

        [Test]
        public void Mod_ByZero()
        {
            string function = "=MOD(10; 0)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            double initialValue = (double) resultCell.GetValue();
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Function evaluation error for input: {function}\nError: Division by zero in MOD function.");
            Assert.AreEqual(initialValue, resultCell.GetValue());
        }

        [Test]
        public void Mod_WithCellReferences()
        {
            string function = "=MOD(C5; C2)"; // C5=5, C2=2
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(1.0, resultCell.GetValue());
        }

        [Test]
        public void Round_WithDecimals()
        {
            string function = "=ROUND(3.14159; 2)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(3.14, resultCell.GetValue());
        }

        [Test]
        public void Round_WithoutDecimals()
        {
            string function = "=ROUND(3.14159)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(3.0, resultCell.GetValue());
        }

        [Test]
        public void Round_NegativeNumber()
        {
            string function = "=ROUND(-3.7; 0)";
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(-4.0, resultCell.GetValue());
        }

        [Test]
        public void Round_WithCellReferences()
        {
            string function = "=ROUND(E1; 1)"; // E1 = 2.5 (row1)
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(2.5, resultCell.GetValue());
        }

        [Test]
        public void Complex_LogicalExpression()
        {
            string function = "=IF(AND(C1>0; NOT(H1)); MULTIPLY(C1; 10); MOD(10; 3))";
            // C1=1>0, H1=false → NOT(H1)=true → AND=true
            // Result should be MULTIPLY(C1; 10) = MULTIPLY(1; 10) = 10
            // If condition is false, it should return MOD(10; 3) = 1
            Cell resultCell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            
            _tableControl.FunctionExecutor.SetCellFunction(resultCell, function);
            
            _tableControl.FunctionExecutor.ExecuteCellFunction(resultCell.Id);
            Assert.AreEqual(10.0, resultCell.GetValue()); // Since condition is true
        }
    }
}