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
        [System.Serializable]
        class GenerationUnit
        {
            public string name;

            [Tooltip("They will be executed in order"), SerializeField]
            public ObjectGenerator[] generators;
            [SerializeField]
            public Tilemap tilemap;
            [SerializeField]
            public Vector3Int worldSize = new(192, 192, 1);
        }

        [SerializeField]
        int seed;
        public int Seed => seed;

        [SerializeField]
        GenerationUnit[] generationUnits;

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
            generating = new(GenerateCoroutine());
        }

        private void Start()
        {
            foreach (var generationUnit in generationUnits)
            {
                var generators = generationUnit.generators;
                foreach (var generator in generators)
                {
                    generator.StopAllGeneration();
                }
            }
        }

        private void OnDestroy()
        {
            if(generating != null) generating.Stop();
            foreach (var generationUnit in generationUnits)
            {
                var generators = generationUnit.generators;
                foreach (var generator in generators)
                {
                    generator.StopAllGeneration();
                }
            }
        }

        IEnumerator GenerateCoroutine()
        {
            foreach (var generationUnit in generationUnits)
            {
                generationUnit.tilemap.origin = Vector3Int.zero;
                generationUnit.tilemap.size = generationUnit.worldSize;
                generationUnit.tilemap.ResizeBounds();
                var generators = generationUnit.generators;
                Debug.Log($"Generating {generationUnit.name}...");
                for (int i = 0; i < generators.Length; i++)
                {
                    var status = generators[i].Generate(generationUnit.tilemap);
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
            }
            Debug.Log("Generated!");
            generating = null;
        }
    }
}
