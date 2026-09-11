using ITF.CustomTiles;
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

        }

        public override void Deinit()
        {

        }
    }
}