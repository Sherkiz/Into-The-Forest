using System.Collections.Generic;
using UnityEngine;
using ITF.Entity;

namespace ITF.World
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapChunk : MonoBehaviour
    {
        [SerializeField] private new BoxCollider2D collider;
        public List<Character> characters;

        public void SetMapChunkRange(Vector2 center, Vector2 size)
        {
            transform.position = center;
            collider.offset = Vector2.zero;
            collider.size = size;
        }
        public void Activate(bool active)
        {
            collider.enabled = active;
        }
        private void AddCharacter(Character character) { characters.Add(character); }
        private void RemoveCharacter(Character character) { characters.Remove(character); }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Character>(out Character character))
            {
                AddCharacter(character);
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<Character>(out Character character))
            {
                RemoveCharacter(character);
            }
        }
    }
}
