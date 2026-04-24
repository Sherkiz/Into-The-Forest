using ITF.Math;
using ITF.Utilities;
using ITF.WorldObjects;
using System.Collections;
using System.Collections.Generic;
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
            public Resource resource;
            public int minNumber;
            public int maxNumber;
            public int minDistance;
            public int XBound;
            public int YBound;
        }
        private int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }
       
        [Space(40)]
        [Tooltip("The resources informations: type + number")]
        private ResourceGenerationInfos resourcesInfos;
        // Map the generate status to the task, 
        private Dictionary<GenerateStatus, Task> statusTaskMap = new();

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
        }

        private IEnumerator GenerateCoroutine(GenerateStatus generateStatus, Tilemap tilemap)
        {
            var bounds = tilemap.cellBounds;
            var size = bounds.size;
            XorShiftRandom random = new((uint)RandomManager.GetSeedFor(name));


            yield break;
        }
    }
}
