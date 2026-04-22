using UnityEngine;
using System;

namespace ITF.EventChannels
{
    public class AudioClipEventChannelSO : ScriptableObject
    {
        public Action<AudioClip> OnPlayAudioClipRaised;
        public Action OnStopAudioClipRaised;
        public void RaisePlayAudioClip(AudioClip clip)
        {
            OnPlayAudioClipRaised?.Invoke(clip);
        }
        public void StopPlayAudioClip()
        {
            OnStopAudioClipRaised?.Invoke();
        }
    }
}