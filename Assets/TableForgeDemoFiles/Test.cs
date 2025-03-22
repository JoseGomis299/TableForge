using System;
using System.Collections.Generic;
using TableForge;
using TableForge.UI;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "Test1", menuName = "Test1")]
public class Test1 : ScriptableObject
{
    public Gradient gradient;
    public Color color;
    public AnimationCurve animationCurve;
    public Object reference;
    public bool B;
    
    [SerializeField] private int intField;
    [SerializeField] private float floatField;
    
    private int _privateIntField;
    private float _privateFloatField;
    
    [field: SerializeField] public int PublicLongProperty { get; set; }
    [field: SerializeField] public float PublicDoubleProperty { get; set; }

    public InheritorClass inheritorClass;
    public InnerClass inheritorClassAsInnerClass;
    [SerializeReference] public InnerClass innerClass;
    [SerializeReference] public InnerClass inheritorClassAsInnerClass2;
    
    public List<InnerClass> innerClasses;
    public List<List<int>> intlList2d;
    public List<List<List<InnerClass>>> innerClasses3d;
    [SerializeField] public int[] intArray = new int[] { 1, 2};
    
    public Vector2 vector2;
    [TableForgeIgnore] public Vector3 vector3;
    public Vector4 vector4;

    [Flags]
    public enum TestEnum
    {
        Null = 0,
        Value1 = 1,
        Value2 = 2,
        SuperLongValueToCheckIfItFits = 4
    }
    
    public TestEnum testEnum = TestEnum.Value2;
    
    public Vector3[] vector3Array;
    public Dictionary<InnerClass, int> innerClassIntDictionary = new Dictionary<InnerClass, int>() { { new InnerClass(1, "text1"), 1 }, { new InnerClass(2, "text2"), 2 } };
    public SerializedDictionary<string, int> stringIntDictionary = new SerializedDictionary<string, int>() { { "key1", 1 }, { "key2", 2 } };
    public Dictionary<string, InnerClass> stringInnerClassDictionary = new Dictionary<string, InnerClass>() { { "key1", null }, { "key2", new InnerClass(1, "") } };
    public Dictionary<Vector3, Vector2> structDictionary = new Dictionary<Vector3, Vector2>() { { new Vector3(1, 2, 3), new Vector2(1, 2) }, { new Vector3(4, 5, 6), new Vector2(3, 4) } };
    public SerializedDictionary<StructTest, Vector2> structDictionary2 = new SerializedDictionary<StructTest, Vector2>() { { new StructTest() { number = 1, boolean = true }, new Vector2(1, 2) }, { new StructTest() { number = 2, boolean = false }, new Vector2(3, 4) } };
    
    public Dictionary<int, InnerClass> hiddenDictionary = new Dictionary<int, InnerClass>() { { 1, null }, { 2, new InnerClass(2, "text2") } };
    
    public string text;
    
    public Matrix4x4 matrix;
    public Quaternion Quaternion;
    public Rect rect;
    public Ray ray;
    public Bounds bounds;
    public Plane plane;
    public byte byteValue;
    public short shortValue;
    public sbyte sbyteValue;
    public BoundingSphere boundingSphere;
    public LayerMask layerMask;
}

[Serializable]
public struct StructTest
{
    public int number;
    public bool boolean;
}

[Serializable]
public class InnerClass
{
    public int number;
    public string text;
    public List<List<int>> intlList2D = new List<List<int>>() { new List<int>() { 1, 2, 3 }, new List<int>() { 4, 5, 6 } };
    public InnerClass innerClass;
    
    private int _privateNumber;
    
    public InnerClass(int number, string text)
    {
        this.number = number;
        this.text = text;
    }
}

[Serializable]
public class InheritorClass : InnerClass
{
    public int inheritorNumber;
    public string inheritorText;
    
    public InheritorClass(int number, string text, int inheritorNumber, string inheritorText) : base(number, text)
    {
        this.inheritorNumber = inheritorNumber;
        this.inheritorText = inheritorText;
    }
}