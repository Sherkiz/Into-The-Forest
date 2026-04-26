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
            public Tile resourceTile;
            public uint minNumber;
            public uint maxNumber;
            public int minDistance;
            public Vector2Int XBounds;
            public Vector2Int YBounds;
            public void OnValidate()
            {
                minNumber = (uint)Mathf.Min(minNumber, maxNumber);
                XBounds.x = (Mathf.Min(XBounds.x, XBounds.y));
                YBounds.x = (Mathf.Min(YBounds.x, YBounds.y));
            }
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

        //Store the resources spawn points
        private Dictionary<Vector2Int, ResourceGenerationInfos> resourcesLocations = new();
        private void OnValidate()
        {
            foreach(var rInfo in resourcesInfosArray) rInfo.OnValidate();
        }
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
            resourcesInfosArray.OrderByDescending(resourceInfo => resourceInfo.minDistance);
            foreach (var resourceInfo in resourcesInfosArray)
            {
                if (resourceInfo.XBounds == Vector2Int.zero) resourceInfo.XBounds = new Vector2Int(bounds.xMin, bounds.xMax);
                if (resourceInfo.YBounds == Vector2Int.zero) resourceInfo.YBounds = new Vector2Int(bounds.yMin, bounds.yMax);
                uint numberToSpawn = random.Range(resourceInfo.minNumber, resourceInfo.maxNumber + 1);
                for (int i = 0; i < numberToSpawn; i++)
                {
                    Debug.Log("Placing " + resourceInfo.resourceTile.name);
                    for (int j = 0; j < maxNumberOfTries; j++)
                    {
                        Vector2Int candidate = new Vector2Int((int)random.Range(resourceInfo.XBounds.x, resourceInfo.XBounds.y), (int)random.Range(resourceInfo.YBounds.x, resourceInfo.YBounds.y));
                        if (IsSpawnPointValid(candidate, resourceInfo.minDistance, tilemap))
                        {
                            Debug.Log(j.ToString() + " tries needed");
                            resourcesLocations[candidate] = resourceInfo;
                            tilemap.SetTile((Vector3Int)candidate, resourceInfo.resourceTile);
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
            }
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);
            yield break;
        }

        private bool IsSpawnPointValid(Vector2Int point, float minDistance, Tilemap tilemap)
        {
            if (tilemap.GetTile((Vector3Int)point) != null) return false;
            foreach (var loc in resourcesLocations) 
            {
                minDistance = Mathf.Max(minDistance, loc.Value.minDistance);
                if ((loc.Key - point).sqrMagnitude < minDistance) return false;
            }
            return true;
        }
    }
}
