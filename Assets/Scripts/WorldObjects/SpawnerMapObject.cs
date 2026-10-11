using ITF.CustomTiles;
using ITF.Spawners;
using ITF.World;
using ITF.Utilities;
using UnityEngine;

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

        }

        [System.Serializable]
        struct SpawnUnit
        {
            public EnemySpawner spawner;
            public MinMaxFloat delayRange;
        }
    }
}