using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TableForge.Editor;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TableForge.Tests
{
    internal class ExcelFunctionsTests
    {
        private TableControl _tableControl;

        private TableControl GetTableControl(int rowCount)
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
                string path = $"{PathUtil.GetTestFolderRelativePath()}/MockedData/ExcelData{i}.asset";
                ExcelFunctionTestData existingData = AssetDatabase.LoadAssetAtPath<ExcelFunctionTestData>(path);
                ExcelFunctionTestData data = existingData != null ? existingData : ScriptableObject.CreateInstance<ExcelFunctionTestData>();
                
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
                
                if(existingData == null)
                    AssetDatabase.CreateAsset(data, path);
                rowGuids.Add(AssetDatabase.AssetPathToGUID(path));
                rowPaths.Add(path);
            }
            
            TableMetadata tableMetadata = TableMetadataManager.CreateMetadata(rowGuids, "TestTable", PathUtil.GetTestFolderRelativePath());
            tableControl.SetTable(TableMetadataManager.GetTable(tableMetadata));

            return tableControl;
        }
        
        [SetUp]
        public void Setup()
        {
            _tableControl = GetTableControl(5);
            _tableControl.Metadata.GetFunctions().Clear();
        }

        #region SingleFunctionTests
        
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
            
            LogAssert.Expect(LogType.Error, new Regex(".*Invalid arguments for function 'SUM'.*"));
            LogAssert.Expect(LogType.Error, new Regex(".*Function evaluation error for input: =SUM.*"));
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
            
            LogAssert.Expect(LogType.Error, new Regex(".*Function evaluation error for input: =INVALID.*"));
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
            
            LogAssert.Expect(LogType.Error, new Regex(".*Unmatched opening parenthesis in function input.*"));
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
            
            LogAssert.Expect(LogType.Error, new Regex(".*Could not resolve range: C1:C10.*"));
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
            LogAssert.Expect(LogType.Error, new Regex(".*Invalid arguments for function 'NOT'.*"));
            LogAssert.Expect(LogType.Error, new Regex(".*Function evaluation error for input: =NOT.*"));
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
            LogAssert.Expect(LogType.Error, new Regex(".*Invalid arguments for function 'NOT'.*"));
            LogAssert.Expect(LogType.Error, new Regex(".*Function evaluation error for input: =NOT.*"));
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
            LogAssert.Expect(LogType.Error, new Regex(".*Error: Division by zero in DIVIDE function.*"));
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
            LogAssert.Expect(LogType.Error, new Regex(".*Error: Division by zero in MOD function.*"));
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
        
        [Test]
        public void SetCell_WithDirectNumberValue()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1 
            string input = "25";
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(25.0, cell.GetValue());
        }

        [Test]
        public void SetCell_WithDirectNegativeNumber()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "-15.5";
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(-15.5, cell.GetValue());
        }

        [Test]
        public void SetCell_WithSingleCellReference()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=C1"; // Reference to C1 (intValue = 1)
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(1.0, cell.GetValue());
        }

        [Test]
        public void SetCell_WithDeeplyNestedExpression()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=10 * (5 + (3 - (2 * (4 / 2))))"; // 10 * (5 + (3 - (2*2))) = 10 * (5 + (3-4)) = 10 * (5 -1) = 40
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(40.0, cell.GetValue());
        }
        
        [Test]
        public void Arithmetic_WithFunctionResult()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=SUM(C1:C5) * 2"; // SUM(1+2+3+4+5)=15 → 15*2=30
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(30.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithFunctionAndReference()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=SUM(C1:C5) + D1"; // 15 + 1.5 = 16.5
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(16.5, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithNestedFunctions()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=AVERAGE(C1:C5) * COUNT(C1:C5)"; // AVERAGE=3, COUNT=5 → 3*5=15
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(15.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithPercentageInFunction()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=SUM(C1:C5) * 10%"; // 15 * 0.1 = 1.5
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(1.5, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithComplexMixedExpression()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=(MAX(C1:C5) + MIN(D1:D5)) * 2 ^ 2"; // MAX=5, MIN=1.5 → (5+1.5)*4=6.5*4=26
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(26.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithConditionalFunction()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=IF(C1 > 2; C1 * 10; C1 * 5)"; // 1>2 is false → 1*5=5
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(5.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithReferenceChain()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=I1.C1 * 2"; // I1.C1 (innerData.intValue) = 10 → 10*2=20
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(20.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithFunctionInParentheses()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=(SUM(C1:C5) - 5) * 3"; // (15-5)=10 → 10*3=30
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(30.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithPercentageAfterFunction()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=SUM(C1:C5)%"; // 15% = 0.15
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(0.15, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithExponentiationAndFunction()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=2 ^ SUM(C1;C2)"; // SUM(C1,C2)=1+2=3 → 2^3=8
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(8.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithComplexNestedExpression()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=(IF(H1; MAX(C1:C5); MIN(C1:C5)) * 10%) + 5"; 
            // H1=false → MIN=1 → 1*0.1=0.1 → 0.1+5=5.1
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(5.1, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithRangeAndArithmetic()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=SUM(C1:C5; D1:D5) * 0.5"; // SUM(ints)=15, SUM(floats)=22.5 → 37.5*0.5=18.75
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(18.75, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithDeepReferenceChain()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=I1.I1.C1 * 2 + K1.I2.C1"; // I1.I1.C1=1*2=2, K1.I2.C1=2 → 4
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(4.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithLogicalFunctionAndArithmetic()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=IF(AND(C1>0; D1<2); C1*10; C1*5)"; 
            // C1>0=true, D1<2=true → AND=true → 1*10=10
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(10.0, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithPercentageInComplexExpression()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=(100 + SUM(C1:C5))% * 50"; // (100+15)=115 → 115%=1.15 → 1.15*50=57.5
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(57.5, (double)cell.GetValue(), 0.01);
        }

        [Test]
        public void Arithmetic_WithFunctionAsPercentageArgument()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=SUM(C1:C5; 50%)"; // 15 + 0.5 = 15.5
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(15.5, cell.GetValue());
        }

        [Test]
        public void Arithmetic_WithArrayReference()
        {
            Cell cell = _tableControl.TableData.Rows[1].Cells[5]; // E1
            string input = "=AVERAGE(I1.I1.C1 + 4; K1.I2.C1; I1.I1.C1; 4) * 3"; 
            // Values: 5, 2, 1, 4 → AVERAGE=3 → 3*3=9
            _tableControl.FunctionExecutor.SetCellFunction(cell, input);
            _tableControl.FunctionExecutor.ExecuteCellFunction(cell.Id);
            Assert.AreEqual(9.0, cell.GetValue());
        }
        
        #endregion

        #region MultipleFunctionsTests
        [Test]
        public void ExecuteAllFunctions_LinearDependency_ExecutesInOrder()
        {
            // Set up: E1 -> E2 -> E3
            var cellE1 = _tableControl.TableData.Rows[1].Cells[5];
            var cellE2 = _tableControl.TableData.Rows[2].Cells[5];
            var cellE3 = _tableControl.TableData.Rows[3].Cells[5];

            _tableControl.FunctionExecutor.SetCellFunction(cellE1, "=C$1+10");  // E1 = C1 (1) + 10 = 11
            _tableControl.FunctionExecutor.SetCellFunction(cellE2, "=E1+10");  // E2 = E1 (11) + 10 = 21
            _tableControl.FunctionExecutor.SetCellFunction(cellE3, "=E2+10");  // E3 = E2 (21) + 10 = 31

            _tableControl.FunctionExecutor.ExecuteAllFunctions();

            // Verify execution order and values
            Assert.AreEqual(11.0, cellE1.GetValue()); // E1
            Assert.AreEqual(21.0, cellE2.GetValue()); // E2
            Assert.AreEqual(31.0, cellE3.GetValue()); // E3
        }

        [Test]
        public void ExecuteAllFunctions_DiamondDependency_ExecutesParentsFirst()
        {
            // Setup: 
            //   E1 = C1 (root)
            //   E2 = E1 + 10
            //   E3 = E1 + 20
            //   E4 = E2 + E3
            var cellE1 = _tableControl.TableData.Rows[1].Cells[5];
            var cellE2 = _tableControl.TableData.Rows[2].Cells[5];
            var cellE3 = _tableControl.TableData.Rows[3].Cells[5];
            var cellE4 = _tableControl.TableData.Rows[4].Cells[5];

            _tableControl.FunctionExecutor.SetCellFunction(cellE1, "=$C1");      // E1 = 1
            _tableControl.FunctionExecutor.SetCellFunction(cellE2, "=$E$1+10"); // E2 = 11
            _tableControl.FunctionExecutor.SetCellFunction(cellE3, "=E1+20");   // E3 = 21
            _tableControl.FunctionExecutor.SetCellFunction(cellE4, "=E$2+E3");  // E4 = 32

            _tableControl.FunctionExecutor.ExecuteAllFunctions();

            Assert.AreEqual(1.0, cellE1.GetValue());  // E1
            Assert.AreEqual(11.0, cellE2.GetValue()); // E2
            Assert.AreEqual(21.0, cellE3.GetValue()); // E3
            Assert.AreEqual(32.0, cellE4.GetValue()); // E4 (11+21)
        }

        [Test]
        public void ExecuteAllFunctions_MultiBranch_ExecutesIndependently()
        {
            // Setup independent chains:
            //   E1 = C1 (branch A)
            //   E2 = C2 (branch B)
            var cellE1 = _tableControl.TableData.Rows[1].Cells[5];
            var cellE2 = _tableControl.TableData.Rows[2].Cells[5];

            _tableControl.FunctionExecutor.SetCellFunction(cellE1, "=C1+5");  // E1 = 1+5=6
            _tableControl.FunctionExecutor.SetCellFunction(cellE2, "=C2+10"); // E2 = 2+10=12

            _tableControl.FunctionExecutor.ExecuteAllFunctions();

            Assert.AreEqual(6.0, cellE1.GetValue());  // E1
            Assert.AreEqual(12.0, cellE2.GetValue()); // E2
        }

        [Test]
        public void ExecuteAllFunctions_DiamondDependencyWithCircularReference_ThrowsError()
        {
            // Setup circular dependency: E1 -> E2 -> E3 -> E1
            var cellE1 = _tableControl.TableData.Rows[1].Cells[5];
            var cellE2 = _tableControl.TableData.Rows[2].Cells[5];
            var cellE3 = _tableControl.TableData.Rows[3].Cells[5];
            var cellE4 = _tableControl.TableData.Rows[4].Cells[5];

            _tableControl.FunctionExecutor.SetCellFunction(cellE1, "=E$4+10");  // E1 = E4 (conflict with E4) + 10 = 20
            _tableControl.FunctionExecutor.SetCellFunction(cellE2, "=E1+10");  // E2 = E1 (11) + 10 = 21
            _tableControl.FunctionExecutor.SetCellFunction(cellE3, "=E2+10");  // E3 = E2 (21) + 10 = 31
            _tableControl.FunctionExecutor.SetCellFunction(cellE4, "=E3+E1");  // E4 = E3 (31) + E1 (conflict with E4) = 42

            LogAssert.Expect(LogType.Error, new Regex(".*Circular dependency detected.*"));

            _tableControl.FunctionExecutor.ExecuteAllFunctions();
        }

        [Test]
        public void ExecuteAllFunctions_ComplexDistributedDependencies_ExecutesInOrder()
        {
            // Setup complex distributed dependencies:
            //    E1---\ 
            //  /   \   \
            // E2   E3---\
            //  \   /    /
            //   E4    / 
            //    \   /
            //     E5

            var cellE1 = _tableControl.TableData.Rows[1].Cells[5];
            var cellE2 = _tableControl.TableData.Rows[2].Cells[5];
            var cellE3 = _tableControl.TableData.Rows[3].Cells[5];
            var cellE4 = _tableControl.TableData.Rows[4].Cells[5];
            var cellE5 = _tableControl.TableData.Rows[5].Cells[5];
            
            _tableControl.FunctionExecutor.SetCellFunction(cellE1, "=1+10");  // E1 = 1 + 10 = 11
            _tableControl.FunctionExecutor.SetCellFunction(cellE2, "=E1+5");   // E2 = 11 + 5 = 16
            _tableControl.FunctionExecutor.SetCellFunction(cellE3, "=E1+20");  // E3 = 11 + 20 = 31
            _tableControl.FunctionExecutor.SetCellFunction(cellE4, "=E2 + E3"); // E4 = 16 + 31 = 47
            _tableControl.FunctionExecutor.SetCellFunction(cellE5, "=E4 + E1 + E3"); // E5 = 47 + 11 + 31 = 89
            
            _tableControl.FunctionExecutor.ExecuteAllFunctions();
            
            Assert.AreEqual(11.0, cellE1.GetValue()); // E1
            Assert.AreEqual(16.0, cellE2.GetValue()); // E2
            Assert.AreEqual(31.0, cellE3.GetValue()); // E3
            Assert.AreEqual(47.0, cellE4.GetValue()); // E4
            Assert.AreEqual(89.0, cellE5.GetValue()); // E5
        }

        #endregion
        
    }
}