using ITF.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Generates spawners.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnerGenerator", menuName = "ITF/WorldGeneration/SpawnerGenerator")]

    public class SpawnerGenerator : ObjectGenerator
    {
        [System.Serializable]
        class SpawnerGenerationInfos
        {
            public string name;
            public MultipleTilesObject resourceTiles;
            public Vector2Int size => resourceTiles.size;
            public uint minNumber;
            public uint maxNumber;
            public int minDistance;
            public BoundsInt spawnBounds;
        }

        int seed;
        public override int Seed
        {
            get => seed;
            set => seed = value;
        }

        [Space(20)]
        [SerializeField]

        [Space(20)]

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
            foreach (var pair in statusTaskMap)
            {
                pair.Value.Stop();
                pair.Key.failed = !pair.Key.finished;
            }
            statusTaskMap.Clear();
        }

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, Tilemap tilemap)
        {
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);
            yield break;
        }
    }
}
