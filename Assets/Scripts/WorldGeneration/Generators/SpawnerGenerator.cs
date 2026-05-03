using ITF.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ITF.CustomTiles;

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
        [Tooltip("The maximal number of tries to fin a suitable spawn point"), SerializeField]
        private int maxNumberOfTries = 100;
        [Tooltip("The maximum tries count per frame"), SerializeField]
        int maxTriesPerFrame = 5000;
        [Space(40)]
        [Tooltip("The spawners informations: type + number")]
        [SerializeField] private SpawnerGenerationInfos[] spawnersInfosArray;

        [Space(20)]
        [SerializeField]

        [Space(20)]

        // Map the generate status to the task, 
        Dictionary<GenerateStatus, Task> statusTaskMap = new();

        public override GenerateStatus Generate(TilemapManager tilemap)
        {
            GenerateStatus generateStatus = new();
            statusTaskMap.Add(generateStatus, new(GenerateCoroutine(generateStatus, tilemap)));
            return generateStatus;
        }
        private List<Vector3Int> GetEmptyPositionsInTilemap(TilemapManager tilemap)
        {
            List<Vector3Int> emptyTiles = new();
            for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax + 1; x++)
            {
                for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax + 1; y++)
                {
                    Vector3Int coord = new(x, y);
                    if (tilemap.GetTile(coord) == null) emptyTiles.Add(coord); 
                }
            }
            return emptyTiles;
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
            generateStatus.progress = 1;
            generateStatus.finished = true;
            statusTaskMap.Remove(generateStatus);
            yield break;
        }
    }
}
