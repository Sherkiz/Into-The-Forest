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

        public CharacterState ToCharacterAttribute(Character host, bool isConstant)
        {
            var attributes = new Dictionary<CharacterAttributeType, float>
            {
                { CharacterAttributeType.MaxHealth, maxHealth },
                { CharacterAttributeType.Health, health },
                { CharacterAttributeType.Power, power },
                { CharacterAttributeType.Speed, speed }
            };
            return new CharacterState(attributes, host, isConstant);
        }
    }

}