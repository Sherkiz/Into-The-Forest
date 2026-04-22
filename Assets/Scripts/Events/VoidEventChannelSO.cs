using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public Action OnEventRaised;
        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}