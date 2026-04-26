using UnityEngine;
using UnityEngine.Tilemaps;

namespace ITF.CustomTiles
{
    [CreateAssetMenu(fileName = "tile", menuName = "ITF/Tiles/ObjectTile")]
    public class ObjectTile : Tile
    {
        public TileType TileType;
    }
    public enum TileType
    {
        None,
        Tree,
        Resource,
        Road
    }
}
