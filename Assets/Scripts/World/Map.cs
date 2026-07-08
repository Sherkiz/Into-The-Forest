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

        [SerializeField]
        public List<MapObject> mapObjectList = new();

        Tilemap pathfindingTilemap;
        public Tilemap PathfindingTilemap
        {
            get
            {
                if (pathfindingTilemap == null)
                {
                    if (!tilemaps.TryGetValue(pathfindingMapName, out pathfindingTilemap))
                    {
                        throw new Exception("Pathfinding map not found: " + pathfindingMapName);
                    }
                }
                return pathfindingTilemap;
            }
        }

        Task rebuildTask;
        PathFinder pathFinder;

        /// <summary>
        /// Triggered after the map is built.
        /// </summary>
        public Action<Map> onBuilt;

        public void Init()
        {
            tilemaps = new Dictionary<string, Tilemap>();
            foreach (var tilemap in _tilemaps)
            {
                tilemaps.Add(tilemap.name, tilemap);
            }
        }

        public TileBase GetTileOnPathfingTilemap(Vector2Int index)
        {
            if (tilemaps.TryGetValue(pathfindingMapName, out Tilemap tilemap))
            {
                for(int z = tilemap.cellBounds.zMin; z < tilemap.cellBounds.zMax; z++)
                {
                    var tile = tilemap.GetTile(new Vector3Int(index.x, index.y, z));
                    if (tile != null)
                    {
                        return tile;
                    }
                }
            }
            return null;
        }

        public bool IsPassable(Vector2Int cell)
        {
            if(cell.x < 0 || cell.y < 0 || cell.x >= PathfindingTilemap.size.x || cell.y >= PathfindingTilemap.size.y)
            {
                return false;
            }
            return pathFinder.GetCost(cell) < maxCosts;
        }

        public void Rebuild()
        {
            if(rebuildTask != null && rebuildTask.Running)
            {
                rebuildTask.Stop();
            }
            rebuildTask = new Task(BuildMap());
        }

        public ResultPath FindPath(Vector2Int startPoint, Vector2Int endPoint, bool isAbstracth = false)
        {
            if (pathFinder == null)
            {
                throw new Exception("Map not built yet.");
            }
            return pathFinder.FindPath(startPoint, endPoint, isAbstracth);
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

        public void DrawGizmos()
        {
            pathFinder?.DrawGizmos(pathfindingTilemap);
        }

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
                        int cost = defaultCost;
                        for (int z = bound.zMin; z < bound.zMax; z++)
                        {
                            var tile = tilemap.GetTile(new Vector3Int(x, y, z));
                            if (tile != null)
                            {
                                cost = (tile is ICustomTile customTile) ? Mathf.Clamp(customTile.PassCost, 0, maxCosts) : maxCosts;
                                break;
                            }

                            if(++counter >= maxTraverse)
                            {
                                yield return null;
                                counter = 0;
                            }
                        }
                        costList.Add(cost);
                    }
                }
            }
            else
            {
                throw new Exception("Pathfinding map not found: " + pathfindingMapName);
            }

            yield return null;

            pathFinder = new PathFinder(costs, hierachies, defaultCost, maxCosts, 4);

            onBuilt?.Invoke(this);
            rebuildTask = null;
        }
    }

    public class MapObject
    {
        public readonly string name;
        public readonly RectInt range;
        public readonly TileType type;
        public readonly Vector3Int entranceOffset;
        public Vector3Int pathEntrancePosition { get => new Vector3Int(range.xMin, range.yMin) + entranceOffset; }
        public MapObject(string name, RectInt range, TileType type, Vector3Int entranceOffset)
        {
            this.name = name;
            this.range = range;
            this.type = type;
            this.entranceOffset = entranceOffset;
        }
        public MapObject(MultipleTilesObject multipleTilesObject, RectInt range)
        {
            name = multipleTilesObject.name;
            this.range = range;
            type = multipleTilesObject.mapObjectType;
            if (multipleTilesObject is MultipleTilesBuilding building) entranceOffset = building.posOffsets[building.entranceTileIndex];
        }
    }

}