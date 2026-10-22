using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        [Header("Subarea")]
        [SerializeField] Vector2 subareaSize = new(10, 10);
        Dictionary<Type, List<HashSet<Component>>> subareaObjects;
        Vector2 mapMinPosition;
        Vector2 mapMaxPosition;
        Vector2Int subareaGrid;

        #region events

        [SerializeField]
        UnityEvent onWorldGenerated;
        public static UnityEvent OnWorldGenerated => instance.onWorldGenerated;

        #endregion

        #region public methods

        public static void AddSubareaObject<T>(T obj) where T : Component
        {
            if (instance == null || instance.subareaObjects == null) return;
            if(!instance.subareaObjects.TryGetValue(typeof(T), out var list))
            {
                list = instance.CreateTypeForSubarea<T>();
            }
            int index = instance.IndexFromPosition(obj.transform.position);
            list[index].Add(obj);
        }

        public static void UpdateSubareaObject<T>(T obj, Vector2 oldPosition) where T : Component
        {
            if (instance == null || instance.subareaObjects == null) return;
            int oldIndex = instance.IndexFromPosition(oldPosition);
            int index = instance.IndexFromPosition(obj.transform.position);
            if(oldIndex == index) return;

            if (!instance.subareaObjects.TryGetValue(typeof(T), out var list))
            {
                list = instance.CreateTypeForSubarea<T>();
            }
            list[oldIndex].Remove(obj);
            list[index].Add(obj);
        }

        public static bool ContainsSubareaObject<T>(T obj, Vector2 position) where T : Component
        {
            if (instance == null || instance.subareaObjects == null) return false;
            if (instance.subareaObjects.TryGetValue(typeof(T), out var list))
            {
                int index = instance.IndexFromPosition(position);
                return list[index].Contains(obj);
            }
            return false;
        }

        public static void RemoveSubareaObject<T>(T obj, Vector2 position) where T : Component
        {
            if (instance == null || instance.subareaObjects == null) return;
            if (instance.subareaObjects.TryGetValue(typeof(T), out var list))
            {
                int index = instance.IndexFromPosition(position);
                list[index].Remove(obj);
            }
        }

        /// <summary>
        /// Don't use this method unless you are sure the object is not in any subarea. This is a fallback method to remove an object from all subareas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void RemoveSubareaObject<T>(T obj) where T : Component
        {
            if (instance == null || instance.subareaObjects == null) return;
            if (instance.subareaObjects.TryGetValue(typeof(T), out var list))
            {
                foreach(var objects in list)
                {
                    objects.Remove(obj);
                }
            }
        }

        public static List<Component> GetSubareaObjectWithin<T>(Vector2 center, float distance) where T : Component
        {
            if(instance == null || instance.subareaObjects == null) return new();
            if(instance.subareaObjects.ContainsKey(typeof(T)) == false) return new();

            var indices = instance.GetIndicesWithin(center, distance);
            List<Component> result = new();
            foreach(int index in indices)
            {
                var list = instance.subareaObjects[typeof(T)][index];
                foreach(var obj in list)
                {
                    if (Vector2.Distance(center, obj.transform.position) <= distance)
                    {
                        result.Add(obj);
                    }
                }
            }
            return result;
        }

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

        #endregion

        #region private methods

        int IndexFromPosition(Vector2 position)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt((position.x - mapMinPosition.x) / subareaSize.x), 0, subareaGrid.x - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((position.y - mapMinPosition.y) / subareaSize.y), 0, subareaGrid.y - 1);
            return y * subareaGrid.x + x;
        }

        List<int> GetIndicesWithin(Vector2 center, float distance)
        {
            List<int> indices = new();
            Vector2 min = center - new Vector2(distance, distance);
            Vector2 max = center + new Vector2(distance, distance);
            int minX = Mathf.Clamp(Mathf.FloorToInt((min.x - mapMinPosition.x) / subareaSize.x), 0, subareaGrid.x - 1);
            int maxX = Mathf.Clamp(Mathf.FloorToInt((max.x - mapMinPosition.x) / subareaSize.x), 0, subareaGrid.x - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt((min.y - mapMinPosition.y) / subareaSize.y), 0, subareaGrid.y - 1);
            int maxY = Mathf.Clamp(Mathf.FloorToInt((max.y - mapMinPosition.y) / subareaSize.y), 0, subareaGrid.y - 1);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    indices.Add(y * subareaGrid.x + x);
                }
            }
            return indices;
        }

        List<HashSet<Component>> CreateTypeForSubarea<T>() where T : Component
        {
            if (subareaObjects == null) subareaObjects = new();
            if(subareaObjects.TryGetValue(typeof(T), out var list))
            {
                return list;
            }
            list = new();
            subareaObjects.Add(typeof(T), list);
            for (int i = 0; i < subareaGrid.x * subareaGrid.y; i++)
            {
                list.Add(new());
            }
            return list;
        }

        void OnRebuilt(Map map)
        {
            isMapBuilt = true;
            var bound = Map.PathfindingTilemap.cellBounds;
            mapMinPosition = Map.PathfindingTilemap.CellToWorld(bound.min);
            mapMaxPosition = Map.PathfindingTilemap.CellToWorld((bound.max + Vector3Int.one));
            subareaGrid = new(Mathf.CeilToInt((mapMaxPosition.x - mapMinPosition.x) / subareaSize.x),
                Mathf.CeilToInt((mapMaxPosition.y - mapMinPosition.y) / subareaSize.y));

            map.onBuilt -= OnRebuilt;
        }

        private void Awake()
        {
            if (instance == null) instance = this;
            Map.mapObjectList = new();
        }

        private void Start()
        {
            map.Init();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        #endregion
    }

}