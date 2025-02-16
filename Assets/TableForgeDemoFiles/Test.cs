using System;
using System.Collections.Generic;
using TableForge;
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

    [TableForgeSerialize] private InnerClass innerClass;
    
    public List<InnerClass> innerClasses;
    [TableForgeSerialize] public List<List<int>> intlList2D;
    public List<List<List<InnerClass>>> innerClasses3D;
    [SerializeField] public int[] intArray = new int[] { 1, 2};
    
    public Vector2 vector2;
    [TableForgeIgnore] public Vector3 vector3;
    public Vector4 vector4;

    public enum TestEnum
    {
        Value1,
        Value2,
        SuperLongValueToCheckIfItFits
    }
    
    public TestEnum testEnum = TestEnum.Value2;
    
    public Vector3[] vector3Array;
    [TableForgeSerialize] public Dictionary<InnerClass, int> innerClassIntDictionary = new Dictionary<InnerClass, int>() { { new InnerClass(1, "text1"), 1 }, { new InnerClass(2, "text2"), 2 } };
    [TableForgeSerialize] public Dictionary<string, int> stringIntDictionary = new Dictionary<string, int>() { { "key1", 1 }, { "key2", 2 } };
    [TableForgeSerialize] public Dictionary<string, InnerClass> stringInnerClassDictionary = new Dictionary<string, InnerClass>() { { "key1", null }, { "key2", new InnerClass(1, "") } };
    
    public Dictionary<int, InnerClass> hiddenDictionary = new Dictionary<int, InnerClass>() { { 1, null }, { 2, new InnerClass(2, "text2") } };
    
    public string text;
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