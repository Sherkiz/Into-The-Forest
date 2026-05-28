using ITF.CustomTiles;
using ITF.Math;
using ITF.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public int zPosition = 0;

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
            tilemap.RemoveAllPlaceHolderTiles(zPosition);
            generateStatus.progress = 1;
            generateStatus.finished = true;

            yield break;
        }
    }

}