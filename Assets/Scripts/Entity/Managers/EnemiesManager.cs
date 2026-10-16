using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity {
    public class EnemiesManager : MonoBehaviour, IEntityManager
    {
        List<Character> characters = new List<Character>();

        public Character[] GetCharacters() => characters.ToArray();

        public void RegisterCharacter(Character character)
        {
            if(characters.Contains(character)) return;
            characters.Add(character);
        }
    }
}
