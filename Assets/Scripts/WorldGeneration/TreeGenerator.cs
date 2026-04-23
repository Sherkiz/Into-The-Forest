using ITF.Math;
using ITF.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generates trees based on Perlin noise
    /// </summary>
    [CreateAssetMenu(fileName = "TreeGenerator", menuName = "ITF/WorldGeneration/TreeGenerator")]
    public class TreeGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }

        [Space(40)]
        [Tooltip("The density of trees. [0, 1]")]
        public float density = 0.3f;
        public float noiseScale = .1f;
        [Tooltip("The maximum traversal count per frame"), SerializeField]
        int maxTraversalPerFrame = 5000;

        [Space(40)]
        public Tile tile;

        // Map the generate status to the task, 
        Dictionary<GenerateStatus, Task> statusTaskMap = new();

        public override GenerateStatus Generate(Tilemap tilemap)
        {
            GenerateStatus generateStatus = new();
            statusTaskMap.Add(generateStatus, new(GenerateCoroutine(generateStatus, tilemap)));
            return generateStatus;
        }

        public override void StopAllGeneration()
        {
            foreach(var pair in statusTaskMap)
            {
                pair.Value.Stop();
                pair.Key.failed = !pair.Key.finished;
            }
            statusTaskMap.Clear();
        }

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, Tilemap tilemap)
        {
            var bounds = tilemap.cellBounds;
            var size = bounds.size;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            Vector2 noiseSeed = new(random.Range(0f, 99_999.99f), random.Range(0f, 99_999.99f));

            int generatedCount = 0;
            int totalCells = size.x * size.y;
            int counter = 0;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for(int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    generatedCount++;
                    counter++;
                    Vector2Int pos = new(x, y);
                    float noiseValue = Mathf.PerlinNoise(noiseSeed.x + pos.x * noiseScale, noiseSeed.y + pos.y * noiseScale);
                    if (noiseValue < density)
                    {
                        tilemap.SetTile((Vector3Int)pos, tile);
                    }
                    if (counter >= maxTraversalPerFrame)
                    {
                        counter = 0;
                        generateStatus.progress = generatedCount / (float)totalCells;
                        if(generatedCount < totalCells) yield return null;
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