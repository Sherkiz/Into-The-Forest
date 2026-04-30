using ITF.Math;
using ITF.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace ITF.WorldGeneration
{    /// <summary>
     /// Generates resources
     /// </summary>
    [CreateAssetMenu(fileName = "ResourceGenerator", menuName = "ITF/WorldGeneration/ResourceGenerator")]
    public class ResourceGenerator : ObjectGenerator
    {
        [System.Serializable]
        public class ResourceGenerationInfos
        {
            public MultipleTilesObject resourceTiles;
            public Vector2Int size => resourceTiles.size;
            public uint minNumber;
            public uint maxNumber;
            public int minDistance;
            public BoundsInt spawnBounds;
        }
        private int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }
        [Space(20)]
        [Tooltip("The maximal number of tries to fin a suitable spawn point"), SerializeField]
        private int maxNumberOfTries = 100; 
        [Tooltip("The maximum tries count per frame"), SerializeField]
        int maxTriesPerFrame = 5000;
        [Space(40)]
        [Tooltip("The resources informations: type + number")]
        [SerializeField] private ResourceGenerationInfos[] resourcesInfosArray;
        // Map the generate status to the task, 
        private Dictionary<GenerateStatus, Task> statusTaskMap = new();
        private List<Vector2Int> excludedTiles;

        //Store the resources spawn points
        private Dictionary<RectInt, ResourceGenerationInfos> resourcesLocations = new();
        public override GenerateStatus Generate(Tilemap tilemap)
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
            resourcesLocations.Clear();
        }

        private IEnumerator GenerateCoroutine(GenerateStatus generateStatus, Tilemap tilemap)
        {
            var bounds = tilemap.cellBounds;
            var size = bounds.size;
            var totalCells = size.x * size.y;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            int counter = 0;
            int generatedCount = 0;
            List<Vector2Int> samplePoints = new();
            // The resources waiting to be generated
            List<ResourceGenerationInfos> generatings = new();
            foreach (var resource in resourcesInfosArray)
            {
                if (resource.spawnBounds.xMax == 0 || resource.spawnBounds.yMax == 0) resource.spawnBounds = bounds; // default bounds to map bounds
                uint numberToSpawn = random.Range(resource.minNumber, resource.maxNumber + 1);
                for (int i = 0; i < numberToSpawn; i++)
                {
                    generatings.Add(resource);
                }
            }
            resourcesInfosArray.OrderByDescending(resourceInfo => resourceInfo.minDistance); // To place resources with the most important constraints first
            while (generatings.Count > 0)
            {
                var resourceToSpawn = generatings[0];
                for (int i = 0; i < maxNumberOfTries; i++)
                {
                    int minX = resourceToSpawn.spawnBounds.xMin;
                    int maxX = resourceToSpawn.spawnBounds.xMax;
                    int minY = resourceToSpawn.spawnBounds.yMin;
                    int maxY = resourceToSpawn.spawnBounds.yMax;

                    Vector2Int candidatePoint = new((int)random.Range(minX, minY), (int)random.Range(maxX, maxY));
                    RectInt candidateRect = new(candidatePoint.x, candidatePoint.y, resourceToSpawn.size.x, resourceToSpawn.size.y);

                    if (candidateRect.xMin < minX || candidateRect.xMax > maxX || candidateRect.yMin < minY || candidateRect.yMax > maxY) // part of resource is out of bounds
                    {
                        continue;
                    }

                    if (IsRectValid(candidateRect, resourceToSpawn.minDistance))
                    {
                        resourcesLocations[candidateRect] = resourceToSpawn;
                        generatings.RemoveAt(0);
                        PlaceMultipleTiles(resourceToSpawn.resourceTiles, tilemap, candidatePoint);
                        break;
                    }
                    if (counter >= maxTriesPerFrame)
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

        private void PlaceMultipleTiles(MultipleTilesObject multipleTilesObject, Tilemap tilemap, Vector2Int position)
        {
            for (int i = 0; i < multipleTilesObject.tiles.Length; i++)
            {
                var tile = multipleTilesObject.tiles[i];
                var posOffset = multipleTilesObject.posOffsets[i] + (Vector3Int)multipleTilesObject.expandLeftBottom;
                tilemap.SetTile((Vector3Int)position + posOffset, tile);
            }
        }
        private bool IsRectValid(RectInt candidateRect, float minDistance)
        {
            foreach (var otherRect in resourcesLocations.Keys)
            {
                if ((otherRect.center - candidateRect.center).sqrMagnitude < minDistance || candidateRect.Overlaps(otherRect)) return false;
            }
            foreach (var excludedPoint in excludedTiles)
            {
                if (candidateRect.Contains(excludedPoint)) return false;
            }
            return true;
        }
    }
}
