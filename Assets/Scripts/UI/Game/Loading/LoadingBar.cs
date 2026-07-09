using ITF.EventChannels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ITF.UI
{
    public class LoadingBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private FloatEventChannelSO loadingValueEvent;

        private void OnEnable()
        {
            loadingValueEvent.OnEventRaised += SetLoadingValue;
        }
        private void OnDisable()
        {
            loadingValueEvent.OnEventRaised -= SetLoadingValue;
        }
        private void SetLoadingValue(float value)
        {
            slider.value = value;
            if (percentageText != null) percentageText.SetText($"{Mathf.RoundToInt(value * 100f)}%");
        }
    }
}