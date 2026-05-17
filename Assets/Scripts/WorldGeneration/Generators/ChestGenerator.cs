using ITF.CustomTiles;
using ITF.Math;
using ITF.Utilities;
using ITF.World;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generates chests near spawners.
    /// </summary>
    [CreateAssetMenu(fileName = "ChestGenerator", menuName = "ITF/WorldGeneration/ChestGenerator")]

    public class ChestGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }
        [Space(20)]
        [Tooltip("The maximal number of tries to find a suitable spawn point"), SerializeField]
        private int maxNumberOfTries = 100;
        [Tooltip("The maximum tries count per frame"), SerializeField]
        int maxTriesPerFrame = 5000;

        [Space(20)]
        [Tooltip("The chest probability for each spawner"), SerializeField]
        private float spawnProbability = 0.3f;
        [Tooltip("The spawn range around each spawner"), SerializeField]
        private Vector2Int spawnSize = new(5,5);
        [SerializeField] private Tile chestTile;
        [SerializeField] private string[] excludedSpawnerNames;

        [Space(20)]
        [SerializeField] private int zPosition = 0;

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
            MapObject[] spawners = WorldManager.Map.GetMapObjectsOfType(TileType.Spawner).
                ExceptWhere(obj => excludedSpawnerNames.Contains(obj.name)).ToArray();

            foreach (var spawner in spawners) 
            {
                if (random.Range01() > spawnProbability)
                {
                    continue;
                }

                RectInt spawnRange = new(spawner.range.position, spawnSize);
                Vector3Int spawnPoint = new((int)random.Range(spawnRange.xMin, spawnRange.xMax), (int)random.Range(spawnRange.yMin, spawnRange.yMax), zPosition);
                int triesCounter = 0;
                while (!tilemap.IsTileEmpty(spawnPoint) && triesCounter < maxNumberOfTries)
                {
                    spawnPoint = new((int)random.Range(spawnRange.xMin, spawnRange.xMax), (int)random.Range(spawnRange.yMin, spawnRange.yMax), zPosition);
                    triesCounter++;
                }
                if (triesCounter == maxNumberOfTries) 
                {
                    continue;
                }
                tilemap.SetTile(spawnPoint, chestTile);
            }

            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);
            yield break;
        }
    }
}
