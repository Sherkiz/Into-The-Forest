using ITF.Entity;
using ITF.Utilities;
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

        public override Character SpawnCharacter()
        {
            if (maxSpawnCount > 0 && spawnCount >= maxSpawnCount) return null;
            GameObject character = GameObjectPool.CreateGameObject(characterPrefab.gameObject);
            spawnCount++;
            return character.GetComponent<Character>();
        }

        private void Update()
        {
            if(maxSpawnCount > 0 && spawnCount >= maxSpawnCount) return;
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnInterval)
            {
                Character character = SpawnCharacter();
                if (character != null)
                {
                    character.transform.position = transform.position;
                }
                spawnTimer = 0;
            }
        }
    }

}