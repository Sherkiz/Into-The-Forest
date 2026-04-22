using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/Float Event Channel")]
    public class FloatEventChannelSO : ScriptableObject
    {
        public Action<float> OnEventRaised;
        public void RaiseEvent(float f)
        {
            OnEventRaised?.Invoke(f);
        }
    }
}