using MBT;
using UnityEngine;

namespace ITF.BehaviourTree
{

    public static class BlackboardExt
    {
        public static void SetValue<T, U>(this Blackboard blackboard, string key, U value) where T : Variable<U>
        {
            var variable = blackboard.GetVariable<T>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        public static bool TryGetValue<T, U>(this Blackboard blackboard, string key, out U value) where T : Variable<U>
        {
            var variable = blackboard.GetVariable<T>(key);
            if (variable != null)
            {
                value = variable.Value;
                return true;
            }
            value = default;
            return false;
        }
    }
}
