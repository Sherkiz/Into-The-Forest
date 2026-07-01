using ITF.EventChannels;
using UnityEngine;

namespace ITF.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private VoidEventChannelSO onLoadingCompleted;

        private void OnEnable()
        {
            onLoadingCompleted.OnEventRaised += SetInactive;
        }
        private void OnDisable()
        {
            onLoadingCompleted.OnEventRaised -= SetInactive;
        }

        private void SetInactive() => gameObject.SetActive(false);
    }
}