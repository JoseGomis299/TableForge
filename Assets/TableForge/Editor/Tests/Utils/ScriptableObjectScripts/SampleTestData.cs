using System;
using System.Collections.Generic;
using TableForge.DataStructures;
using TableForge.Attributes;
using UnityEngine;

namespace TableForge.Tests
{

    [CreateAssetMenu(fileName = "SampleTestData", menuName = "TableForge/Tests/Sample Test Data")]
    internal class SampleTestData : AbstractTestData, ITestInterface
    {
        #region Basic Fields

        [SerializeField] private int intField;
        [SerializeField] private float floatField;

        private int _privateIntField;
        private float _privateFloatField;

        [field: SerializeField] public int PublicIntProperty { get; set; }
        [field: SerializeField] public float PublicFloatProperty { get; set; }

        #endregion

        #region Unity-Specific Fields

        public Gradient gradient;
        public Color color;
        public AnimationCurve animationCurve;
        public SampleTestData unityObjectReference;

        #endregion

        #region Collections

        public NestedData nestedData;

        public List<NestedData> nestedDataList;
        public List<List<int>> intList2D;
        public List<List<List<NestedData>>> nestedData3D;
        [SerializeField] public int[] intArray = new int[] { 1, 2 };

        #endregion

        #region Vector Data

        public Vector2 vector2;
        [TableForgeIgnore] public Vector3 vector3;
        public Vector4 vector4;
        public Vector3[] vector3Array;

        #endregion

        #region Enum

        public enum SampleEnum
        {
            OptionA,
            OptionB,
            OptionC
        }

        public SampleEnum sampleEnum = SampleEnum.OptionB;

        #endregion

        #region Dictionaries

        public Dictionary<NestedData, int> nestedDataToIntDictionary =
            new()
            {
                { new NestedData(1, "Example1"), 10 },
                { new NestedData(2, "Example2"), 20 }
            };

        public SerializedDictionary<string, int> stringToIntDictionary =
            new()
            {
                { "KeyA", 100 },
                { "KeyB", 200 }
            };

        public Dictionary<string, NestedData> stringToNestedDataDictionary =
            new()
            {
                { "Entry1", null },
                { "Entry2", new NestedData(4, "Sample4") }
            };

        private SerializedDictionary<int, NestedData> _hiddenDictionary =
            new()
            {
                { 1, new NestedData(5, "Hidden1") },
                { 2, new NestedData(6, "Hidden2") }
            };

        #endregion

        [field: SerializeField] public int InterfaceInt { get; set; }
        [field: SerializeField] public override int AbstractInt { get; protected set; } = 3;
        [field: SerializeField] public override string AbstractString { get; set; }
        [field: SerializeField] public override float AbstractFloat { get; } = 5;
    }

    [Serializable]
    internal class NestedData
    {
        #region Fields

        public int number;
        public string text;
        public NestedData nestedData;

        public List<List<int>> intList2D = new()
        {
            new List<int>() { 1, 2, 3 },
            new List<int>() { 4, 5, 6 }
        };

        private int _privateNumber;

        #endregion

        #region Constructors

        public NestedData(int number, string text)
        {
            this.number = number;
            this.text = text;
        }

        #endregion
    }

    internal abstract class AbstractTestData : ScriptableObject
    {
        public int inheritedInt;
        [field: SerializeField] public int InheritedIntProperty { get; set; } = 42;

        public abstract int AbstractInt { get; protected set; }
        public abstract string AbstractString { get; set; }
        public abstract float AbstractFloat { get; }
        [field: SerializeField] public virtual int VirtualInt { get; set; } = 42;
    }

    internal interface ITestInterface
    {
        int InterfaceInt { get; set; }
    }
}