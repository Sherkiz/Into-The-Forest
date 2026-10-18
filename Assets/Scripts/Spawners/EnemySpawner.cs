using ITF.Entity;
using ITF.Utilities;
using ITF.World;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Spawners
{
    public class EnemySpawner : CharacterSpawner
    {
        [SerializeField] private Character enemyPrefab;
        [Tooltip("Number of enemies to spawn in each wave")]
        [SerializeField] private MinMaxInt countRange;
        [Tooltip("Number of waves")]
        [SerializeField] private int maxWaves = 2;
        [Tooltip("Time between two waves")]
        [SerializeField] private MinMaxFloat spawnDelayRange;
        [Tooltip("Time between two spawns in the same wave")]
        [SerializeField] private MinMaxFloat spawnIntervalRange;
        [SerializeField] private int spawnRadius = 4;

        private Vector2Int centerCell;
        private float spawnTimer = 0;
        private float waveTimer = 0;
        int currentWave = 0;
        int spawnCount;
        int spawned = 0;

        private UnityEvent<Character> onCharacterSpawned;
        public override UnityEvent<Character> OnCharacterSpawned => onCharacterSpawned;

        private void OnEnable()
        {
            centerCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(transform.position);
            spawnTimer = spawnDelayRange.Value;
        }

        public override Character SpawnCharacter()
        {
            if(currentWave >= maxWaves) return null;
            spawnTimer = spawnIntervalRange.GetOtherValue();
            if (++spawned >= spawnCount)
            {
                currentWave++;
                spawned = 0;
                if (currentWave < maxWaves)
                {
                    spawnCount = countRange.Value;
                    spawnTimer = spawnDelayRange.GetOtherValue();
                }
            }

            Character character = GameObjectPool.CreateGameObject(enemyPrefab.gameObject).GetComponent<Character>();
            onCharacterSpawned?.Invoke(character);
            return character;
        }

        public override Character[] SpawnCharacters()
        {
            if(currentWave >= maxWaves) return null;
            int wave = currentWave;
            List<Character> characters = new List<Character>();
            while(wave == currentWave)
            {
                characters.Add(SpawnCharacter());
            }
            return characters.ToArray();
        }

        private void Update()
        {
            if(currentWave >= maxWaves) return;
            if (spawnTimer > 0)
            {
                spawnTimer -= Time.deltaTime;
                if(spawnTimer <= 0)
                {
                    SpawnCharacter();
                }
            }
        }

        Vector2Int RandomSpawnCell()
        {
            Vector2Int cell = centerCell + new Vector2Int(Random.Range(-spawnRadius, spawnRadius + 1), Random.Range(-spawnRadius, spawnRadius + 1));
            WorldManager.Map.GetNearestEmptyCell(cell, out cell);
            return cell;
        }
    }
}
