using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/Vector2 Event Channel")]
    public class Vector2EventChannelSO : ScriptableObject
    {
        public Action<Vector2> OnEventRaised;
        public void RaiseEvent(Vector2 vect)
        {
            OnEventRaised?.Invoke(vect);
        }
    }
}