using UnityEngine;

namespace ITF.World
{

    public class WorldManager : MonoBehaviour
    {
        static WorldManager instance;
        public static WorldManager Instance => instance;

        [SerializeField] Map map;
        public static Map Map => instance.map;

        bool isMapBuilt = false;
        public static bool IsMapBuilt => instance.isMapBuilt;

        public static void RebuildMap()
        {
            if (instance != null)
            {
                instance.map.onBuilt -= instance.OnRebuilt;
                instance.map.onBuilt += instance.OnRebuilt;
                instance.isMapBuilt = false;
                instance.map.Rebuild();
            }
        }

        void OnRebuilt(Map map)
        {
            isMapBuilt = true;
            map.onBuilt -= OnRebuilt;
        }

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }

}