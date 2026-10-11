using ITF.Entity;
using ITF.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Spawners
{
    public class EnemySpawner : CharacterSpawner
    {
        [SerializeField] private Character enemyPrefab;
        [SerializeField] private MinMaxInt countRange;
        [SerializeField] private MinMaxFloat spawnDelayRange;

        private SimpleTimer spawnTimer;

        private UnityEvent<Character> onCharacterSpawned;
        public override UnityEvent<Character> OnCharacterSpawned => onCharacterSpawned;

        public override Character SpawnCharacter()
        {
            throw new System.NotImplementedException();
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
    }
}
