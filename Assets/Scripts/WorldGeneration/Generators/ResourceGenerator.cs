using ITF.Math;
using ITF.Utilities;
using ITF.CustomTiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            public string name;
            public MultipleTilesObject resourceTiles;
            public Vector2Int size => resourceTiles.size;
            public uint minNumber;
            public uint maxNumber;
            public float minDistance;
            public RectInt mapRange;
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

        [Space(20)]
        [SerializeField] string targetResourceName;

        // Map the generate status to the task, 
        private Dictionary<GenerateStatus, Task> statusTaskMap = new();
        private List<Vector2Int> excludedTiles = new();

        //Store the resources spawn points
        private Dictionary<RectInt, ResourceGenerationInfos> resourcesLocations = new();
        public override GenerateStatus Generate(TilemapManager tilemap)
        {
            GenerateStatus generateStatus = new();
            statusTaskMap.Add(generateStatus, new(GenerateCoroutine(generateStatus, tilemap)));
            return generateStatus;
        }

        [ContextMenu("Auto Create Pos Offsets")]
        public void AutoCreatePosOffsets()
        {
            foreach (var resource in resourcesInfosArray)
            {
                if (resource.name != targetResourceName) continue;

                Vector2Int size = resource.size - resource.resourceTiles.expandLeftBottom - resource.resourceTiles.expandRightTop;
                Vector3Int[] posOffsets = new Vector3Int[size.x * size.y];
                for (int y = size.y - 1; y >= 0; y--)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        posOffsets[(size.y - 1 - y) * size.x + x] = new Vector3Int(x, y, 0);
                    }
                }
                resource.resourceTiles.posOffsets = posOffsets;
            }
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
        private IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            var bounds = tilemap.cellBounds;
            var size = bounds.size;
            var totalCells = size.x * size.y;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));
            // The resources waiting to be generated
            List<ResourceGenerationInfos> generatings = new();
            foreach (var resource in resourcesInfosArray)
            {
                if (resource.mapRange.xMax == 0 || resource.mapRange.yMax == 0) resource.mapRange = new RectInt(bounds.xMin, bounds.yMin, size.x, size.y); // default bounds to map bounds
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
                int minX = resourceToSpawn.mapRange.xMin;
                int maxX = resourceToSpawn.mapRange.xMax;
                int minY = resourceToSpawn.mapRange.yMin;
                int maxY = resourceToSpawn.mapRange.yMax;
                bool success = false;
                for (int i = 0; i < maxNumberOfTries; i++)
                {
                    Vector2Int candidatePoint = new((int)random.Range(minX, maxX), (int)random.Range(minY, maxY));
                    RectInt candidateRect = new(candidatePoint.x, candidatePoint.y, resourceToSpawn.size.x, resourceToSpawn.size.y);
                    if (candidateRect.xMin < minX || candidateRect.xMax > maxX || candidateRect.yMin < minY || candidateRect.yMax > maxY) // part of resource is out of bounds
                    {
                        continue;
                    }
                    if (IsRectValid(candidateRect, resourceToSpawn.minDistance, tilemap))
                    {
                        resourcesLocations[candidateRect] = resourceToSpawn;
                        generatings.RemoveAt(0);
                        tilemap.PlaceMultipleTiles(resourceToSpawn.resourceTiles, (Vector3Int) candidatePoint);
                        success = true; 
                        break;
                    }                    
                }
                if (!success)
                {
                    // To update !! Needs to do something more when placement failed
                    generatings.RemoveAt(0);
                    Debug.Log("Failed to place " + resourceToSpawn.name);
                }
            }
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);
            yield break;
        }
        private bool IsRectValid(RectInt candidateRect, float minDistance, TilemapManager tilemap)
        {
            foreach (var otherRect in resourcesLocations.Keys)
            {
                if ((candidateRect.center - otherRect.center).sqrMagnitude < minDistance || candidateRect.Overlaps(otherRect)) return false;
            }
            foreach (var excludedPoint in excludedTiles)
            {
                if (candidateRect.Contains(excludedPoint)) return false;
            }
            return !tilemap.OverlapOccupiedTiles(candidateRect);
        }
    }
}
