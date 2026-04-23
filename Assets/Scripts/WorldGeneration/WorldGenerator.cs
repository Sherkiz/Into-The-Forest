using ITF.Math;
using ITF.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.WorldGeneration
{
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField]
        int seed;
        public int Seed => seed;

        [Tooltip("They will be executed in order"), SerializeField]
        ObjectGenerator[] generators;
        [SerializeField]
        Tilemap tilemap;
        [SerializeField]
        Vector3Int worldSize = new(192, 192, 1);

        Task generating;

        public void Init(int seed)
        {
            this.seed = seed;
            RandomManager.Init(seed);
        }

        [ContextMenu("Random Init")]
        public void InitWithRandomSeed()
        {
            Init((int)DateTimeOffset.Now.ToUnixTimeSeconds());
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if(generating != null) generating.Stop();
            tilemap.origin = Vector3Int.zero;
            tilemap.size = worldSize;
            tilemap.ResizeBounds();
            generating = new(GenerateCoroutine());
        }

        private void Start()
        {
            foreach(var generator in generators)
            {
                generator.StopAllGeneration();
            }
        }

        private void OnDestroy()
        {
            if(generating != null) generating.Stop();
            foreach (var generator in generators)
            {
                generator.StopAllGeneration();
            }
        }

        IEnumerator GenerateCoroutine()
        {
            for (int i = 0; i < generators.Length; i++)
            {
                var status = generators[i].Generate(tilemap);
                while (!status.finished)
                {
                    if (status.failed)
                    {
                        Debug.LogError($"Generation failed for {generators[i].name}");
                        generating = null;
                        yield break;
                    }
                    Debug.Log($"Generating {(i + status.progress) / generators.Length * 100f}%");
                    yield return null;
                }
            }
            Debug.Log("Generated!");
            generating = null;
        }
    }
}
