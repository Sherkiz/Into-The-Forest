using ITF.CustomTiles;
using ITF.Math;
using ITF.Navigation;
using ITF.Utilities;
using ITF.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Ensure that all objects are passable between them
    /// </summary>
    [CreateAssetMenu(fileName = "PassableGenerator", menuName = "ITF/WorldGeneration/PassableGenerator")]
    public class PassableGenerator : ObjectGenerator
    {

        int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }

        [Tooltip("How many path to be foundd  per frame")]
        public int passCountPerFrame = 10;

        [Header("PathFinder")]
        public int maxCosts = 9999_9999;
        public int defaultCost = 10;
        [Tooltip("From low to high")]
        public Vector2Int[] hierachies;
        public int pathWidth = 3;

        [Space(20)]
        public float noiseScale = .1f;
        public Vector2 noiseRange = new(0, 100);

        [Header("Tree tile")]
        public Tile treeTopLeft;
        public Tile treeBottomRight;
        public Tile treeTopRight;
        public Tile treeBottomLeft;

        [Space(20)]
        public int topZ = 1;
        public int bottomZ = 0;

        // Map the generate status to the task, 
        Dictionary<GenerateStatus, Task> statusTaskMap = new();

        public override GenerateStatus Generate(TilemapManager tilemap)
        {
            GenerateStatus generateStatus = new();
            statusTaskMap.Add(generateStatus, new(GenerateCoroutine(generateStatus, tilemap)));
            return generateStatus;
        }

        public override void StopAllGeneration()
        {
            foreach (var pair in statusTaskMap)
            {
                pair.Value.Stop();
                pair.Key.failed = !pair.Key.finished;
            }
            statusTaskMap.Clear();
        }

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            PathFinder pathFinder = BuildPathFinder(tilemap, tilemap.cellBounds, random);
            generateStatus.progress = .5f;
            yield return null;

            var mapObjects = WorldManager.Map.GetMapObjects();
            if(mapObjects.Length == 0)
            {
                generateStatus.progress = 1;
                generateStatus.finished = true;
                statusTaskMap.Remove(generateStatus);
                yield break;
            }

            int counter = 0;
            MapObject object1 = mapObjects[0];
            for(int i = 1; i < mapObjects.Length; i++)
            {
                MapObject object2 = mapObjects[i];
                Vector2Int startPos = Vector2Int.zero;
                Vector2Int endPos = Vector2Int.zero;

                if(object1.range.xMin < object2.range.xMin)
                {
                    startPos.x = object1.range.xMax;
                    endPos.x = object2.range.xMin - 1;
                }
                else
                {
                    startPos.x = object2.range.xMin - 1;
                    endPos.x = object1.range.xMax;
                }
                if(object1.range.yMin < object2.range.yMin)
                {
                    startPos.y = object1.range.yMax;
                    endPos.y = object2.range.yMin - 1;
                }
                else
                {
                    startPos.y = object2.range.yMin - 1;
                    endPos.y = object1.range.yMax;
                }

                var path = pathFinder.FindPath(startPos, endPos);
                Vector2Int pos1 = startPos;
                for(int j = 0; j < path.path.Count; j++)
                {
                    Vector2Int pos2 = path.path[j];
                    if(pos1.x == pos2.x)
                    {
                        int yMax = Mathf.Max(pos1.y, pos2.y);
                        for (int y = Mathf.Min(pos1.y, pos2.y); y <= yMax; y++)
                        {
                            int xMin = pos1.x - pathWidth / 2;
                            for (int x = 0; x < pathWidth; x++)
                            {
                                TryRemoveTree(tilemap, xMin + x, y);
                            }
                        }
                    }
                    else
                    {
                        int xMax = Mathf.Max(pos1.x, pos2.x);
                        for (int x = Mathf.Min(pos1.x, pos2.x); x <= xMax; x++)
                        {
                            int yMin = pos1.y - pathWidth / 2;
                            for (int y = 0; y < pathWidth; y++)
                            {
                                TryRemoveTree(tilemap, x, yMin + y);
                            }
                        }
                    }
                    pos1 = pos2;
                }

                object1 = object2;

                if (++counter >= passCountPerFrame)
                {
                    counter = 0;
                    generateStatus.progress = .5f + (float)(i + 1) / mapObjects.Length * .5f;
                    yield return null;
                }
            }
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);

            yield break;
        }

        PathFinder BuildPathFinder(TilemapManager tilemap, BoundsInt bounds, XorShiftRandom random)
        {
            Vector2 noiseStart = new(random.Range(noiseRange.x, noiseRange.y), random.Range(noiseRange.x, noiseRange.y));

            var size = bounds.size;
            List<List<int>> map = new(size.x);
            for (int x = 0; x < size.x; x++)
            {
                int xPos = x + bounds.xMin;
                map.Add(new List<int>(size.y));
                for (int y = 0; y < size.y; y++)
                {
                    int yPos = y + bounds.yMin;
                    bool passable = true;
                    for (int z = bounds.zMin; z < bounds.zMax; z++)
                    {
                        TileBase tile = tilemap.GetTile(new Vector3Int(xPos, yPos, z));
                        if(tile != null)
                        {
                            passable = IsTreeTile(tile);
                            break;
                        }
                    }
                    map[x].Add(passable ?
                        (int)(Mathf.PerlinNoise(noiseStart.x + x * noiseScale, noiseStart.y + y * noiseScale) * noiseRange.y + noiseRange.x)
                        : maxCosts);
                }
            }
            Debug.Log($"map: {map.Count}, {map[0].Count}");
            return new PathFinder(map, hierachies, defaultCost, maxCosts);
        }

        bool IsTreeTile(TileBase tile) =>
            tile == treeBottomLeft || tile == treeBottomRight || tile == treeTopLeft || tile == treeTopRight;

        bool IsTreeTile(TilemapManager tilemap, int x, int y, int z)
        {
            TileBase tile = tilemap.GetTile(new Vector3Int(x, y, z));
            return tile == treeBottomLeft || tile == treeBottomRight || tile == treeTopLeft || tile == treeTopRight;
        }

        void TryRemoveTree(TilemapManager tilemap, int x, int y)
        {
            Vector3Int pos = new(x, y, bottomZ);
            TileBase tile = tilemap.GetTile(pos);
            if (tile == null) return;
            Vector3Int startPos = Vector3Int.zero;
            bool removable = false;
            if(tile == treeBottomLeft)
            {
                startPos = new Vector3Int(x, y, bottomZ);
                removable = true;
            }
            else if(tile == treeBottomRight)
            {
                startPos = new Vector3Int(x - 1, y, bottomZ);
                removable = true;
            }
            else
            {
                tile = tilemap.GetTile(new Vector3Int(x, y, topZ));
                if(tile == treeTopLeft)
                {
                    startPos = new Vector3Int(x, y - 1, bottomZ);
                    removable = true;
                }
                else if (tile == treeTopRight)
                {
                    startPos = new Vector3Int(x - 1, y - 1, bottomZ);
                    removable = true;
                }
            }

            if (removable)
            {
                tilemap.SetTile(startPos, null);
                startPos.x += 1;
                tilemap.SetTile(startPos, null);
                startPos.y += 1;
                startPos.z = topZ;
                tilemap.SetTile(startPos, null);
                startPos.x -= 1;
                tilemap.SetTile(startPos, null);
            }
        }
    }

}