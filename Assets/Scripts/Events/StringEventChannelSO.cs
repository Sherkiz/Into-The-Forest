using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/String Event Channel")]
    public class StringEventChannelSO : ScriptableObject
    {
        public Action<string> OnEventRaised;
        public void RaiseEvent(string str)
        {
            OnEventRaised?.Invoke(str);
        }
    }
}