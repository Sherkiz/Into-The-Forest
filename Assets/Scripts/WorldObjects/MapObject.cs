using ITF.CustomTiles;
using UnityEngine;

namespace ITF.WorldObjects
{

    public abstract class MapObject : MonoBehaviour
    {
        public abstract string Name { get; }
        public abstract RectInt Range { get; }
        public abstract TileType Type { get; }
        public abstract Vector3Int EntranceOffset { get; }

        public abstract void SetName(string name);

        public abstract void SetLocation(RectInt range, TileType tileType, Vector3Int entranceOffset);

        public abstract void Init();

        public abstract void Deinit();
    }
}
