using UnityEngine;

namespace ITF.Utilities
{
    public static class GameObjectMethods
    {
        public static void SetLayerAllChildren(this GameObject GO, int layer, bool includeInactive = true)
        {
            var children = GO.GetComponentsInChildren<Transform>(includeInactive : includeInactive);
            GO.layer = layer;
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
        }
    }

}