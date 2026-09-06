using ITF.Entity;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Spawners
{

    public abstract class CharacterSpawner : MonoBehaviour
    {
        
        public abstract UnityEvent<Character> OnCharacterSpawned { get; }

        public abstract Character SpawnCharacter();

        public abstract Character[] SpawnCharacters();
    }

}