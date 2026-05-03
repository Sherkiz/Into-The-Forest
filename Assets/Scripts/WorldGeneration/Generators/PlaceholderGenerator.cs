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
    /// Remove the specific tile
    /// </summary>
    [CreateAssetMenu(fileName = "PlaceholderGenerator", menuName = "ITF/WorldGeneration/PlaceholderGenerator")]
    public class PlaceholderGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed { get => seed; set => seed = value; }

        [Space(40)]
        [Tooltip("The maximum traversal count per frame"), SerializeField]
        int maxTraversalPerFrame = 2000;

        [Space(20)]
        [SerializeField] Tile tile;
        public int z = 0;

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
            if(tile == null)
            {
                generateStatus.progress = 1;
                generateStatus.finished = true;
                yield break;
            }

            var bounds = tilemap.cellBounds;
            var size = bounds.size;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));

            int generatedCount = 0;
            int totalCells = size.x * size.y;
            int counter = 0;
            for(int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for(int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    if (tilemap.GetTile(pos) == tile)
                    {
                        tilemap.SetTile(pos, null);
                    }

                    counter++;
                    generatedCount++;
                    if(counter >= maxTraversalPerFrame)
                    {
                        counter = 0;
                        generateStatus.progress = generatedCount / (float)totalCells;
                        if(generatedCount < maxTraversalPerFrame) yield return null;
                    }
                }
            }
            generateStatus.progress = 1;
            generateStatus.finished = true;

            yield break;
        }
    }

}