using ITF.CustomTiles;
using UnityEngine;

namespace ITF.WorldObjects
{

    public abstract class MapObject : MonoBehaviour
    {
        public virtual string Name { get; private set; }
        public virtual RectInt Range { get; private set; }
        public virtual TileType Type { get; private set; }
        public virtual Vector3Int EntranceOffset { get; private set; }

        public virtual void SetName(string name)
        {
            Name = name;
        }

        public virtual void SetLocation(RectInt range, TileType tileType, Vector3Int entranceOffset)
        {
            Range = range;
            Type = tileType;
            EntranceOffset = entranceOffset;
        }

        public abstract void Init();

        public abstract void Deinit();
    }
}
