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
    /// Generate grass as background. The probability of identical tiles being adjacent decreases.
    /// </summary>
    [CreateAssetMenu(fileName = "GrassGenerator", menuName = "ITF/WorldGeneration/GrassGenerator")]
    public class GrassGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed { get => seed; set => seed = value; }

        [Space(40)]
        [Tooltip("The maximum traversal count per frame"), SerializeField]
        int maxTraversalPerFrame = 5000;

        [Space(40)]
        [SerializeField] Tile[] tiles;

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

            int generatedCount = 0;
            int totalCells = size.x * size.y;
            int counter = 0;
            for(int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for(int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    generatedCount++;
                    counter++;

                    int index = (int)random.Range(0, (uint)tiles.Length);
                    Tile tile = tiles[index];
                    // If the Tile on the left is the same, regenerate it.
                    if (x > bounds.xMin)
                    {
                        var leftTile = tilemap.GetTile(new Vector3Int(x - 1, y, 0));
                        if(leftTile == tile)
                        {
                            index = (int)random.Range(0, (uint)tiles.Length);
                            tile = tiles[index];
                        }
                    }
                    // If the Tile on the up is the same, regenerate it.
                    if (y > bounds.yMin)
                    {
                        var upTile = tilemap.GetTile(new Vector3Int(x, y - 1, 0));
                        if (upTile == tile)
                        {
                            index = (int)random.Range(0, (uint)tiles.Length);
                            tile = tiles[index];
                        }
                    }
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);

                    if (counter >= maxTraversalPerFrame)
                    {
                        counter = 0;
                        generateStatus.progress = generatedCount / (float)totalCells;
                        if (generatedCount < totalCells) yield return null;
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