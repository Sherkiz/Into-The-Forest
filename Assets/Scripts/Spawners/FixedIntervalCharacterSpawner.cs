using ITF.Entity;
using ITF.Utilities;
using ITF.World;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Spawners
{
    /// <summary>
    /// Spawns characters at fixed intervals.
    /// </summary>

    public class FixedIntervalCharacterSpawner : CharacterSpawner
    {
        [SerializeField]
        Character characterPrefab;
        public float spawnInterval = 5f;
        [Tooltip("Maximum number of characters to spawn. Set to -1 for infinite spawns.")]
        public int maxSpawnCount = -1;

        float spawnTimer = 0;
        int spawnCount = 0;

        [SerializeField]
        UnityEvent<Character> onCharacterSpawned;
        public override UnityEvent<Character> OnCharacterSpawned => onCharacterSpawned;

        public static int spawned = 0;

        public override Character SpawnCharacter()
        {
            if (spawned > 0) return null;
            if (maxSpawnCount > 0 && spawnCount >= maxSpawnCount) return null;
            GameObject character = GameObjectPool.CreateGameObject(characterPrefab.gameObject);
            spawnCount++;
            spawned++;
            return character.GetComponent<Character>();
        }

        private void Update()
        {
            if(maxSpawnCount > 0 && spawnCount >= maxSpawnCount || !WorldManager.IsMapBuilt) return;
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnInterval)
            {
                Character character = SpawnCharacter();
                if (character != null)
                {
                    character.transform.position = transform.position;
                    character.Init();
                }
                spawnTimer = 0;
            }
        }
    }

}