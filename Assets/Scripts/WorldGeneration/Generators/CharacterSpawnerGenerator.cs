using ITF.CustomTiles;
using ITF.Math;
using ITF.Spawners;
using ITF.Utilities;
using ITF.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.WorldGeneration
{
    /// <summary>
    /// Place the CharacterSpawner on MapObject
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSpawnerGenerator", menuName = "ITF/WorldGeneration/CharacterSpawnerGenerator")]
    public class CharacterSpawnerGenerator : ObjectGenerator
    {
        int seed;
        public override int Seed { get => seed; set => seed = value; }

        [SerializeField]
        CharacterSpawnerGenerateUnit[] characterSpawnerGenerateUnits;
        Dictionary<string, CharacterSpawnerGenerateUnit> spawnerUnits;

        // Map the generate status to the task, 
        Dictionary<GenerateStatus, Task> statusTaskMap = new();

        public override GenerateStatus Generate(TilemapManager tilemap)
        {
            seed = RandomManager.GetSeedFor(name);
            if (spawnerUnits == null)
            {
                spawnerUnits = new();
                foreach (var unit in characterSpawnerGenerateUnits)
                {
                    spawnerUnits.Add(unit.mapObjectName, unit);
                }
            }
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
            spawnerUnits?.Clear();
        }

        IEnumerator GenerateCoroutine(GenerateStatus generateStatus, TilemapManager tilemap)
        {
            foreach(var mapObject in WorldManager.Map.GetMapObjects())
            {
                if(spawnerUnits.TryGetValue(mapObject.name, out var unit))
                {
                    GameObject spawner = GameObjectPool.CreateGameObject(unit.spawnerPrefab.gameObject);
                    spawner.transform.position = tilemap.GetCellCenterWorld(((Vector3Int)mapObject.range.min) + mapObject.entranceOffset);
                }
            }

            generateStatus.progress = 1;
            generateStatus.finished = true;

            yield break;
        }

        [System.Serializable]
        class CharacterSpawnerGenerateUnit
        {
            public string mapObjectName;
            public CharacterSpawner spawnerPrefab;
        }
    }

}