using UnityEngine;
using System.Collections.Generic;

namespace TableForge.Demo
{
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "TableForge/Demo/Character Stats")]
    public class CharacterStats : ScriptableObject
    {
        public string characterName;
        public int level;
        public float health;
        public Color skinColor;
        public AnimationCurve staminaOverTime;
        public Vector3 spawnPosition;
        public List<Ability> abilities;
        public WeaponStats weapon;

        [System.Serializable]
        public class Ability
        {
            public string abilityName;
            public float cooldown;
            public int power;
            public StatsDetails details;

            [System.Serializable]
            public class StatsDetails
            {
                public float baseAttack;
                public double baseCastingTime;
            }
        }
    }
}