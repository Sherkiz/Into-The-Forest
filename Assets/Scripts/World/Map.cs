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
        Dictionary<string, List<MapObject>> mapObjectDict = new();

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
        PathFinder pathFinderEightWay;

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

        public bool GetNearestPassableCell(Vector2Int cell, RectInt range, out Vector2Int passableCell)
        {
            passableCell = cell;
            if (IsPassable(cell)) return true;

            List<Vector2Int> opening = new();
            List<Vector2Int> closed = new();
            closed.Add(cell);

            if(cell.y < range.yMax) opening.Add(cell + Vector2Int.up);
            if(cell.y > range.yMin) opening.Add(cell + Vector2Int.down);
            if(cell.x > range.xMin) opening.Add(cell + Vector2Int.left);
            if(cell.x < range.xMax) opening.Add(cell + Vector2Int.right);
            while(opening.Count > 0)
            {
                var current = opening[0];
                opening.RemoveAt(0);
                closed.Add(current);
                if (IsPassable(current))
                {
                    passableCell = current;
                    return true;
                }
                if (current.y < range.yMax && !closed.Contains(current + Vector2Int.up)) opening.Add(current + Vector2Int.up);
                if (current.y > range.yMin && !closed.Contains(current + Vector2Int.down)) opening.Add(current + Vector2Int.down);
                if (current.x > range.xMin && !closed.Contains(current + Vector2Int.left)) opening.Add(current + Vector2Int.left);
                if (current.x < range.xMax && !closed.Contains(current + Vector2Int.right)) opening.Add(current + Vector2Int.right);
            }

            return false;
        }

        public void Rebuild()
        {
            if(rebuildTask != null && rebuildTask.Running)
            {
                rebuildTask.Stop();
            }
            rebuildTask = new Task(BuildMap());
        }

        public ResultPath FindPath(Vector2Int startPoint, Vector2Int endPoint, bool eightWay = false, bool isAbstracth = false)
        {
            if (pathFinder == null)
            {
                throw new Exception("Map not built yet.");
            }
            return eightWay ? pathFinderEightWay.FindPath(startPoint, endPoint, isAbstracth) :
                pathFinder.FindPath(startPoint, endPoint, isAbstracth);
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
            if (mapObjectList.Contains(mapObject)) return;
            mapObjectList.Add(mapObject);
            if (!mapObjectDict.TryGetValue(mapObject.name, out List<MapObject> list))
            {
                list = new List<MapObject>();
                mapObjectDict.Add(mapObject.name, list);
            }
            list.Add(mapObject);
        }

        public MapObject[] GetMapObjects() => mapObjectList.ToArray();
        public MapObject[] GetMapObjectsOfType(TileType tileType) => mapObjectList.Where(obj => obj.type == tileType).ToArray();

        public MapObject[] GetMapObjectsByName(string name)
        {
            if (mapObjectDict.TryGetValue(name, out List<MapObject> list))
            {
                return list.ToArray();
            }
            return Array.Empty<MapObject>();
        }

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

            yield return null;

            pathFinderEightWay = new PathFinder(costs, hierachies, defaultCost, maxCosts, 4, true);

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
            if (multipleTilesObject is MultipleTilesBuilding building) entranceOffset = building.posOffsets[building.entranceTileIndex] + Vector3Int.down;
        }
    }

}