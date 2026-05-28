using ITF.CustomTiles;
using ITF.Math;
using ITF.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generate roads.
    /// </summary>
    [CreateAssetMenu(fileName = "GrassGenerator", menuName = "ITF/WorldGeneration/RoadGenerator")]
    public class RoadGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed { get => seed; set => seed = value; }

        [Space(20)]
        [Tooltip("The maximum traversal count per frame"), SerializeField]
        int maxTraversalPerFrame = 5000;

        [Space(20)]
        [SerializeField] Tile roadTile;

        [Space(20)]
        [SerializeField] private float roadGenerationPercentChance = 80f;
        [SerializeField] private float mainRoadGenerationPercentChance = 60f;
        [SerializeField] private int mainRoadXMax = 30;

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
            var bounds = tilemap.cellBounds;
            var size = bounds.size;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            float value = random.Range(0f, 100f);
            if (value < roadGenerationPercentChance)
            {
                int generatedCount = 0;
                int totalCells = size.x * size.y;
                int counter = 0;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {

                    }
                }
            }
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);

            yield break;
        }
    }

}