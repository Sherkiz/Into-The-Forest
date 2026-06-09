using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    [CreateAssetMenu(fileName = "CharacterAttribute", menuName = "ITF/Entity/CharacterAttribute")]
    public class CharacterAttributeScriptObject : ScriptableObject
    {
        public float maxHealth;
        public float health;
        public float power;
        public float speed;

        public CharacterAttribute ToCharacterAttribute(bool isConstant)
        {
            var attributes = new Dictionary<CharacterAttributeType, float>
            {
                { CharacterAttributeType.MaxHealth, maxHealth },
                { CharacterAttributeType.Health, health },
                { CharacterAttributeType.Power, power },
                { CharacterAttributeType.Speed, speed }
            };
            return new CharacterAttribute(attributes, isConstant);
        }
    }

}