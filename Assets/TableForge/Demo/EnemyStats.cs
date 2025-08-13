using UnityEngine;
using System.Collections.Generic;

namespace TableForge.Demo
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "TableForge/Demo/Enemy Stats")]
    public class EnemyStats : CharacterStats
    {
        public Gradient aggressionGradient;
        [SerializeReference] Vector4 patrolArea;
        public List<string> lootDrops;
        [SerializeField] private EnemyMeta meta;

        [System.Serializable]
        public class EnemyMeta
        {
            public string species;
            public bool isBoss;
            public int threatLevel;
        }
    }
}