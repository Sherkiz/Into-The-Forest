using ITF.Spawners;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity {
    public class EnemiesManager : MonoBehaviour, IEntityManager
    {
        static EnemiesManager instance;
        public static EnemiesManager Instance => instance;

        List<Character> characters = new List<Character>();

        public Character[] GetCharacters() => characters.ToArray();

        Dictionary<Guid, EnemyGroup> enemyGroups = new Dictionary<Guid, EnemyGroup>();

        public void RegisterCharacter(Character character)
        {
            if(characters.Contains(character)) return;
            characters.Add(character);
        }

        public void AddCharacterToGroup(Guid guid, Character character)
        {
            if(!enemyGroups.ContainsKey(guid)) return;
            enemyGroups[guid].AddCharacter(character);
        }

        public void RemoveChracterFromGroup(Guid guid, Character character)
        {
            if(!enemyGroups.ContainsKey(guid)) return;
            enemyGroups[guid].RemoveCharacter(character);
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
