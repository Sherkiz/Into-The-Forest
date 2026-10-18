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
        [SerializeField] private SpawnUnit[] spawnUnits;
        [SerializeField] private float health;

        private SimpleTimer spawnTimer;
        private int spawnUnitIndex;
        private List<Character> enemies;

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
            spawnUnitIndex = 0;
        }
        public override void Deinit()
        {

        }
        private void OnWorldGenerated()
        {
            WorldManager.Map.onBuilt += SpawnEnemies;
        }
        public void SpawnEnemies(Map map)
        {
            if (spawnTimer != null) TimeManager.RemoveSimpleTimer(spawnTimer);
            spawnTimer = new SimpleTimer(spawnUnits[spawnUnitIndex].delay, _ =>
            {
                EnemySpawner spawner = Instantiate(spawnUnits[spawnUnitIndex].spawner);
                Character[] characters = spawner.SpawnCharacters();
                Destroy(spawner.gameObject);
                enemies.AddRange(characters);
                foreach (var character in characters)
                {
                    character.Init();
                }

                spawnUnitIndex++;
                if (spawnUnitIndex < spawnUnits.Length)
                {
                    _.timer = 0;
                    _.interval = spawnUnits[spawnUnitIndex].delay;
                }
            });
            TimeManager.AddSimpleTimer(spawnTimer);
            WorldManager.Map.onBuilt -= SpawnEnemies;
        }

        [System.Serializable]
        struct SpawnUnit
        {
            public EnemySpawner spawner;
            [SerializeField] private MinMaxFloat delayRange;
            public float delay => delayRange.Value;
        }
    }
}