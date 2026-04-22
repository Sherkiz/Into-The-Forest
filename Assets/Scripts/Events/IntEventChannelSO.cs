using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/Int Event Channel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public Action<int> OnEventRaised;
        public void RaiseEvent(int i)
        {
            OnEventRaised?.Invoke(i);
        }
    }
}