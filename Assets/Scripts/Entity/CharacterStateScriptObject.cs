using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    [CreateAssetMenu(fileName = "CharacterState", menuName = "ITF/Entity/CharacterState")]
    public class CharacterStateScriptObject : ScriptableObject
    {
        public float maxHealth;
        public float health;
        public float power;
        public float speed;

        public CharacterState ToCharacterState(Character host, bool isConstant)
        {
            var states = new Dictionary<CharacterStateType, float>
            {
                { CharacterStateType.MaxHealth, maxHealth },
                { CharacterStateType.Health, health },
                { CharacterStateType.Power, power },
                { CharacterStateType.Speed, speed }
            };
            return new CharacterState(states, host, isConstant);
        }
    }

}