using ITF.Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity {
    public class EnemiesManager : MonoBehaviour, IEntityManager
    {
        static EnemiesManager instance;
        public static EnemiesManager Instance => instance;

        List<Character> characters = new List<Character>();

        public Character[] GetCharacters() => characters.ToArray();

        public void RegisterCharacter(Character character)
        {
            if(characters.Contains(character)) return;
            characters.Add(character);
        }

        void Awake()
        {
            if(instance == null) instance = this;
        }

        private void OnDestroy()
        {
            if(instance == this) instance = null;
        }
    }
}
