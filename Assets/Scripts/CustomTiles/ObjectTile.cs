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

        [SerializeField] int passCose = 10;
        public int PassCose { get => passCose; set => passCose = value; }
    }
}
