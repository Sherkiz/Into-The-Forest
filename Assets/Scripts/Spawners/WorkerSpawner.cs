using ITF.Entity;
using ITF.Utilities;
using ITF.World;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Spawners
{

    public class WorkerSpawner : CharacterSpawner
    {
        [SerializeField]
        SpawnPair[] spawnPairs;

        int spawnCount;
        int pairIndex = 0;
        int characterIndex = 0;

        [Tooltip("The area in which to spawn the workers.")]
        public RectInt spawnArea;

        [SerializeField]
        UnityEvent<Character> onCharacterSpawned;
        public override UnityEvent<Character> OnCharacterSpawned => onCharacterSpawned;
        public override Character SpawnCharacter()
        {
            if (pairIndex >= spawnPairs.Length) return null;
            var pair = spawnPairs[pairIndex];
            while (characterIndex == 0)
            {
                spawnCount = (pair.spawnCount.x > pair.spawnCount.y) ? pair.spawnCount.x : Random.Range(pair.spawnCount.x, pair.spawnCount.y + 1);
                if (spawnCount > 0)
                    break;
                else
                {
                    pairIndex++;
                    if (pairIndex >= spawnPairs.Length) return null;
                    pair = spawnPairs[pairIndex];
                }
            }

            if (spawnCount > characterIndex)
            {
                Character characterPrefab = pair.characterPrefabs[Random.Range(0, pair.characterPrefabs.Length)];
                Character character = GameObjectPool.CreateGameObject(characterPrefab.gameObject).GetComponent<Character>();
                Vector2Int spawnPosition = new Vector2Int(Random.Range(spawnArea.xMin, spawnArea.xMax), Random.Range(spawnArea.yMin, spawnArea.yMax));
                for (int i = 0; i < 30; i++)
                {
                    if (WorldManager.Map.GetNearestEmptyCell(spawnPosition, spawnArea, out Vector2Int emptyCell)){
                        spawnPosition = emptyCell;
                        break;
                    }
                }
                character.transform.position = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)spawnPosition);
                characterIndex++;
                if(characterIndex >= spawnCount)
                {
                    pairIndex++;
                    characterIndex = 0;
                }
                return character;
            }

            return null;
        }

        public override Character[] SpawnCharacters()
        {
            List<Character> characters = new();
            while (true)
            {
                Character character = SpawnCharacter();
                if (character == null) break;
                characters.Add(character);
            }
            return characters.ToArray();
        }

        [System.Serializable]
        struct SpawnPair
        {
            [Tooltip("The count of workers to spawn in this pair. Random between the two values.")]
            public Vector2Int spawnCount;
            [Tooltip("The prefab of the worker to spawn. Randomly chosen from the array.")]
            public Character[] characterPrefabs;
        }
    }

}