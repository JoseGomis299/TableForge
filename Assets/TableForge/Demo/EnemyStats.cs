using UnityEngine;
using System.Collections.Generic;

namespace TableForge.Demo
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "TableForge/Demo/Enemy Stats")]
    public class EnemyStats : CharacterStats
    {
        public Gradient aggressionGradient;
        public Vector4 patrolArea;
        public List<string> lootDrops;
        public EnemyMeta meta;

        [System.Serializable]
        public class EnemyMeta
        {
            public string species;
            public bool isBoss;
            public int threatLevel;
        }
    }
}