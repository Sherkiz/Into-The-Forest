using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    [CreateAssetMenu(fileName = "CharacterState", menuName = "ITF/Entity/CharacterState")]
    public class CharacterStateScriptObject : ScriptableObject
    {
        public float maxHealth;
        public float health;
        public float damage;
        public float speed;
        public float attackSpeed;
        public float attackRange;
        public float armor;
        public float mana;
        public float detectionRange;

        public CharacterState ToCharacterState(Character host, bool isConstant)
        {
            var states = new Dictionary<CharacterStateType, float>
            {
                { CharacterStateType.MaxHealth, maxHealth },
                { CharacterStateType.Health, health },
                { CharacterStateType.Damage, damage },
                { CharacterStateType.Speed, speed },
                { CharacterStateType.AttackRange, attackRange },
                { CharacterStateType.AttackSpeed, attackSpeed },
                { CharacterStateType.Armor, armor },
                { CharacterStateType.Mana, mana },
                { CharacterStateType.DetectionRange, detectionRange }
            };
            return new CharacterState(states, host, isConstant);
        }
    }

}