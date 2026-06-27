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
        Dictionary< CharacterStateType, float> states = new();

        GameEvent<CharacterState, CharacterStateChangedInfo> gameEvent = new();
        public GameEvent<CharacterState, CharacterStateChangedInfo> GameEvent { get { return gameEvent; } }

        public readonly Character host;
        public readonly bool isConstant;

        public CharacterState(Character host, bool isConstant)
        {
            this.host = host;
            this.isConstant = isConstant;
        }

        public CharacterState(Dictionary<CharacterStateType, float> states, Character host, bool isConstant)
        {
            this.states = states;
            this.host = host;
            this.isConstant = isConstant;
        }

        public void SetState(CharacterStateType type, float value)
        {
            if (isConstant)
            {
                throw new System.InvalidOperationException("Cannot modify a constant attribute.");
            }
            if(states.ContainsKey(type))
            {
                CharacterStateChangedInfo changeInfo = new CharacterStateChangedInfo(type, states[type], value);
                gameEvent.Invoke(this, changeInfo);
                states[type] = changeInfo.newValue;
            }
        }

        public float GetState(CharacterStateType type)
        {
            return states.TryGetValue(type, out var value) ? value : 0f;
        }

        public bool HasState(CharacterStateType type)
        {
            return states.ContainsKey(type);
        }

        public CharacterState Clone()
        {
            return new CharacterState(new Dictionary<CharacterStateType, float>(states), host, isConstant);
        }
    }

    public class CharacterStateChangedInfo
    {
        public CharacterStateType StateType { get; private set; }
        public readonly float oldValue;
        public float newValue;

        public CharacterStateChangedInfo(CharacterStateType stateType, float oldValue, float newValue)
        {
            StateType = stateType;
            this.oldValue = oldValue; 
            this.newValue = newValue;
        }
    }

    public enum CharacterStateType
    {
        None = 0,
        MaxHealth = 1,
        Health = 2,
        Speed = 3,
        Power = 4,
    }

}