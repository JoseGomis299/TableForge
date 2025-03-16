using System.Collections.Generic;
using TableForge.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TableForge.Tests
{
    internal class SupportedTypes : ScriptableObject
    {
        public AnimationCurve animationCurve;

        public bool booleanField;

        public Color colorField;

        public object fallbackField;
        
        // floating point data types (float, double)
        public float floatField;
        public double doubleField;

        public Gradient gradientField;

        // integer data types (int, long, byte, short, uint)
        public int intField;
        public long longField;
        public byte byteField;
        public short shortField;
        public uint uintField;

        public Object unityObjectReference;

        public string stringField;
        
        public SerializedDictionary<string, int> stringToIntDictionary;
        
        public int[] intArray;
        public List<int> intList;
        public List<List<int>> intList2D;

        public NestedData complexTypeData;
        
        public Vector2 vector2Field;
        
        public Vector3 vector3Field;
        
        public Vector4 vector4Field;

        public SampleEnum enumField;
        public enum SampleEnum
        {
            Option1,
            Option2,
            Option3
        }
    }
    
    internal class AnimationCurveScriptableObject : ScriptableObject
    {
        public AnimationCurve animationCurve;
    }

    internal class BooleanScriptableObject : ScriptableObject
    {
        public bool booleanField;
    }

    internal class ColorScriptableObject : ScriptableObject
    {
        public Color colorField;
    }

    internal class EnumScriptableObject : ScriptableObject
    {
        public enum SampleEnum
        {
            Option1,
            Option2,
            Option3
        }

        public SampleEnum enumField;
    }

    internal class FloatingPointScriptableObject : ScriptableObject
    {
        public float floatField;
        public double doubleField;
    }

    internal class GradientScriptableObject : ScriptableObject
    {
        public Gradient gradientField;
    }

    internal class IntegralTypesScriptableObject : ScriptableObject
    {
        public int intField;
        public long longField;
        public ulong ulongField;
        public uint uintField;
    }

    internal class UnityObjectScriptableObject : ScriptableObject
    {
        public Object unityObjectReference;
    }

    internal class StringScriptableObject : ScriptableObject
    {
        public string stringField;
    }

    internal class DictionaryScriptableObject : ScriptableObject
    {
        public SerializedDictionary<string, int> stringToIntDictionary;
    }

    internal class ListScriptableObject : ScriptableObject
    {
        public int[] intArray;
        public List<int> intList;
    }

    internal class ComplexTypeScriptableObject : ScriptableObject
    {
        public NestedData complexTypeData;

        [System.Serializable]
        public class NestedData
        {
            public int number;
            public string text;

            public NestedData(int number, string text)
            {
                this.number = number;
                this.text = text;
            }
        }
    }

    internal class Vector2ScriptableObject : ScriptableObject
    {
        public Vector2 vector2Field;
    }

    internal class Vector3ScriptableObject : ScriptableObject
    {
        public Vector3 vector3Field;
    }

    internal class Vector4ScriptableObject : ScriptableObject
    {
        public Vector4 vector4Field;
    }
}
