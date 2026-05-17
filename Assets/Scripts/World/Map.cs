using ITF.CustomTiles;
using ITF.Navigation;
using ITF.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.World
{
    [System.Serializable]
    public class Map
    {
        [SerializeField]
        Tilemap[] _tilemaps;
        Dictionary<string, Tilemap> tilemaps;
        public string pathfindingMapName;
        [SerializeField]
        public int maxCosts = 9999_9999;
        [SerializeField]
        public int defaultCost = 10;
        [SerializeField, Tooltip("From low to high")]
        public Vector2Int[] hierachies;

        [Space(20)]
        public int maxTraverse = 5_000;

        List<MapObject> mapObjectList = new();

        Task rebuildTask;
        PathFinder pathFinder;

        /// <summary>
        /// Triggered after the map is built.
        /// </summary>
        public Action<Map> onBuilt;

        public void Rebuild()
        {
            if(rebuildTask != null && rebuildTask.Running)
            {
                rebuildTask.Stop();
            }
            rebuildTask = new Task(BuildMap());
        }

        public ResultPath FindPath(Vector2Int startPoint, Vector2Int endPoint)
        {
            if (pathFinder == null)
            {
                throw new Exception("Map not built yet.");
            }
            return pathFinder.FindPath(startPoint, endPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint">starting index of the update area</param>
        /// <param name="costs">updated costs</param>
        public void UpdateMap(Vector2Int startPoint, int[][] costs)
        {
            pathFinder.UpdateMap(startPoint, costs);
        }

        public void AddMapObject(MapObject mapObject)
        {
            mapObjectList.Add(mapObject);
        }

        public MapObject[] GetMapObjects() => mapObjectList.ToArray();
        public MapObject[] GetMapObjectsOfType(TileType tileType) => mapObjectList.Where(obj => obj.type == tileType).ToArray();

        IEnumerator BuildMap()
        {
            List<List<int>> costs;
            if (tilemaps.TryGetValue(pathfindingMapName, out Tilemap tilemap))
            {
                var bound = tilemap.cellBounds;
                var size = bound.size;
                costs = new List<List<int>>(size.x);
                int counter = 0;
                for(int x = bound.xMin; x < bound.xMax; x++)
                {
                    List<int> costList = new List<int>(size.y);
                    costs.Add(costList);
                    for(int y = bound.yMin; y < bound.yMax; y++)
                    {
                        for(int z = bound.zMin; z < bound.zMax; z++)
                        {
                            var tile = tilemap.GetTile(new Vector3Int(x, y, z));
                            if (tile != null)
                            {
                                int cost = (tile is ICustomTile customTile) ? customTile.PassCost : maxCosts;
                                costList.Add(cost);
                            }
                            else
                            {
                                costList.Add(defaultCost);
                            }

                            if(++counter >= maxTraverse)
                            {
                                yield return null;
                                counter = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Pathfinding map not found: " + pathfindingMapName);
            }

            yield return null;

            pathFinder = new PathFinder(costs, hierachies, defaultCost, maxCosts);

            onBuilt?.Invoke(this);
            rebuildTask = null;
        }
    }

    public class MapObject
    {
        public readonly string name;
        public readonly RectInt range;
        public readonly TileType type;

        public MapObject(string name, RectInt range, TileType type)
        {
            this.name = name;
            this.range = range;
            this.type = type;
        }
    }

}