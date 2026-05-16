using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{
    [CreateAssetMenu(fileName = "tile", menuName = "ITF/Tiles/ObjectTile")]
    public class ObjectTile : Tile, ICustomTile
    {
        [Header("Custom Tile Properties")]
        [SerializeField] string id;
        public string ID => id;

        [SerializeField] TileType tileType;
        public TileType TileType { get => tileType; set => tileType = value; }

        [SerializeField] int passCost = 10;
        public int PassCost { get => passCost; set => passCost = value; }

#if UNITY_EDITOR
        public void SetID(string newID)
        {
            id = newID;
        }

#endif
    }
}
