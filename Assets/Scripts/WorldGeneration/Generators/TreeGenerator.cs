using ITF.Math;
using ITF.Utilities;
using ITF.CustomTiles;
using ProceduralNoiseProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generates trees based on Perlin noise
    /// Use worley noise to avoid having no access
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

        [Space(20)]
        [Tooltip("The density of trees. [0, 1]")]
        public float density = 0.3f;
        public float noiseScale = .1f;
        [Tooltip("If worley noise smaller than this value, the tree won't be generated")]
        public float minWorleyValue = .3f;
        public float worleyFrequency = 10f;
        public float worleyJitter = 1f;
        public Vector2 worleyScale = new(1f, .75f);
        [Tooltip("The maximum traversal count per frame"), SerializeField]
        int maxTraversalPerFrame = 5000;

        [Space(20)]
        public Tile tileTopLeft;
        public Tile tileBottomRight;
        public Tile tileTopRight;
        public Tile tileBottomLeft;

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
            foreach(var pair in statusTaskMap)
            {
                pair.Value.Stop();
                pair.Key.failed = !pair.Key.finished;
            }
            statusTaskMap.Clear();
        }

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            var bounds = tilemap.cellBounds;
            Debug.Log(bounds.yMax);
            var size = bounds.size;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            Vector2 noiseSeed = new(random.Range(0f, 99_999.99f), random.Range(0f, 99_999.99f));

            //worley noise
            WorleyNoise worleyNoise = new((int)random.Next(), worleyFrequency, worleyJitter);

            int generatedCount = 0;
            int totalCells = size.x * size.y;
            int counter = 0;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for(int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    generatedCount++;
                    counter++;
                    float noiseValue = Mathf.PerlinNoise(noiseSeed.x + x * noiseScale, noiseSeed.y + y * noiseScale);
                    float worleyValue = worleyNoise.Sample2D(x * worleyScale.x, y * worleyScale.y);
                    if (noiseValue < density && worleyValue > minWorleyValue)
                    {
                        //Avoid covering other tile
                        RectInt treeRect = new RectInt(x, y, 2, 2);
                        if (!tilemap.OverlapOccupiedTiles(treeRect))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, bottomZ), tileBottomLeft);
                            if (x + 1 < bounds.xMax) tilemap.SetTile(new Vector3Int(x + 1, y, bottomZ), tileBottomRight);
                            if (y + 1 < bounds.yMax)
                            {
                                Debug.Log(y);
                                tilemap.SetTile(new Vector3Int(x, y + 1, topZ), tileTopLeft);
                                if (x + 1 < bounds.xMax) tilemap.SetTile(new Vector3Int(x + 1, y + 1, topZ), tileTopRight);
                            }
                        }
                    }
                    //if (worleyValue < minWorleyValue)
                    //{
                    //    tilemap.SetTile(new Vector3Int(x, y, bottomZ), tileBottomLeft);
                    //}
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