using ITF.StateMachine;
using UnityEngine;
using UnityEngine.Events;
using AYellowpaper.SerializedCollections;
using ITF.Skill.Passive;
using System.Collections.Generic;

namespace ITF.Entity
{

    public class SkilledCharacter : Character, IStateContainer
    {
        [SerializeField, SerializedDictionary("name", "GameObject")]
        SerializedDictionary<string, GameObject> references;
        [SerializeField]
        CharacterStateScriptObject _characterStateScriptObject;

        CharacterState baseState;
        public override CharacterState BaseState => baseState;
        CharacterState currentState;
        public override CharacterState CurrentState => currentState;

        bool inited = false;
        public override bool Inited => inited;

        [SerializeField]
        PassiveSkillAddor[] passiveSkillAddors;
        public PassiveSkillAddor[] PassiveSkillAddors => (PassiveSkillAddor[])passiveSkillAddors.Clone();
        List<PassiveSkill> passiveSkills = new();

        [SerializeField]
        UnityEvent<IStateContainer, StateUnit> onStateUnitAdded;
        public UnityEvent<IStateContainer, StateUnit> OnStateUnitAdded => onStateUnitAdded;

        [SerializeField]
        UnityEvent<IStateContainer, StateUnit> onStateUnitRemoved;
        public UnityEvent<IStateContainer, StateUnit> OnStateUnitRemoved => onStateUnitRemoved;

        #region public methods

        public void AddStateUnit(StateUnit stateUnit)
        {
            throw new System.NotImplementedException();
        }

        public T GetFirstStateUnit<T>() where T : StateUnit
        {
            throw new System.NotImplementedException();
        }

        public override GameObject GetReference(string name)
        {
            throw new System.NotImplementedException();
        }

        public T[] GetStateUnits<T>() where T : StateUnit
        {
            throw new System.NotImplementedException();
        }

        public override void Init()
        {
            if(inited) return;

            //add passive skills
            foreach (var addor in passiveSkillAddors)
            {
                if (addor != null)
                {
                    passiveSkills.Add(addor.AddPassiveSkill(this));
                }
            }

            inited = true;
        }

        public void RemoveStateUnit(StateUnit stateUnit)
        {
            throw new System.NotImplementedException();
        }

        public override void SetOrAddReference(string name, GameObject reference)
        {
            throw new System.NotImplementedException();
        }

        public override void Deinit()
        {
            foreach (var skill in passiveSkills)
            {
                skill.ToState(null);
            }
            passiveSkills.Clear();
        }

        #endregion
    }

}