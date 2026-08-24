using UnityEngine;
using System;

namespace ITF.EventChannels
{
    [CreateAssetMenu(fileName = "Event Channel", menuName = "Events/Rect Event Channel")]
    public class RectEventChannelSO : ScriptableObject
    {
        public Action<Rect> OnEventRaised;
        public void RaiseEvent(Rect rect)
        {
            OnEventRaised?.Invoke(rect);
        }
    }
}