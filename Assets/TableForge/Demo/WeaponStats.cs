using UnityEngine;
using TableForge.DataStructures;
using TableForge.Attributes;

namespace TableForge.Demo
{
    [CreateAssetMenu(fileName = "WeaponStats", menuName = "TableForge/Demo/Weapon Stats")]
    public class WeaponStats : ScriptableObject
    {
        public string weaponName;
        public int damage;
        public float attackSpeed;
        public Color bladeColor;
        public AnimationCurve damageCurve;
        public Vector2 size;
        public SerializedDictionary<string, float> upgradeLevels;
        public WeaponMeta meta;
        [TableForgeIgnore] public string developerNotes; // This field will be ignored by TableForge

        [System.Serializable]
        public class WeaponMeta
        {
            public string manufacturer;
            public int year;
            public bool isPrototype;
        }
    }
}