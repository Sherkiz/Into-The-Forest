using ITF.CustomTiles;
using ITF.Math;
using ITF.Navigation;
using ITF.Utilities;
using ITF.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generate roads.
    /// </summary>
    [CreateAssetMenu(fileName = "RoadGenerator", menuName = "ITF/WorldGeneration/RoadGenerator")]
    public class RoadGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed { get => seed; set => seed = value; }

        [Space(20)]
        [Tooltip("The maximum traversal count per frame"), SerializeField]
        int maxTraversalPerFrame = 5000;

        [Space(20)]
        [SerializeField] private Tile roadTileHorizontal;
        [SerializeField] private Tile roadTileVertical;
        [SerializeField] private Tile roadTileTJonctionUp;
        [SerializeField] private Tile roadTileTJonctionDown;
        [SerializeField] private Tile roadTileTJonctionLeft;
        [SerializeField] private Tile roadTileTJonctionRight;
        [SerializeField] private Tile roadTileCornerUpLeft;
        [SerializeField] private Tile roadTileCornerUpRight;
        [SerializeField] private Tile roadTileCornerDownLeft;
        [SerializeField] private Tile roadTileCornerDownRight;
        [SerializeField] private Tile roadTile4Jonction;
        [SerializeField] private Tile roadTileDefault;

        private Dictionary<NeighboursConfig, Tile> tilesPerNeighboursConfig = new();

        [Space(20)]
        [SerializeField] private float roadGenerationPercentChance = 80f;
        [SerializeField] private float mainRoadGenerationPercentChance = 60f;
        [SerializeField] private int mainRoadXMax = 30;

        [Space(20)]
        [SerializeField] private Vector2Int[] pathfindingHierachies;
        [SerializeField] private int pathfindingMaxCost = 9999_9999;
        [SerializeField] private int pathfindingRoadCost = 4;
        [SerializeField] private int pathfindingDefaultCost = 10;
        [SerializeField] private int roadZ = 0;

        // Map the generate status to the task, 
        Dictionary<GenerateStatus, Task> statusTaskMap = new();
        private class NeighboursConfig
        {
            public bool left;
            public bool right;
            public bool up;
            public bool down;

            public bool[] bools
            {
                get
                {
                    return new bool[]{ left, right, up, down };
                }
            }

            public NeighboursConfig(Vector3Int position, List<Vector3Int> roadTilesPositionList)
            {
                left = roadTilesPositionList.Contains(position + Vector3Int.left);
                right = roadTilesPositionList.Contains(position + Vector3Int.right);
                up = roadTilesPositionList.Contains(position + Vector3Int.up);
                down = roadTilesPositionList.Contains(position + Vector3Int.down);
            }
            public NeighboursConfig(bool[] bools)
            {
                left = bools[0];
                right = bools[1];
                up = bools[2];
                down = bools[3];
            }
            public override bool Equals(object obj)
            {
                NeighboursConfig other = obj as NeighboursConfig;
                if (other == null)
                {
                    return false;
                }
                else
                {
                    return (left == other.left && right == other.right && up == other.up && down == other.down);
                }
            }
            public override int GetHashCode()
            {
                int result = 29;
                foreach (bool b in bools) {
                    if (b) result++;
                    result *= 23;
                }
                return result;
            }
            public static bool operator == (NeighboursConfig left, NeighboursConfig right)
            {
                if (left is null) return right is null;
                return left.Equals(right);
            }

            public static bool operator != (NeighboursConfig left, NeighboursConfig right)
            {
                return !(left == right);
            }
        }

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
        private void InitializeRoadTilesMapper()
        {
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, false, false, false })] = roadTileDefault;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, false, false, false })] = roadTileHorizontal;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, true, false, false })] = roadTileHorizontal;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, false, true, false })] = roadTileVertical;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, false, false, true })] = roadTileVertical;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, true, false, false })] = roadTileHorizontal;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, false, true, false })] = roadTileCornerUpLeft;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, false, false, true })] = roadTileCornerDownLeft;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, true, true, false })] = roadTileCornerUpRight;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, true, false, true })] = roadTileCornerDownRight;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, false, true, true })] = roadTileVertical;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, true, true, false })] = roadTileTJonctionUp;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, true, false, true })] = roadTileTJonctionDown;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, false, true, true })] = roadTileTJonctionLeft;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { false, true, true, true })] = roadTileTJonctionRight;
            tilesPerNeighboursConfig[new NeighboursConfig(new bool[] { true, true, true, true })] = roadTile4Jonction;
        }
        private void SetRoadTiles(List<Vector3Int> roadTilesPositionList, TilemapManager tilemap)
        {
            InitializeRoadTilesMapper();
            foreach (var pos in roadTilesPositionList)
            {
                NeighboursConfig neighboursConfig = new(pos, roadTilesPositionList);
                NeighboursConfig neighboursConfig2 = new(pos, roadTilesPositionList);

                tilemap.SetTile(pos, tilesPerNeighboursConfig[neighboursConfig]);
            }
        }
        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            var bounds = tilemap.cellBounds;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            float value = random.Range(0f, 100f);
            List<Vector3Int> roadTilesPositionList = new();
            if (value < roadGenerationPercentChance)
            {                
                PathFinder pathFinder = BuildPathFinder(tilemap);
                MapObject[] playerBuildings = WorldManager.Map.GetMapObjectsOfType(TileType.Building);
                MapObject firstBuilding = playerBuildings[0];
                for (int i = 1; i < playerBuildings.Length; i++)
                {
                    MapObject building = playerBuildings[i];
                    ResultPath path = pathFinder.FindPath(firstBuilding.range.position, building.range.position, false);
                    Vector3Int lastRoadPos = (Vector3Int) firstBuilding.range.position;
                    Debug.Log(building.name);
                    Debug.Log(building.range.position);
                    if (path.path != null)
                    {
                        foreach (var pos in path.path)
                        {
                            Vector3Int newRoadPos = new Vector3Int(pos.x + bounds.xMin, pos.y + bounds.yMin, roadZ);
                            if (lastRoadPos != Vector3Int.zero)
                            {
                                if (Mathf.Abs(lastRoadPos.x - newRoadPos.x) > 1)
                                {
                                    for (int x = Mathf.Min(lastRoadPos.x, newRoadPos.x); x <= Mathf.Max(lastRoadPos.x, newRoadPos.x); x++)
                                    {
                                        roadTilesPositionList.Add(new Vector3Int(x, pos.y + bounds.yMin, roadZ));
                                        pathFinder.UpdateMap(new Vector2Int(x, pos.y + bounds.yMin), new int[][] { new int[] { pathfindingRoadCost } }); //Could be done in one call instead of calling it in the loop
                                    }
                                }
                                if (Mathf.Abs(lastRoadPos.y - newRoadPos.y) > 1)
                                {
                                    for (int y = Mathf.Min(lastRoadPos.y, newRoadPos.y); y <= Mathf.Max(lastRoadPos.y, newRoadPos.y); y++)
                                    {
                                        roadTilesPositionList.Add(new Vector3Int(pos.x, y, roadZ));
                                        pathFinder.UpdateMap(new Vector2Int(pos.x, pos.y), new int[][] { new int[] { pathfindingRoadCost } });
                                    }
                                }
                            }
                            lastRoadPos = newRoadPos;
                            roadTilesPositionList.Add(lastRoadPos);
                            pathFinder.UpdateMap(new Vector2Int(lastRoadPos.x, lastRoadPos.y), new int[][] { new int[] { pathfindingRoadCost } });
                        }
                    }
                    else
                    {
                        throw new Exception("Path not found between buildings to generate road!");
                    }
                }
                SetRoadTiles(roadTilesPositionList, tilemap);
            }
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);

            yield break;
        }
        private PathFinder BuildPathFinder(TilemapManager tilemap)
        {
            var bounds = tilemap.cellBounds;
            Vector3Int size = bounds.size;
            List<List<int>> map = new(size.x);
            for (int x = 0; x < size.x; x++)
            {
                int posX = x + bounds.xMin;
                map.Add(new(size.y));
                for (int y = 0; y < size.y; y++)
                {
                    int posY = y + bounds.yMin;
                    int cost = (tilemap.GetTile(new Vector3Int(posX, posY, 0)) != null) ? pathfindingMaxCost : pathfindingDefaultCost;
                    map[x].Add(cost);
                }
            }
            return new PathFinder(map, pathfindingHierachies, pathfindingDefaultCost, pathfindingMaxCost);
        }
    }

}