using System;
using System.Collections.Generic;
using TableForge.DataStructures;
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
        public SerializedFilteringTestData serializedFilteringTestData = new SerializedFilteringTestData();
    }

    [Serializable]
    internal class SerializedFilteringTestData
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
}