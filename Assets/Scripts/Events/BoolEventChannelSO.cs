using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/Bool Event Channel")]
    public class BoolEventChannelSO : ScriptableObject
    {
        public Action<bool> OnEventRaised;
        public void RaiseEvent(bool boolean)
        {
            OnEventRaised?.Invoke(boolean);
        }
    }
}