using UnityEngine;

[CreateAssetMenu(fileName = "Test3", menuName = "Test3")]
public class Test3 : ScriptableObject
{
    public Gradient gradient;
    public Color color;
    
    [SerializeField] private int intField;
    [SerializeField] private float floatField;
    
    private int _privateIntField;
    private float _privateFloatField;
    
    [field: SerializeField] public int PublicLongProperty { get; set; }
    [field: SerializeField] public float PublicDoubleProperty { get; set; }
}