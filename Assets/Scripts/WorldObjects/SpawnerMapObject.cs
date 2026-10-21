using ITF.CustomTiles;
using ITF.Spawners;
using ITF.World;
using ITF.Utilities;
using UnityEngine;
using ITF.Entity;
using System.Collections.Generic;

namespace ITF.WorldObjects
{

    public class SpawnerMapObject : MapObject
    {
        [SerializeField]
        new string name;
        public override string Name => name;
        [SerializeField]
        RectInt range;
        public override RectInt Range => range;
        [SerializeField]
        TileType type;
        public override TileType Type => type;
        [SerializeField]
        Vector3Int entranceOffset;
        public override Vector3Int EntranceOffset => entranceOffset;
        public float maxHealth = 100;
        [SerializeField] private float health;
        public float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, maxHealth);
                if (health <= 0)
                {
                    Deinit();
                }
            }
        }
        [SerializeField] private CharacterSpawner _characterSpawner;

        CharacterSpawner characterSpawner;

        public override void SetLocation(RectInt range, TileType tileType, Vector3Int entranceOffset)
        {
            this.range = range;
            this.type = tileType;
            this.entranceOffset = entranceOffset;
        }

        public override void SetName(string name)
        {
            this.name = name;
        }

        public override void Init()
        {
            WorldManager.OnWorldGenerated.AddListener(OnWorldGenerated);
            health = maxHealth;
        }
        public override void Deinit()
        {
            WorldManager.Map.onBuilt -= OnMapBuilt;
            if(characterSpawner)
            {
                characterSpawner.OnCharacterSpawned.RemoveListener(OnSpawned);
                Destroy(characterSpawner.gameObject);
            }
        }
        private void OnWorldGenerated()
        {
            WorldManager.Map.onBuilt += OnMapBuilt;
        }
        
        void OnMapBuilt(Map map)
        {
            characterSpawner = Instantiate(_characterSpawner);
            characterSpawner.transform.position = transform.position;
            characterSpawner.OnCharacterSpawned.AddListener(OnSpawned);
            characterSpawner.Init();
            WorldManager.Map.onBuilt -= OnMapBuilt;
        }

        void OnSpawned(Character character)
        {
            EnemiesManager.Instance.RegisterCharacter(character);
        }
    }
}