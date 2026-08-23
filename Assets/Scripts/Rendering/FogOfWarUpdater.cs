using UnityEngine;

namespace ITF.Rendering.FOW
{
    public class FogOfWarUpdater : MonoBehaviour
    {
        private FogOfWarRenderingFeature feature;
        [SerializeField] private Material mat;
        private void Update()
        {
            feature = FogOfWarRenderingFeature.Instance;
            if (feature == null) return;

            var texture = feature.GetFogOfWarTexture();
            if (texture != null) mat.SetTexture("_MainTex", texture);
        }
    }
}
