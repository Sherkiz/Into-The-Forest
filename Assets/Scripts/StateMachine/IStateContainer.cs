using UnityEngine.Events;

namespace ITF.StateMachine
{
    public interface IStateContainer
    {
        public UnityEvent<IStateContainer, StateUnit> OnStateUnitAdded { get; }

        public UnityEvent<IStateContainer, StateUnit> OnStateUnitRemoved { get; }

        public void AddStateUnit(StateUnit stateUnit);

        public T[] GetStateUnits<T>() where T : StateUnit;

        public T GetFirstStateUnit<T>() where T : StateUnit;

        public void RemoveStateUnit(StateUnit stateUnit);
    }

}