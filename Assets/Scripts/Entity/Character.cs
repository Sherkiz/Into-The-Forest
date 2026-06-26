using ITF.EventChannels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Entity
{

    public abstract class Character : MonoBehaviour, IReferencer
    {
        public abstract CharacterState BaseState { get; }
        public abstract CharacterState CurrentState { get; }

        public abstract Faction Faction { get; }

        public abstract bool Inited { get; }

        public abstract UnityEvent<Character> OnInited { get; }

        public abstract UnityEvent<Character> OnDeinited { get; }

        public abstract GameObject GetReference(string name);
        public abstract void SetOrAddReference(string name, GameObject reference);

        public abstract void Init();

        public abstract void Deinit();
    }

    [System.Serializable]
    public class CharacterState
    {
        Dictionary<CharacterAttributeType, float> attributes = new();

        GameEvent<CharacterState, CharacterAttributeChangedInfo> gameEvent = new();
        public GameEvent<CharacterState, CharacterAttributeChangedInfo> GameEvent { get { return gameEvent; } }

        public readonly Character host;
        public readonly bool isConstant;

        public CharacterState(Character host, bool isConstant)
        {
            this.host = host;
            this.isConstant = isConstant;
        }

        public CharacterState(Dictionary<CharacterAttributeType, float> attributes, Character host, bool isConstant)
        {
            this.attributes = attributes;
            this.host = host;
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
                CharacterAttributeChangedInfo changeInfo = new CharacterAttributeChangedInfo(type, attributes[type], value);
                gameEvent.Invoke(this, changeInfo);
                attributes[type] = changeInfo.newValue;
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

        public CharacterState Clone()
        {
            return new CharacterState(new Dictionary<CharacterAttributeType, float>(attributes), host, isConstant);
        }
    }

    public class CharacterAttributeChangedInfo
    {
        public CharacterAttributeType AttributeType { get; private set; }
        public readonly float oldValue;
        public float newValue;

        public CharacterAttributeChangedInfo(CharacterAttributeType attributeType, float oldValue, float newValue)
        {
            AttributeType = attributeType;
            this.oldValue = oldValue; 
            this.newValue = newValue;
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