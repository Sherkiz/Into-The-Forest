using UnityEngine;

namespace ITF.StateMachine
{
    public abstract class StateAddor : ScriptableObject
    {
        public abstract void AddStates(IStateContainer stateContainer);
    }

}