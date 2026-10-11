using ITF.Entity;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Spawners
{
    public class EnemySpawner : CharacterSpawner
    {
        [SerializeField] private Character enemyPrefab;
        [SerializeField] private int count;

        private UnityEvent<Character> onCharacterSpawned;
        public override UnityEvent<Character> OnCharacterSpawned => onCharacterSpawned;

        public override Character SpawnCharacter()
        {
            throw new System.NotImplementedException();
        }

        public override Character[] SpawnCharacters()
        {
            throw new System.NotImplementedException();
        }
    }
}
