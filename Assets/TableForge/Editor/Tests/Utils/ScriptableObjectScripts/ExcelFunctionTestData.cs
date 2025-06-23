using System;
using UnityEngine;

namespace TableForge.Tests
{
    internal class ExcelFunctionTestData : ScriptableObject
    {
        public string stringValue1; //A
        public string stringValue2; //B
        public int intValue; //C
        public float floatValue; //D
        public double doubleValue; //E
        public long longValue; //F
        public ulong ulongValue; //G
        public bool boolValue; //H
        public SerializedExcelFunctionTestData innerData; //I
        public SerializedExcelFunctionTestData innerData2; //J
        public SerializedExcelFunctionTestData[] innerDataArray; //K
        public SerializedExcelFunctionTestData[] innerDataArray2; //L
    }
    
    [Serializable]
    internal class SerializedExcelFunctionTestData
    {
        public string stringValue1; // I.A
        public string stringValue2; // I.B
        public int intValue; // I.C
        public float floatValue; // I.D
        public double doubleValue; // I.E
        public long longValue; // I.F
        public ulong ulongValue; // I.G
        public bool boolValue; // I.H
        public SerializedExcelFunctionTestData2 innerData; // I.I
    }
    
    [Serializable]
    internal class SerializedExcelFunctionTestData2
    {
        public string stringValue1; // I.A
        public string stringValue2; // I.B
        public int intValue; // I.C
        public float floatValue; // I.D
    }}