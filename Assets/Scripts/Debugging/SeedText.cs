using UnityEngine;
using ITF.EventChannels;
using TMPro;

namespace ITF.Debugging
{
    public class SeedText : MonoBehaviour
    {
        [SerializeField] IntEventChannelSO onSeedInitialized;
        private TextMeshProUGUI seedText;
        private void Awake()
        {
            seedText = GetComponent<TextMeshProUGUI>();
            onSeedInitialized.OnEventRaised += (seed) => seedText.SetText(seed.ToString());
        }
    }
}
