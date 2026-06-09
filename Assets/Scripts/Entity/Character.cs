using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    public abstract class Character : MonoBehaviour
    {
        public abstract CharacterAttribute BaseAttribute { get; }
        public abstract CharacterAttribute CurrentAttribute { get; }
    }

    [System.Serializable]
    public class CharacterAttribute
    {
        Dictionary<CharacterAttributeType, float> attributes = new();

        public readonly bool isConstant;

        public CharacterAttribute(bool isConstant)
        {
            this.isConstant = isConstant;
        }

        public CharacterAttribute(Dictionary<CharacterAttributeType, float> attributes, bool isConstant)
        {
            this.attributes = attributes;
            this.isConstant = isConstant;
        }

        public void SetAttribute(CharacterAttributeType type, float value)
        {
            if (isConstant)
            {
                throw new System.InvalidOperationException("Cannot modify a constant attribute.");
            }
            if(attributes.ContainsKey(type))
            {
                attributes[type] = value;
            }
        }

        public float GetAttribute(CharacterAttributeType type)
        {
            return attributes.TryGetValue(type, out var value) ? value : 0f;
        }

        public bool HasAttribute(CharacterAttributeType type)
        {
            return attributes.ContainsKey(type);
        }

        public CharacterAttribute Clone()
        {
            return new CharacterAttribute(new Dictionary<CharacterAttributeType, float>(attributes), isConstant);
        }
    }

    public enum CharacterAttributeType
    {
        None = 0,
        MaxHealth = 1,
        Health = 2,
        Speed = 3,
        Power = 4,
    }

}