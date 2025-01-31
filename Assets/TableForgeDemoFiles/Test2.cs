using UnityEngine;

[CreateAssetMenu(fileName = "Test2", menuName = "Test2")]
public class Test2 : ScriptableObject
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