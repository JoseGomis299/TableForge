using System;
using System.Collections.Generic;
using TableForge.UI;
using UnityEngine;

namespace TableForge.Tests
{
    internal class FilteringTestData : ScriptableObject
    {
        public int intValue;
        public float floatValue;
        public string stringValue;
        public bool boolValue;
        public TestEnum enumValue;
        public List<string> stringList = new List<string>();
        public List<int> intList = new List<int>();
        public SerializedDictionary<int, string> intStringDictionary = new SerializedDictionary<int, string>();
        public SerializedFilteringData serializedFilteringData = new SerializedFilteringData();
    }

    [Serializable]
    internal class SerializedFilteringData
    {
        public int intValue;
        public float floatValue;
        public string stringValue;
        public bool boolValue;
        public TestEnum enumValue;
        public List<string> stringList = new List<string>();
        public List<int> intList = new List<int>();
        public SerializedDictionary<int, string> intStringDictionary = new SerializedDictionary<int, string>();
    }
    
    internal enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}