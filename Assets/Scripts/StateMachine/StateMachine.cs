using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.StateMachine
{
    public class StateMachine : MonoBehaviour
    {
        static StateMachine instance;
        public static StateMachine Instance => instance;

        HashSet<StateUnit> stateUnits = new();

        HashSet<StateUnit> addingUnits = new();

        HashSet<StateUnit> removingUnits = new();

        #region 公共方法

        public bool AddStateUnit(StateUnit stateUnit)
        {
            if (stateUnit == null) return false;

            addingUnits.Add(stateUnit);
            return true;
        }

        public void RemoveStateUnit(StateUnit stateUnit)
        {
            if(stateUnit != null)
            {
                addingUnits.Remove(stateUnit);
                removingUnits.Add(stateUnit);
            }
        }

        public bool Contains(StateUnit stateUnit)
        {
            if (addingUnits.Contains(stateUnit)) return true;

            return stateUnits.Contains(stateUnit) && !removingUnits.Contains(stateUnit);
        }

        #endregion

        #region 内部方法

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            UpdateStateUnits();

            stateUnits.RemoveWhere(_ => _.Active == null);

            foreach(StateUnit state in stateUnits)
            {
                try
                {
                    state.Active.value.onUpdate?.Invoke(state, Time.deltaTime);
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e.Message+"\n\n"+e.StackTrace);
                    state.Active = null;
                }
            }
        }

        void UpdateStateUnits()
        {
            foreach (StateUnit unit in removingUnits) stateUnits.Remove(unit);
            foreach (StateUnit unit in addingUnits) stateUnits.Add(unit);
        }

        #endregion
    }

    public abstract class StateUnit
    {
        [Tooltip("Currently active StateUnit")]
        public abstract StatePair<string, StateActions> Active { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="actions"></param>
        /// <returns>Is the addition successful</returns>
        public abstract bool AddState(string name, StateActions actions);

        public abstract void RemoveState(string name);

        public abstract void RemoveAllStates();

        /// <summary>
        /// Switch to the state with the specified name
        /// </summary>
        /// <param name="name">if null, it will cause the state machine to remove this state unit</param>
        /// <returns>Whether the switch was successful, if already in the state, it returns false</returns>
        public abstract bool ToState(string name);
    }

    public struct StateActions
    {
        public UnityAction<StateUnit> onEntry;

        [Tooltip("Triggered every frame after entering the state (usually starts triggering one frame after onEntry), the second parameter is the time interval")]
        public UnityAction<StateUnit, float> onUpdate;

        public UnityAction<StateUnit> onExit;

        public StateActions(UnityAction<StateUnit> onEntry, UnityAction<StateUnit, float> onUpdate, UnityAction<StateUnit> onExit)
        {
            this.onEntry = onEntry;
            this.onUpdate = onUpdate;
            this.onExit = onExit;
        }
    }

    public class SimpleStateUnit : StateUnit
    {
        protected StatePair<string, StateActions> active = null;
        public override StatePair<string, StateActions> Active { get => active; set { } }

        protected Dictionary<string, StateActions> states = new();

        public override bool AddState(string name, StateActions actions)
        {
            if (name == null || states.ContainsKey(name)) return false;

            states.Add(name, actions);
            return true;
        }

        public override void RemoveState(string name)
        {
            if (active != null && active.key.Equals(name))
            {
                active.value.onExit?.Invoke(this);
                active = null;
            }
            states.Remove(name);
        }

        public override void RemoveAllStates()
        {
            states.Clear();
            if (active != null) active.value.onEntry?.Invoke(this);
            active = null;
        }

        public override bool ToState(string name)
        {
            if (active != null && active.key.Equals(name)) return false;

            var lastState = active;
            if(name == null)
            {
                active = null;
                if (lastState != null)
                {
                    lastState.value.onExit?.Invoke(this);
                }
                return true;
            }

            if(states.TryGetValue(name, out StateActions action))
            {
                active = new()
                {
                    key = name,
                    value = action
                };
                if (lastState != null) lastState.value.onExit?.Invoke(this);
                active.value.onEntry?.Invoke(this);

                return true;
            }

            return false;
        }

        public virtual StateUnit Copy()
        {
            SimpleStateUnit unit = (SimpleStateUnit)MemberwiseClone();
            unit.states = new(states);
            if(active != null)
            {
                unit.active = new StatePair<string, StateActions>()
                {
                    key = active.key,
                    value = unit.states[active.key]
                };
            }

            return unit;
        }
    }

    public class StatePair<T, U>
    {
        public T key;
        public U value;
    }

}