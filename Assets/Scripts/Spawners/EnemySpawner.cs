using ITF.BehaviourTree;
using ITF.Entity;
using ITF.Utilities;
using ITF.World;
using MBT;
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
        [SerializeField] private Vector2Int spawnRadius = new Vector2Int(4,4);

        private Vector2Int centerCell;
        private float spawnTimer = 0;
        private float waveTimer = 0;
        int currentWave = 0;
        int spawnCount;
        int spawned = 0;

        private UnityEvent<Character> onCharacterSpawned;
        public override UnityEvent<Character> OnCharacterSpawned => onCharacterSpawned;

        public override void Init()
        {
            centerCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(transform.position);
            spawnTimer = 0;
            waveTimer = 0;
            currentWave = 0;
            spawnCount = 0;
            spawned = 0;
        }

        public override Character SpawnCharacter()
        {
            if(currentWave >= maxWaves) return null;
            spawnTimer = 0f;
            if (++spawned >= spawnCount)
            {
                currentWave++;
                waveTimer = spawnDelayRange.Value;
                spawned = 0;
                if (currentWave < maxWaves)
                {
                    spawnCount = countRange.Value;
                    spawnTimer = spawnDelayRange.GetOtherValue();
                }
            }

            Character character = GameObjectPool.CreateGameObject(enemyPrefab.gameObject).GetComponent<Character>();
            if(character.TryGetComponentInReferences<Blackboard>(GlobalConstants.BLACKBOARD, out var blackboard))
            {
                var cell = RandomSpawnCell();
                blackboard.SetValue<Vector2Variable, Vector2>(GlobalConstants.TARGET_CELL, cell);
                blackboard.SetValue<Vector2Variable, Vector2>(GlobalConstants.START_CELL, cell);
            }
            character.Init();
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
            if(waveTimer > 0)
            {
                waveTimer -= Time.deltaTime;
                return;
            }
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
            Vector2Int cell = centerCell + new Vector2Int(Random.Range(-spawnRadius.x, spawnRadius.x + 1), Random.Range(-spawnRadius.y, spawnRadius.y + 1));
            WorldManager.Map.GetNearestEmptyCell(cell, out cell);
            return cell;
        }
    }
}
