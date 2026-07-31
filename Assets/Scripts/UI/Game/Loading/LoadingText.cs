using ITF.EventChannels;
using TMPro;
using UnityEngine;

namespace ITF.UI
{
    public class LoadingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textUI;
        [SerializeField] private StringEventChannelSO loadingMessageEvent;

        private void OnEnable()
        {
            loadingMessageEvent.OnEventRaised += SetLoadingText;
        }
        private void OnDisable()
        {
            loadingMessageEvent.OnEventRaised -= SetLoadingText;
        }
        private void SetLoadingText(string text) => textUI.SetText(text);
    }
}